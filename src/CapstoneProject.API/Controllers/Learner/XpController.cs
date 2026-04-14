using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Leaderboards.Queries.GetTopLevelLeaderboard;
using CapstoneProject.Application.Features.Leaderboards.Queries.GetXpGainLeaderboard;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpHistory;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpProfile;
using CapstoneProject.Application.Features.Xp.Queries.GetXpLeaderboard;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/xp")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - XP")]
[SwaggerTag("Learner - XP profile, history, leaderboard")]
public class LearnerXpController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerXpController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get my XP profile (current XP/level and next level progress).</summary>
    /// <remarks>
    /// Returns XP profile for current learner including currentXp, currentLevel, nextLevel, xpToNextLevel and progressPercent.
    ///
    /// **METHOD and path:** GET /api/learner/xp/profile
    /// </remarks>
    [HttpGet("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MyXpProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "My XP profile", Description = "Get current XP/level and progress to next level for the logged-in learner.", OperationId = "Learner_GetMyXpProfile", Tags = new[] { "Learner - XP" })]
    public async Task<IActionResult> GetMyXpProfile()
    {
        var result = await _mediator.Send(new GetMyXpProfileQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get my XP history (paginated).</summary>
    /// <remarks>
    /// Returns XP transactions for the current learner. Supports filtering by sourceType and date range.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Default 1
    /// - pageSize (int, optional): Default 20, max 100
    /// - sourceType (XpSourceTypeEnum?, optional)
    /// - dateFrom, dateTo (DateTime?, optional)
    ///
    /// **METHOD and path:** GET /api/learner/xp/history
    /// </remarks>
    [HttpGet("history")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<XpHistoryItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "My XP history", Description = "Get paginated XP transactions for current learner with source/date filters.", OperationId = "Learner_GetMyXpHistory", Tags = new[] { "Learner - XP" })]
    public async Task<IActionResult> GetMyXpHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] XpSourceTypeEnum? sourceType = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
    {
        var result = await _mediator.Send(new GetMyXpHistoryQuery(pageNumber, pageSize, sourceType, dateFrom, dateTo));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get XP leaderboard (paginated).</summary>
    /// <remarks>
    /// Returns ranked users by CurrentXp and CurrentLevel. Supports pagination for FE leaderboard screens.
    ///
    /// **METHOD and path:** GET /api/learner/xp/leaderboard
    /// </remarks>
    [HttpGet("leaderboard")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<XpLeaderboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "XP leaderboard", Description = "Get paginated XP leaderboard ranked by total XP.", OperationId = "Learner_GetXpLeaderboard", Tags = new[] { "Learner - XP" })]
    public async Task<IActionResult> GetXpLeaderboard([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetXpLeaderboardQuery(pageNumber, pageSize));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get Top Level leaderboard (paginated).</summary>
    /// <remarks>
    /// Returns ranked users by highest current level. Tie-breakers: currentXp desc, joiningAt asc.
    ///
    /// **METHOD and path:** GET /api/learner/xp/leaderboard/top-level
    /// </remarks>
    [HttpGet("leaderboard/top-level")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<TopLevelLeaderboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Top Level leaderboard", Description = "Get paginated leaderboard ranked by current level.", OperationId = "Learner_GetTopLevelLeaderboard", Tags = new[] { "Learner - XP" })]
    public async Task<IActionResult> GetTopLevelLeaderboard([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetTopLevelLeaderboardQuery(pageNumber, pageSize));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get XP gain leaderboard by period (week/month).</summary>
    /// <remarks>
    /// Returns ranked users by XP gained during selected period.
    ///
    /// **METHOD and path:** GET /api/learner/xp/leaderboard/xp-gain
    ///
    /// **Query:**
    /// - periodType (LeaderboardPeriodTypeEnum, optional): Week | Month. Default Week.
    /// - pageNumber, pageSize.
    /// </remarks>
    [HttpGet("leaderboard/xp-gain")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<XpGainLeaderboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "XP gain leaderboard", Description = "Get leaderboard by XP gained in Week/Month period.", OperationId = "Learner_GetXpGainLeaderboard", Tags = new[] { "Learner - XP" })]
    public async Task<IActionResult> GetXpGainLeaderboard(
        [FromQuery] LeaderboardPeriodTypeEnum periodType = LeaderboardPeriodTypeEnum.Week,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetXpGainLeaderboardQuery(periodType, pageNumber, pageSize));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

