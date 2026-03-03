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
    /// Trả về danh sách map thử thách có phân trang và bộ lọc. Dùng cho trang catalog Learner. Khi publishedOnly=true chỉ trả về map đã xuất bản.
    ///
    ///     GET /api/learner/challenges
    ///     Query: pageNumber=1, pageSize=10, publishedOnly=true, difficulty=1, conceptId, tagId, search=abc, sortBy=Title, sortAscending=true
    ///
    /// **Query (GetMapsQuery):**
    /// - pageNumber (int, tùy chọn): Trang. Mặc định 1.
    /// - pageSize (int, tùy chọn): Số bản ghi mỗi trang. Mặc định 20.
    /// - publishedOnly (bool?, tùy chọn): true = chỉ map đã xuất bản (catalog), false/null = gồm draft/pending (cho admin/tác giả). Mặc định true.
    /// - difficulty (int?, tùy chọn): Lọc theo độ khó. Giá trị theo enum Difficulty.
    /// - conceptId (Guid?, tùy chọn): Lọc theo concept.
    /// - tagId (Guid?, tùy chọn): Lọc theo tag.
    /// - search (string, tùy chọn): Tìm trong title và mô tả.
    /// - sortBy (string, tùy chọn): Sắp xếp theo: CreatedAt, Title, Difficulty, TimeLimitMs.
    /// - sortAscending (bool, tùy chọn): true = tăng dần, false = giảm dần. Mặc định false.
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
    /// Trả về chi tiết map (spec, hints, constraints). includeEditorialForUser=true để lấy editorial khi user đạt đủ sao.
    ///
    ///     GET /api/learner/challenges/{id}
    ///     Query: includeEditorialForUser=false
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
    /// Tạo map thử thách mới ở trạng thái Draft. Sau đó có thể Update rồi Submit để duyệt. Yêu cầu Bearer token (Learner/Admin/Moderator).
    ///
    ///     POST /api/learner/challenges
    ///     Body: CreateMapRequest (title, difficulty, specs, hints, tagIds, conceptIds...)
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
    /// Cập nhật map ở trạng thái Draft. Chỉ tác giả hoặc Admin/Moderator. Yêu cầu Bearer token.
    ///
    ///     PUT /api/learner/challenges/{id}
    ///     Body: UpdateMapRequest (title, difficulty, specs, hints, tagIds, conceptIds...)
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
    /// Gửi map Draft lên để moderator duyệt. Chỉ tác giả. Yêu cầu Bearer token.
    ///
    ///     POST /api/learner/challenges/{id}/submit
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
    /// Xóa mềm map. Chỉ tác giả hoặc Admin/Moderator. Yêu cầu Bearer token.
    ///
    ///     DELETE /api/learner/challenges/{id}
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
    /// Trả về danh sách tag (chỉ đọc). Dùng cho dropdown khi tạo/sửa map.
    ///
    ///     GET /api/learner/challenges/tags
    ///     Query: search (optional)
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
    /// Trả về danh sách concept (chỉ đọc). Dùng cho dropdown khi tạo/sửa map.
    ///
    ///     GET /api/learner/challenges/concepts
    ///     Query: search (optional)
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
