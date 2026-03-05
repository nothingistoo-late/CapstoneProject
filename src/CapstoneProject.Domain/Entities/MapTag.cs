using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class MapTag : BaseEntity
{
    public Guid MapId { get; set; }
    public Guid TagId { get; set; }

    public virtual Map Map { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
