using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Thông tin tổng quát level (catalog): name, type, difficulty.
/// </summary>
public class LevelCatalog : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Difficulty { get; set; }

    public virtual LevelDetail? Detail { get; set; }
}
