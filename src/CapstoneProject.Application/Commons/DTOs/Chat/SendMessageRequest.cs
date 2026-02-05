using System.ComponentModel.DataAnnotations;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Chat;

/// <summary>
/// DTO for sending a message in a conversation
/// </summary>
public class SendMessageRequest
{
    [Required(ErrorMessage = "Chat room ID is required")]
    public Guid ChatRoomId { get; set; }

    [MaxLength(5000, ErrorMessage = "Message content must not exceed 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message type is required")]
    public MessageTypeEnum MessageType { get; set; } = MessageTypeEnum.Text;

    public Guid? ReplyToMessageId { get; set; }

    [MaxLength(255, ErrorMessage = "File name must not exceed 255 characters")]
    public string? FileName { get; set; }

    [Range(0, 10485760, ErrorMessage = "File size must be between 0 and 10MB")]
    public long? FileSize { get; set; }
}
