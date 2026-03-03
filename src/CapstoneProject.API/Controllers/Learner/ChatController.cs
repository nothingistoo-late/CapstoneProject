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
    /// Lấy hoặc tạo hội thoại riêng với một user. Body chứa otherUserId. Yêu cầu Bearer token.
    ///
    ///     POST /api/learner/chat/conversations/private
    ///     Body: { "otherUserId": "guid" }
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

    /// <summary>Đóng hội thoại nhóm.</summary>
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

    /// <summary>Danh sách hội thoại (phân trang, tìm kiếm).</summary>
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

    /// <summary>Tin nhắn trong hội thoại (phân trang, beforeMessageId).</summary>
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

    /// <summary>Gửi tin nhắn vào hội thoại.</summary>
    [HttpPost("conversations/{conversationId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Send message", Description = "Sends a message to the conversation. Body contains content. conversationId in route. Requires Bearer token. Use SignalR for real-time delivery.", OperationId = "Learner_SendMessage", Tags = new[] { "Learner - Chat" })]
    public async Task<IActionResult> SendMessage([FromRoute] Guid conversationId, [FromBody] SendMessageCommand command)
    {
        command.Request.ChatRoomId = conversationId;
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cập nhật nội dung tin nhắn (chỉ author).</summary>
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

    /// <summary>Xóa tin nhắn (chỉ author hoặc quyền cao hơn).</summary>
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

    /// <summary>Danh sách user để tìm bạn chat (Learner, active).</summary>
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
