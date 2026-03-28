using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Mục tiêu học tập user đã chọn (khi đăng nhập / vào dashboard).
/// Mỗi user có thể có một mục tiêu đang theo (hoặc chưa chọn).
/// </summary>
public class UserLearningGoal : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LearningGoalId { get; set; }
    public DateTime SelectedAt { get; set; } = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;

    public virtual AppUser User { get; set; } = null!;
    public virtual LearningGoal LearningGoal { get; set; } = null!;
}



