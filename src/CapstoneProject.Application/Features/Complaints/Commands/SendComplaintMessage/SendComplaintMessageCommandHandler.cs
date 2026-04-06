using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessage;

public class SendComplaintMessageCommandHandler : IRequestHandler<SendComplaintMessageCommand, Result<ComplaintMessagePostedDto>>
{
    private const int MaxAttachmentCount = 5;
    private const long MaxAttachmentSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;

    public SendComplaintMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ComplaintMessagePostedDto>> Handle(SendComplaintMessageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ComplaintMessagePostedDto>.Failure("Authentication required. Please log in to send a message.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        if (command.ComplaintId == Guid.Empty)
            return Result<ComplaintMessagePostedDto>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Content))
            return Result<ComplaintMessagePostedDto>.Failure("Message content is required.", ErrorCodeEnum.ValidationFailed);

        var attachmentValidationError = ValidateAttachments(command.Attachments);
        if (!string.IsNullOrWhiteSpace(attachmentValidationError))
            return Result<ComplaintMessagePostedDto>.Failure(attachmentValidationError, ErrorCodeEnum.ValidationFailed);

        var complaintRepo = _unitOfWork.Repository<Complaint>();
        var complaint = await complaintRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<ComplaintMessagePostedDto>.Failure($"Complaint not found with Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);
        if (complaint.UserId != userId)
            return Result<ComplaintMessagePostedDto>.Failure("You do not have permission to send messages for this complaint.", ErrorCodeEnum.Forbidden);
        if (complaint.ComplaintStatus == ComplaintStatusEnum.Resolved)
            return Result<ComplaintMessagePostedDto>.Failure("This complaint is already resolved. You cannot send new messages.", ErrorCodeEnum.Forbidden);

        var message = new ComplaintMessage
        {
            ComplaintId = complaint.Id,
            SenderId = userId,
            Content = command.Content.Trim(),
            IsInternal = false
        };
        message.InitializeEntity(userId);

        await _unitOfWork.Repository<ComplaintMessage>().AddAsync(message);

        var uploadedUrls = new List<string>();
        var uploadedAttachments = new List<ComplaintMessageAttachment>();
        if (command.Attachments is { Count: > 0 })
        {
            for (var i = 0; i < command.Attachments.Count; i++)
            {
                var file = command.Attachments.ElementAt(i);
                var url = await _cloudinaryService.UploadImageAsync(
                    file,
                    "complaints/messages",
                    $"complaint_{complaint.Id:N}_message_{message.Id:N}_{i + 1}",
                    cancellationToken);

                if (string.IsNullOrWhiteSpace(url))
                {
                    await CleanupUploadedFilesAsync(uploadedUrls, cancellationToken);
                    return Result<ComplaintMessagePostedDto>.Failure("Failed to upload one or more attachments.", ErrorCodeEnum.FileUploadFailed);
                }

                uploadedUrls.Add(url);

                var row = new ComplaintMessageAttachment
                {
                    ComplaintMessageId = message.Id,
                    FileName = Path.GetFileName(file.FileName),
                    Url = url,
                    MimeType = file.ContentType,
                    SizeBytes = file.Length,
                    SortOrder = i
                };
                row.InitializeEntity(userId);
                uploadedAttachments.Add(row);

                await _unitOfWork.Repository<ComplaintMessageAttachment>().AddAsync(row);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ComplaintMessagePostedDto
        {
            MessageId = message.Id,
            ComplaintId = complaint.Id,
            Content = message.Content,
            IsInternal = message.IsInternal,
            CreatedAt = message.CreatedAt,
            Attachments = uploadedAttachments
                .OrderBy(x => x.SortOrder)
                .Select(x => new ComplaintAttachmentDto
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    Url = x.Url,
                    MimeType = x.MimeType,
                    SizeBytes = x.SizeBytes,
                    SortOrder = x.SortOrder
                })
                .ToList()
        };

        return Result<ComplaintMessagePostedDto>.Success(response, "Message sent.");
    }

    private static string? ValidateAttachments(IReadOnlyCollection<IFormFile>? files)
    {
        if (files == null || files.Count == 0)
            return null;

        if (files.Count > MaxAttachmentCount)
            return $"You can upload up to {MaxAttachmentCount} images each time.";

        foreach (var file in files)
        {
            if (file.Length <= 0)
                return "Attachment file is empty.";

            if (file.Length > MaxAttachmentSizeBytes)
                return "Each attachment must be 5MB or smaller.";

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                return "Only PNG, JPG, GIF, WEBP images are allowed.";
        }

        return null;
    }

    private async Task CleanupUploadedFilesAsync(IEnumerable<string> urls, CancellationToken cancellationToken)
    {
        foreach (var url in urls)
        {
            if (string.IsNullOrWhiteSpace(url))
                continue;

            try
            {
                var publicId = _cloudinaryService.GetPublicIdFromUrl(url);
                if (!string.IsNullOrWhiteSpace(publicId))
                    await _cloudinaryService.DeleteAsync(publicId, cancellationToken);
            }
            catch
            {
                // ignore cleanup failures
            }
        }
    }
}

