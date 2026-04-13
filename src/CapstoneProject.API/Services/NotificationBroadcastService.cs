using CapstoneProject.API.Hubs;
using CapstoneProject.Application.Commons.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CapstoneProject.API.Services;

public class NotificationBroadcastService : INotificationBroadcastService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationBroadcastService> _logger;

    public NotificationBroadcastService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationBroadcastService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastNotificationCreatedAsync(
        Guid recipientUserId,
        NotificationBroadcastDto notification,
        int unreadCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = $"User_{recipientUserId}";

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification", new
                {
                    Notification = notification,
                    UnreadCount = unreadCount
                }, cancellationToken);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("UnreadCountChanged", new
                {
                    UnreadCount = unreadCount
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification to user {UserId}", recipientUserId);
        }
    }

    public async Task BroadcastNotificationReadAsync(
        Guid recipientUserId,
        Guid userNotificationId,
        DateTime? readAt,
        int unreadCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = $"User_{recipientUserId}";

            await _hubContext.Clients.Group(groupName)
                .SendAsync("NotificationReadStatusChanged", new
                {
                    NotificationId = userNotificationId,
                    IsRead = true,
                    ReadAt = readAt
                }, cancellationToken);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("UnreadCountChanged", new
                {
                    UnreadCount = unreadCount
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification read status to user {UserId}", recipientUserId);
        }
    }

    public async Task BroadcastAllNotificationsReadAsync(
        Guid recipientUserId,
        int unreadCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = $"User_{recipientUserId}";

            await _hubContext.Clients.Group(groupName)
                .SendAsync("AllNotificationsRead", new
                {
                    IsRead = true,
                    ReadAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
                }, cancellationToken);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("UnreadCountChanged", new
                {
                    UnreadCount = unreadCount
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting all notifications read to user {UserId}", recipientUserId);
        }
    }
}
