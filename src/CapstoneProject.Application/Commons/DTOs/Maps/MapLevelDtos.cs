using System.Text.Json;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>Input một level khi tạo/sửa map (API JSON body).</summary>
public class MapLevelInputDto
{
    public int LevelOrder { get; set; }
    public string? Title { get; set; }
    public JsonElement JsonContent { get; set; }
    /// <summary>Gợi ý theo level (hoặc lấy từ JSON field <c>hints</c> trong map detail).</summary>
    public List<HintItemDto> Hints { get; set; } = new();
    /// <summary>Giới hạn thời gian (ms) cho level; có thể lấy từ JSON (<c>timeLimitMs</c>) hoặc body API.</summary>
    public int TimeLimitMs { get; set; }
    /// <summary>Điều kiện thắng cho level; có thể lấy từ JSON (<c>winCondition</c>) hoặc body API.</summary>
    public int WinCondition { get; set; }
    /// <summary>Topdown hoặc Platform; bắt buộc trong JSON level (root hoặc wrapper cạnh <c>jsonContent</c>) hoặc trong body API (<c>type</c>).</summary>
    public MapTypeEnum? Type { get; set; }
}

/// <summary>Level trong response GET map detail.</summary>
public class MapLevelItemDto
{
    public Guid Id { get; set; }
    public int LevelOrder { get; set; }
    public string? Title { get; set; }
    public JsonElement? DetailJson { get; set; }
    public List<HintItemDto> Hints { get; set; } = new();
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    /// <summary>Topdown hoặc Platform.</summary>
    public string Type { get; set; } = string.Empty;
}
