using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Thử thách (bản đồ/challenge) - có thể do hệ thống hoặc UGC (Learner tạo, Admin/Moderator duyệt).
/// </summary>
public class Map : BaseEntity
{
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Độ khó (1-5 hoặc enum)</summary>
    public int Difficulty { get; set; }
    /// <summary>Thời gian giới hạn (ms)</summary>
    public int TimeLimitMs { get; set; }
    public bool IsPublished { get; set; }
    public MapStatusEnum MapStatus { get; set; } = MapStatusEnum.Draft;
    /// <summary>Giá (0 = miễn phí; >0 = trả phí, đơn vị tùy quy ước)</summary>
    public decimal? Price { get; set; }
    /// <summary>Bài giải mẫu (mở khóa khi đạt đủ sao)</summary>
    public string? EditorialContent { get; set; }
    /// <summary>Số sao tối thiểu để mở khóa editorial (mặc định 3)</summary>
    public int UnlockEditorialAfterStars { get; set; } = 3;

    public virtual ICollection<MapSpec> MapSpecs { get; set; } = new List<MapSpec>();
    public virtual ICollection<Hint> Hints { get; set; } = new List<Hint>();
    public virtual ICollection<MapConstraint> Constraints { get; set; } = new List<MapConstraint>();
    public virtual ICollection<MapTag> MapTags { get; set; } = new List<MapTag>();
    public virtual ICollection<MapConcept> MapConcepts { get; set; } = new List<MapConcept>();
}
