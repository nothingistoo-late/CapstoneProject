using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Trò chơi (challenge) - metadata chung cho map.
/// </summary>
public class Map : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    /// <summary>Loại map: Topdown hoặc Platform (user chọn khi tạo).</summary>
    public MapTypeEnum Type { get; set; } = MapTypeEnum.Topdown;
    public bool IsPublished { get; set; }
    public MapStatusEnum MapStatus { get; set; } = MapStatusEnum.Draft;
    public decimal? Price { get; set; }
    public string? EditorialContent { get; set; }
    public int UnlockEditorialAfterStars { get; set; } = 3;
    /// <summary>URL avatar của map (lưu trên Cloudinary).</summary>
    public string? AvatarUrl { get; set; }

    public virtual ICollection<Hint> Hints { get; set; } = new List<Hint>();
    public virtual ICollection<MapTag> MapTags { get; set; } = new List<MapTag>();
    public virtual MapDetail MapDetail { get; set; } = null!;
}
