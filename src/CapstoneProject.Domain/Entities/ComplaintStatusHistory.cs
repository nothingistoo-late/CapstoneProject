using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

public class ComplaintStatusHistory : BaseEntity
{
    public Guid ComplaintId { get; set; }
    public ComplaintStatusEnum FromStatus { get; set; }
    public ComplaintStatusEnum ToStatus { get; set; }

    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }

    public virtual Complaint Complaint { get; set; } = null!;
    public virtual AppUser ChangedByUser { get; set; } = null!;
}

