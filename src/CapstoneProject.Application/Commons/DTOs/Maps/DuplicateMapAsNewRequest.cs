namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Xuất bản map mới từ map nguồn: tạo <see cref="Domain.Entities.Map"/> mới (MapId mới), map nguồn không đổi.
/// Các trường tùy chọn: null = copy từ map nguồn (trừ <see cref="Title"/> và <see cref="TagIds"/> — xem mô tả).
/// </summary>
public class DuplicateMapAsNewRequest
{
    /// <summary>Tiêu đề map mới. Null hoặc rỗng = dùng "{title nguồn} (Copy)".</summary>
    public string? Title { get; set; }

    /// <summary>Mô tả. Null = copy từ nguồn.</summary>
    public string? Description { get; set; }

    /// <summary>Độ khó 1–5. Null = copy từ nguồn.</summary>
    public int? Difficulty { get; set; }

    /// <summary>Giá. Null = copy từ nguồn.</summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Nếu property được gửi (kể cả mảng rỗng), gán tag map mới theo danh sách này.
    /// Null = copy toàn bộ tag từ map nguồn.
    /// </summary>
    public List<Guid>? TagIds { get; set; }

    public string? EditorialContent { get; set; }

    public int? UnlockEditorialAfterStars { get; set; }

    /// <summary>Null = copy LearnedTags từ nguồn.</summary>
    public List<Guid>? LearnedTags { get; set; }

    /// <summary>true = map mới published ngay. Mặc định false = Draft (gửi duyệt như tạo map thường).</summary>
    public bool AutoPublish { get; set; }
}
