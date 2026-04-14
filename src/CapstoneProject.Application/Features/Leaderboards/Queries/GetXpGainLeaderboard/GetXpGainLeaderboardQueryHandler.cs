using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Leaderboards.Queries.GetXpGainLeaderboard;

public class GetXpGainLeaderboardQueryHandler : IRequestHandler<GetXpGainLeaderboardQuery, Result<PaginationResult<XpGainLeaderboardItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetXpGainLeaderboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<XpGainLeaderboardItemDto>>> Handle(GetXpGainLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<XpGainLeaderboardItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var (dateFrom, dateTo) = LeaderboardPeriodHelper.GetRange(request.PeriodType);

        var xpByUserQuery = _unitOfWork.Repository<XpTransaction>().GetQueryable()
            .Where(tx => !tx.IsDeleted
                         && tx.CreatedAt.HasValue
                         && tx.CreatedAt.Value >= dateFrom
                         && tx.CreatedAt.Value <= dateTo)
            .GroupBy(tx => tx.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                XpGained = g.Sum(x => x.Delta > 0 ? x.Delta : 0),
                LastGainAt = g.Max(x => x.CreatedAt)
            });

        var leaderboardQuery =
            from agg in xpByUserQuery
            join user in _unitOfWork.Repository<AppUser>().GetQueryable() on agg.UserId equals user.Id
            where agg.XpGained > 0 && user.Status == EntityStatusEnum.Active
            orderby agg.XpGained descending, agg.LastGainAt ascending, user.JoiningAt ascending
            select new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.CurrentLevel,
                agg.XpGained,
                agg.LastGainAt
            };

        var total = await leaderboardQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (pageNumber - 1) * pageSize;

        var rows = await leaderboardQuery.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select((x, idx) => new XpGainLeaderboardItemDto
        {
            Rank = skip + idx + 1,
            UserId = x.Id,
            DisplayName = $"{x.FirstName} {x.LastName}".Trim(),
            XpGained = x.XpGained,
            CurrentLevel = x.CurrentLevel,
            LastGainAt = x.LastGainAt
        }).ToList();

        var result = PaginationResult<XpGainLeaderboardItemDto>.Success(items, pageNumber, pageSize, total);
        return Result<PaginationResult<XpGainLeaderboardItemDto>>.Success(result, "Đã lấy bảng xếp hạng XP kiếm được trong kỳ.");
    }
}
