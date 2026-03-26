using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Má»¥c tiÃªu há»c táº­p user Ä‘Ã£ chá»n (khi Ä‘Äƒng nháº­p / vÃ o dashboard).
/// Má»—i user cÃ³ thá»ƒ cÃ³ má»™t má»¥c tiÃªu Ä‘ang theo (hoáº·c chÆ°a chá»n).
/// </summary>
public class UserLearningGoal : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LearningGoalId { get; set; }
    public DateTime SelectedAt { get; set; } = CapstoneProject.Domain.Common.VietnamDateTime.Now;

    public virtual AppUser User { get; set; } = null!;
    public virtual LearningGoal LearningGoal { get; set; } = null!;
}

