using System.Text.Json;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class UpdateMapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại map: Topdown (0) hoặc Platform (1). Null = không đổi.</summary>
    public MapTypeEnum? Type { get; set; }
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    public decimal? Price { get; set; }
    public JsonElement? MapDetailJson { get; set; }
    public string? EditorialContent { get; set; }
    public int? UnlockEditorialAfterStars { get; set; }
    public List<HintItemDto>? Hints { get; set; }
    public List<Guid>? TagIds { get; set; }
}
