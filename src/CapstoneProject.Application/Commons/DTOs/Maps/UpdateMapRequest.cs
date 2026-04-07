using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class UpdateMapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public decimal? Price { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. Null = không đổi; 0 = tắt trial.</summary>
    public int? FreeTrialAttemptLimit { get; set; }
    /// <summary>Thay thế toàn bộ levels khi có giá trị (xóa level cũ, tạo mới).</summary>
    public List<MapLevelInputDto>? Levels { get; set; }
    /// <summary>Cập nhật một level: level đầu (order nhỏ nhất) hoặc order 0 khi không dùng <see cref="Levels"/>.</summary>
    public JsonElement? MapDetailJson { get; set; }
    /// <summary>Loại map mặc định cho level nếu JSON không tự khai báo. Hỗ trợ Topdown | Platform | Snake.</summary>
    public string? Type { get; set; }
    public string? EditorialContent { get; set; }
    public int? UnlockEditorialAfterStars { get; set; }
    /// <summary>Gợi ý khi chỉ cập nhật <see cref="MapDetailJson"/> (một level). Khi gửi <see cref="Levels"/> thì dùng hint trên từng phần tử.</summary>
    public List<HintItemDto>? Hints { get; set; }
    /// <summary>Tag map hiện tại. Null = không đổi, [] = xóa hết.</summary>
    public List<Guid>? TagIds { get; set; }
    /// <summary>Tag map (UID). Null = không đổi, [] = xóa hết.</summary>
    public List<Guid>? LearnedTags { get; set; }
    /// <summary>URL avatar map (Cloudinary). Null = không đổi.</summary>
    public string? AvatarUrl { get; set; }
}
