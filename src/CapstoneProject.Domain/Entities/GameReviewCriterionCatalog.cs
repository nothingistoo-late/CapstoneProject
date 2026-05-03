using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// CMS-configurable checklist items used when moderators approve/reject games.
/// </summary>
public class GameReviewCriterionCatalog : BaseEntity
{
    public string CriterionKey { get; set; } = string.Empty;
    public string SectionKey { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}
