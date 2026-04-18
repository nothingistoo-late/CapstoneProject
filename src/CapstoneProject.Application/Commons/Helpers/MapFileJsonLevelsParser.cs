using System.Text.Json;
using CapstoneProject.Application.Commons.DTOs.Games;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Parse file JSON game: mảng root hoặc object <c>{ "levels": [...] }</c>.</summary>
public static class MapFileJsonLevelsParser
{
    /// <summary>Null = một level đơn (object game legacy), không dùng danh sách Levels.</summary>
    public static List<MapLevelInputDto>? TryParseLevels(string raw)
    {
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            var list = new List<MapLevelInputDto>();
            var i = 0;
            foreach (var el in root.EnumerateArray())
                // JsonElement chỉ hợp lệ trong đời JsonDocument; clone để dùng sau khi doc dispose.
                list.Add(new MapLevelInputDto { LevelOrder = i++, Title = null, JsonContent = el.Clone() });
            return list;
        }
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("levels", out var levelsEl) &&
            levelsEl.ValueKind == JsonValueKind.Array)
        {
            var list = new List<MapLevelInputDto>();
            foreach (var el in levelsEl.EnumerateArray())
            {
                if (el.TryGetProperty("jsonContent", out var jc))
                {
                    var order = el.TryGetProperty("levelOrder", out var lo) && lo.TryGetInt32(out var o) ? o : list.Count;
                    var title = el.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
                    var dto = new MapLevelInputDto { LevelOrder = order, Title = title, JsonContent = jc.Clone() };
                    MapLevelMetadataExtractor.MergeWrapperMetadata(el, dto);
                    list.Add(dto);
                }
                else
                    list.Add(new MapLevelInputDto { LevelOrder = list.Count, Title = null, JsonContent = el.Clone() });
            }
            return list.OrderBy(x => x.LevelOrder).ToList();
        }
        return null;
    }

    /// <summary>
    /// Parse từ <see cref="CreateMapFromJsonFileInput"/>: nhiều chuỗi JSON (từ nhiều file) hoặc một chuỗi (mảng / levels / object đơn).
    /// </summary>
    public static (List<MapLevelInputDto>? Levels, JsonElement? SingleLevelJson, string? Error) ParseFromCreateMapInput(
        CreateMapFromJsonFileInput input)
    {
        var hasMulti = input.GameDetailJsonContents is { Count: > 0 };
        var hasSingle = !string.IsNullOrWhiteSpace(input.GameDetailJsonContent);
        if (!hasMulti && !hasSingle)
            return (null, null, "mapDetailFiles content is required.");

        try
        {
            if (hasMulti)
            {
                var list = new List<MapLevelInputDto>();
                var order = 0;
                foreach (var raw in input.GameDetailJsonContents!)
                {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var el = JsonSerializer.Deserialize<JsonElement>(raw);
                    list.Add(new MapLevelInputDto { LevelOrder = order++, Title = null, JsonContent = el });
                }
                if (list.Count == 0)
                    return (null, null, "mapDetailFiles: no valid JSON in uploaded files.");
                return (list, null, null);
            }

            var levels = TryParseLevels(input.GameDetailJsonContent);
            if (levels is { Count: > 0 })
                return (levels, null, null);
            if (levels != null && levels.Count == 0)
                return (null, null, "Game JSON must contain at least one level (root array or \"levels\" array is empty).");
            var single = JsonSerializer.Deserialize<JsonElement>(input.GameDetailJsonContent);
            return (null, single, null);
        }
        catch (JsonException)
        {
            return (null, null, "Uploaded file is not valid JSON.");
        }
    }
}
