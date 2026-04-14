using Microsoft.AspNetCore.Authorization;
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

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/chat")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Chat")]
[SwaggerTag("Learner - Private conversations, group chats, messages")]
public class LearnerChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LearnerChatController> _logger;

    public LearnerChatController(IMediator mediator, ILogger<LearnerChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get or create private conversation
    /// </summary>
    /// <remarks>
    /// Gets existing or creates new private conversation with the target user. Requires Bearer token.
    ///
    /// **Body (JSON):**
    /// - otherUserId (Guid, required): User ID of the other participant.
    ///
    /// **METHOD and path:** POST /api/learner/chat/conversations/private
    ///
    /// **Example request body:** { "otherUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
    /// </remarks>
    /// <response code="200">Returns message and data (conversation).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("conversations/private")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get or create private conversation", Description = "Gets existing or creates new private conversation with target userId. Requires Bearer token.", OperationId = "Learner_CreatePrivateConversation", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> CreatePrivateConversation([FromBody] CreatePrivateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create temporary group conversation
    /// </summary>
    /// <remarks>
    /// Creates a new temporary group chat with a name. Requires Bearer token.
    ///
    /// **Body (JSON):**
    /// - name (string, required): Group conversation name.
    ///
    /// **METHOD and path:** POST /api/learner/chat/conversations/temporary-group
    ///
    /// **Example request body:** { "name": "Study Group" }
    /// </remarks>
    [HttpPost("conversations/temporary-group")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Create temporary group conversation", OperationId = "Learner_CreateTemporaryGroupConversation", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> CreateTemporaryGroupConversation([FromBody] CreateTemporaryGroupConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Close group conversation.</summary>
    /// <remarks>
    /// Closes a group conversation by ID. Requires Bearer token.
    ///
    /// **Route:** conversationId (Guid, required): Conversation ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/learner/chat/conversations/{conversationId}/close
    ///
    /// **Example request:** POST /api/learner/chat/conversations/3fa85f64-5717-4562-b3fc-2c963f66afa6/close
    /// </remarks>
    [HttpPost("conversations/{conversationId}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Close group conversation", OperationId = "Learner_CloseConversation", Description = "Closes a group conversation by conversationId. Requires Bearer token.", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> CloseConversation([FromRoute] Guid conversationId)
    {
        var command = new CloseConversationCommand { ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get list of conversations (paginated, search).</summary>
    /// <remarks>
    /// Returns paginated list of conversations for the current user. Requires Bearer token.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - searchTerm (string, optional): Search in conversation names/participants.
    ///
    /// **METHOD and path:** GET /api/learner/chat/conversations
    ///
    /// **Example request:** GET /api/learner/chat/conversations?pageNumber=1&amp;pageSize=20&amp;searchTerm=john
    /// </remarks>
    [HttpGet("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get conversations", Description = "Returns paginated list of conversations for current user. Query: pageNumber, pageSize, searchTerm. Requires Bearer token.", OperationId = "Learner_GetConversations", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> GetConversations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null)
    {
        var query = new GetChatRoomsQuery { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm };
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get messages in a conversation (paginated, load more with beforeMessageId).</summary>
    /// <remarks>
    /// Returns paginated messages for a conversation. Use beforeMessageId for load-more. Requires Bearer token.
    ///
    /// **Route:** conversationId (Guid, required): Conversation ID.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Messages per page. Default 50.
    /// - beforeMessageId (Guid?, optional): Get messages before this message ID (for load more).
    ///
    /// **METHOD and path:** GET /api/learner/chat/conversations/{conversationId}/messages
    ///
    /// **Example request:** GET /api/learner/chat/conversations/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages?pageNumber=1&amp;pageSize=50
    /// </remarks>
    [HttpGet("conversations/{conversationId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get messages", Description = "Returns paginated messages for a conversation. Query: pageNumber, pageSize, beforeMessageId (for load more). Requires Bearer token.", OperationId = "Learner_GetMessages", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> GetMessages([FromRoute] Guid conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] Guid? beforeMessageId = null)
    {
        var query = new GetMessagesQuery { Request = new() { ChatRoomId = conversationId, PageNumber = pageNumber, PageSize = pageSize, BeforeMessageId = beforeMessageId } };
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Send message to conversation.</summary>
    /// <remarks>
    /// Sends a message to the conversation. conversationId is in route; body contains content and optional reply. Use SignalR for real-time delivery. Requires Bearer token.
    ///
    /// **Route:** conversationId (Guid, required): Conversation ID (also set in body.request.chatRoomId).
    ///
    /// **Body (multipart/form-data):** Fields with the following names:
    /// - ChatRoomId (Guid, required): Same as route conversationId.
    /// - Content (string, optional for image messages): Message text. Max 5000 chars.
    /// - MessageType (int, optional): MessageTypeEnum: 0=Text, 1=Image, 2=File, 3=Video. Default 0.
    /// - ReplyToMessageId (Guid?, optional): Reply to message ID.
    /// - ImageFile (IFormFile?, optional): Image attachment.
    ///
    /// **METHOD and path:** POST /api/learner/chat/conversations/{conversationId}/messages
    ///
    /// **Example request body:** multipart/form-data with ChatRoomId=3fa85f64-5717-4562-b3fc-2c963f66afa6, Content=Hello!, MessageType=0
    /// </remarks>
    [HttpPost("conversations/{conversationId}/messages")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Send message", Description = "Sends a message to the conversation. Body contains content. conversationId in route. Requires Bearer token. Use SignalR for real-time delivery.", OperationId = "Learner_SendMessage", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> SendMessage([FromRoute] Guid conversationId, [FromForm] SendMessageCommand command)
    {
        command.ChatRoomId = conversationId;
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update message content (author only).</summary>
    /// <remarks>
    /// Updates message content by messageId. Only message author. Requires Bearer token.
    ///
    /// **Route:** messageId (Guid, required): Message ID.
    ///
    /// **Body (JSON):**
    /// - messageId (Guid, required): Same as route (or omit if set in route).
    /// - content (string, required): New message content.
    ///
    /// **METHOD and path:** PUT /api/learner/chat/messages/{messageId}
    ///
    /// **Example request body:** { "messageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "content": "Updated text" }
    /// </remarks>
    [HttpPut("messages/{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update message", Description = "Updates message content by messageId. Only message author. Body contains new content. Requires Bearer token.", OperationId = "Learner_UpdateMessage", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> UpdateMessage([FromRoute] Guid messageId, [FromBody] UpdateMessageCommand command)
    {
        command.MessageId = messageId;
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete message (author or higher role).</summary>
    /// <remarks>
    /// Deletes a message by messageId. Only message author or admin. Requires Bearer token.
    ///
    /// **Route:** messageId (Guid, required): Message ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/learner/chat/messages/{messageId}
    ///
    /// **Example request:** DELETE /api/learner/chat/messages/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpDelete("messages/{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete message", Description = "Deletes a message by messageId. Only message author or admin. Requires Bearer token.", OperationId = "Learner_DeleteMessage", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> DeleteMessage([FromRoute] Guid messageId)
    {
        var command = new DeleteMessageCommand { MessageId = messageId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get users for chat (Learner, active).</summary>
    /// <remarks>
    /// Returns paginated users (Learner role, Active status) for starting private chat. Requires Bearer token.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 100.
    /// - searchTerm (string, optional): Search by name/email.
    ///
    /// **METHOD and path:** GET /api/learner/chat/users
    ///
    /// **Example request:** GET /api/learner/chat/users?pageNumber=1&amp;pageSize=100&amp;searchTerm=john
    /// </remarks>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get users for chat", Description = "Returns paginated users (Learner, Active) for starting private chat. Query: pageNumber, pageSize, searchTerm. Requires Bearer token.", OperationId = "Learner_GetChatUsers", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100, [FromQuery] string? searchTerm = null)
    {
        var filter = new UserFilter { Page = pageNumber, PageSize = pageSize, Search = searchTerm, Role = RoleEnum.Learner.ToString(), Status = Domain.Enums.EntityStatusEnum.Active };
        var result = await _mediator.Send(new GetPagedUsersQuery(filter));
        return Ok(result);
    }
}
