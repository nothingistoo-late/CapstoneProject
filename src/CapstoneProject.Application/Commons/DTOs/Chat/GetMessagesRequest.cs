using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.Commons.DTOs.Chat;

/// <summary>
/// DTO for retrieving messages from a conversation with pagination
/// </summary>
public class GetMessagesRequest
{
    [Required(ErrorMessage = "Chat room ID is required")]
    public Guid ChatRoomId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 50;

    public Guid? BeforeMessageId { get; set; } // For pagination (messages before this message ID)
}
