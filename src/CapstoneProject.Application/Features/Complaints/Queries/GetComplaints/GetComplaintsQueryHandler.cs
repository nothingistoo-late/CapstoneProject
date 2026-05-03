using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Complaints;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaints;

public class GetComplaintsQueryHandler : IRequestHandler<GetComplaintsQuery, Result<PaginationResult<ComplaintListItemDto>>>
{
    /// <summary>Matches CMS "solved" chip (terminal / closed outcomes).</summary>
    private static readonly ComplaintStatusEnum[] SolvedStatusGroup =
    {
        ComplaintStatusEnum.Resolved,
        ComplaintStatusEnum.ResolvedRefund,
        ComplaintStatusEnum.ResolvedReject,
        ComplaintStatusEnum.Closed
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetComplaintsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<ComplaintListItemDto>>> Handle(GetComplaintsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<ComplaintListItemDto>>.Failure("Authentication required. Please log in to view complaints.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<PaginationResult<ComplaintListItemDto>>.Failure("You do not have permission to view complaints. Only Admin or Moderator can access this list.", ErrorCodeEnum.Forbidden);

        var scopedQuery = ApplyScopeFilters(request);

        var pendingInScope = await scopedQuery
            .Where(c => !SolvedStatusGroup.Contains(c.ComplaintStatus))
            .CountAsync(cancellationToken);
        var solvedInScope = await scopedQuery
            .Where(c => SolvedStatusGroup.Contains(c.ComplaintStatus))
            .CountAsync(cancellationToken);
        var totalInScope = pendingInScope + solvedInScope;

        var query = scopedQuery;

        if (!string.IsNullOrWhiteSpace(request.StatusGroup))
        {
            var g = request.StatusGroup.Trim();
            if (string.Equals(g, "solved", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => SolvedStatusGroup.Contains(c.ComplaintStatus));
            else if (string.Equals(g, "pending", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => !SolvedStatusGroup.Contains(c.ComplaintStatus));
        }
        else if (request.Status.HasValue)
            query = query.Where(c => c.ComplaintStatus == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var buyerIds = complaints.Select(c => c.UserId).Distinct().ToList();
        var buyerNames = await ComplaintUserDisplayHelper.LoadDisplayNamesAsync(_unitOfWork, buyerIds, cancellationToken);

        var list = new List<ComplaintListItemDto>(complaints.Count);
        foreach (var complaint in complaints)
        {
            list.Add(new ComplaintListItemDto
            {
                Id = complaint.Id,
                UserId = complaint.UserId,
                BuyerUserId = complaint.UserId,
                BuyerDisplayName = buyerNames.GetValueOrDefault(complaint.UserId) ?? "",
                Subject = complaint.Subject,
                Category = complaint.Category,
                CategoryKey = complaint.CategoryKey,
                ComplaintStatus = complaint.ComplaintStatus.ToString(),
                ContextType = complaint.ContextType,
                ContextId = complaint.ContextId,
                ContextKey = complaint.ContextKey,
                ContextDataJson = complaint.ContextDataJson,
                OccurredAt = complaint.OccurredAt,
                ContextResolved = null,
                CreatedAt = complaint.CreatedAt,
                ResolvedAt = complaint.ResolvedAt,
                RefundProcessed = complaint.RefundProcessed,
                RefundedPaymentRecordId = complaint.RefundedPaymentRecordId,
                RefundAmount = complaint.RefundAmount,
                RefundedAt = complaint.RefundedAt
            });
        }

        var paginated = PaginationResult<ComplaintListItemDto>.Success(list, pageNumber, pageSize, total);
        paginated.ComplaintScopeStats = new ComplaintListScopeStatsDto
        {
            TotalInScope = totalInScope,
            PendingInScope = pendingInScope,
            SolvedInScope = solvedInScope
        };
        return Result<PaginationResult<ComplaintListItemDto>>.Success(paginated, "Đã lấy danh sách khiếu nại.");
    }

    private IQueryable<Complaint> ApplyScopeFilters(GetComplaintsQuery request)
    {
        var query = _unitOfWork.Repository<Complaint>().GetQueryable()
            .Where(c => !c.IsDeleted);

        var dateFrom = VietnamDateTime.ToDbDateTime(request.DateFrom);
        var dateTo = VietnamDateTime.ToDbDateTime(request.DateTo);

        if (request.UserId.HasValue)
            query = query.Where(c => c.UserId == request.UserId.Value);
        if (dateFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(c => c.CreatedAt != null && c.CreatedAt.Value <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(c =>
                c.Subject.Contains(keyword) ||
                c.Category.Contains(keyword) ||
                c.Description.Contains(keyword));
        }

        return query;
    }
}
