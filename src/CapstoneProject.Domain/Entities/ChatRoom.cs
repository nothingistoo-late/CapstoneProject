using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Represents a conversation - either a private chat (1-1) or a temporary competition group chat.
/// Private chats: No name, implicitly created, exactly 2 participants.
/// Group chats: Temporary, can be closed, flexible participants.
/// </summary>
public class ChatRoom : BaseEntity
{
    public ChatRoomTypeEnum RoomType { get; set; } = ChatRoomTypeEnum.Private;
    
    // Only for temporary group chats
    public string? Name { get; set; }
    
    // Closure management for temporary group chats
    public bool IsClosed { get; set; } = false;
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    
    // Last message tracking
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    // Business logic methods
    public bool CanSendMessages() => !IsClosed;
    public bool CanJoin() => !IsClosed;
    public void Close(Guid closedByUserId)
    {
        IsClosed = true;
        ClosedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        ClosedBy = closedByUserId;
        UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        UpdatedBy = closedByUserId;
    }
}

