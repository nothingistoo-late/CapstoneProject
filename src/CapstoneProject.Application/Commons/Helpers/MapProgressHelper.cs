using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Tiến độ map nhiều level (UserMapResult theo MapDetailId).</summary>
public static class MapProgressHelper
{
    public static async Task<bool> MapHasAllLevelsCompletedAsync(
        IUnitOfWork uow,
        Guid userId,
        Guid mapId,
        int minStars,
        CancellationToken cancellationToken)
    {
        var levelIds = await uow.Repository<MapDetail>().GetQueryable()
            .AsNoTracking()
            .Where(d => d.MapId == mapId && !d.IsDeleted)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);
        if (levelIds.Count == 0) return false;

        var umrs = await uow.Repository<UserMapResult>().GetQueryable()
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.MapId == mapId && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var lid in levelIds)
        {
            var row = umrs.FirstOrDefault(u => u.MapDetailId == lid);
            if (row != null && row.BestStars >= minStars)
                continue;
            if (levelIds.Count == 1)
            {
                var legacy = umrs.FirstOrDefault(u => u.MapDetailId == null);
                if (legacy != null && legacy.BestStars >= minStars)
                    continue;
            }
            return false;
        }
        return true;
    }
}
