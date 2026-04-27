using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints;

internal static class ComplaintUserDisplayHelper
{
    public static string FormatDisplayName(string? firstName, string? lastName, string? userName)
    {
        var full = $"{firstName ?? ""} {lastName ?? ""}".Trim();
        if (!string.IsNullOrWhiteSpace(full))
            return full;
        return userName?.Trim() ?? "";
    }

    /// <summary>Loads display names for complaint participants (FirstName + LastName, else UserName).</summary>
    public static async Task<Dictionary<Guid, string>> LoadDisplayNamesAsync(
        IUnitOfWork unitOfWork,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var distinct = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (distinct.Count == 0)
            return new Dictionary<Guid, string>();

        var rows = await unitOfWork.Repository<AppUser>().GetQueryable()
            .AsNoTracking()
            .Where(u => distinct.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.Id,
            x => FormatDisplayName(x.FirstName, x.LastName, x.UserName));
    }
}
