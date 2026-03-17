using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Khái niệm lập trình thuộc một mục tiêu học tập (vd: "Biến", "If-else", "For loop").
/// Nội dung lý thuyết do FE handle: FE dùng ContentKey để load file tĩnh (vd. content/variables.md).
/// </summary>
public class Concept : BaseEntity
{
    public Guid LearningGoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Key để FE resolve nội dung (vd. "variables" → load content/variables.md hoặc bundle).</summary>
    public string? ContentKey { get; set; }
    /// <summary>Thứ tự trong mục tiêu (hiển thị và unlock).</summary>
    public int SortOrder { get; set; }

    public virtual LearningGoal LearningGoal { get; set; } = null!;
    public virtual ICollection<UserConceptProgress> UserConceptProgresses { get; set; } = new List<UserConceptProgress>();
}
