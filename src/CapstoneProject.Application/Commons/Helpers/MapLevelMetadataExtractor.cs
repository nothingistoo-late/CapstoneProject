using System.Text.Json;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Đọc <c>timeLimitMs</c> / <c>winCondition</c> / <c>type</c> từ JSON level (wrapper hoặc nội dung JSON).</summary>
public static class MapLevelMetadataExtractor
{
    /// <summary>
    /// Gộp từ object bọc ngoài (vd. <c>{ "jsonContent", "timeLimitMs" }</c>); chỉ điền khi DTO đang = 0 (time/win) hoặc chưa có type từ API.
    /// </summary>
    public static void MergeWrapperMetadata(JsonElement wrapper, MapLevelInputDto level)
    {
        if (wrapper.ValueKind != JsonValueKind.Object) return;
        MergeFromObject(level, wrapper, overwrite: false);
    }

    /// <summary>Gộp từ nội dung level (root JSON của map detail).</summary>
    public static void MergeFromJson(MapLevelInputDto level)
    {
        MergeFromJson(level.JsonContent, level);
    }

    public static void MergeFromJson(JsonElement json, MapLevelInputDto level)
    {
        if (json.ValueKind != JsonValueKind.Object) return;
        MergeFromObject(level, json, overwrite: true);
    }

    /// <summary>Áp dụng cho danh sách level (thường sau import file).</summary>
    public static void MergeFromJson(IEnumerable<MapLevelInputDto> levels)
    {
        foreach (var lv in levels.OrderBy(x => x.LevelOrder))
            MergeFromJson(lv);
    }

    static void MergeFromObject(MapLevelInputDto level, JsonElement el, bool overwrite)
    {
        if (TryGetPositiveInt(el, "timeLimitMs", out var t) || TryGetPositiveInt(el, "TimeLimitMs", out t))
        {
            if (overwrite || level.TimeLimitMs <= 0)
                level.TimeLimitMs = t;
        }

        if (TryGetPositiveInt(el, "winCondition", out var w) || TryGetPositiveInt(el, "WinCondition", out w))
        {
            if (overwrite || level.WinCondition <= 0)
                level.WinCondition = w;
        }

        if (TryParseMapTypeProperty(el, out var mapType))
            level.Type = mapType;
    }

    static bool TryParseMapTypeProperty(JsonElement el, out MapTypeEnum mapType)
    {
        mapType = MapTypeEnum.Topdown;
        JsonElement? p = null;
        if (el.TryGetProperty("type", out var p1)) p = p1;
        else if (el.TryGetProperty("Type", out var p2)) p = p2;
        else if (el.TryGetProperty("mapType", out var p3)) p = p3;
        else if (el.TryGetProperty("MapType", out var p4)) p = p4;
        else return false;

        return TryParseMapTypeValue(p.Value, out mapType);
    }

    static bool TryParseMapTypeValue(JsonElement p, out MapTypeEnum mapType)
    {
        mapType = MapTypeEnum.Topdown;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var n) && Enum.IsDefined(typeof(MapTypeEnum), n))
        {
            mapType = (MapTypeEnum)n;
            return true;
        }

        if (p.ValueKind == JsonValueKind.String)
        {
            var s = p.GetString();
            if (string.Equals(s, "Platform", StringComparison.OrdinalIgnoreCase))
            {
                mapType = MapTypeEnum.Platform;
                return true;
            }

            if (string.Equals(s, "Topdown", StringComparison.OrdinalIgnoreCase))
            {
                mapType = MapTypeEnum.Topdown;
                return true;
            }
        }

        return false;
    }

    static bool TryGetPositiveInt(JsonElement el, string name, out int value)
    {
        value = 0;
        if (!el.TryGetProperty(name, out var p)) return false;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var n) && n > 0)
        {
            value = n;
            return true;
        }

        return false;
    }
}
