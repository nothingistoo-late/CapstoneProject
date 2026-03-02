using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Đánh giá (rate) của người dùng cho một map.
/// </summary>
public class ChallengeRating : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
