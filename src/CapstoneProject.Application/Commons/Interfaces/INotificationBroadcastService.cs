namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Service for broadcasting notification events via SignalR.
/// Abstracts SignalR hub context to avoid API layer dependency.
/// </summary>
public interface INotificationBroadcastService
{
    Task BroadcastNotificationCreatedAsync(
        Guid recipientUserId,
        NotificationBroadcastDto notification,
        int unreadCount,
        CancellationToken cancellationToken = default);

    Task BroadcastNotificationReadAsync(
        Guid recipientUserId,
        Guid userNotificationId,
        DateTime? readAt,
        int unreadCount,
        CancellationToken cancellationToken = default);

    Task BroadcastAllNotificationsReadAsync(
        Guid recipientUserId,
        int unreadCount,
        CancellationToken cancellationToken = default);
}

public class NotificationBroadcastDto
{
    public Guid UserNotificationId { get; set; }
    public Guid NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? PayloadJson { get; set; }
    public SimpleActorBroadcastDto? Actor { get; set; }
}

public class SimpleActorBroadcastDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
