using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;

public class GetComplaintDetailQueryHandler : IRequestHandler<GetComplaintDetailQuery, Result<ComplaintDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;

    public GetComplaintDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
    }

    public async Task<Result<ComplaintDetailDto>> Handle(GetComplaintDetailQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<ComplaintDetailDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xem chi tiết khiếu nại.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<ComplaintDetailDto>.Failure("Bạn không có quyền xem chi tiết khiếu nại. Chỉ có Quản trị viên hoặc Người điều hành mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        if (request.ComplaintId == Guid.Empty)
            return Result<ComplaintDetailDto>.Failure("Khiếu nạiId là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .Include(c => c.Messages)
                .ThenInclude(m => m.Attachments)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId && !c.IsDeleted, cancellationToken);

        if (complaint == null)
            return Result<ComplaintDetailDto>.Failure($"Không tìm thấy khiếu nại với Id: {request.ComplaintId}.", ErrorCodeEnum.NotFound);

        var dto = new ComplaintDetailDto
        {
            Id = complaint.Id,
            UserId = complaint.UserId,
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
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt,
            RefundProcessed = complaint.RefundProcessed,
            RefundedPaymentRecordId = complaint.RefundedPaymentRecordId,
            RefundAmount = complaint.RefundAmount,
            RefundedAt = complaint.RefundedAt,
            RefundReason = complaint.RefundReason,
            Messages = complaint.Messages
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ComplaintMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    IsInternal = m.IsInternal,
                    CreatedAt = m.CreatedAt,
                    Attachments = m.Attachments
                        .Where(a => !a.IsDeleted)
                        .OrderBy(a => a.SortOrder)
                        .Select(a => new ComplaintAttachmentDto
                        {
                            Id = a.Id,
                            FileName = a.FileName,
                            Url = a.Url,
                            MimeType = a.MimeType,
                            SizeBytes = a.SizeBytes,
                            SortOrder = a.SortOrder
                        })
                        .ToList()
                })
                .ToList(),
            StatusHistories = complaint.StatusHistories
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.ChangedAt)
                .Select(h => new ComplaintStatusHistoryDto
                {
                    Id = h.Id,
                    FromStatus = h.FromStatus.ToString(),
                    ToStatus = h.ToStatus.ToString(),
                    ChangedBy = h.ChangedBy,
                    ChangedAt = h.ChangedAt,
                    Note = h.Note
                })
                .ToList()
        };

        return Result<ComplaintDetailDto>.Success(dto, "Đã lấy chi tiết khiếu nại.");
    }
}

