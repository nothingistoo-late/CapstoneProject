using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Features.Maps.Commands.ApproveMap;
using CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;
using CapstoneProject.Application.Features.Maps.Commands.BatchPublishMaps;
using CapstoneProject.Application.Features.Maps.Commands.BatchRejectMaps;
using CapstoneProject.Application.Features.Maps.Commands.CreateMap;
using CapstoneProject.Application.Features.Maps.Commands.CreateTag;
using CapstoneProject.Application.Features.Maps.Commands.DeleteMap;
using CapstoneProject.Application.Features.Maps.Commands.DeleteTag;
using CapstoneProject.Application.Features.Maps.Commands.PublishMap;
using CapstoneProject.Application.Features.Maps.Commands.RejectMap;
using CapstoneProject.Application.Features.Maps.Commands.UpdateTag;
using CapstoneProject.Application.Features.Maps.Queries.GetMapById;
using CapstoneProject.Application.Features.Maps.Queries.GetMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetTags;
using CapstoneProject.Application.Common.Enums;
using System.Text.Json;
using BatchMapResultDto = CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps.BatchMapResultDto;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/maps")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Maps")]
[SwaggerTag("CMS - Moderate maps, CRUD tags")]
public class CmsMapController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsMapController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get maps for moderation (paginated, filter).</summary>
    /// <remarks>
    /// Returns paginated challenge maps for moderation. Filter by mapStatus, difficulty, search, etc. Admin/Moderator only.
    ///
    /// **Query:** Same as Learner GetMaps (pageNumber, pageSize, publishedOnly, difficulty, conceptId, tagId, mapStatus, search, createdByUserId, sortBy, sortAscending). mapStatus: 0=Draft, 1=PendingReview, 2=Approved, 3=Rejected, 4=Published.
    ///
    /// **METHOD and path:** GET /api/cms/maps
    ///
    /// **Example request:** GET /api/cms/maps?pageNumber=1&amp;pageSize=20&amp;mapStatus=1
    /// </remarks>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get maps (moderation)", Description = "Returns paginated challenge maps for moderation. Filter by mapStatus, difficulty, search, etc. Admin/Moderator only.", OperationId = "Cms_GetMaps", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** GET /api/cms/maps/{id}
    ///
    /// **Example request:** GET /api/cms/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpGet("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get map by ID", Description = "Returns full map detail for moderation. includeEditorialForUser optional. Admin/Moderator only.", OperationId = "Cms_GetMapById", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> GetMapById(Guid id, [FromQuery] bool includeEditorialForUser = false)
    {
        var result = await _mediator.Send(new GetMapByIdQuery(id, includeEditorialForUser));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Create and publish map immediately (Admin only).</summary>
    /// <remarks>
    /// Creates a map and publishes it directly without approval workflow.
    ///
    /// **METHOD and path:** POST /api/cms/maps
    /// </remarks>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create and publish map", Description = "Admin creates a map and publishes it immediately (no approval required).", OperationId = "Cms_CreateAndPublishMap", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> CreateAndPublishMap([FromBody] CreateMapRequest request)
    {
        var result = await _mediator.Send(new CreateMapCommand(request, true));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Create and publish map from uploaded JSON file (Admin only).</summary>
    [HttpPost("upload-json")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create and publish map from JSON file", Description = "Admin uploads a JSON file and publishes map immediately.", OperationId = "Cms_CreateAndPublishMapFromJsonFile", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> CreateAndPublishMapFromJsonFile([FromForm] CreateMapFromJsonFileRequest request)
    {
        if (request.MapDetailFile == null || request.MapDetailFile.Length == 0)
            return BadRequest(Result<Guid>.Failure("MapDetailFile is required.", ErrorCodeEnum.ValidationFailed));

        string jsonContent;
        using (var reader = new StreamReader(request.MapDetailFile.OpenReadStream()))
        {
            jsonContent = await reader.ReadToEndAsync();
        }

        JsonElement detailJson;
        try
        {
            detailJson = JsonSerializer.Deserialize<JsonElement>(jsonContent);
        }
        catch (JsonException)
        {
            return BadRequest(Result<Guid>.Failure("Uploaded file is not valid JSON.", ErrorCodeEnum.ValidationFailed));
        }

        List<HintItemDto> hints = new();
        if (!string.IsNullOrWhiteSpace(request.HintsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(request.HintsJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    hints = JsonSerializer.Deserialize<List<HintItemDto>>(request.HintsJson) ?? new List<HintItemDto>();
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var one = JsonSerializer.Deserialize<HintItemDto>(request.HintsJson);
                    if (one != null) hints.Add(one);
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.String)
                {
                    var inner = doc.RootElement.GetString();
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        using var innerDoc = JsonDocument.Parse(inner);
                        if (innerDoc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            hints = JsonSerializer.Deserialize<List<HintItemDto>>(inner) ?? new List<HintItemDto>();
                        }
                        else if (innerDoc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            var one = JsonSerializer.Deserialize<HintItemDto>(inner);
                            if (one != null) hints.Add(one);
                        }
                        else
                        {
                            return BadRequest(Result<Guid>.Failure("HintsJson must be a JSON array or object.", ErrorCodeEnum.ValidationFailed));
                        }
                    }
                }
                else
                {
                    return BadRequest(Result<Guid>.Failure("HintsJson must be a JSON array or object.", ErrorCodeEnum.ValidationFailed));
                }
            }
            catch (JsonException)
            {
                return BadRequest(Result<Guid>.Failure("HintsJson must be valid JSON.", ErrorCodeEnum.ValidationFailed));
            }
        }

        List<Guid> tagIds = new();
        if (!string.IsNullOrWhiteSpace(request.TagIdsCsv))
        {
            var tokens = request.TagIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var token in tokens)
            {
                if (!Guid.TryParse(token, out var tagId))
                    return BadRequest(Result<Guid>.Failure($"Invalid TagId: {token}", ErrorCodeEnum.ValidationFailed));
                tagIds.Add(tagId);
            }
        }

        var cmd = new CreateMapCommand(new CreateMapRequest
        {
            Title = request.Title,
            Description = request.Description,
            Difficulty = request.Difficulty,
            TimeLimitMs = request.TimeLimitMs,
            WinCondition = request.WinCondition,
            Price = request.Price,
            TagIds = tagIds,
            Hints = hints,
            MapDetailJson = detailJson
        }, true);

        var result = await _mediator.Send(cmd);
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
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
    /// **METHOD and path:** POST /api/cms/maps/{id}/approve
    ///
    /// **Example request:** POST /api/cms/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/approve?reviewNote=Approved
    /// </remarks>
    [HttpPost("{id:guid}/approve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Approve map", Description = "Marks map as Approved (from PendingReview). Optional query: reviewNote. Admin/Moderator only.", OperationId = "Cms_ApproveMap", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** POST /api/cms/maps/{id}/reject
    ///
    /// **Example request:** POST /api/cms/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/reject?rejectReason=Incomplete
    /// </remarks>
    [HttpPost("{id:guid}/reject")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Reject map", Description = "Marks map as Rejected. Optional query: rejectReason. Admin/Moderator only.", OperationId = "Cms_RejectMap", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** POST /api/cms/maps/{id}/publish
    ///
    /// **Example request:** POST /api/cms/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/publish
    /// </remarks>
    [HttpPost("{id:guid}/publish")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Publish map", Description = "Publishes an Approved map so it appears in learner catalog. Admin/Moderator only.", OperationId = "Cms_PublishMap", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** DELETE /api/cms/maps/{id}
    ///
    /// **Example request:** DELETE /api/cms/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete map", Description = "Soft-deletes a challenge map. Admin/Moderator only.", OperationId = "Cms_DeleteMap", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** POST /api/cms/maps/batch/approve
    ///
    /// **Example request body:** { "mapIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "reviewNote": "Approved" }
    /// </remarks>
    [HttpPost("batch/approve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchMapResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch approve maps", Description = "Approves multiple maps. Body: mapIds, optional reviewNote. Returns successCount, failedCount, notFoundIds, invalidStatusIds.", OperationId = "Cms_BatchApproveMaps", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** POST /api/cms/maps/batch/reject
    ///
    /// **Example request body:** { "mapIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "rejectReason": "Quality" }
    /// </remarks>
    [HttpPost("batch/reject")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchMapResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch reject maps", Description = "Rejects multiple maps. Body: mapIds, optional rejectReason. Returns successCount, failedCount, notFoundIds, invalidStatusIds.", OperationId = "Cms_BatchRejectMaps", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** POST /api/cms/maps/batch/publish
    ///
    /// **Example request body:** { "mapIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ] }
    /// </remarks>
    [HttpPost("batch/publish")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchMapResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch publish maps", Description = "Publishes multiple Approved maps. Body: mapIds. Returns successCount, failedCount, notFoundIds, invalidStatusIds.", OperationId = "Cms_BatchPublishMaps", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** GET /api/cms/maps/tags
    ///
    /// **Example request:** GET /api/cms/maps/tags?search=logic
    /// </remarks>
    [HttpGet("tags")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get tags", Description = "Returns all tags with optional search. Admin/Moderator only.", OperationId = "Cms_GetTags", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** POST /api/cms/maps/tags
    ///
    /// **Example request body:** { "name": "Logic" }
    /// </remarks>
    [HttpPost("tags")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create tag", Description = "Creates a new tag. Body: { name }. Returns tag Id. Admin/Moderator only.", OperationId = "Cms_CreateTag", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** PUT /api/cms/maps/tags/{id}
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
    [SwaggerOperation(Summary = "Update tag", Description = "Updates tag name by Id. Body: { name }. Admin/Moderator only.", OperationId = "Cms_UpdateTag", Tags = new[] { "CMS - Maps" })]
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
    /// **METHOD and path:** DELETE /api/cms/maps/tags/{id}
    ///
    /// **Example request:** DELETE /api/cms/maps/tags/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    [HttpDelete("tags/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete tag", Description = "Deletes a tag by Id. Admin/Moderator only.", OperationId = "Cms_DeleteTag", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var result = await _mediator.Send(new DeleteTagCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

}

public class CmsCreateTagBody { public string Name { get; set; } = string.Empty; }
public class CmsUpdateTagBody { public string Name { get; set; } = string.Empty; }
public class CreateMapFromJsonFileRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    public decimal? Price { get; set; }
    public string HintsJson { get; set; } = "[]";
    public string TagIdsCsv { get; set; } = string.Empty;
    public IFormFile MapDetailFile { get; set; } = null!;
}
