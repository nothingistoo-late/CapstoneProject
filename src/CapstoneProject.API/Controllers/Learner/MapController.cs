using CapstoneProject.API.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Features.Maps.Commands.CreateMap;
using CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;
using CapstoneProject.Application.Features.Maps.Commands.DeleteMap;
using CapstoneProject.Application.Features.Maps.Commands.SubmitMapForReview;
using CapstoneProject.Application.Features.Maps.Commands.UpdateMap;
using CapstoneProject.Application.Features.Maps.Commands.UploadMapAvatar;
using CapstoneProject.Application.Features.Maps.Queries.GetMapById;
using CapstoneProject.Application.Features.Maps.Queries.GetMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetMyMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetTags;
using CapstoneProject.Application.Features.Maps.Commands.UpdateMapFromJsonFile;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API thử thách dành cho Learner: catalog, tạo/sửa map (UGC), gửi duyệt. Tags/Concepts chỉ đọc.
/// </summary>
[ApiController]
[Route("api/learner/maps")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Maps")]
[SwaggerTag("Learner - Maps (catalog, create, update, submit), tags (read-only)")]
public class LearnerMapController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerMapController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get list of challenge maps (catalog)
    /// </summary>
    /// <remarks>
    /// Returns paginated challenge maps for the learner catalog. Use filters for difficulty, type, tag, and search. When publishedOnly=true only published maps are returned.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - publishedOnly (bool?, optional): true = only published maps (catalog). Ignored when mapStatus is set. Default true.
    /// - mapStatus (int?, optional): Filter by map status: 0=Draft, 1=PendingReview, 2=Approved, 3=Rejected, 4=Published. When set, publishedOnly is ignored.
    /// - difficulty (int?, optional): Filter by difficulty (e.g. 0=Easy, 1=Medium, 2=Hard).
    /// - type (int?, optional): Filter by map type: 0=Topdown, 1=Platform.
    /// - tagId (Guid?, optional): Filter by tag ID.
    /// - search (string, optional): Search in title and description.
    /// - sortBy (string, optional): Sort by: CreatedAt, Title, Difficulty, TimeLimitMs.
    /// - sortAscending (bool, optional): true = ascending, false = descending. Default false.
    ///
    /// **METHOD and path:** GET /api/learner/maps
    ///
    /// **Example request:** GET /api/learner/maps?pageNumber=1&amp;pageSize=10&amp;mapStatus=4&amp;difficulty=1&amp;search=abc&amp;sortBy=Title&amp;sortAscending=true
    /// </remarks>
    /// <response code="200">Returns message and data (paginated list of maps).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách map (catalog)", Description = "Returns paginated challenge maps for catalog. Filter by mapStatus (0–4) or publishedOnly, difficulty, tagId, search, sortBy.", OperationId = "Learner_GetMaps", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> GetMaps([FromQuery] GetMapsQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get all maps owned by the current user (created by user + purchased with OrbitCoin)
    /// </summary>
    /// <remarks>
    /// Returns paginated list of maps the user owns: maps they created and maps they purchased. Requires Bearer token.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - sortBy (string, optional): CreatedAt, Title, Difficulty, TimeLimitMs.
    /// - sortAscending (bool, optional): Default false.
    /// - isAuthorOnly (bool, optional): true = chỉ lấy map do chính user tạo; false (mặc định) = bao gồm cả map đã mua.
    ///
    /// **Response item fields (MapListItemDto):**
    /// - id, title, description, difficulty, type, timeLimitMs, isPublished, mapStatus, price, createdByUserId, createdAt, tagNames, winCondition, avatarUrl
    /// - isAuthor (bool): true nếu map do chính user hiện tại tạo (CreatedBy); false nếu chỉ là map đã mua.
    ///
    /// **METHOD and path:** GET /api/learner/maps/my-maps
    /// </remarks>
    /// <response code="200">Returns message and data (paginated list of owned maps).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("my-maps")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Danh sách map của tôi", Description = "Returns maps owned by current user: created by user + purchased with OrbitCoin. Requires Bearer token.", OperationId = "Learner_GetMyMaps", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> GetMyMaps([FromQuery] GetMyMapsQuery query)
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
    /// **METHOD and path:** GET /api/learner/maps/{id}
    ///
    /// **Example request:** GET /api/learner/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6?includeEditorialForUser=false
    /// </remarks>
    /// <response code="200">Returns message and data (map detail).</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<MapDetailDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Chi tiết map theo ID", Description = "Returns map detail (spec, hints, constraints). Optional includeEditorialForUser for editorial when user has enough stars.", OperationId = "Learner_GetMapById", Tags = new[] { "Learner - Maps" })]
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
    /// - winCondition (int, required): Win condition value stored in Map metadata.
    /// - price (decimal?, optional): Price for paid map; null = free.
    /// - mapDetailJson (object, required): Full JSON map detail payload (level/layers/start-goal/objects/metadata...).
    /// - hints (array of { orderNo: int, content: string }, optional): Ordered hints.
    /// - tagIds (array of Guid, optional): Tag IDs.
    /// - avatarUrl (string, optional): URL avatar map (Cloudinary). Hoặc upload sau qua POST /api/learner/maps/{id}/avatar.
    ///
    /// **METHOD and path:** POST /api/learner/maps
    ///
    /// **Body:** type (int, optional): 0 = Topdown, 1 = Platform. Mặc định Topdown.
    /// **Example request body:** { "title": "My Map", "description": "Description", "difficulty": 1, "type": 0, "timeLimitMs": 60000, "winCondition": 10, "mapDetailJson": { "id": "level-1", "layers": {} }, "hints": [], "tagIds": [] }
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
    [SwaggerOperation(Summary = "Tạo map (nháp)", Description = "Creates new challenge map as Draft. Returns mapId. Then Update and Submit for review. Requires Bearer token.", OperationId = "Learner_CreateMap", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> CreateMap([FromBody] CreateMapRequest request)
    {
        var result = await _mediator.Send(new CreateMapCommand(request));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create new map from uploaded JSON file (draft)
    /// </summary>
    /// <remarks>
    /// Tạo map mới từ file JSON (multipart/form-data). Map được tạo ở trạng thái Draft; sau đó cập nhật và gửi duyệt qua Submit. Yêu cầu Bearer token (Learner/Admin/Moderator).
    ///
    /// **METHOD and path:** POST /api/learner/maps/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title (string, required): Tiêu đề map.
    /// - description (string, required): Mô tả.
    /// - difficulty (int, required): Độ khó (0=Easy, 1=Medium, 2=Hard).
    /// - timeLimitMs (int, required): Thời gian giới hạn (ms).
    /// - winCondition (int, required): Điều kiện thắng (metadata).
    /// - price (decimal?, optional): Giá; null = miễn phí.
    /// - hintsJson (string, optional): JSON array hints, mặc định "[]".
    /// - tagIdsCsv (string, optional): Danh sách tag ID cách nhau bằng dấu phẩy.
    /// - mapDetailFile (file, required): File JSON chứa chi tiết map (level/layers/objects...).
    /// - avatarFile (file, optional): Ảnh avatar map; upload lên Cloudinary khi tạo.
    ///
    /// **Example:** Content-Type: multipart/form-data với các field trên và mapDetailFile là file .json.
    /// </remarks>
    /// <response code="201">Map created. Returns message and data (mapId).</response>
    /// <response code="400">Validation error or MapDetailFile is required</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("upload-json")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Tạo map từ file JSON", Description = "Uploads a JSON file; creates map as Draft. Form: title, description, difficulty, type? (Topdown|Platform), timeLimitMs, winCondition, price?, hintsJson?, tagIdsCsv?, mapDetailFile (required). Requires Bearer token.", OperationId = "Learner_CreateMapFromJsonFile", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> CreateMapFromJsonFile([FromForm] CreateMapFromJsonFileRequest request)
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

        var result = await _mediator.Send(new CreateMapFromJsonFileCommand(input, AutoPublish: false, request.AvatarFile));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update challenge map (draft only)
    /// </summary>
    /// <remarks>
    /// Updates a map in Draft status. Chỉ tác giả (Learner) hoặc Admin/Moderator mới được sửa. Sau khi update, map sẽ quay lại trạng thái Draft (MapStatus=Draft, IsPublished=false) và cần submit lại để duyệt. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body (JSON):**
    /// - title (string, required): Map title.
    /// - description (string, required): Map description.
    /// - difficulty (int, required): Difficulty (0=Easy, 1=Medium, 2=Hard).
    /// - timeLimitMs (int, required): Time limit in ms.
    /// - winCondition (int, required): Win condition value stored in Map metadata.
    /// - price (decimal?, optional): Price; null = free.
    /// - mapDetailJson (object, optional): Full JSON map detail payload.
    /// - editorialContent (string, optional): Editorial text.
    /// - unlockEditorialAfterStars (int?, optional): Stars required to unlock editorial.
    /// - hints (array, optional): Hint items.
    /// - tagIds (array of Guid, optional): Tag IDs.
    ///
    /// **METHOD and path:** PUT /api/learner/maps/{id}
    ///
    /// **Example request body:** { "title": "Updated Map", "description": "Desc", "difficulty": 1, "timeLimitMs": 60000, "winCondition": 10, "mapDetailJson": { "id": "level-1", "layers": {} }, "tagIds": [] }
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
    [SwaggerOperation(Summary = "Update map", Description = "Updates a draft map. Author or Admin/Moderator only. Requires Bearer token.", OperationId = "Learner_UpdateMap", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> UpdateMap(Guid id, [FromBody] UpdateMapRequest request)
    {
        var result = await _mediator.Send(new UpdateMapCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update challenge map from uploaded JSON file (draft only)
    /// </summary>
    /// <remarks>
    /// Cập nhật map (ở trạng thái Draft) từ file JSON (multipart/form-data). Author hoặc Admin/Moderator. Yêu cầu Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID cần update.
    ///
    /// **METHOD and path:** PUT /api/learner/maps/{id}/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title (string, required): Tiêu đề map.
    /// - description (string, required): Mô tả.
    /// - difficulty (int, required): Độ khó (0=Easy, 1=Medium, 2=Hard).
    /// - timeLimitMs (int, required): Thời gian giới hạn (ms).
    /// - winCondition (int, required): Điều kiện thắng (metadata).
    /// - price (decimal?, optional): Giá; null = miễn phí.
    /// - hintsJson (string, optional): JSON array hints, mặc định "[]".
    /// - tagIdsCsv (string, optional): Danh sách tag ID cách nhau bằng dấu phẩy.
    /// - mapDetailFile (file, required): File JSON chứa chi tiết map (level/layers/objects...).
    ///
    /// **Lưu ý:** API này chỉ cập nhật nội dung map (spec, hints, tags, metadata) dựa trên file JSON; avatar map cập nhật qua API riêng `/api/learner/maps/{id}/avatar`.
    /// </remarks>
    /// <response code="200">Map updated. Returns message only.</response>
    /// <response code="400">Validation error or MapDetailFile is required</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author or admin)</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/upload-json")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Update map từ file JSON", Description = "Uploads a JSON file; updates existing Draft map. Form: title, description, difficulty, type? (Topdown|Platform), timeLimitMs, winCondition, price?, hintsJson?, tagIdsCsv?, mapDetailFile (required). Requires Bearer token.", OperationId = "Learner_UpdateMapFromJsonFile", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> UpdateMapFromJsonFile(Guid id, [FromForm] CreateMapFromJsonFileRequest request)
    {
        if (request.MapDetailFile == null || request.MapDetailFile.Length == 0)
            return BadRequest(Result.Failure("MapDetailFile is required.", ErrorCodeEnum.ValidationFailed));

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

        var result = await _mediator.Send(new UpdateMapFromJsonFileCommand(id, input));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Upload map avatar (image) to Cloudinary
    /// </summary>
    /// <remarks>
    /// Upload avatar cho map. Author hoặc Admin/Moderator. Body: multipart/form-data, field "avatar" (file ảnh).
    /// **METHOD and path:** POST /api/learner/maps/{id}/avatar
    /// </remarks>
    [HttpPost("{id:guid}/avatar")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Upload avatar map", Description = "Upload avatar image for map (Cloudinary). Author or Admin/Moderator. Form: avatar (file).", OperationId = "Learner_UploadMapAvatar", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> UploadMapAvatar(Guid id, IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
            return BadRequest(Result<string>.Failure("Avatar file is required.", ErrorCodeEnum.ValidationFailed));
        var result = await _mediator.Send(new UploadMapAvatarCommand(id, avatar));
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
    /// **METHOD and path:** POST /api/learner/maps/{id}/submit
    ///
    /// **Example request:** POST /api/learner/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6/submit
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
    [SwaggerOperation(Summary = "Submit map for review", Description = "Submits draft map for moderator review. Author only. Requires Bearer token.", OperationId = "Learner_SubmitMapForReview", Tags = new[] { "Learner - Maps" })]
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
    /// **METHOD and path:** DELETE /api/learner/maps/{id}
    ///
    /// **Example request:** DELETE /api/learner/maps/3fa85f64-5717-4562-b3fc-2c963f66afa6
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
    [SwaggerOperation(Summary = "Xóa map", Description = "Soft-deletes map. Author or Admin/Moderator only. Requires Bearer token.", OperationId = "Learner_DeleteMap", Tags = new[] { "Learner - Maps" })]
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
    /// **METHOD and path:** GET /api/learner/maps/tags
    ///
    /// **Example request:** GET /api/learner/maps/tags?search=logic
    /// </remarks>
    /// <response code="200">Returns message and data (list of tags).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách tag", Description = "Returns all tags. Optional query: search. Read-only, for map create/edit dropdown.", OperationId = "Learner_GetTags", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> GetTags([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetTagsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    private static MapTypeEnum? ParseMapType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return null;
        return string.Equals(type.Trim(), "Platform", StringComparison.OrdinalIgnoreCase) ? MapTypeEnum.Platform : MapTypeEnum.Topdown;
    }
}
