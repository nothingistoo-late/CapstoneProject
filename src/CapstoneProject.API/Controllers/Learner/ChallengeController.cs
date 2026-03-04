using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Features.Challenge.Commands.CreateMap;
using CapstoneProject.Application.Features.Challenge.Commands.DeleteMap;
using CapstoneProject.Application.Features.Challenge.Commands.SubmitMapForReview;
using CapstoneProject.Application.Features.Challenge.Commands.UpdateMap;
using CapstoneProject.Application.Features.Challenge.Queries.GetMapById;
using CapstoneProject.Application.Features.Challenge.Queries.GetMaps;
using CapstoneProject.Application.Features.Challenge.Queries.GetConcepts;
using CapstoneProject.Application.Features.Challenge.Queries.GetTags;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API thử thách dành cho Learner: catalog, tạo/sửa map (UGC), gửi duyệt. Tags/Concepts chỉ đọc.
/// </summary>
[ApiController]
[Route("api/learner/challenges")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Challenges")]
[SwaggerTag("Learner - Challenge maps (catalog, create, update, submit), tags & concepts (read-only)")]
public class LearnerChallengeController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerChallengeController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get list of challenge maps (catalog)
    /// </summary>
    /// <remarks>
    /// Returns paginated challenge maps for the learner catalog. Use filters for difficulty, concept, tag, and search. When publishedOnly=true only published maps are returned.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - publishedOnly (bool?, optional): true = only published maps (catalog), false/null = include draft/pending. Default true.
    /// - difficulty (int?, optional): Filter by difficulty (e.g. 0=Easy, 1=Medium, 2=Hard).
    /// - conceptId (Guid?, optional): Filter by concept ID.
    /// - tagId (Guid?, optional): Filter by tag ID.
    /// - search (string, optional): Search in title and description.
    /// - sortBy (string, optional): Sort by: CreatedAt, Title, Difficulty, TimeLimitMs.
    /// - sortAscending (bool, optional): true = ascending, false = descending. Default false.
    ///
    /// **METHOD and path:** GET /api/learner/challenges
    ///
    /// **Example request:** GET /api/learner/challenges?pageNumber=1&amp;pageSize=10&amp;publishedOnly=true&amp;difficulty=1&amp;search=abc&amp;sortBy=Title&amp;sortAscending=true
    /// </remarks>
    /// <response code="200">Returns message and data (paginated list of maps).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách map (catalog)", Description = "Returns paginated challenge maps for catalog. Filter by publishedOnly, difficulty, conceptId, tagId, search, sortBy.", OperationId = "Learner_GetMaps", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetMaps([FromQuery] GetMapsQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get challenge map detail by ID
    /// </summary>
    /// <remarks>
    /// Returns full map detail (spec, hints, constraints). Set includeEditorialForUser=true to get editorial when the user has earned enough stars.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Query:**
    /// - includeEditorialForUser (bool, optional): If true, includes editorial content when user has sufficient stars. Default false.
    ///
    /// **METHOD and path:** GET /api/learner/challenges/{id}
    ///
    /// **Example request:** GET /api/learner/challenges/3fa85f64-5717-4562-b3fc-2c963f66afa6?includeEditorialForUser=false
    /// </remarks>
    /// <response code="200">Returns message and data (map detail).</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Chi tiết map theo ID", Description = "Returns map detail (spec, hints, constraints). Optional includeEditorialForUser for editorial when user has enough stars.", OperationId = "Learner_GetMapById", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetMapById(Guid id, [FromQuery] bool includeEditorialForUser = false)
    {
        var result = await _mediator.Send(new GetMapByIdQuery(id, includeEditorialForUser));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create new challenge map (draft)
    /// </summary>
    /// <remarks>
    /// Creates a new challenge map in Draft status. Then Update and Submit for moderator review. Requires Bearer token (Learner/Admin/Moderator).
    ///
    /// **Body (JSON):**
    /// - title (string, required): Map title.
    /// - description (string, required): Map description.
    /// - difficulty (int, required): Difficulty level (e.g. 0=Easy, 1=Medium, 2=Hard).
    /// - timeLimitMs (int, required): Time limit in milliseconds.
    /// - price (decimal?, optional): Price for paid map; null = free.
    /// - gridSpec (string, required): Grid specification.
    /// - initialStateSpec (string, required): Initial state spec.
    /// - winConditionSpec (string, required): Win condition spec.
    /// - failConditionSpec (string, required): Fail condition spec.
    /// - hints (array of { orderNo: int, content: string }, optional): Ordered hints.
    /// - constraints (array of { type: string, payload: string }, optional): Constraints.
    /// - tagIds (array of Guid, optional): Tag IDs.
    /// - conceptIds (array of Guid, optional): Concept IDs.
    ///
    /// **METHOD and path:** POST /api/learner/challenges
    ///
    /// **Example request body:** { "title": "My Map", "description": "Description", "difficulty": 1, "timeLimitMs": 60000, "gridSpec": "{}", "initialStateSpec": "{}", "winConditionSpec": "{}", "failConditionSpec": "{}", "hints": [], "constraints": [], "tagIds": [], "conceptIds": [] }
    /// </remarks>
    /// <response code="201">Map created. Returns message and data (mapId).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Tạo map (nháp)", Description = "Creates new challenge map as Draft. Returns mapId. Then Update and Submit for review. Requires Bearer token.", OperationId = "Learner_CreateMap", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> CreateMap([FromBody] CreateMapRequest request)
    {
        var result = await _mediator.Send(new CreateMapCommand(request));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update challenge map (draft only)
    /// </summary>
    /// <remarks>
    /// Updates a map in Draft status. Author or Admin/Moderator only. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body (JSON):**
    /// - title (string, required): Map title.
    /// - description (string, required): Map description.
    /// - difficulty (int, required): Difficulty (0=Easy, 1=Medium, 2=Hard).
    /// - timeLimitMs (int, required): Time limit in ms.
    /// - price (decimal?, optional): Price; null = free.
    /// - gridSpec, initialStateSpec, winConditionSpec, failConditionSpec (string, optional): Specs.
    /// - editorialContent (string, optional): Editorial text.
    /// - unlockEditorialAfterStars (int?, optional): Stars required to unlock editorial.
    /// - hints, constraints (arrays, optional): Hint and constraint items.
    /// - tagIds, conceptIds (array of Guid, optional): Tag and concept IDs.
    ///
    /// **METHOD and path:** PUT /api/learner/challenges/{id}
    ///
    /// **Example request body:** { "title": "Updated Map", "description": "Desc", "difficulty": 1, "timeLimitMs": 60000, "tagIds": [], "conceptIds": [] }
    /// </remarks>
    /// <response code="200">Map updated. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author or admin)</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Update map", Description = "Updates a draft map. Author or Admin/Moderator only. Requires Bearer token.", OperationId = "Learner_UpdateMap", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> UpdateMap(Guid id, [FromBody] UpdateMapRequest request)
    {
        var result = await _mediator.Send(new UpdateMapCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Submit map for review
    /// </summary>
    /// <remarks>
    /// Submits a Draft map for moderator review. Author only. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body:** None. Headers only (Authorization: Bearer &lt;token&gt;).
    ///
    /// **METHOD and path:** POST /api/learner/challenges/{id}/submit
    ///
    /// **Example request:** POST /api/learner/challenges/3fa85f64-5717-4562-b3fc-2c963f66afa6/submit
    /// </remarks>
    /// <response code="200">Map submitted for review. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/submit")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Submit map for review", Description = "Submits draft map for moderator review. Author only. Requires Bearer token.", OperationId = "Learner_SubmitMapForReview", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> SubmitMapForReview(Guid id)
    {
        var result = await _mediator.Send(new SubmitMapForReviewCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Delete challenge map (soft delete)
    /// </summary>
    /// <remarks>
    /// Soft-deletes the map. Author or Admin/Moderator only. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/learner/challenges/{id}
    ///
    /// **Example request:** DELETE /api/learner/challenges/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    /// <response code="200">Map deleted. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Xóa map", Description = "Soft-deletes map. Author or Admin/Moderator only. Requires Bearer token.", OperationId = "Learner_DeleteMap", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> DeleteMap(Guid id)
    {
        var result = await _mediator.Send(new DeleteMapCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get list of tags
    /// </summary>
    /// <remarks>
    /// Returns all tags (read-only). Use for dropdown when creating/editing maps.
    ///
    /// **Query:**
    /// - search (string, optional): Filter tags by name.
    ///
    /// **METHOD and path:** GET /api/learner/challenges/tags
    ///
    /// **Example request:** GET /api/learner/challenges/tags?search=logic
    /// </remarks>
    /// <response code="200">Returns message and data (list of tags).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách tag", Description = "Returns all tags. Optional query: search. Read-only, for map create/edit dropdown.", OperationId = "Learner_GetTags", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetTags([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetTagsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get list of concepts
    /// </summary>
    /// <remarks>
    /// Returns all concepts (read-only). Use for dropdown when creating/editing maps.
    ///
    /// **Query:**
    /// - search (string, optional): Filter concepts by name.
    ///
    /// **METHOD and path:** GET /api/learner/challenges/concepts
    ///
    /// **Example request:** GET /api/learner/challenges/concepts?search=loop
    /// </remarks>
    /// <response code="200">Returns message and data (list of concepts).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("concepts")]
    [ProducesResponseType(typeof(Result<List<ConceptDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<ConceptDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách concept", Description = "Returns all concepts. Optional query: search. Read-only, for map create/edit dropdown.", OperationId = "Learner_GetConcepts", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetConcepts([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetConceptsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
