using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Games;

public class CreateMapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public decimal? Price { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. 0 = không có trial.</summary>
    public int FreeTrialAttemptLimit { get; set; }
    /// <summary>Nhiều level; nếu null/rỗng thì dùng <see cref="GameDetailJson"/> (một level order 0).</summary>
    public List<MapLevelInputDto>? Levels { get; set; }
    /// <summary>Một level duy nhất (order 0) khi không gửi <see cref="Levels"/>.</summary>
    public JsonElement? GameDetailJson { get; set; }
    /// <summary>Gợi ý cho game một level (khi chỉ gửi <see cref="GameDetailJson"/>). Nếu gửi <see cref="Levels"/> thì dùng <see cref="MapLevelInputDto.Hints"/> theo từng level.</summary>
    public List<HintItemDto> Hints { get; set; } = new();
    /// <summary>Tag game hiện tại để hiển thị/chọn lọc game.</summary>
    public List<Guid> TagIds { get; set; } = new();
    /// <summary>Tag game (UID) theo taxonomy hệ thống.</summary>
    public List<Guid> LearnedTags { get; set; } = new();
    /// <summary>Loại: platform | topdown | snake. Nếu không gửi sẽ lấy từ level.type hoặc level.metadata.</summary>
    public string? Type { get; set; }
    /// <summary>URL avatar game (Cloudinary). Optional khi tạo game.</summary>
    public string? AvatarUrl { get; set; }
}

public class HintItemDto
{
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;
}
