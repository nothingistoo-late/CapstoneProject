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
    public bool IsPublished { get; set; }
    public MapStatusEnum MapStatus { get; set; } = MapStatusEnum.Draft;
    public decimal? Price { get; set; }
    public string? EditorialContent { get; set; }
    public int UnlockEditorialAfterStars { get; set; } = 3;
    /// <summary>Danh sách tag kiến thức user học được sau khi chơi map (UID của Tag).</summary>
    public List<Guid> LearnedTags { get; set; } = new();
    /// <summary>URL avatar của map (lưu trên Cloudinary).</summary>
    public string? AvatarUrl { get; set; }

    public virtual AppUser? Creator { get; set; }
    public virtual ICollection<MapTag> MapTags { get; set; } = new List<MapTag>();
    /// <summary>Các level (JSON layout) của map, sắp xếp theo <see cref="MapDetail.LevelOrder"/>.</summary>
    public virtual ICollection<MapDetail> MapDetails { get; set; } = new List<MapDetail>();
    /// <summary>Ảnh / video mô tả gameplay (gallery).</summary>
    public virtual ICollection<MapMedia> MapMedias { get; set; } = new List<MapMedia>();
}
