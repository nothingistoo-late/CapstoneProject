using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Leaderboards.Queries.GetTopLevelLeaderboard;

public class GetTopLevelLeaderboardQueryHandler : IRequestHandler<GetTopLevelLeaderboardQuery, Result<PaginationResult<TopLevelLeaderboardItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetTopLevelLeaderboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<TopLevelLeaderboardItemDto>>> Handle(GetTopLevelLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<TopLevelLeaderboardItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var query = _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => u.Status == EntityStatusEnum.Active)
            .OrderByDescending(u => u.CurrentLevel)
            .ThenByDescending(u => u.CurrentXp)
            .ThenBy(u => u.JoiningAt);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (pageNumber - 1) * pageSize;

        var users = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        var items = users.Select((u, idx) => new TopLevelLeaderboardItemDto
        {
            Rank = skip + idx + 1,
            UserId = u.Id,
            DisplayName = $"{u.FirstName} {u.LastName}".Trim(),
            CurrentLevel = u.CurrentLevel,
            CurrentXp = u.CurrentXp
        }).ToList();

        var result = PaginationResult<TopLevelLeaderboardItemDto>.Success(items, pageNumber, pageSize, total);
        return Result<PaginationResult<TopLevelLeaderboardItemDto>>.Success(result, "Đã lấy bảng xếp hạng level.");
    }
}
