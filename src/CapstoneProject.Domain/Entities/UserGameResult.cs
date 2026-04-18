using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Tiến độ người chơi theo từng level (GameDetail): điểm tốt nhất, sao, số lần chơi.
/// </summary>
public class UserGameResult : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    /// <summary>Level tương ứng. Unique cùng UserId sau khi migrate.</summary>
    public Guid? GameDetailId { get; set; }
    public virtual GameDetail? GameDetail { get; set; }
    public int BestScore { get; set; }
    public int BestStars { get; set; }
    public int Attempts { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public string? MasteryDeltaSpec { get; set; }
}
