using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

public class Notification : BaseEntity
{
    public NotificationTypeEnum NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActionUrl { get; set; }

    public virtual AppUser? ActorUser { get; set; }
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
