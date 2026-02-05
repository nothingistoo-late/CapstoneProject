using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Chat;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageTypeEnum MessageType { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public MessageResponse? ReplyToMessage { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MessageReadResponse> ReadBy { get; set; } = new();
}
