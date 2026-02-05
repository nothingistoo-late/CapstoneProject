using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Chat;

public class ChatRoomResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; } // null for private chats
    public ChatRoomTypeEnum RoomType { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public List<ChatRoomMemberResponse> Members { get; set; } = new();
    public MessageResponse? LastMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
