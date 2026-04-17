using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Trò chơi (challenge) - metadata chung cho game.
/// </summary>
public class Game : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public bool IsPublished { get; set; }
    public GameStatusEnum GameStatus { get; set; } = GameStatusEnum.Draft;
    public decimal? Price { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. 0 = không có trial.</summary>
    public int FreeTrialAttemptLimit { get; set; }
    public string? EditorialContent { get; set; }
    /// <summary>Ghi chú kiểm duyệt gần nhất khi Admin/Moderator duyệt hoặc từ chối game.</summary>
    public string? ReviewNote { get; set; }
    public int UnlockEditorialAfterStars { get; set; } = 3;
    /// <summary>Danh sách tag kiến thức user học được sau khi chơi game (UID của Tag).</summary>
    public List<Guid> LearnedTags { get; set; } = new();
    /// <summary>URL avatar của game (lưu trên Cloudinary).</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Tăng khi nội dung game thay đổi có ý nghĩa (update level/JSON). Bản mới = 1.</summary>
    public int ContentVersion { get; set; } = 1;

    /// <summary>
    /// Nhóm phiên bản của cùng một game line. Bản đầu tiên tự tham chiếu chính nó.
    /// </summary>
    public Guid? RootGameId { get; set; }

    /// <summary>
    /// Chỉ một phiên bản active trong mỗi game line tại một thời điểm.
    /// </summary>
    public bool IsActiveVersion { get; set; } = true;

    public virtual AppUser? Creator { get; set; }
    public virtual ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
    /// <summary>Các level (JSON layout) của game, sắp xếp theo <see cref="GameDetail.LevelOrder"/>.</summary>
    public virtual ICollection<GameDetail> GameDetails { get; set; } = new List<GameDetail>();
    /// <summary>Ảnh / video mô tả gameplay (gallery).</summary>
    public virtual ICollection<GameMedia> GameMedias { get; set; } = new List<GameMedia>();
}
