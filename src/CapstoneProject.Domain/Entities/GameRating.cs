using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class GameRating : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
