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

namespace CapstoneProject.Application.Features.Complaints.Commands.CreateComplaint;

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, Result<CreateComplaintResponseDto>>
{
    private const int MaxAttachmentCount = 5;
    private const long MaxAttachmentSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintPolicyService _complaintPolicyService;
    private readonly IComplaintContextResolver _complaintContextResolver;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintPolicyService complaintPolicyService,
        IComplaintContextResolver complaintContextResolver,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintPolicyService = complaintPolicyService;
        _complaintContextResolver = complaintContextResolver;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<CreateComplaintResponseDto>> Handle(CreateComplaintCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<CreateComplaintResponseDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để gửi khiếu nại.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;
        if (string.IsNullOrWhiteSpace(command.Subject))
            return Result<CreateComplaintResponseDto>.Failure("Chủ đề là bắt buộc.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.CategoryKey))
            return Result<CreateComplaintResponseDto>.Failure("CategoryKey là bắt buộc.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Description))
            return Result<CreateComplaintResponseDto>.Failure("Mô tả là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var attachmentValidationError = ValidateAttachments(command.Attachments);
        if (!string.IsNullOrEmpty(attachmentValidationError))
            return Result<CreateComplaintResponseDto>.Failure(attachmentValidationError, ErrorCodeEnum.ValidationFailed);

        var policyResult = await _complaintPolicyService.ValidateCreateAsync(new CapstoneProject.Application.Commons.Models.Complaints.ComplaintCreatePolicyInput
        {
            UserId = userId,
            CategoryKey = command.CategoryKey,
            Subject = command.Subject,
            Description = command.Description,
            Context = command.Context
        }, cancellationToken);

        if (!policyResult.IsSuccess)
            return Result<CreateComplaintResponseDto>.Failure(policyResult.ErrorMessage ?? "Xác thực chính sách khiếu nại không thành công.", ErrorCodeEnum.ValidationFailed);

        var complaint = new Complaint
        {
            UserId = userId,
            Subject = command.Subject.Trim(),
            Category = policyResult.CategoryDisplayName,
            CategoryKey = policyResult.CategoryKey,
            Description = command.Description.Trim(),
            ContextType = policyResult.ContextType,
            ContextId = policyResult.ContextId,
            ContextKey = policyResult.ContextKey,
            ContextDataJson = policyResult.NormalizedContextJson,
            OccurredAt = CapstoneProject.Domain.Common.VietnamDateTime.ToDbDateTime(policyResult.OccurredAt),
            ComplaintStatus = ComplaintStatusEnum.Open
        };
        complaint.InitializeEntity(userId);

        var firstMessage = new ComplaintMessage
        {
            ComplaintId = complaint.Id,
            SenderId = userId,
            Content = complaint.Description,
            IsInternal = false
        };
        firstMessage.InitializeEntity(userId);

        await _unitOfWork.Repository<Complaint>().AddAsync(complaint);
        await _unitOfWork.Repository<ComplaintMessage>().AddAsync(firstMessage);

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
                    $"complaint_{complaint.Id:N}_message_{firstMessage.Id:N}_{i + 1}",
                    cancellationToken);

                if (string.IsNullOrWhiteSpace(url))
                {
                    await CleanupUploadedFilesAsync(uploadedUrls, cancellationToken);
                    return Result<CreateComplaintResponseDto>.Failure("Không thể tải lên một hoặc nhiều tệp đính kèm.", ErrorCodeEnum.FileUploadFailed);
                }

                uploadedUrls.Add(url);

                var row = new ComplaintMessageAttachment
                {
                    ComplaintMessageId = firstMessage.Id,
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

        var response = new CreateComplaintResponseDto
        {
            Id = complaint.Id,
            Subject = complaint.Subject,
            Category = complaint.Category,
            CategoryKey = complaint.CategoryKey,
            Description = complaint.Description,
            ComplaintStatus = complaint.ComplaintStatus.ToString(),
            ContextType = complaint.ContextType,
            ContextId = complaint.ContextId,
            ContextKey = complaint.ContextKey,
            ContextDataJson = complaint.ContextDataJson,
            OccurredAt = complaint.OccurredAt,
            ContextResolved = await _complaintContextResolver.ResolveAsync(
                complaint.ContextType,
                complaint.ContextId,
                complaint.ContextDataJson,
                complaint.UserId,
                cancellationToken),
            InitialMessageId = firstMessage.Id,
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
                .ToList(),
            CreatedAt = complaint.CreatedAt
        };

        return Result<CreateComplaintResponseDto>.Success(response, "Đã gửi đơn khiếu nại.");
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

