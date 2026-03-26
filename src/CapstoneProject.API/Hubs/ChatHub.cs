using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.API.Hubs;

/// <summary>
/// SignalR Hub for real-time chat communication.
/// Uses conversation-based groups: Conversation_{conversationId}
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ICurrentUserService currentUserService, ILogger<ChatHub> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString))
        {
            // Join user to their personal group for direct notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userIdString}");
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userIdString, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString))
        {
            _logger.LogInformation("User {UserId} disconnected", userIdString);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a conversation (private chat or temporary group).
    /// Client should call this when opening a conversation.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(conversationId, out var convId) && Guid.TryParse(userIdString, out var userId))
        {
            var groupName = $"Conversation_{conversationId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {UserId} joined conversation {ConversationId}", userId, conversationId);
            
            // Notify others in the conversation (except sender)
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserJoinedConversation", new
            {
                UserId = userId,
                ConversationId = conversationId,
                Timestamp = CapstoneProject.Domain.Common.VietnamDateTime.Now
            });
        }
    }

    /// <summary>
    /// Leave a conversation.
    /// Client should call this when closing/closing a conversation view.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(conversationId, out var convId))
        {
            var groupName = $"Conversation_{conversationId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {UserId} left conversation {ConversationId}", userIdString, conversationId);
        }
    }

    /// <summary>
    /// Send typing indicator to other participants.
    /// </summary>
    public async Task SendTyping(string conversationId, bool isTyping)
    {
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(conversationId, out var convId) && Guid.TryParse(userIdString, out var userId))
        {
            var groupName = $"Conversation_{conversationId}";
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserTyping", new
            {
                UserId = userId,
                ConversationId = conversationId,
                IsTyping = isTyping,
                Timestamp = CapstoneProject.Domain.Common.VietnamDateTime.Now
            });
        }
    }
}

/// <summary>
/// Extension methods for broadcasting chat events via SignalR.
/// Used by services to notify clients.
/// </summary>
public static class ChatHubExtensions
{
    /// <summary>
    /// Broadcast a new message to all participants in a conversation.
    /// </summary>
    public static async Task BroadcastMessageAsync(this IHubContext<ChatHub> hubContext, Guid conversationId, object messageDto)
    {
        await hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ReceiveMessage", messageDto);
    }

    /// <summary>
    /// Notify all participants that a conversation (group room) has been closed.
    /// </summary>
    public static async Task BroadcastConversationClosedAsync(this IHubContext<ChatHub> hubContext, Guid conversationId, Guid closedByUserId)
    {
        await hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ConversationClosed", new
        {
            ConversationId = conversationId,
            ClosedBy = closedByUserId,
            ClosedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now
        });
        
        // Also notify via user groups for users who might not be in the conversation group
        await hubContext.Clients.Group($"User_{closedByUserId}").SendAsync("ConversationClosed", new
        {
            ConversationId = conversationId,
            ClosedBy = closedByUserId,
            ClosedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now
        });
    }

    /// <summary>
    /// Notify participants that a user has joined the conversation.
    /// </summary>
    public static async Task BroadcastUserJoinedAsync(this IHubContext<ChatHub> hubContext, Guid conversationId, Guid userId)
    {
        await hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("UserJoinedConversation", new
        {
            UserId = userId,
            ConversationId = conversationId,
            Timestamp = CapstoneProject.Domain.Common.VietnamDateTime.Now
        });
    }

    /// <summary>
    /// Notify participants that a user has left the conversation.
    /// </summary>
    public static async Task BroadcastUserLeftAsync(this IHubContext<ChatHub> hubContext, Guid conversationId, Guid userId)
    {
        await hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("UserLeftConversation", new
        {
            UserId = userId,
            ConversationId = conversationId,
            Timestamp = CapstoneProject.Domain.Common.VietnamDateTime.Now
        });
    }
}

