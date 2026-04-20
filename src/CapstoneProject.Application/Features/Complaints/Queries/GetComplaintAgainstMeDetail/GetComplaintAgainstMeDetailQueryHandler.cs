using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintAgainstMeDetail;

public class GetComplaintAgainstMeDetailQueryHandler : IRequestHandler<GetComplaintAgainstMeDetailQuery, Result<ComplaintDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;

    public GetComplaintAgainstMeDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
    }

    public async Task<Result<ComplaintDetailDto>> Handle(GetComplaintAgainstMeDetailQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ComplaintDetailDto>.Failure("Authentication required. Please log in to view complaint detail.", ErrorCodeEnum.Unauthorized);

        if (request.ComplaintId == Guid.Empty)
            return Result<ComplaintDetailDto>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);

        var userId = userIdNullable.Value;

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .Include(c => c.Messages)
                .ThenInclude(m => m.Attachments)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId && !c.IsDeleted, cancellationToken);

        if (complaint == null)
            return Result<ComplaintDetailDto>.Failure($"Complaint not found: {request.ComplaintId}", ErrorCodeEnum.NotFound);

        var gameId = await ResolveComplaintGameIdAsync(complaint, cancellationToken);
        if (!gameId.HasValue)
            return Result<ComplaintDetailDto>.Failure("This complaint is not associated with a seller-owned game context.", ErrorCodeEnum.Forbidden);

        var isSeller = await _unitOfWork.Repository<Game>().GetQueryable()
            .AnyAsync(g => !g.IsDeleted && g.Id == gameId.Value && g.CreatedBy == userId, cancellationToken);
        if (!isSeller)
            return Result<ComplaintDetailDto>.Failure("You do not have permission to view this complaint.", ErrorCodeEnum.Forbidden);

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
                .Where(m => !m.IsDeleted && !m.IsInternal)
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

        return Result<ComplaintDetailDto>.Success(dto, "Retrieved complaint detail.");
    }

    private async Task<Guid?> ResolveComplaintGameIdAsync(Complaint complaint, CancellationToken cancellationToken)
    {
        if (string.Equals(complaint.ContextType, "Game", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
            return complaint.ContextId.Value;

        if (string.Equals(complaint.ContextType, "PaymentRecord", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
        {
            return await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == complaint.ContextId.Value)
                .Select(x => x.GameId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }
}
