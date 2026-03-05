using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, Result<PaginationResult<ReportListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetReportsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<ReportListItemDto>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<ReportListItemDto>>.Failure("Authentication required. Please log in to view reports.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<PaginationResult<ReportListItemDto>>.Failure("You do not have permission to view reports. Only Admin or Moderator can access this list.", ErrorCodeEnum.Forbidden);

        var query = _unitOfWork.Repository<MapReport>().GetQueryable().Where(r => !r.IsDeleted);
        if (request.Status.HasValue && request.Status != ReportStatusFilter.All)
        {
            var status = request.Status.Value switch
            {
                ReportStatusFilter.Pending => ReportStatusEnum.Pending,
                ReportStatusFilter.Reviewed => ReportStatusEnum.Reviewed,
                ReportStatusFilter.Resolved => ReportStatusEnum.Resolved,
                ReportStatusFilter.Dismissed => ReportStatusEnum.Dismissed,
                _ => (ReportStatusEnum?)null
            };
            if (status.HasValue)
                query = query.Where(r => r.ReportStatus == status.Value);
        }
        if (request.MapId.HasValue)
            query = query.Where(r => r.MapId == request.MapId.Value);
        if (request.UserId.HasValue)
            query = query.Where(r => r.UserId == request.UserId.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(r => r.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(r => r.CreatedAt != null && r.CreatedAt.Value <= request.DateTo.Value);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var list = await query
            .Include(r => r.Map)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReportListItemDto
            {
                Id = r.Id,
                MapId = r.MapId,
                MapTitle = r.Map.Title,
                UserId = r.UserId,
                Reason = r.Reason,
                Details = r.Details,
                ReportStatus = r.ReportStatus.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var paginated = PaginationResult<ReportListItemDto>.Success(list, pageNumber, pageSize, total);
        return Result<PaginationResult<ReportListItemDto>>.Success(paginated);
    }
}
