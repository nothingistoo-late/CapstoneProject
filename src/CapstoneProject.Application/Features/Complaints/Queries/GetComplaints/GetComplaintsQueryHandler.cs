using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaints;

public class GetComplaintsQueryHandler : IRequestHandler<GetComplaintsQuery, Result<PaginationResult<ComplaintListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetComplaintsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
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

        var query = _unitOfWork.Repository<Complaint>().GetQueryable()
            .Where(c => !c.IsDeleted);

        if (request.Status.HasValue)
            query = query.Where(c => c.ComplaintStatus == request.Status.Value);
        if (request.UserId.HasValue)
            query = query.Where(c => c.UserId == request.UserId.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(c => c.CreatedAt != null && c.CreatedAt.Value <= request.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(c =>
                c.Subject.Contains(keyword) ||
                c.Category.Contains(keyword) ||
                c.Description.Contains(keyword));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ComplaintListItemDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Subject = c.Subject,
                Category = c.Category,
                ComplaintStatus = c.ComplaintStatus.ToString(),
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt
            })
            .ToListAsync(cancellationToken);

        var paginated = PaginationResult<ComplaintListItemDto>.Success(list, pageNumber, pageSize, total);
        return Result<PaginationResult<ComplaintListItemDto>>.Success(paginated);
    }
}

