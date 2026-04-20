namespace CapstoneProject.Application.Commons.DTOs.Games;

/// <summary>
/// Thông tin game (metadata only), không bao gồm GameDetail (JSON), Hints, Editorial.
/// </summary>
public class MapInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại level đầu tiên: "Topdown", "Platform", hoặc "Snake".</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Giới hạn thời gian (ms) của level đầu tiên.</summary>
    public int TimeLimitMs { get; set; }
    public bool IsPublished { get; set; }
    /// <summary>Trạng thái game: Draft, PendingReview, Approved, Rejected, Published.</summary>
    public string GameStatus { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public Guid CreatedByUserId { get; set; }
    /// <summary>Tên người tạo game (FirstName + LastName).</summary>
    public string? CreatedByUserName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ContentVersion { get; set; }
    public List<string> TagNames { get; set; } = new();
    public List<string> LearnedTags { get; set; } = new();
    /// <summary>Điều kiện thắng của level đầu tiên.</summary>
    public int WinCondition { get; set; }
    public string? AvatarUrl { get; set; }
    /// <summary>Danh sách level metadata (không bao gồm JSON/hints) theo thứ tự.</summary>
    public List<MapLevelItemDto> Levels { get; set; } = new();
}
