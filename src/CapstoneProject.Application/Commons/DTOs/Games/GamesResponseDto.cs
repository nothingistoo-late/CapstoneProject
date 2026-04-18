using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Games;

public class MapsResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Difficulty { get; set; }
    /// <summary>Level JSON as object so the API returns nested JSON instead of an escaped string.</summary>
    public JsonElement? JsonContent { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
