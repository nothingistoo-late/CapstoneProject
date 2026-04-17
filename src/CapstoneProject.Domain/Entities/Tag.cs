using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Thẻ phân loại (tag) cho game - many-to-many với Game qua GameTag.
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
}
