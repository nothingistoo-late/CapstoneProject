using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class GameTag : BaseEntity
{
    public Guid GameId { get; set; }
    public Guid TagId { get; set; }

    public virtual Game Game { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
