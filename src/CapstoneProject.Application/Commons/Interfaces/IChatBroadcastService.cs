using CapstoneProject.Application.Commons.DTOs.Chat;

namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Service for broadcasting chat events via SignalR.
/// Abstracts SignalR hub context to avoid API layer dependency.
/// </summary>
public interface IChatBroadcastService
{
    Task BroadcastMessageAsync(Guid conversationId, MessageResponse message);
    Task BroadcastConversationClosedAsync(Guid conversationId, Guid closedByUserId);
}
