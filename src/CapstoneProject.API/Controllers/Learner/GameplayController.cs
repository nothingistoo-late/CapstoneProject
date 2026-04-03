using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;
using CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;
using CapstoneProject.Application.Features.Gameplay.Queries.GetMyMapPlayHistory;
using CapstoneProject.Application.Features.Gameplay.Queries.GetProgressDashboard;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/gameplay")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Gameplay")]
[SwaggerTag("Learner - Solution validation, hints, progress dashboard")]
public class LearnerGameplayController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerGameplayController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Validate solution
    /// </summary>
    /// <remarks>
    /// Submits solution (mapId, astSpec or bytecodeSpec, language). Returns Accepted/Rejected, stars, XP. Trial attempts still record progress, but do not grant XP. Creates Submission and updates UserMapResult. Requires Bearer token.
    ///
    /// **Body (JSON):**
    /// - mapId (Guid, required): Challenge map ID.
    /// - language (string, optional): Solution language. Default "Blockly".
    /// - astSpec (string, optional): AST specification (JSON). Use either astSpec or bytecodeSpec.
    /// - bytecodeSpec (string, optional): Bytecode specification. Use either astSpec or bytecodeSpec.
    /// - playMode (string, optional): Single | Lobby | Competitive — dùng để ghi lịch sử chơi. Mặc định Single.
    /// - roomId (Guid, optional): Room lobby (khi playMode = Lobby).
    /// - matchId (Guid, optional): Match competitive (khi có submit server-side); lưu cả trên Submission.
    ///
    /// **METHOD and path:** POST /api/learner/gameplay/validate
    ///
    /// **Example request body:** { "mapId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "language": "Blockly", "astSpec": "{}" }
    /// </remarks>
    /// <response code="200">Returns message and data (result, stars, XP).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("validate")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ValidateSolutionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ValidateSolutionResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ValidateSolutionResultDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ValidateSolutionResultDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ValidateSolutionResultDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Validate solution", Description = "Submits solution. Returns Accepted/Rejected, stars, XP. Trial attempts still record progress, but do not grant XP. Creates Submission and updates UserMapResult. Requires Bearer token.", OperationId = "Learner_ValidateSolution", Tags = new[] { "Learner - Gameplay" })]
    public async Task<IActionResult> ValidateSolution([FromBody] ValidateSolutionRequest request)
    {
        var result = await _mediator.Send(new ValidateSolutionCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Lịch sử chơi map của tôi (phân trang).</summary>
    /// <remarks>
    /// Trả về các lần validate/submit đã ghi (UserMapPlayHistories), sort theo StartTime mới nhất trước.
    /// Query: pageNumber (default 1), pageSize (default 20, max 100), mapId (optional), playMode (optional: Single|Lobby|Competitive).
    ///
    /// **METHOD and path:** GET /api/learner/gameplay/my-play-history
    /// </remarks>
    [HttpGet("my-play-history")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapPlayHistoryItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapPlayHistoryItemDto>>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "My map play history", Description = "Paginated play history for the current user.", OperationId = "Learner_GetMyMapPlayHistory", Tags = new[] { "Learner - Gameplay" })]
    public async Task<IActionResult> GetMyMapPlayHistory([FromQuery] GetMyMapPlayHistoryQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get hints for map (by level).</summary>
    /// <remarks>
    /// Returns ordered hints (orderNo, content) for the given map. Use after loading map detail.
    ///
    /// **Route:** mapId (Guid, required): Map ID.
    /// **Query (optional):** mapDetailId — chỉ lấy hint của một level.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** GET /api/learner/gameplay/maps/{mapId}/hints
    ///
    /// **Example request:** GET /api/learner/gameplay/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/hints
    /// </remarks>
    [HttpGet("maps/{mapId:guid}/hints")]
    [ProducesResponseType(typeof(Result<List<HintLevelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get hints for map", Description = "Returns hints per level (levelOrder, mapDetailId, orderNo, content). Optional mapDetailId filters to one level.", OperationId = "Learner_GetHintsForMap", Tags = new[] { "Learner - Gameplay" })]
    public async Task<IActionResult> GetHintsForMap(Guid mapId, [FromQuery] Guid? mapDetailId = null)
    {
        var result = await _mediator.Send(new GetHintsForMapQuery(mapId, mapDetailId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get progress dashboard (XP, stars, badges, recent activity).</summary>
    /// <remarks>
    /// Returns totalXp, mapsCompleted, totalStars, badges, conceptsPracticed, recentActivities for the current user. Requires Bearer token.
    ///
    /// **Body:** None. Headers only (Authorization: Bearer &lt;token&gt;).
    ///
    /// **METHOD and path:** GET /api/learner/gameplay/dashboard
    ///
    /// **Example request:** GET /api/learner/gameplay/dashboard
    /// </remarks>
    [HttpGet("dashboard")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProgressDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get progress dashboard", Description = "Returns totalXp, mapsCompleted, totalStars, badges, conceptsPracticed, recentActivities for the current user.", OperationId = "Learner_GetProgressDashboard", Tags = new[] { "Learner - Gameplay" })]
    public async Task<IActionResult> GetProgressDashboard()
    {
        var result = await _mediator.Send(new GetProgressDashboardQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
