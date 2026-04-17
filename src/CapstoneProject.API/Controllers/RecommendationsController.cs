using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Recommendations.DTOs;
using CapstoneProject.Application.Features.Recommendations.Queries.GetRecommendations;

namespace CapstoneProject.API.Controllers;

[ApiController]
[Route("api/recommendations")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Recommendations")]
[SwaggerTag("Recommendation system: review games + suggested practice")]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecommendationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get game recommendations for current user.</summary>
    /// <remarks>
    /// Returns:
    /// - recommendedMaps: top scored games (rule-based MVP + optional scoring)
    /// - reviewMaps: games with failures >= 3
    /// - suggestedPracticeMaps: games matching user's weakest concept
    /// - nextConcept: the concept after the last completed one in current learning path
    /// </remarks>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<RecommendationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<RecommendationResultDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<RecommendationResultDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get recommendations",
        Description = "Returns recommended games + review/suggested practice lists.",
        OperationId = "Learner_GetRecommendations",
        Tags = new[] { "Learner - Recommendations" })]
    public async Task<IActionResult> GetRecommendations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRecommendationsQuery(), cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

