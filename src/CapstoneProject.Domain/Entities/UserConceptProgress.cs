using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Tiến độ user với từng khái niệm: đã đọc/xong chưa (mở khóa item tiếp theo).
/// </summary>
public class UserConceptProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ConceptId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public virtual Concept Concept { get; set; } = null!;
}
