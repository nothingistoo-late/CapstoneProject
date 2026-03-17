using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Mục tiêu học tập (vd: Logic cơ bản, Điều kiện, Vòng lặp, Giải quyết vấn đề).
/// User chọn một goal khi bắt đầu → hệ thống tạo lộ trình concept + map theo thứ tự.
/// </summary>
public class LearningGoal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Thứ tự hiển thị khi chọn mục tiêu.</summary>
    public int SortOrder { get; set; }
    public string? IconUrl { get; set; }

    public virtual ICollection<Concept> Concepts { get; set; } = new List<Concept>();
    public virtual ICollection<LearningPathItem> LearningPathItems { get; set; } = new List<LearningPathItem>();
    public virtual ICollection<UserLearningGoal> UserLearningGoals { get; set; } = new List<UserLearningGoal>();
}
