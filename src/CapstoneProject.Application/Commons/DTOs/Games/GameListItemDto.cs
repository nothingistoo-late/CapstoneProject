namespace CapstoneProject.Application.Commons.DTOs.Games;

public class MapListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại level đầu tiên: "Topdown", "Platform", hoặc "Snake" (tóm tắt cho list).</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Giới hạn thời gian (ms) của level đầu tiên (tóm tắt cho list).</summary>
    public int TimeLimitMs { get; set; }
    public bool IsPublished { get; set; }
    /// <summary>Trạng thái game: Draft, PendingReview, Approved, Rejected, Published.</summary>
    public string GameStatus { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. 0 = không có trial.</summary>
    public int FreeTrialAttemptLimit { get; set; }
    /// <summary>Ghi chú kiểm duyệt gần nhất từ Admin/Moderator (nếu có).</summary>
    public string? ReviewNote { get; set; }
    public Guid CreatedByUserId { get; set; }
    /// <summary>Tên người tạo game (FirstName + LastName).</summary>
    public string? CreatedByUserName { get; set; }
    /// <summary>true nếu game do chính user đang gửi request tạo ra (Game.CreatedBy == currentUserId); false nếu user chỉ sở hữu (mua/thêm). Không dùng để kiểm tra sở hữu.</summary>
    public bool IsAuthor { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ContentVersion { get; set; }
    public List<string> TagNames { get; set; } = new();
    public List<string> LearnedTags { get; set; } = new();
    /// <summary>Điều kiện thắng của level đầu tiên (tóm tắt cho list).</summary>
    public int WinCondition { get; set; }
    /// <summary>URL avatar game (Cloudinary).</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Ảnh / video gallery của game (Cloudinary).</summary>
    public List<GameMediaItemDto> Gallery { get; set; } = new();
}
