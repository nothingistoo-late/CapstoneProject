using CapstoneProject.Application.Features.Notifications.Commands.MarkAllAsRead;
using CapstoneProject.Application.Features.Notifications.Commands.MarkAsRead;
using CapstoneProject.Application.Features.Notifications.Queries.GetNotifications;
using CapstoneProject.Application.Features.Notifications.Queries.GetUnreadCount;
using CapstoneProject.Application.Features.Notifications.Queries.GetUnreadNotifications;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/notifications")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Notifications")]
[SwaggerTag("User notifications: list, read status, unread count")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user notifications with pagination
    /// </summary>
    /// <remarks>
    /// Trả về danh sách thông báo của user đang đăng nhập, sắp xếp theo ngày tạo (mới nhất trước).
    /// 
    /// **METHOD and path:** GET /api/learner/notifications?pageNumber=1&amp;pageSize=20
    /// 
    /// **Query Parameters:**
    /// - `pageNumber`: Trang (mặc định 1)
    /// - `pageSize`: Số item trên trang (mặc định 20)
    /// </remarks>
    /// <response code="200">Returns paginated notification list</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get user notifications", OperationId = "Learner_GetNotifications")]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mediator.Send(new GetNotificationsQuery(pageNumber, pageSize), cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get unread notifications
    /// </summary>
    /// <remarks>
    /// Trả về danh sách các thông báo chưa đọc của user đang đăng nhập.
    /// 
    /// **METHOD and path:** GET /api/learner/notifications/unread
    /// </remarks>
    /// <response code="200">Returns unread notifications list</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("unread")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get unread notifications", OperationId = "Learner_GetUnreadNotifications")]
    public async Task<IActionResult> GetUnreadNotifications(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUnreadNotificationsQuery(), cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    /// <remarks>
    /// Trả về số lượng thông báo chưa đọc của user đang đăng nhập.
    /// 
    /// **METHOD and path:** GET /api/learner/notifications/unread-count
    /// </remarks>
    /// <response code="200">Returns unread count</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("unread-count")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get unread notifications count", OperationId = "Learner_GetUnreadNotificationsCount")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <remarks>
    /// Đánh dấu một thông báo cụ thể là đã đọc.
    /// 
    /// **METHOD and path:** POST /api/learner/notifications/{id}/read
    /// 
    /// **URL Parameters:**
    /// - `id`: UserNotification ID
    /// </remarks>
    /// <response code="200">Notification marked as read</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id:guid}/read")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Mark notification as read", OperationId = "Learner_MarkNotificationAsRead")]
    public async Task<IActionResult> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id), cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    /// <remarks>
    /// Đánh dấu tất cả thông báo của user đang đăng nhập là đã đọc.
    /// 
    /// **METHOD and path:** POST /api/learner/notifications/read-all
    /// </remarks>
    /// <response code="200">All notifications marked as read</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("read-all")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Mark all notifications as read", OperationId = "Learner_MarkAllNotificationsAsRead")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new MarkAllAsReadCommand(), cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

