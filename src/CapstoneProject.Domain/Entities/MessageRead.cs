using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class MessageRead : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = CapstoneProject.Domain.Common.VietnamDateTime.Now;
    
    // Navigation properties
    public virtual Message Message { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}

