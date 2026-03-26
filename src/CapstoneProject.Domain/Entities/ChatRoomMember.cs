using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Represents a participant in a conversation.
/// For private chats: Always exactly 2 participants, no roles needed.
/// For group chats: Flexible participants, can join/leave while room is active.
/// </summary>
public class ChatRoomMember : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = CapstoneProject.Domain.Common.VietnamDateTime.Now;
    public DateTime? LeftAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    
    // Navigation properties
    public virtual ChatRoom ChatRoom { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    
    public bool IsActive() => LeftAt == null;
    public void Leave()
    {
        LeftAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
    }
}

