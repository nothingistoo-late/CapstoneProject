using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaints;

public class GetMyComplaintsQueryHandler : IRequestHandler<GetMyComplaintsQuery, Result<PaginationResult<MyComplaintListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;

    public GetMyComplaintsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
    }

    public async Task<Result<PaginationResult<MyComplaintListItemDto>>> Handle(GetMyComplaintsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<PaginationResult<MyComplaintListItemDto>>.Failure("Authentication required. Please log in to view your complaints.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var query = _unitOfWork.Repository<Complaint>().GetQueryable()
            .Where(c => !c.IsDeleted && c.UserId == userId);

        var dateFrom = VietnamDateTime.ToDbDateTime(request.DateFrom);
        var dateTo = VietnamDateTime.ToDbDateTime(request.DateTo);

        if (request.Status.HasValue)
            query = query.Where(c => c.ComplaintStatus == request.Status.Value);
        if (dateFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(c => c.CreatedAt != null && c.CreatedAt.Value <= dateTo.Value);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var list = new List<MyComplaintListItemDto>(complaints.Count);
        foreach (var complaint in complaints)
        {
            list.Add(new MyComplaintListItemDto
            {
                Id = complaint.Id,
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
                ResolvedAt = complaint.ResolvedAt
            });
        }

        var paginated = PaginationResult<MyComplaintListItemDto>.Success(list, pageNumber, pageSize, total);
        return Result<PaginationResult<MyComplaintListItemDto>>.Success(paginated, "Đã lấy danh sách khiếu nại của bạn.");
    }
}


