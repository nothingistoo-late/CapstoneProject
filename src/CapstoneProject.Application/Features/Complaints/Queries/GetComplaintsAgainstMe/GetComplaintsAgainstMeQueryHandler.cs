using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaints;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintsAgainstMe;

public class GetComplaintsAgainstMeQueryHandler : IRequestHandler<GetComplaintsAgainstMeQuery, Result<PaginationResult<ComplaintListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;

    public GetComplaintsAgainstMeQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
    }

    public async Task<Result<PaginationResult<ComplaintListItemDto>>> Handle(GetComplaintsAgainstMeQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<PaginationResult<ComplaintListItemDto>>.Failure("Authentication required. Please log in to view complaints against your games.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;

        var sellerGameIds = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => !g.IsDeleted && g.CreatedBy == userId)
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);

        if (sellerGameIds.Count == 0)
        {
            var empty = PaginationResult<ComplaintListItemDto>.Success(new List<ComplaintListItemDto>(), 1, Math.Clamp(request.PageSize, 1, 100), 0);
            return Result<PaginationResult<ComplaintListItemDto>>.Success(empty, "Retrieved complaints against your games.");
        }

        var sellerPaymentIds = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(p => !p.IsDeleted && p.GameId.HasValue && sellerGameIds.Contains(p.GameId.Value))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var baseQuery = _unitOfWork.Repository<Complaint>().GetQueryable()
            .Where(c => !c.IsDeleted)
            .Where(c =>
                (c.ContextType == "Game" && c.ContextId.HasValue && sellerGameIds.Contains(c.ContextId.Value))
                ||
                (c.ContextType == "PaymentRecord" && c.ContextId.HasValue && sellerPaymentIds.Contains(c.ContextId.Value)));

        var dateFrom = VietnamDateTime.ToDbDateTime(request.DateFrom);
        var dateTo = VietnamDateTime.ToDbDateTime(request.DateTo);

        if (request.Status.HasValue)
            baseQuery = baseQuery.Where(c => c.ComplaintStatus == request.Status.Value);
        if (dateFrom.HasValue)
            baseQuery = baseQuery.Where(c => c.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            baseQuery = baseQuery.Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            baseQuery = baseQuery.Where(c => c.Subject.Contains(keyword) || c.Description.Contains(keyword) || c.Category.Contains(keyword));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var complaints = await baseQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var list = new List<ComplaintListItemDto>(complaints.Count);
        foreach (var complaint in complaints)
        {
            list.Add(new ComplaintListItemDto
            {
                Id = complaint.Id,
                UserId = complaint.UserId,
                Subject = complaint.Subject,
                Category = complaint.Category,
                CategoryKey = complaint.CategoryKey,
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
                RefundedAt = complaint.RefundedAt
            });
        }

        var paginated = PaginationResult<ComplaintListItemDto>.Success(list, pageNumber, pageSize, total);
        return Result<PaginationResult<ComplaintListItemDto>>.Success(paginated, "Retrieved complaints against your games.");
    }
}
