using System.Text.Json;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class CreateMapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại map: Topdown (0) hoặc Platform (1). Mặc định Topdown.</summary>
    public MapTypeEnum? Type { get; set; }
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    public decimal? Price { get; set; }
    public JsonElement MapDetailJson { get; set; }
    public List<HintItemDto> Hints { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
}

public class HintItemDto
{
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;
}
