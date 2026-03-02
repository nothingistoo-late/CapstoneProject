using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Người dùng đã mở khóa huy hiệu nào.
/// </summary>
public class UserAchievement : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }

    public virtual Achievement Achievement { get; set; } = null!;
}
