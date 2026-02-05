using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Represents a message in a conversation.
/// Messages are only allowed in active (non-closed) conversations.
/// </summary>
public class Message : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageTypeEnum MessageType { get; set; } = MessageTypeEnum.Text;
    
    // Optional: File attachments
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    
    // Optional: Reply to another message
    public Guid? ReplyToMessageId { get; set; }
    
    // Optional: Edit tracking
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
    
    // Note: IsDeleted and DeletedAt are inherited from BaseEntity
    
    // Navigation properties
    public virtual ChatRoom ChatRoom { get; set; } = null!;
    public virtual AppUser Sender { get; set; } = null!;
    public virtual Message? ReplyToMessage { get; set; }
    public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
}
