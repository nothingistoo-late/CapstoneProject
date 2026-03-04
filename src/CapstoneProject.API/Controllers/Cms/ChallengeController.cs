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

    /// <summary>Get maps for moderation (paginated, filter).</summary>
    /// <remarks>
    /// Returns paginated challenge maps for moderation. Filter by mapStatus, difficulty, search, etc. Admin/Moderator only.
    ///
    /// **Query:** Same as Learner GetMaps (pageNumber, pageSize, publishedOnly, difficulty, conceptId, tagId, mapStatus, search, createdByUserId, sortBy, sortAscending). mapStatus: 0=Draft, 1=PendingReview, 2=Approved, 3=Rejected, 4=Published.
    ///
    /// **METHOD and path:** GET /api/cms/challenges/maps
    ///
    /// **Example request:** GET /api/cms/challenges/maps?pageNumber=1&amp;pageSize=20&amp;mapStatus=1
    /// </remarks>
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

    /// <summary>Get map by ID (for moderation).</summary>
    /// <remarks>
    /// Returns full map detail for moderation. includeEditorialForUser optional. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Query:** includeEditorialForUser (bool, optional). Default false.
    ///
    /// **METHOD and path:** GET /api/cms/challenges/maps/{id}
    ///
    /// **Example request:** GET /api/cms/challenges/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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

    /// <summary>Approve map (PendingReview → Approved).</summary>
    /// <remarks>
    /// Marks map as Approved (from PendingReview). Optional query: reviewNote. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Query:** reviewNote (string, optional).
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/maps/{id}/approve
    ///
    /// **Example request:** POST /api/cms/challenges/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/approve?reviewNote=Approved
    /// </remarks>
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

    /// <summary>Reject map (PendingReview → Rejected).</summary>
    /// <remarks>
    /// Marks map as Rejected. Optional query: rejectReason. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Query:** rejectReason (string, optional).
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/maps/{id}/reject
    ///
    /// **Example request:** POST /api/cms/challenges/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/reject?rejectReason=Incomplete
    /// </remarks>
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

    /// <summary>Publish map (Approved → Published, appears on catalog).</summary>
    /// <remarks>
    /// Publishes an Approved map so it appears in learner catalog. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/maps/{id}/publish
    ///
    /// **Example request:** POST /api/cms/challenges/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/publish
    /// </remarks>
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

    /// <summary>Delete map (soft delete).</summary>
    /// <remarks>
    /// Soft-deletes a challenge map. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/cms/challenges/maps/{id}
    ///
    /// **Example request:** DELETE /api/cms/challenges/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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

    /// <summary>Batch approve maps.</summary>
    /// <remarks>
    /// Approves multiple maps. Returns successCount, failedCount, notFoundIds, invalidStatusIds. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - mapIds (array of Guid, required): Map IDs to approve.
    /// - reviewNote (string, optional): Common review note.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/maps/batch/approve
    ///
    /// **Example request body:** { "mapIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "reviewNote": "Approved" }
    /// </remarks>
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

    /// <summary>Batch reject maps.</summary>
    /// <remarks>
    /// Rejects multiple maps. Returns successCount, failedCount, notFoundIds, invalidStatusIds. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - mapIds (array of Guid, required): Map IDs to reject.
    /// - rejectReason (string, optional): Common reject reason.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/maps/batch/reject
    ///
    /// **Example request body:** { "mapIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "rejectReason": "Quality" }
    /// </remarks>
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

    /// <summary>Batch publish maps.</summary>
    /// <remarks>
    /// Publishes multiple Approved maps. Returns successCount, failedCount, notFoundIds, invalidStatusIds. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - mapIds (array of Guid, required): Map IDs to publish.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/maps/batch/publish
    ///
    /// **Example request body:** { "mapIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ] }
    /// </remarks>
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

    /// <summary>Get tags (CRUD for CMS).</summary>
    /// <remarks>
    /// Returns all tags with optional search. Admin/Moderator only.
    ///
    /// **Query:** search (string, optional).
    ///
    /// **METHOD and path:** GET /api/cms/challenges/tags
    ///
    /// **Example request:** GET /api/cms/challenges/tags?search=logic
    /// </remarks>
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

    /// <summary>Create tag.</summary>
    /// <remarks>
    /// Creates a new tag. Returns tag Id. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - name (string, required): Tag name.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/tags
    ///
    /// **Example request body:** { "name": "Logic" }
    /// </remarks>
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

    /// <summary>Update tag.</summary>
    /// <remarks>
    /// Updates tag name by Id. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Tag ID.
    ///
    /// **Body (JSON):** name (string, required): New tag name.
    ///
    /// **METHOD and path:** PUT /api/cms/challenges/tags/{id}
    ///
    /// **Example request body:** { "name": "Logic Updated" }
    /// </remarks>
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

    /// <summary>Delete tag.</summary>
    /// <remarks>
    /// Deletes a tag by Id. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Tag ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/cms/challenges/tags/{id}
    ///
    /// **Example request:** DELETE /api/cms/challenges/tags/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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

    /// <summary>Get concepts (CRUD for CMS).</summary>
    /// <remarks>
    /// Returns all concepts with optional search. Admin/Moderator only.
    ///
    /// **Query:** search (string, optional).
    ///
    /// **METHOD and path:** GET /api/cms/challenges/concepts
    ///
    /// **Example request:** GET /api/cms/challenges/concepts?search=loop
    /// </remarks>
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

    /// <summary>Create concept.</summary>
    /// <remarks>
    /// Creates a new concept. Returns concept Id. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - name (string, required): Concept name.
    /// - description (string, optional): Concept description.
    ///
    /// **METHOD and path:** POST /api/cms/challenges/concepts
    ///
    /// **Example request body:** { "name": "Loops", "description": "Loop constructs" }
    /// </remarks>
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

    /// <summary>Update concept.</summary>
    /// <remarks>
    /// Updates concept name/description by Id. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Concept ID.
    ///
    /// **Body (JSON):** name (string, required), description (string, optional).
    ///
    /// **METHOD and path:** PUT /api/cms/challenges/concepts/{id}
    ///
    /// **Example request body:** { "name": "Loops", "description": "Updated" }
    /// </remarks>
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

    /// <summary>Delete concept.</summary>
    /// <remarks>
    /// Deletes a concept by Id. Admin/Moderator only.
    ///
    /// **Route:** id (Guid, required): Concept ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/cms/challenges/concepts/{id}
    ///
    /// **Example request:** DELETE /api/cms/challenges/concepts/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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
