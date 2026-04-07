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

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessageAsStaff;

public class SendComplaintMessageAsStaffCommandHandler : IRequestHandler<SendComplaintMessageAsStaffCommand, Result<ComplaintMessagePostedDto>>
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

    public SendComplaintMessageAsStaffCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ComplaintMessagePostedDto>> Handle(SendComplaintMessageAsStaffCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ComplaintMessagePostedDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để gửi tin nhắn cho nhân viên.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<ComplaintMessagePostedDto>.Failure("Bạn không có quyền gửi tin nhắn cho nhân viên. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        if (command.ComplaintId == Guid.Empty)
            return Result<ComplaintMessagePostedDto>.Failure("Khiếu nạiId là bắt buộc.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Content))
            return Result<ComplaintMessagePostedDto>.Failure("Nội dung tin nhắn là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var attachmentValidationError = ValidateAttachments(command.Attachments);
        if (!string.IsNullOrWhiteSpace(attachmentValidationError))
            return Result<ComplaintMessagePostedDto>.Failure(attachmentValidationError, ErrorCodeEnum.ValidationFailed);

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<ComplaintMessagePostedDto>.Failure($"Không tìm thấy khiếu nại với Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);

        var message = new ComplaintMessage
        {
            ComplaintId = complaint.Id,
            SenderId = userId,
            Content = command.Content.Trim(),
            IsInternal = command.IsInternal
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
                    return Result<ComplaintMessagePostedDto>.Failure("Không thể tải lên một hoặc nhiều tệp đính kèm.", ErrorCodeEnum.FileUploadFailed);
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

        return Result<ComplaintMessagePostedDto>.Success(response, "Đã gửi tin nhắn.");
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

