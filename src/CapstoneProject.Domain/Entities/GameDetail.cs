using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Một level trong game (nhiều bản ghi / game). Thứ tự bởi <see cref="LevelOrder"/>.
/// </summary>
public class GameDetail : BaseEntity
{
    public Guid GameId { get; set; }

    /// <summary>Thứ tự level trong game (0, 1, …). Unique cùng GameId.</summary>
    public int LevelOrder { get; set; }

    /// <summary>Tên hiển thị tùy chọn (vd. Level 1).</summary>
    public string? Title { get; set; }

    /// <summary>Nội dung JSON của level.</summary>
    public string JsonContent { get; set; } = string.Empty;

    /// <summary>Giới hạn thời gian (ms) cho level này.</summary>
    public int TimeLimitMs { get; set; }

    /// <summary>Điều kiện thắng (vd. số bước / điểm tối thiểu) cho level này.</summary>
    public int WinCondition { get; set; }

    /// <summary>Topdown hoặc Platform cho level này.</summary>
    public GameTypeEnum Type { get; set; } = GameTypeEnum.Topdown;

    public virtual Game Game { get; set; } = null!;
    public virtual ICollection<Hint> Hints { get; set; } = new List<Hint>();
}

