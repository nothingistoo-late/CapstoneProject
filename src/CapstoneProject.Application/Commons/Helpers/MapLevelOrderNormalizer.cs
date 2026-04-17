using CapstoneProject.Application.Commons.DTOs.Games;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>
/// Khi client không gửi <c>levelOrder</c> (mặc định 0) hoặc gửi trùng — gán lại 0, 1, 2, … theo thứ tự phần tử trong mảng.
/// </summary>
public static class MapLevelOrderNormalizer
{
    public static void NormalizeIfDuplicate(List<MapLevelInputDto>? levels)
    {
        if (levels is not { Count: > 1 }) return;
        if (levels.Select(x => x.LevelOrder).Distinct().Count() == levels.Count)
            return;
        for (var i = 0; i < levels.Count; i++)
            levels[i].LevelOrder = i;
    }
}
