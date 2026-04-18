using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Giá trị TimeLimit / WinCondition / Type hiển thị ở list: lấy từ level đầu tiên (LevelOrder nhỏ nhất).</summary>
public static class MapFirstLevelHelper
{
    public static (int TimeLimitMs, int WinCondition, GameTypeEnum Type) FirstLevelMetadata(IEnumerable<GameDetail>? details)
    {
        if (details == null) return (0, 0, GameTypeEnum.Topdown);
        var d = details.Where(x => !x.IsDeleted).OrderBy(x => x.LevelOrder).FirstOrDefault();
        return d == null ? (0, 0, GameTypeEnum.Topdown) : (d.TimeLimitMs, d.WinCondition, d.Type);
    }
}
