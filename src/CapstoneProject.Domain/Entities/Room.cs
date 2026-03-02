using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Phòng thi đấu 2-8 người, thuộc một Match.
/// </summary>
public class Room : BaseEntity
{
    public Guid MatchId { get; set; }
    public int MaxPlayers { get; set; } = 8;
    public string? Code { get; set; }
    public RoomStatusEnum RoomStatus { get; set; } = RoomStatusEnum.Waiting;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public virtual Match Match { get; set; } = null!;
    public virtual ICollection<RoomParticipant> RoomParticipants { get; set; } = new List<RoomParticipant>();
}
