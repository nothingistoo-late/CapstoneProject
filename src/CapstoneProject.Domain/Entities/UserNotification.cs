using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class UserNotification : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public virtual Notification Notification { get; set; } = null!;
}
