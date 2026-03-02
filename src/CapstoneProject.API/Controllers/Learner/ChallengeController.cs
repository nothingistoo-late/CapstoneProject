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

    /// <summary>Lấy danh sách map thử thách (catalog, phân trang, filter).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Danh sách map (catalog)", Description = "Trả về danh sách map thử thách có phân trang. Query: pageNumber, pageSize, publishedOnly=true (chỉ map đã xuất bản), difficulty, conceptId, tagId, search, sortBy. Dùng cho trang catalog.", OperationId = "Learner_GetMaps", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetMaps([FromQuery] GetMapsQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xem chi tiết một map (spec, hints, editorial nếu đủ sao).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Chi tiết map theo ID", Description = "Trả về thông tin chi tiết map (spec, hints, constraints). Query: includeEditorialForUser=true để lấy editorial khi user đạt đủ sao (UnlockEditorialAfterStars).", OperationId = "Learner_GetMapById", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetMapById(Guid id, [FromQuery] bool includeEditorialForUser = false)
    {
        var result = await _mediator.Send(new GetMapByIdQuery(id, includeEditorialForUser));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo map mới (trạng thái nháp).</summary>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Tạo map (nháp)", Description = "Tạo map thử thách mới ở trạng thái Draft. Body: CreateMapRequest (title, difficulty, specs, hints, tagIds, conceptIds...). Trả về mapId. Sau đó có thể Update rồi Submit để duyệt.", OperationId = "Learner_CreateMap", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> CreateMap([FromBody] CreateMapRequest request)
    {
        var result = await _mediator.Send(new CreateMapCommand(request));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update map", Description = "Updates a draft map. Author or Admin/Moderator only.", OperationId = "Learner_UpdateMap", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> UpdateMap(Guid id, [FromBody] UpdateMapRequest request)
    {
        var result = await _mediator.Send(new UpdateMapCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpPost("{id:guid}/submit")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Submit map for review", Description = "Submits draft map for moderator review. Author only.", OperationId = "Learner_SubmitMapForReview", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> SubmitMapForReview(Guid id)
    {
        var result = await _mediator.Send(new SubmitMapForReviewCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa map (soft delete; author hoặc Admin/Moderator).</summary>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Xóa map", Description = "Xóa mềm map. Chỉ tác giả hoặc Admin/Moderator. Map bị đánh dấu IsDeleted.", OperationId = "Learner_DeleteMap", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> DeleteMap(Guid id)
    {
        var result = await _mediator.Send(new DeleteMapCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Lấy danh sách tag (chỉ đọc, dùng cho dropdown tạo/sửa map).</summary>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Danh sách tag", Description = "Trả về tất cả tag. Query: search (tùy chọn). Dùng khi tạo/sửa map để chọn tagIds. Chỉ đọc.", OperationId = "Learner_GetTags", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetTags([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetTagsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Lấy danh sách concept (chỉ đọc, dùng cho dropdown tạo/sửa map).</summary>
    [HttpGet("concepts")]
    [ProducesResponseType(typeof(Result<List<ConceptDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Danh sách concept", Description = "Trả về tất cả concept. Query: search (tùy chọn). Dùng khi tạo/sửa map để chọn conceptIds. Chỉ đọc.", OperationId = "Learner_GetConcepts", Tags = new[] { "Learner - Challenges" })]
    public async Task<IActionResult> GetConcepts([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetConceptsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
