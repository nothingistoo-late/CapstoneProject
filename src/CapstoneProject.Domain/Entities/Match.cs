using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Một trận thi đấu (competitive) - gắn với một Map, có nhiều Room.
/// </summary>
public class Match : BaseEntity
{
    public Guid MapId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? RulesSpec { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    public virtual ICollection<UserMatchResult> UserMatchResults { get; set; } = new List<UserMatchResult>();
}
