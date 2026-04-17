using System.Text.Json;
using CapstoneProject.Application.Commons.DTOs.Games;

namespace CapstoneProject.Application.Commons.Helpers;

public static class MapHintsExtractor
{
    /// <summary>
    /// Nếu JSON có <c>hints</c> thì dùng; không thì giữ <see cref="MapLevelInputDto.Hints"/> (từ API body).
    /// </summary>
    public static void MergeHintsFromJson(MapLevelInputDto level)
    {
        var fromJson = TryExtractFromJson(level.JsonContent);
        if (fromJson is { Count: > 0 })
        {
            level.Hints = RenumberOrder(fromJson);
            return;
        }

        level.Hints = RenumberOrder(level.Hints);
    }

    /// <summary>Áp dụng merge cho danh sách level (thường từ file import).</summary>
    public static void MergeHintsFromJson(IEnumerable<MapLevelInputDto> levels)
    {
        foreach (var level in levels.OrderBy(x => x.LevelOrder))
            MergeHintsFromJson(level);
    }

    private static List<HintItemDto> RenumberOrder(List<HintItemDto> hints)
    {
        var ordered = hints.OrderBy(x => x.OrderNo).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].OrderNo = i;
        return ordered;
    }

    /// <summary>
    /// Parse field <c>hints</c> trong JSON game detail:
    /// - { "hints": [ "a", "b" ] }
    /// - { "hints": [ { "orderNo": 0, "content": "a" }, ... ] }
    /// - { "hints": { "orderNo": 0, "content": "a" } }
    /// </summary>
    private static List<HintItemDto>? TryExtractFromJson(JsonElement json)
    {
        if (json.ValueKind != JsonValueKind.Object) return null;
        if (!json.TryGetProperty("hints", out var hintsEl)) return null;

        try
        {
            if (hintsEl.ValueKind == JsonValueKind.Array)
            {
                var list = new List<HintItemDto>();
                var idx = 0;
                foreach (var el in hintsEl.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.String)
                    {
                        list.Add(new HintItemDto { OrderNo = idx++, Content = el.GetString() ?? string.Empty });
                        continue;
                    }

                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        var content = el.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String
                            ? c.GetString()
                            : null;
                        var orderNo = el.TryGetProperty("orderNo", out var o) && o.TryGetInt32(out var no) ? no : idx;
                        list.Add(new HintItemDto { OrderNo = orderNo, Content = content ?? string.Empty });
                        idx++;
                    }
                }

                return list.OrderBy(x => x.OrderNo).ToList();
            }

            if (hintsEl.ValueKind == JsonValueKind.Object)
            {
                var content = hintsEl.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String
                    ? c.GetString()
                    : null;
                var orderNo = hintsEl.TryGetProperty("orderNo", out var o) && o.TryGetInt32(out var no) ? no : 0;
                return new List<HintItemDto> { new HintItemDto { OrderNo = orderNo, Content = content ?? string.Empty } };
            }
        }
        catch (JsonException)
        {
            // ignore invalid hints section
        }

        return null;
    }
}
