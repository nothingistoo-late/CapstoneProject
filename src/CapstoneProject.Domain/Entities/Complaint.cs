using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

public class Complaint : BaseEntity
{
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextKey { get; set; }
    public string? ContextDataJson { get; set; }
    public DateTime? OccurredAt { get; set; }

    public ComplaintStatusEnum ComplaintStatus { get; set; } = ComplaintStatusEnum.Open;

    public DateTime? ResolvedAt { get; set; }
    public bool RefundProcessed { get; set; }
    public Guid? RefundedPaymentRecordId { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? RefundReason { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<ComplaintMessage> Messages { get; set; } = new List<ComplaintMessage>();
    public virtual ICollection<ComplaintStatusHistory> StatusHistories { get; set; } = new List<ComplaintStatusHistory>();
}

