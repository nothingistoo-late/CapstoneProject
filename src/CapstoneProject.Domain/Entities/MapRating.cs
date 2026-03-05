using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class MapRating : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
