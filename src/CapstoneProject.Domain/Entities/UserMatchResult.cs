using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Kết quả của một user trong một match (xếp hạng, điểm, submission).
/// </summary>
public class UserMatchResult : BaseEntity
{
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public Guid? SubmissionId { get; set; }
    public int Rank { get; set; }
    public int FinalScore { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public virtual Match Match { get; set; } = null!;
    public virtual Submission? Submission { get; set; }
}
