using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Tiến độ người chơi theo từng level (MapDetail): điểm tốt nhất, sao, số lần chơi.
/// </summary>
public class UserMapResult : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    /// <summary>Level tương ứng. Unique cùng UserId sau khi migrate.</summary>
    public Guid? MapDetailId { get; set; }
    public virtual MapDetail? MapDetail { get; set; }
    public int BestScore { get; set; }
    public int BestStars { get; set; }
    public int Attempts { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public string? MasteryDeltaSpec { get; set; }
}
