using Microsoft.AspNetCore.SignalR;
using CapstoneProject.API.Hubs;
using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.API.Services;

public class ChatBroadcastService : IChatBroadcastService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatBroadcastService> _logger;

    public ChatBroadcastService(
        IHubContext<ChatHub> hubContext,
        ILogger<ChatBroadcastService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastMessageAsync(Guid conversationId, MessageResponse message)
    {
        try
        {
            await _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ReceiveMessage", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting message to conversation {ConversationId}", conversationId);
        }
    }

    public async Task BroadcastConversationClosedAsync(Guid conversationId, Guid closedByUserId)
    {
        try
        {
            var notification = new
            {
                ConversationId = conversationId,
                ClosedBy = closedByUserId,
                ClosedAt = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ConversationClosed", notification);
            await _hubContext.Clients.Group($"User_{closedByUserId}").SendAsync("ConversationClosed", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting conversation closed notification for {ConversationId}", conversationId);
        }
    }
}
