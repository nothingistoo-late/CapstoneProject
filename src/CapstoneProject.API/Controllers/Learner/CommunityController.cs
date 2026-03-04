using CapstoneProject.Application.Features.Community.Commands.RateMap;
using CapstoneProject.Application.Features.Community.Commands.ReportMap;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API cộng đồng dành cho Learner: đánh giá map, báo cáo nội dung.
/// </summary>
[ApiController]
[Route("api/learner/community")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Community")]
[SwaggerTag("Learner - Rate maps, report content")]
public class LearnerCommunityController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerCommunityController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Rate challenge map (1–5 stars)
    /// </summary>
    /// <remarks>
    /// Submit or update rating (1–5 stars) and optional comment for a map. Requires Bearer token.
    ///
    /// **Route:** mapId (Guid, required): Map ID to rate.
    ///
    /// **Body (JSON):**
    /// - rating (int, required): Star rating 1–5.
    /// - comment (string, optional): Optional comment text.
    ///
    /// **METHOD and path:** POST /api/learner/community/maps/{mapId}/rate
    ///
    /// **Example request body:** { "rating": 5, "comment": "Great map!" }
    /// </remarks>
    /// <response code="200">Rating submitted. Returns message only.</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("maps/{mapId:guid}/rate")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Đánh giá map (1–5 sao)", Description = "Submit or update rating (1–5) and optional comment for map. Requires Bearer token.", OperationId = "Learner_RateMap", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> RateMap(Guid mapId, [FromBody] RateMapRequest request)
    {
        var result = await _mediator.Send(new RateMapCommand(mapId, request.Rating, request.Comment));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Report map (inappropriate content)
    /// </summary>
    /// <remarks>
    /// Submit a report for inappropriate content on a map. Processed by Admin/Moderator in CMS. Requires Bearer token.
    ///
    /// **Route:** mapId (Guid, required): Map ID to report.
    ///
    /// **Body (JSON):**
    /// - reason (string, required): Report reason/category.
    /// - details (string, optional): Additional details.
    ///
    /// **METHOD and path:** POST /api/learner/community/maps/{mapId}/report
    ///
    /// **Example request body:** { "reason": "Inappropriate content", "details": "Description of issue" }
    /// </remarks>
    /// <response code="201">Report created. Returns message and data (reportId).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("maps/{mapId:guid}/report")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Báo cáo map", Description = "Submit report for inappropriate content. Returns reportId. Processed in CMS. Requires Bearer token.", OperationId = "Learner_ReportMap", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> ReportMap(Guid mapId, [FromBody] ReportMapRequest request)
    {
        var result = await _mediator.Send(new ReportMapCommand(mapId, request.Reason, request.Details));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class RateMapRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReportMapRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}
