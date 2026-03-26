using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaints;

public class GetMyComplaintsQueryHandler : IRequestHandler<GetMyComplaintsQuery, Result<PaginationResult<MyComplaintListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyComplaintsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MyComplaintListItemDto>>> Handle(GetMyComplaintsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<PaginationResult<MyComplaintListItemDto>>.Failure("Authentication required. Please log in to view your complaints.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var query = _unitOfWork.Repository<Complaint>().GetQueryable()
            .Where(c => !c.IsDeleted && c.UserId == userId);

        if (request.Status.HasValue)
            query = query.Where(c => c.ComplaintStatus == request.Status.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(c => c.CreatedAt != null && c.CreatedAt.Value <= request.DateTo.Value);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new MyComplaintListItemDto
            {
                Id = c.Id,
                Subject = c.Subject,
                Category = c.Category,
                ComplaintStatus = c.ComplaintStatus.ToString(),
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt
            })
            .ToListAsync(cancellationToken);

        var paginated = PaginationResult<MyComplaintListItemDto>.Success(list, pageNumber, pageSize, total);
        return Result<PaginationResult<MyComplaintListItemDto>>.Success(paginated);
    }
}

