using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class ComplaintCategoryCatalog : BaseEntity
{
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
