using CapstoneProject.Application.Commons.DTOs.Chat;

namespace CapstoneProject.Application.Commons.Interfaces;

public interface IChatNotificationService
{
    Task NotifyMessageSentAsync(MessageResponse message, Guid chatRoomId);
    Task NotifyUserJoinedAsync(Guid userId, Guid chatRoomId);
    Task NotifyUserTypingAsync(Guid userId, Guid chatRoomId, bool isTyping);
    Task NotifyMessageReadAsync(Guid userId, Guid chatRoomId, Guid messageId);
}
