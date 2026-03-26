using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

public class Complaint : BaseEntity
{
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ComplaintStatusEnum ComplaintStatus { get; set; } = ComplaintStatusEnum.Open;

    public DateTime? ResolvedAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<ComplaintMessage> Messages { get; set; } = new List<ComplaintMessage>();
    public virtual ICollection<ComplaintStatusHistory> StatusHistories { get; set; } = new List<ComplaintStatusHistory>();
}

