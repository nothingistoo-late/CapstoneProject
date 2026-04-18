using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Features.Gameplay.Commands.UpdateGameSolveScoreConfig;
using CapstoneProject.Application.Features.Gameplay.Queries.GetGameSolveScoreConfig;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/gameplay")]
[ApiExplorerSettings(GroupName = "v1")]
[CapstoneProject.API.Configurations.TagsAttribute("CMS - Gameplay")]
[SwaggerTag("CMS - Game solve scoring configuration")]
public class CmsGameplayController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsGameplayController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("game-solve-score")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<GameSolveScoreConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get game solve score weights", Description = "Base + time/steps/blocks (sum 100) used when validating solutions with engine metrics.", OperationId = "Cms_GetGameSolveScoreConfig", Tags = new[] { "CMS - Gameplay" })]
    public async Task<IActionResult> GetGameSolveScoreConfig()
    {
        var result = await _mediator.Send(new GetGameSolveScoreConfigQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpPut("game-solve-score")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Update game solve score weights", Description = "Four integers must sum to 100.", OperationId = "Cms_UpdateGameSolveScoreConfig", Tags = new[] { "CMS - Gameplay" })]
    public async Task<IActionResult> UpdateGameSolveScoreConfig([FromBody] UpdateGameSolveScoreConfigRequest body)
    {
        var result = await _mediator.Send(new UpdateGameSolveScoreConfigCommand(
            body.BaseScore,
            body.TimeScore,
            body.StepsScore,
            body.BlocksScore));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
