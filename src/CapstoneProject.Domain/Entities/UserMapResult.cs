using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Tiến độ người chơi trên từng map: điểm tốt nhất, sao, số lần chơi, mastery.
/// </summary>
public class UserMapResult : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    public int BestScore { get; set; }
    public int BestStars { get; set; }
    public int Attempts { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public string? MasteryDeltaSpec { get; set; }
}
