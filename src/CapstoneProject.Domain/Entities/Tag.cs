using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Thẻ phân loại (tag) cho map - many-to-many với Map qua MapTag.
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<MapTag> MapTags { get; set; } = new List<MapTag>();
}
