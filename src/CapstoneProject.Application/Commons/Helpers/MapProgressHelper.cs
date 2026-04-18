using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Tiến độ game nhiều level (UserGameResult theo GameDetailId).</summary>
public static class MapProgressHelper
{
    public static async Task<bool> MapHasAllLevelsCompletedAsync(
        IUnitOfWork uow,
        Guid userId,
        Guid gameId,
        int minStars,
        CancellationToken cancellationToken)
    {
        var levelIds = await uow.Repository<GameDetail>().GetQueryable()
            .AsNoTracking()
            .Where(d => d.GameId == gameId && !d.IsDeleted)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);
        if (levelIds.Count == 0) return false;

        var umrs = await uow.Repository<UserGameResult>().GetQueryable()
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.GameId == gameId && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var lid in levelIds)
        {
            var row = umrs.FirstOrDefault(u => u.GameDetailId == lid);
            if (row != null && row.BestStars >= minStars)
                continue;
            if (levelIds.Count == 1)
            {
                var legacy = umrs.FirstOrDefault(u => u.GameDetailId == null);
                if (legacy != null && legacy.BestStars >= minStars)
                    continue;
            }
            return false;
        }
        return true;
    }
}
