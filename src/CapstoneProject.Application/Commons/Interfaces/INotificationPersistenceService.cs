using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Service for managing persistent notifications in database
/// </summary>
public interface INotificationPersistenceService
{
    Task<Guid> CreateNotificationAsync(
        NotificationTypeEnum type,
        string title,
        string body,
        List<Guid> recipientUserIds,
        Guid? actorUserId = null,
        string? payloadJson = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default
    );

    Task MarkAsReadAsync(Guid userNotificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<NotificationListDto>> GetNotificationsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );

    Task<List<NotificationListDto>> GetUnreadNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}

public class NotificationListDto
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
    public SimpleUserDto? Actor { get; set; }
}

public class SimpleUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
