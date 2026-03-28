using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Features.Gameplay.Commands.UpdateMapSolveScoreConfig;
using CapstoneProject.Application.Features.Gameplay.Queries.GetMapSolveScoreConfig;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/gameplay")]
[ApiExplorerSettings(GroupName = "v1")]
[CapstoneProject.API.Configurations.TagsAttribute("CMS - Gameplay")]
[SwaggerTag("CMS - Map solve scoring configuration")]
public class CmsGameplayController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsGameplayController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("map-solve-score")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapSolveScoreConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get map solve score weights", Description = "Base + time/steps/blocks (sum 100) used when validating solutions with engine metrics.", OperationId = "Cms_GetMapSolveScoreConfig", Tags = new[] { "CMS - Gameplay" })]
    public async Task<IActionResult> GetMapSolveScoreConfig()
    {
        var result = await _mediator.Send(new GetMapSolveScoreConfigQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpPut("map-solve-score")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Update map solve score weights", Description = "Four integers must sum to 100.", OperationId = "Cms_UpdateMapSolveScoreConfig", Tags = new[] { "CMS - Gameplay" })]
    public async Task<IActionResult> UpdateMapSolveScoreConfig([FromBody] UpdateMapSolveScoreConfigRequest body)
    {
        var result = await _mediator.Send(new UpdateMapSolveScoreConfigCommand(
            body.BaseScore,
            body.TimeScore,
            body.StepsScore,
            body.BlocksScore));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
