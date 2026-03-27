using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Xp;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Application.Features.Xp.Commands.GrantXpToUser;
using CapstoneProject.Application.Features.Xp.Commands.UpdateXpPolicyConfig;
using CapstoneProject.Application.Features.Xp.Commands.UpdateXpSourceConfig;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpHistory;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpProfile;
using CapstoneProject.Application.Features.Xp.Queries.GetUserXpHistory;
using CapstoneProject.Application.Features.Xp.Queries.GetUserXpProfile;
using CapstoneProject.Application.Features.Xp.Queries.GetXpPolicyConfigs;
using CapstoneProject.Application.Features.Xp.Queries.GetXpSourceConfigs;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/xp")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - XP")]
[SwaggerTag("CMS - Grant XP, user XP audit, XP configuration")]
public class CmsXpController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsXpController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Grant XP to a user manually (Admin/Moderator).</summary>
    /// <remarks>
    /// Grants XP with idempotency key to avoid duplicate reward. Use for support operations, compensations, or testing.
    ///
    /// **METHOD and path:** POST /api/cms/xp/grant
    /// </remarks>
    [HttpPost("grant")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<XpGrantResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Grant XP to user", Description = "Grant XP manually with idempotency key (duplicate-safe).", OperationId = "Cms_GrantXpToUser", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> GrantXp([FromBody] GrantXpRequest request)
    {
        var result = await _mediator.Send(new GrantXpToUserCommand(
            request.UserId,
            request.Amount,
            request.SourceType,
            request.SourceId,
            request.IdempotencyKey,
            request.Reason,
            request.Metadata));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get XP profile of a specific user.</summary>
    /// <remarks>
    /// Returns current XP/level and next-level progress for target user.
    ///
    /// **METHOD and path:** GET /api/cms/xp/users/{userId}
    /// </remarks>
    [HttpGet("users/{userId:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MyXpProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get user XP profile", Description = "Get XP/level profile of a target user for audit/support.", OperationId = "Cms_GetUserXpProfile", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> GetUserXpProfile(Guid userId)
    {
        var result = await _mediator.Send(new GetUserXpProfileQuery(userId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get XP history of a specific user (paginated).</summary>
    /// <remarks>
    /// Returns XP transactions of target user with optional filters by sourceType and date range.
    ///
    /// **METHOD and path:** GET /api/cms/xp/users/{userId}/history
    /// </remarks>
    [HttpGet("users/{userId:guid}/history")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<XpHistoryItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get user XP history", Description = "Get paginated XP ledger of target user for audit.", OperationId = "Cms_GetUserXpHistory", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> GetUserXpHistory(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] XpSourceTypeEnum? sourceType = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
    {
        var result = await _mediator.Send(new GetUserXpHistoryQuery(userId, pageNumber, pageSize, sourceType, dateFrom, dateTo));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>List XP policy configs.</summary>
    [HttpGet("config/policies")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<XpPolicyConfigDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get XP policy configs", Description = "Get runtime policy configuration list (enabled/priority/config).", OperationId = "Cms_GetXpPolicyConfigs", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> GetPolicyConfigs()
    {
        var result = await _mediator.Send(new GetXpPolicyConfigsQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Update XP policy config by key.</summary>
    [HttpPut("config/policies/{policyKey}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Update XP policy config", Description = "Enable/disable or change priority/config JSON for a policy.", OperationId = "Cms_UpdateXpPolicyConfig", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> UpdatePolicyConfig(string policyKey, [FromBody] UpdateXpPolicyConfigRequest request)
    {
        var result = await _mediator.Send(new UpdateXpPolicyConfigCommand(policyKey, request.IsEnabled, request.Priority, request.ConfigJson));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>List XP source configs.</summary>
    [HttpGet("config/sources")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<XpSourceConfigDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get XP source configs", Description = "Get XP source configuration list (baseXp/dailyCap/multiplier).", OperationId = "Cms_GetXpSourceConfigs", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> GetSourceConfigs()
    {
        var result = await _mediator.Send(new GetXpSourceConfigsQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Update XP source config by source type.</summary>
    [HttpPut("config/sources/{sourceType}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Update XP source config", Description = "Enable/disable and adjust base XP/cap/bonus for a source type.", OperationId = "Cms_UpdateXpSourceConfig", Tags = new[] { "CMS - XP" })]
    public async Task<IActionResult> UpdateSourceConfig(XpSourceTypeEnum sourceType, [FromBody] UpdateXpSourceConfigRequest request)
    {
        var result = await _mediator.Send(new UpdateXpSourceConfigCommand(sourceType, request.IsEnabled, request.BaseXp, request.DailyCap, request.BonusMultiplier, request.ConfigJson));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

