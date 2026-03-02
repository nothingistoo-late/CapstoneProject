using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Thành viên trong phòng thi đấu.
/// </summary>
public class RoomParticipant : BaseEntity
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsReady { get; set; }
    public bool IsOwner { get; set; }
    public int? Rank { get; set; }
    public int? FinalScore { get; set; }
    public Guid? SubmissionId { get; set; }

    public virtual Room Room { get; set; } = null!;
    public virtual Submission? Submission { get; set; }
}
