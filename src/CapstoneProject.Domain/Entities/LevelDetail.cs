using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Thông tin chi tiết level: full JSON (layers, startPosition, goalPosition, metadata...).
/// 1-1 với LevelCatalog.
/// </summary>
public class LevelDetail : BaseEntity
{
    public Guid LevelCatalogId { get; set; }
    public string JsonContent { get; set; } = string.Empty;

    public virtual LevelCatalog LevelCatalog { get; set; } = null!;
}
