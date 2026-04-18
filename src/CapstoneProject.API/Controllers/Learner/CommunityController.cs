using CapstoneProject.Application.Commons.DTOs.Community;
using CapstoneProject.Application.Features.Community.Commands.RateMap;
using CapstoneProject.Application.Features.Community.Commands.ReportMap;
using CapstoneProject.Application.Features.Community.Queries.GetGameRatings;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API cộng đồng dành cho Learner: đánh giá game, báo cáo nội dung.
/// </summary>
[ApiController]
[Route("api/learner/community")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Community")]
[SwaggerTag("Learner - Rate games, report content")]
public class LearnerCommunityController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerCommunityController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Rate game (1–5 stars)
    /// </summary>
    /// <remarks>
    /// Submit or update rating (1–5 stars) and optional comment for a game. Requires Bearer token.
    ///
    /// **Route:** gameId (Guid, required): Game ID to rate.
    ///
    /// **Body (JSON):**
    /// - rating (int, required): Star rating 1–5.
    /// - comment (string, optional): Optional comment text.
    ///
    /// **METHOD and path:** POST /api/learner/community/games/{gameId}/rate
    ///
    /// **Example request body:** { "rating": 5, "comment": "Great game!" }
    /// </remarks>
    /// <response code="200">Rating submitted. Returns message only.</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("games/{gameId:guid}/rate")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Đánh giá game (1–5 sao)", Description = "Submit or update rating (1–5) and optional comment for game. Requires Bearer token.", OperationId = "Learner_RateMap", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> RateMap(Guid gameId, [FromBody] RateMapRequest request)
    {
        var result = await _mediator.Send(new RateMapCommand(gameId, request.Rating, request.Comment));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get all ratings for a game
    /// </summary>
    /// <remarks>
    /// Trả về danh sách rate (đánh giá) cho 1 game, sắp xếp từ mới nhất tới cũ nhất. Yêu cầu Bearer token.
    ///
    /// **Route:** gameId (Guid, required): ID của game.
    ///
    /// **Query:**
    /// - isAuthor (bool, optional): true = chỉ lấy những rate của chính user hiện tại; false hoặc không truyền = lấy tất cả rate của game.
    ///
    /// **METHOD and path:** GET /api/learner/community/games/{gameId}/ratings
    ///
    /// **Response item fields (GameRatingDto):**
    /// - id, userId, gameId, rating, comment, createdAt
    /// - isAuthor (bool): true nếu rate này thuộc về user hiện tại; có thể dùng để lọc các rate của chính mình.
    /// </remarks>
    /// <response code="200">Danh sách rate của game.</response>
    /// <response code="401">Không được phép (chưa đăng nhập).</response>
    /// <response code="404">Không tìm thấy game.</response>
    [HttpGet("games/{gameId:guid}/ratings")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<GameRatingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<GameRatingDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<List<GameRatingDto>>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Danh sách rate của game", Description = "Get ratings for a game, ordered by CreatedAt desc. Query isAuthor=true để chỉ lấy các rate của chính mình.", OperationId = "Learner_GetGameRatings", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> GetGameRatings(Guid gameId, [FromQuery] bool isAuthor = false)
    {
        var result = await _mediator.Send(new GetGameRatingsQuery(gameId, isAuthor));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Report game (inappropriate content)
    /// </summary>
    /// <remarks>
    /// Submit a report for inappropriate content on a game. Processed by Admin/Moderator in CMS. Requires Bearer token.
    ///
    /// **Route:** gameId (Guid, required): Game ID to report.
    ///
    /// **Body (JSON):**
    /// - reason (string, required): Report reason/category.
    /// - details (string, optional): Additional details.
    ///
    /// **METHOD and path:** POST /api/learner/community/games/{gameId}/report
    ///
    /// **Example request body:** { "reason": "Inappropriate content", "details": "Description of issue" }
    /// </remarks>
    /// <response code="201">Report created. Returns message and data (reportId).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("games/{gameId:guid}/report")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Báo cáo game", Description = "Submit report for inappropriate content. Returns reportId. Processed in CMS. Requires Bearer token.", OperationId = "Learner_ReportMap", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> ReportMap(Guid gameId, [FromBody] ReportMapRequest request)
    {
        var result = await _mediator.Send(new ReportMapCommand(gameId, request.Reason, request.Details));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
