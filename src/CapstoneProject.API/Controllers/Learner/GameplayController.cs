using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;
using CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;
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

    /// <summary>Nộp bài và kiểm tra kết quả (AST hoặc bytecode).</summary>
    [HttpPost("validate")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ValidateSolutionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Validate solution", Description = "Submits solution (mapId, astSpec or bytecodeSpec, language). Returns Accepted/Rejected, stars, XP. Creates Submission and updates UserMapResult.", OperationId = "Learner_ValidateSolution", Tags = new[] { "Learner - Gameplay" })]
    public async Task<IActionResult> ValidateSolution([FromBody] ValidateSolutionRequest request)
    {
        var result = await _mediator.Send(new ValidateSolutionCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Lấy danh sách gợi ý theo cấp độ cho map.</summary>
    [HttpGet("maps/{mapId:guid}/hints")]
    [ProducesResponseType(typeof(Result<List<HintLevelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get hints for map", Description = "Returns ordered hints (OrderNo, Content) for the given map. Use after loading map detail.", OperationId = "Learner_GetHintsForMap", Tags = new[] { "Learner - Gameplay" })]
    public async Task<IActionResult> GetHintsForMap(Guid mapId)
    {
        var result = await _mediator.Send(new GetHintsForMapQuery(mapId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Dashboard tiến trình: XP, sao, badge, hoạt động gần đây.</summary>
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
