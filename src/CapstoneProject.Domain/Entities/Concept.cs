using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Khái niệm logic (vòng lặp, điều kiện, biến...) - many-to-many với Map qua MapConcept.
/// </summary>
public class Concept : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual ICollection<MapConcept> MapConcepts { get; set; } = new List<MapConcept>();
}
