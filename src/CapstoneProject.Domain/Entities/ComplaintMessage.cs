using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class ComplaintMessage : BaseEntity
{
    public Guid ComplaintId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Internal notes visible to Admin/Moderator only.
    /// </summary>
    public bool IsInternal { get; set; } = false;

    public virtual Complaint Complaint { get; set; } = null!;
    public virtual AppUser Sender { get; set; } = null!;
}

