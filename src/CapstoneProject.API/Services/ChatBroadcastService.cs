using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.API.Hubs;
using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Infrastructure.Context;

namespace CapstoneProject.API.Services;

public class ChatBroadcastService : IChatBroadcastService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatBroadcastService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatBroadcastService(
        IHubContext<ChatHub> hubContext,
        ILogger<ChatBroadcastService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task BroadcastMessageAsync(Guid conversationId, MessageResponse message)
    {
        try
        {
            // 1. Real-time: push ReceiveMessage to everyone currently viewing this conversation
            await _hubContext.Clients
                .Group($"Conversation_{conversationId}")
                .SendAsync("ReceiveMessage", message);

            // 2. Sidebar update: push ConversationUpdated to ALL members' personal groups
            //    (including the sender so their sidebar refreshes on other tabs/devices)
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CapstoneProjectDbContext>();

            // Single query: fetch all active members + their unread count in one go
            var members = await db.ChatRoomMembers
                .Where(m => m.ChatRoomId == conversationId && m.LeftAt == null)
                .Select(m => new
                {
                    m.UserId,
                    // If sender: unread = 0; otherwise count unread messages after last read time
                    UnreadCount = m.UserId == message.SenderId
                        ? 0
                        : db.Messages.Count(msg =>
                            msg.ChatRoomId == conversationId &&
                            !msg.IsDeleted &&
                            msg.SenderId != m.UserId &&
                            (msg.CreatedAt ?? DateTime.MinValue) > (m.LastReadAt ?? DateTime.MinValue))
                })
                .ToListAsync();

            // 3. Fan-out to all personal groups in parallel
            var broadcastTasks = members.Select(m =>
                _hubContext.Clients
                    .Group($"User_{m.UserId}")
                    .SendAsync("ConversationUpdated", new
                    {
                        ConversationId = conversationId,
                        LastMessage  = message,
                        UnreadCount  = m.UnreadCount,
                    })
            );

            await Task.WhenAll(broadcastTasks);
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
                ClosedBy       = closedByUserId,
                ClosedAt       = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            };

            await Task.WhenAll(
                _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ConversationClosed", notification),
                _hubContext.Clients.Group($"User_{closedByUserId}").SendAsync("ConversationClosed", notification)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting conversation closed notification for {ConversationId}", conversationId);
        }
    }
}
