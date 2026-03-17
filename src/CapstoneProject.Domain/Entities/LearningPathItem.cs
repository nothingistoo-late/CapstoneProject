using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Một item trong lộ trình: là Concept (khái niệm) hoặc Map (thử thách), có thứ tự từ dễ đến khó.
/// Lộ trình = danh sách LearningPathItem của một LearningGoal, sort theo SortOrder.
/// </summary>
public class LearningPathItem : BaseEntity
{
    public Guid LearningGoalId { get; set; }
    public LearningPathItemTypeEnum ItemType { get; set; }
    public Guid? ConceptId { get; set; }
    public Guid? MapId { get; set; }
    /// <summary>Thứ tự trong lộ trình (1, 2, 3...). Unlock theo thứ tự.</summary>
    public int SortOrder { get; set; }

    public virtual LearningGoal LearningGoal { get; set; } = null!;
    public virtual Concept? Concept { get; set; }
    public virtual Map? Map { get; set; }
}
