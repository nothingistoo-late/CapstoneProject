    using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapstoneProject.Application.Features.Chat.Commands.CloseConversation;
using CapstoneProject.Application.Features.Chat.Commands.CreatePrivateConversation;
using CapstoneProject.Application.Features.Chat.Commands.CreateTemporaryGroupConversation;
using CapstoneProject.Application.Features.Chat.Commands.DeleteMessage;
using CapstoneProject.Application.Features.Chat.Commands.SendMessage;
using CapstoneProject.Application.Features.Chat.Commands.UpdateMessage;
using CapstoneProject.Application.Features.Chat.Queries.GetChatRooms;
using CapstoneProject.Application.Features.Chat.Queries.GetMessages;
using CapstoneProject.Application.Features.User.Queries.GetPagedUsers;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Domain.Enums;
using CapstoneProject.API.Attributes;

namespace CapstoneProject.API.Controllers.Chat;

/// <summary>
/// API Controller for chat operations.
/// Supports both private 1-1 chats and temporary competition group chats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Chat")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMediator mediator, ILogger<ChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get or create a private conversation with another user.
    /// Returns existing conversation if one exists, otherwise creates a new one.
    /// </summary>
    [HttpPost("conversations/private")]
    public async Task<IActionResult> CreatePrivateConversation([FromBody] CreatePrivateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create a new temporary competition group conversation.
    /// </summary>
    [HttpPost("conversations/temporary-group")]
    public async Task<IActionResult> CreateTemporaryGroupConversation([FromBody] CreateTemporaryGroupConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Close a temporary group conversation.
    /// Prevents new messages and new participants. Notifies all connected clients.
    /// </summary>
    [HttpPost("conversations/{conversationId}/close")]
    public async Task<IActionResult> CloseConversation([FromRoute] Guid conversationId)
    {
        var command = new CloseConversationCommand { ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all conversations for current user (both private and group)
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null)
    {
        var query = new GetChatRoomsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get messages from a conversation
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(
        [FromRoute] Guid conversationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? beforeMessageId = null)
    {
        var query = new GetMessagesQuery
        {
            Request = new()
            {
                ChatRoomId = conversationId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                BeforeMessageId = beforeMessageId
            }
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Send a message to a conversation.
    /// Validates conversation is not closed and user is a participant.
    /// </summary>
    [HttpPost("conversations/{conversationId}/messages")]
    public async Task<IActionResult> SendMessage([FromRoute] Guid conversationId, [FromBody] SendMessageCommand command)
    {
        command.Request.ChatRoomId = conversationId;
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update a message
    /// </summary>
    [HttpPut("messages/{messageId}")]
    public async Task<IActionResult> UpdateMessage([FromRoute] Guid messageId, [FromBody] UpdateMessageCommand command)
    {
        command.MessageId = messageId;
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a message
    /// </summary>
    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage([FromRoute] Guid messageId)
    {
        var command = new DeleteMessageCommand { MessageId = messageId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get list of users (Student role) for chat
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100, [FromQuery] string? searchTerm = null)
    {
        var filter = new UserFilter
        {
            Page = pageNumber,
            PageSize = pageSize,
            Search = searchTerm,
            Role = RoleEnum.Student.ToString(), // Only get Student users
            Status = Domain.Enums.EntityStatusEnum.Active // Only active users
        };

        var query = new GetPagedUsersQuery(filter);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
