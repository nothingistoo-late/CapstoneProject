namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại level đầu tiên: "Topdown" hoặc "Platform" (tóm tắt cho list).</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Giới hạn thời gian (ms) của level đầu tiên (tóm tắt cho list).</summary>
    public int TimeLimitMs { get; set; }
    public bool IsPublished { get; set; }
    /// <summary>Trạng thái map: Draft, PendingReview, Approved, Rejected, Published.</summary>
    public string MapStatus { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. 0 = không có trial.</summary>
    public int FreeTrialAttemptLimit { get; set; }
    public Guid CreatedByUserId { get; set; }
    /// <summary>Tên người tạo map (FirstName + LastName).</summary>
    public string? CreatedByUserName { get; set; }
    /// <summary>true nếu map do chính user đang gửi request tạo ra (Map.CreatedBy == currentUserId); false nếu user chỉ sở hữu (mua/thêm). Không dùng để kiểm tra sở hữu.</summary>
    public bool IsAuthor { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ContentVersion { get; set; }
    public List<string> TagNames { get; set; } = new();
    public List<string> LearnedTags { get; set; } = new();
    /// <summary>Điều kiện thắng của level đầu tiên (tóm tắt cho list).</summary>
    public int WinCondition { get; set; }
    /// <summary>URL avatar map (Cloudinary).</summary>
    public string? AvatarUrl { get; set; }
}
