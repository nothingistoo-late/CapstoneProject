using CapstoneProject.Application.Features.Notifications.Commands.CreateSystemAnnouncement;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/notifications")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Notifications")]
[SwaggerTag("CMS - System announcements")]
public class CmsNotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsNotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Send system announcement to all active users.
    /// </summary>
    /// <remarks>
    /// Sends one SystemAnnouncement notification to all active users.
    ///
    /// **METHOD and path:** POST /api/cms/notifications/system-announcement
    ///
    /// **Body:**
    /// - title (string, required)
    /// - body (string, required)
    /// - actionUrl (string, optional)
    /// - payloadJson (string, optional)
    /// </remarks>
    [HttpPost("system-announcement")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<CreateSystemAnnouncementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Send system announcement",
        Description = "Admin-only endpoint to send a system announcement to all active users.",
        OperationId = "Cms_SendSystemAnnouncement",
        Tags = new[] { "CMS - Notifications" })]
    public async Task<IActionResult> SendSystemAnnouncement(
        [FromBody] SendSystemAnnouncementRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new CreateSystemAnnouncementCommand(
                request.Title,
                request.Body,
                request.ActionUrl,
                request.PayloadJson),
            cancellationToken);

        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class SendSystemAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? PayloadJson { get; set; }
}
