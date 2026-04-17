using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaintDetail;

public class GetMyComplaintDetailQueryHandler : IRequestHandler<GetMyComplaintDetailQuery, Result<MyComplaintDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;

    public GetMyComplaintDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
    }

    public async Task<Result<MyComplaintDetailDto>> Handle(GetMyComplaintDetailQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<MyComplaintDetailDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xem chi tiết khiếu nại.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        if (request.ComplaintId == Guid.Empty)
            return Result<MyComplaintDetailDto>.Failure("Khiếu nạiId là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .Include(c => c.Messages)
                .ThenInclude(m => m.Attachments)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId && !c.IsDeleted, cancellationToken);

        if (complaint == null)
            return Result<MyComplaintDetailDto>.Failure($"Không tìm thấy khiếu nại với Id: {request.ComplaintId}.", ErrorCodeEnum.NotFound);

        // Full view: complaint creator can see all details
        bool isFullView = complaint.UserId == userId;
        
        // Limited view: game creator (if complaint is about their game) can see selected fields
        bool isLimitedView = false;
        if (!isFullView && complaint.ContextType == "Game" && complaint.ContextId.HasValue)
        {
            var game = await _unitOfWork.Repository<Game>()
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == complaint.ContextId && !m.IsDeleted, cancellationToken);
            
            if (game?.CreatedBy == userId)
                isLimitedView = true;
        }

        if (!isFullView && !isLimitedView)
            return Result<MyComplaintDetailDto>.Failure("Bạn không có quyền xem khiếu nại này.", ErrorCodeEnum.Forbidden);

        var dto = new MyComplaintDetailDto
        {
            Id = complaint.Id,
            Subject = complaint.Subject,
            Category = complaint.Category,
            CategoryKey = complaint.CategoryKey,
            Description = complaint.Description,
            ComplaintStatus = complaint.ComplaintStatus.ToString(),
            ContextType = complaint.ContextType,
            ContextId = complaint.ContextId,
            ContextKey = isFullView ? complaint.ContextKey : null,
            ContextDataJson = isFullView ? complaint.ContextDataJson : null,
            OccurredAt = complaint.OccurredAt,
            ContextResolved = await _complaintContextResolver.ResolveAsync(
                complaint.ContextType,
                complaint.ContextId,
                complaint.ContextDataJson,
                complaint.UserId,
                cancellationToken),
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = isFullView ? complaint.ResolvedAt : null,
            Messages = complaint.Messages
                .Where(m => !m.IsDeleted && !m.IsInternal)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MyComplaintMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    Content = isFullView ? m.Content : string.Empty,
                    IsInternal = m.IsInternal,
                    CreatedAt = m.CreatedAt,
                    Attachments = m.Attachments
                        .Where(a => !a.IsDeleted)
                        .OrderBy(a => a.SortOrder)
                        .Select(a => new ComplaintAttachmentDto
                        {
                            Id = a.Id,
                            FileName = a.FileName,
                            Url = isFullView ? a.Url : string.Empty,
                            MimeType = a.MimeType,
                            SizeBytes = a.SizeBytes,
                            SortOrder = a.SortOrder
                        })
                        .ToList()
                })
                .ToList(),
            StatusHistories = isFullView ? complaint.StatusHistories
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.ChangedAt)
                .Select(h => new MyComplaintStatusHistoryDto
                {
                    Id = h.Id,
                    FromStatus = h.FromStatus.ToString(),
                    ToStatus = h.ToStatus.ToString(),
                    ChangedBy = h.ChangedBy,
                    ChangedAt = h.ChangedAt,
                    Note = h.Note
                })
                .ToList() : new List<MyComplaintStatusHistoryDto>(),
            IsLimitedView = isLimitedView
        };

        return Result<MyComplaintDetailDto>.Success(dto, "Đã lấy chi tiết khiếu nại của bạn.");
    }
}

