using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Junction: Map - Concept (many-to-many).
/// </summary>
public class MapConcept : BaseEntity
{
    public Guid MapId { get; set; }
    public Guid ConceptId { get; set; }

    public virtual Map Map { get; set; } = null!;
    public virtual Concept Concept { get; set; } = null!;
}
