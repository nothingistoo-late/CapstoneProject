using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Features.Challenge.Commands.ApproveMap;
using CapstoneProject.Application.Features.Challenge.Commands.BatchApproveMaps;
using CapstoneProject.Application.Features.Challenge.Commands.BatchPublishMaps;
using CapstoneProject.Application.Features.Challenge.Commands.BatchRejectMaps;
using CapstoneProject.Application.Features.Challenge.Commands.CreateConcept;
using CapstoneProject.Application.Features.Challenge.Commands.CreateTag;
using CapstoneProject.Application.Features.Challenge.Commands.DeleteMap;
using CapstoneProject.Application.Features.Challenge.Commands.DeleteConcept;
using CapstoneProject.Application.Features.Challenge.Commands.DeleteTag;
using CapstoneProject.Application.Features.Challenge.Commands.PublishMap;
using CapstoneProject.Application.Features.Challenge.Commands.RejectMap;
using CapstoneProject.Application.Features.Challenge.Commands.UpdateConcept;
using CapstoneProject.Application.Features.Challenge.Commands.UpdateTag;
using CapstoneProject.Application.Features.Challenge.Queries.GetMapById;
using CapstoneProject.Application.Features.Challenge.Queries.GetMaps;
using CapstoneProject.Application.Features.Challenge.Queries.GetConcepts;
using CapstoneProject.Application.Features.Challenge.Queries.GetTags;
using BatchMapResultDto = CapstoneProject.Application.Features.Challenge.Commands.BatchApproveMaps.BatchMapResultDto;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/challenges")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Challenges")]
[SwaggerTag("CMS - Moderate maps, CRUD tags & concepts")]
public class CmsChallengeController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsChallengeController(IMediator mediator) => _mediator = mediator;

    /// <summary>Danh sách map để duyệt (phân trang, filter).</summary>
    [HttpGet("maps")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get maps (moderation)", Description = "Returns paginated challenge maps for moderation. Filter by mapStatus, difficulty, search, etc. Admin/Moderator only.", OperationId = "Cms_GetMaps", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> GetMaps([FromQuery] GetMapsQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Chi tiết một map theo ID (dùng khi duyệt).</summary>
    [HttpGet("maps/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get map by ID", Description = "Returns full map detail for moderation. includeEditorialForUser optional. Admin/Moderator only.", OperationId = "Cms_GetMapById", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> GetMapById(Guid id, [FromQuery] bool includeEditorialForUser = false)
    {
        var result = await _mediator.Send(new GetMapByIdQuery(id, includeEditorialForUser));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Duyệt map (PendingReview → Approved).</summary>
    [HttpPost("maps/{id:guid}/approve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Approve map", Description = "Marks map as Approved (from PendingReview). Optional query: reviewNote. Admin/Moderator only.", OperationId = "Cms_ApproveMap", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> ApproveMap(Guid id, [FromQuery] string? reviewNote = null)
    {
        var result = await _mediator.Send(new ApproveMapCommand(id, reviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Từ chối map (PendingReview → Rejected).</summary>
    [HttpPost("maps/{id:guid}/reject")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Reject map", Description = "Marks map as Rejected. Optional query: rejectReason. Admin/Moderator only.", OperationId = "Cms_RejectMap", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> RejectMap(Guid id, [FromQuery] string? rejectReason = null)
    {
        var result = await _mediator.Send(new RejectMapCommand(id, rejectReason));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xuất bản map (Approved → Published, hiện trên catalog).</summary>
    [HttpPost("maps/{id:guid}/publish")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Publish map", Description = "Publishes an Approved map so it appears in learner catalog. Admin/Moderator only.", OperationId = "Cms_PublishMap", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> PublishMap(Guid id)
    {
        var result = await _mediator.Send(new PublishMapCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa map (soft delete).</summary>
    [HttpDelete("maps/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete map", Description = "Soft-deletes a challenge map. Admin/Moderator only.", OperationId = "Cms_DeleteMap", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> DeleteMap(Guid id)
    {
        var result = await _mediator.Send(new DeleteMapCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Duyệt nhiều map cùng lúc.</summary>
    [HttpPost("maps/batch/approve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchMapResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch approve maps", Description = "Approves multiple maps. Body: mapIds, optional reviewNote. Returns successCount, failedCount, notFoundIds, invalidStatusIds.", OperationId = "Cms_BatchApproveMaps", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> BatchApproveMaps([FromBody] BatchApproveMapsRequest request)
    {
        var result = await _mediator.Send(new BatchApproveMapsCommand(request.MapIds, request.ReviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Từ chối nhiều map cùng lúc.</summary>
    [HttpPost("maps/batch/reject")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchMapResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch reject maps", Description = "Rejects multiple maps. Body: mapIds, optional rejectReason. Returns successCount, failedCount, notFoundIds, invalidStatusIds.", OperationId = "Cms_BatchRejectMaps", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> BatchRejectMaps([FromBody] BatchRejectMapsRequest request)
    {
        var result = await _mediator.Send(new BatchRejectMapsCommand(request.MapIds, request.RejectReason));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xuất bản nhiều map cùng lúc.</summary>
    [HttpPost("maps/batch/publish")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchMapResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch publish maps", Description = "Publishes multiple Approved maps. Body: mapIds. Returns successCount, failedCount, notFoundIds, invalidStatusIds.", OperationId = "Cms_BatchPublishMaps", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> BatchPublishMaps([FromBody] BatchPublishMapsRequest request)
    {
        var result = await _mediator.Send(new BatchPublishMapsCommand(request.MapIds));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Danh sách tag (CRUD cho CMS).</summary>
    [HttpGet("tags")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get tags", Description = "Returns all tags with optional search. Admin/Moderator only.", OperationId = "Cms_GetTags", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> GetTags([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetTagsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo tag mới.</summary>
    [HttpPost("tags")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create tag", Description = "Creates a new tag. Body: { name }. Returns tag Id. Admin/Moderator only.", OperationId = "Cms_CreateTag", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> CreateTag([FromBody] CmsCreateTagBody body)
    {
        var result = await _mediator.Send(new CreateTagCommand(body.Name));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Cập nhật tag.</summary>
    [HttpPut("tags/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update tag", Description = "Updates tag name by Id. Body: { name }. Admin/Moderator only.", OperationId = "Cms_UpdateTag", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] CmsUpdateTagBody body)
    {
        var result = await _mediator.Send(new UpdateTagCommand(id, body.Name));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa tag.</summary>
    [HttpDelete("tags/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete tag", Description = "Deletes a tag by Id. Admin/Moderator only.", OperationId = "Cms_DeleteTag", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var result = await _mediator.Send(new DeleteTagCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Danh sách concept (CRUD cho CMS).</summary>
    [HttpGet("concepts")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<ConceptDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get concepts", Description = "Returns all concepts with optional search. Admin/Moderator only.", OperationId = "Cms_GetConcepts", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> GetConcepts([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetConceptsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo concept mới.</summary>
    [HttpPost("concepts")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create concept", Description = "Creates a new concept. Body: { name, description }. Returns concept Id. Admin/Moderator only.", OperationId = "Cms_CreateConcept", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> CreateConcept([FromBody] CmsCreateConceptBody body)
    {
        var result = await _mediator.Send(new CreateConceptCommand(body.Name, body.Description));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Cập nhật concept.</summary>
    [HttpPut("concepts/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update concept", Description = "Updates concept name/description by Id. Body: { name, description }. Admin/Moderator only.", OperationId = "Cms_UpdateConcept", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> UpdateConcept(Guid id, [FromBody] CmsUpdateConceptBody body)
    {
        var result = await _mediator.Send(new UpdateConceptCommand(id, body.Name, body.Description));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa concept.</summary>
    [HttpDelete("concepts/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete concept", Description = "Deletes a concept by Id. Admin/Moderator only.", OperationId = "Cms_DeleteConcept", Tags = new[] { "CMS - Challenges" })]
    public async Task<IActionResult> DeleteConcept(Guid id)
    {
        var result = await _mediator.Send(new DeleteConceptCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class CmsCreateTagBody { public string Name { get; set; } = string.Empty; }
public class CmsUpdateTagBody { public string Name { get; set; } = string.Empty; }
public class CmsCreateConceptBody { public string Name { get; set; } = string.Empty; public string? Description { get; set; } }
public class CmsUpdateConceptBody { public string Name { get; set; } = string.Empty; public string? Description { get; set; } }
