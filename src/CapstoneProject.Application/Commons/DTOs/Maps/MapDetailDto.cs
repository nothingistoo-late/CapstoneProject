using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại map: "Topdown" hoặc "Platform".</summary>
    public string Type { get; set; } = string.Empty;
    public int TimeLimitMs { get; set; }
    public bool IsPublished { get; set; }
    /// <summary>Trạng thái map: Draft, PendingReview, Approved, Rejected, Published.</summary>
    public string MapStatus { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public Guid CreatedByUserId { get; set; }
    /// <summary>Tên người tạo map (FirstName + LastName).</summary>
    public string? CreatedByUserName { get; set; }
    public string? EditorialContent { get; set; }
    public int UnlockEditorialAfterStars { get; set; }
    public DateTime? CreatedAt { get; set; }
    /// <summary>Map level JSON (layers, startPosition, goalPosition, objects...) returned as object, not escaped string.</summary>
    public JsonElement? MapDetailJson { get; set; }
    public List<HintItemDto> Hints { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
    public int WinCondition { get; set; }
    /// <summary>URL avatar map (Cloudinary).</summary>
    public string? AvatarUrl { get; set; }
}
