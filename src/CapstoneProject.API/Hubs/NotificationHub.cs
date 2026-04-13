using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.API.Hubs;

/// <summary>
/// SignalR Hub for real-time notification delivery.
/// 
/// Usage:
/// - Each authenticated user joins a group: User_{userId}
/// - Notifications can be sent to users via their personal group
/// - Example: await notificationHub.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ICurrentUserService currentUserService, ILogger<NotificationHub> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString))
        {
            // Join user to their personal group for notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userIdString}");
            _logger.LogInformation("User {UserId} connected to NotificationHub with connection {ConnectionId}", userIdString, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString))
        {
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userIdString);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Mark notification as read (client can callback to server if needed)
    /// </summary>
    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);
            
            // Notify other connections of the same user about the read status
            await Clients.GroupExcept($"User_{userIdString}", Context.ConnectionId)
                .SendAsync("NotificationReadStatusChanged", new
                {
                    NotificationId = notificationId,
                    IsRead = true
                });
        }
    }
}
