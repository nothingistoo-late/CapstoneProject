using CapstoneProject.API.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Features.Maps.Commands.ApproveMap;
using CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;
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
using CapstoneProject.Application.Features.Maps.Commands.UploadMapAvatar;
using CapstoneProject.Application.Features.Maps.Queries.GetMapById;
using CapstoneProject.Application.Features.Maps.Queries.GetAllMapsForAdmin;
using CapstoneProject.Application.Features.Maps.Queries.GetMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetTags;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Domain.Enums;
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
    /// Returns paginated challenge maps for moderation. Admin/Moderator only.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Default 1.
    /// - pageSize (int, optional): Default 20.
    /// - mapStatus (int?, optional): 0=Draft, 1=PendingReview, 2=Approved, 3=Rejected, 4=Published.
    /// - publishedOnly (bool?, optional): true = only published; ignored when mapStatus is set.
    /// - createdByUserId (Guid?, optional): Lọc theo user tạo map.
    /// - difficulty (int?, optional): Difficulty level (1-5).
    /// - tagId (Guid?, optional): Lọc theo tag.
    /// - search (string, optional): Tìm trong title, description.
    /// - minPrice (decimal?, optional): Chỉ map có giá &gt;= minPrice (null/0 = free).
    /// - maxPrice (decimal?, optional): Chỉ map có giá &lt;= maxPrice.
    /// - sortBy (string, optional): CreatedAt | Title | Difficulty | TimeLimitMs | Price. Default CreatedAt.
    /// - sortAscending (bool, optional): Default false.
    ///
    /// **METHOD and path:** GET /api/cms/maps
    ///
    /// **Example:** GET /api/cms/maps?pageNumber=1&amp;pageSize=20&amp;mapStatus=1&amp;createdByUserId=...&amp;minPrice=0&amp;maxPrice=100&amp;sortBy=Price
    /// </remarks>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get maps (moderation)", Description = "Returns paginated maps. Filter by user (createdByUserId), price (minPrice, maxPrice), mapStatus, difficulty, tagId, search; sort by CreatedAt, Title, Difficulty, TimeLimitMs, Price. Admin/Moderator only.", OperationId = "Cms_GetMaps", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> GetMaps([FromQuery] GetMapsQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get all maps (no filter) for Admin.</summary>
    /// <remarks>
    /// Trả về tất cả map, không lọc theo status hay điều kiện nào. Chỉ phân trang và sắp xếp. Admin/Moderator only.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Default 1.
    /// - pageSize (int, optional): Default 20.
    /// - sortBy (string, optional): CreatedAt | Title | Difficulty | TimeLimitMs | Price | MapStatus. Default CreatedAt.
    /// - sortAscending (bool, optional): Default false.
    ///
    /// **METHOD and path:** GET /api/cms/maps/all
    /// </remarks>
    [HttpGet("all")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get all maps (no filter)", Description = "Returns all maps without any filter (status, user, price, etc.). Pagination and sort only. Admin/Moderator only.", OperationId = "Cms_GetAllMaps", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> GetAllMaps([FromQuery] GetAllMapsForAdminQuery query)
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
    /// Tạo map và publish ngay, không qua quy trình duyệt. Chỉ Admin. Body JSON giống Learner CreateMap (title, description, difficulty, timeLimitMs, winCondition, price?, mapDetailJson, hints?, tagIds?).
    ///
    /// **METHOD and path:** POST /api/cms/maps
    ///
    /// **Body (JSON):**
    /// - title (string, required), description (string, required), difficulty (int), timeLimitMs (int), winCondition (int).
    /// - type (int, optional): 0 = Topdown, 1 = Platform. Mặc định 0 (Topdown).
    /// - price (decimal?, optional), mapDetailJson (object, required), hints (array, optional), tagIds (array of Guid, optional).
    ///
    /// **Example request body:** { "title": "Official Map", "description": "Desc", "difficulty": 1, "timeLimitMs": 60000, "winCondition": 10, "mapDetailJson": { "id": "level-1", "layers": {} }, "hints": [], "tagIds": [] }
    /// </remarks>
    /// <response code="201">Map created and published. Returns message and data (mapId).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Admin only</response>
    /// <response code="500">Internal server error</response>
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
    /// <remarks>
    /// Admin tạo map từ file JSON và publish ngay (không qua duyệt). Form giống Learner upload-json: title, description, difficulty, timeLimitMs, winCondition, price?, hintsJson?, tagIdsCsv?, mapDetailFile (file JSON, required).
    ///
    /// **METHOD and path:** POST /api/cms/maps/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title, description, difficulty, timeLimitMs, winCondition (required); price, hintsJson, tagIdsCsv (optional); mapDetailFile (file, required).
    /// </remarks>
    /// <response code="201">Map created and published. Returns message and data (mapId).</response>
    /// <response code="400">Validation error or MapDetailFile is required</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Admin only</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("upload-json")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Create and publish map from JSON file", Description = "Admin uploads a JSON file and publishes map immediately. Form: title, description, difficulty, type? (Topdown|Platform), timeLimitMs, winCondition, price?, hintsJson?, tagIdsCsv?, mapDetailFile (required), avatarFile? (optional).", OperationId = "Cms_CreateAndPublishMapFromJsonFile", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> CreateAndPublishMapFromJsonFile([FromForm] CreateMapFromJsonFileRequest request)
    {
        if (request.MapDetailFile == null || request.MapDetailFile.Length == 0)
            return BadRequest(Result<Guid>.Failure("MapDetailFile is required.", ErrorCodeEnum.ValidationFailed));

        string jsonContent;
        using (var reader = new StreamReader(request.MapDetailFile.OpenReadStream()))
        {
            jsonContent = await reader.ReadToEndAsync();
        }

        var input = new CreateMapFromJsonFileInput
        {
            Title = request.Title,
            Description = request.Description,
            Difficulty = request.Difficulty,
            Type = ParseMapType(request.Type),
            TimeLimitMs = request.TimeLimitMs,
            WinCondition = request.WinCondition,
            Price = request.Price,
            HintsJson = request.HintsJson ?? "[]",
            TagIdsCsv = request.TagIdsCsv ?? string.Empty,
            MapDetailJsonContent = jsonContent
        };

        var result = await _mediator.Send(new CreateMapFromJsonFileCommand(input, AutoPublish: true, request.AvatarFile));
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

    /// <summary>Upload map avatar (Cloudinary).</summary>
    /// <remarks>Admin/Moderator. Form: avatar (file). POST /api/cms/maps/{id}/avatar</remarks>
    [HttpPost("{id:guid}/avatar")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Upload map avatar", Description = "Upload map avatar image to Cloudinary. Admin/Moderator only. Form: avatar (file).", OperationId = "Cms_UploadMapAvatar", Tags = new[] { "CMS - Maps" })]
    public async Task<IActionResult> UploadMapAvatar(Guid id, IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
            return BadRequest(Result<string>.Failure("Avatar file is required.", ErrorCodeEnum.ValidationFailed));
        var result = await _mediator.Send(new UploadMapAvatarCommand(id, avatar));
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
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        var result = await _mediator.Send(new CreateTagCommand(request.Name));
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
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagRequest request)
    {
        var result = await _mediator.Send(new UpdateTagCommand(id, request.Name));
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

    private static MapTypeEnum? ParseMapType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return null;
        return string.Equals(type.Trim(), "Platform", StringComparison.OrdinalIgnoreCase) ? MapTypeEnum.Platform : MapTypeEnum.Topdown;
    }
}
