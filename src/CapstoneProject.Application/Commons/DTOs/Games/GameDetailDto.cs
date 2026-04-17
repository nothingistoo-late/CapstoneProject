using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Games;

public class GameDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
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
    public string? EditorialContent { get; set; }
    public int UnlockEditorialAfterStars { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    /// <summary>Tăng mỗi lần cập nhật nội dung game (level/JSON). Dùng cho cache client.</summary>
    public int ContentVersion { get; set; }
    /// <summary>Các level (JSON layout) theo thứ tự.</summary>
    public List<MapLevelItemDto> Levels { get; set; } = new();
    /// <summary>Tương thích client cũ: JSON của level đầu tiên (cùng <see cref="Levels"/>[0] nếu có).</summary>
    public JsonElement? GameDetailJson { get; set; }
    /// <summary>Tất cả hint của game (theo thứ tự level rồi OrderNo trong level). Chi tiết theo level: <see cref="MapLevelItemDto.Hints"/>.</summary>
    public List<HintItemDto> Hints { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
    public List<string> LearnedTags { get; set; } = new();
    /// <summary>URL avatar game (Cloudinary).</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Ảnh / video mô tả game (gallery), sắp xếp theo <see cref="GameMediaItemDto.SortOrder"/>.</summary>
    public List<GameMediaItemDto> Gallery { get; set; } = new();
}
