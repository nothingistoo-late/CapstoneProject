using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Leaderboards.Queries.GetMostPlayedCreatedMapsLeaderboard;

public class GetMostPlayedCreatedMapsLeaderboardQueryHandler : IRequestHandler<GetMostPlayedCreatedMapsLeaderboardQuery, Result<PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMostPlayedCreatedMapsLeaderboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>>> Handle(GetMostPlayedCreatedMapsLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var (dateFrom, dateTo) = LeaderboardPeriodHelper.GetRange(request.PeriodType);

        var mapPlayAggQuery = _unitOfWork.Repository<UserGamePlayHistory>().GetQueryable()
            .Where(p => !p.IsDeleted
                        && p.StartTime >= dateFrom
                        && p.StartTime <= dateTo)
            .GroupBy(p => p.GameId)
            .Select(g => new
            {
                GameId = g.Key,
                PlayCount = g.Count(),
                UniquePlayerCount = g.Select(x => x.UserId).Distinct().Count(),
                LastPlayedAt = g.Max(x => x.StartTime)
            });

        var leaderboardQuery =
            from agg in mapPlayAggQuery
            join game in _unitOfWork.Repository<Game>().GetQueryable() on agg.GameId equals game.Id
            join creator in _unitOfWork.Repository<AppUser>().GetQueryable() on game.CreatedBy equals creator.Id
            where !game.IsDeleted && game.CreatedBy.HasValue
            orderby agg.PlayCount descending, agg.UniquePlayerCount descending, agg.LastPlayedAt descending, game.CreatedAt ascending
            select new
            {
                game.Id,
                game.Title,
                CreatorUserId = creator.Id,
                creator.FirstName,
                creator.LastName,
                agg.PlayCount,
                agg.UniquePlayerCount,
                agg.LastPlayedAt
            };

        var total = await leaderboardQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (pageNumber - 1) * pageSize;

        var rows = await leaderboardQuery.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select((x, idx) => new MostPlayedCreatedMapLeaderboardItemDto
        {
            Rank = skip + idx + 1,
            GameId = x.Id,
            MapTitle = x.Title,
            CreatorUserId = x.CreatorUserId,
            CreatorDisplayName = $"{x.FirstName} {x.LastName}".Trim(),
            PlayCount = x.PlayCount,
            UniquePlayerCount = x.UniquePlayerCount,
            LastPlayedAt = x.LastPlayedAt
        }).ToList();

        var result = PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>.Success(items, pageNumber, pageSize, total);
        return Result<PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>>.Success(result, "Đã lấy bảng xếp hạng game được chơi nhiều nhất trong kỳ.");
    }
}
