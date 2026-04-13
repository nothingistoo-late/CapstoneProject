using System.Text.Json;
using CapstoneProject.API.Helpers;
using CapstoneProject.API.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Features.Maps.Commands.CreateMap;
using CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;
using CapstoneProject.Application.Features.Maps.Commands.DeleteMap;
using CapstoneProject.Application.Features.Maps.Commands.DuplicateMapAsNew;
using CapstoneProject.Application.Features.Maps.Commands.SubmitMapForReview;
using CapstoneProject.Application.Features.Maps.Commands.UpdateMap;
using CapstoneProject.Application.Features.Maps.Commands.UploadMapAvatar;
using CapstoneProject.Application.Features.Maps.Commands.AddMapGalleryMedia;
using CapstoneProject.Application.Features.Maps.Commands.DeleteMapGalleryMedia;
using CapstoneProject.Application.Features.Maps.Queries.GetMapById;
using CapstoneProject.Application.Features.Maps.Queries.GetMapInfo;
using CapstoneProject.Application.Features.Maps.Queries.GetMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetMyMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetMyMapList;
using CapstoneProject.Application.Features.Maps.Queries.GetTags;
using CapstoneProject.Application.Features.Maps.Queries.CheckMapOwnership;
using CapstoneProject.Application.Features.Maps.Commands.UpdateMapFromJsonFile;
using CapstoneProject.Application.Features.Maps.Commands.PublishMap;
using CapstoneProject.Application.Features.Maps.Commands.AddMapToMyMaps;
using CapstoneProject.Application.Features.Maps.Commands.CreateMapVersionFromApproved;
using CapstoneProject.Application.Common.Enums;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API thá»­ thÃ¡ch dÃ nh cho Learner: catalog, táº¡o/sá»­a map (UGC), gá»­i duyá»‡t. Tags/Concepts chá»‰ Ä‘á»c.
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
    /// - difficulty (int?, optional): Filter by difficulty level (1-5).
    /// - type (int?, optional): Filter by map type: 0=Topdown, 1=Platform, 2=Snake.
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
    [SwaggerOperation(Summary = "Danh sách map (catalog)", Description = "Returns paginated challenge maps for catalog. Filter by mapStatus (0-4) or publishedOnly, difficulty, tagId, search, sortBy.", OperationId = "Learner_GetMaps", Tags = new[] { "Learner - Maps" })]
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
    /// - isAuthorOnly (bool, optional): true = chá»‰ láº¥y map do chÃ­nh user táº¡o; false (máº·c Ä‘á»‹nh) = bao gá»“m cáº£ map Ä‘Ã£ mua.
    ///
    /// **Response item fields (MapListItemDto):**
    /// - id, title, description, difficulty, type, timeLimitMs, isPublished, mapStatus, price, createdByUserId, createdAt, tagNames, winCondition, avatarUrl
    /// - isAuthor (bool): true = map do chÃ­nh user Ä‘ang gá»­i request táº¡o ra (Map.CreatedBy); false = user chá»‰ sá»Ÿ há»¯u (mua/thÃªm). DÃ¹ng Ä‘á»ƒ phÃ¢n biá»‡t tÃ¡c giáº£, khÃ´ng pháº£i kiá»ƒm tra sá»Ÿ há»¯u.
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
    /// Get list of maps from báº£ng MyMap (tá»± táº¡o, mua, thÃªm free). Filter isAuthor: null = láº¥y háº¿t, true = chá»‰ map tá»± táº¡o, false = chá»‰ map mua/thÃªm vÃ o.
    /// </summary>
    /// <remarks>
    /// API má»›i láº¥y dá»¯ liá»‡u tá»« báº£ng MyMap. KhÃ´ng gá»­i isAuthor = láº¥y háº¿t; isAuthor=true = chá»‰ map tá»± táº¡o (author); isAuthor=false = chá»‰ map Ä‘Ã£ mua hoáº·c thÃªm vÃ o.
    /// **Query:** pageNumber, pageSize, sortBy (CreatedAt, Title, Difficulty, TimeLimitMs), sortAscending, isAuthor (bool?, optional).
    /// **METHOD and path:** GET /api/learner/maps/my-map-list
    /// </remarks>
    /// <response code="200">Paginated list of maps (MapListItemDto, isAuthor tá»« báº£ng MyMap).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("my-map-list")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Danh sách map từ bảng MyMap", Description = "Returns maps from MyMap table with filter isAuthor. null=all, true=author only, false=purchased/added only.", OperationId = "Learner_GetMyMapList", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> GetMyMapList([FromQuery] GetMyMapListQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Check if current user owns a map (created or purchased)
    /// </summary>
    /// <remarks>
    /// Nháº­p map ID, tráº£ vá» map cÃ³ tá»“n táº¡i khÃ´ng vÃ  user hiá»‡n táº¡i Ä‘Ã£ sá»Ÿ há»¯u map chÆ°a (tá»± táº¡o hoáº·c Ä‘Ã£ mua báº±ng OrbitCoin). Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Response (CheckMapOwnershipDto):**
    /// - mapExists (bool): Map cÃ³ tá»“n táº¡i vÃ  active.
    /// - isOwned (bool): User cÃ³ sá»Ÿ há»¯u (tÃ¡c giáº£ hoáº·c Ä‘Ã£ mua).
    /// - isAuthor (bool): true náº¿u user lÃ  tÃ¡c giáº£; false náº¿u chá»‰ mua hoáº·c khÃ´ng sá»Ÿ há»¯u.
    ///
    /// **METHOD and path:** GET /api/learner/maps/{id}/check-ownership
    /// </remarks>
    /// <response code="200">Returns message and data (mapExists, isOwned, isAuthor).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id:guid}/check-ownership")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CheckMapOwnershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CheckMapOwnershipDto>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Kiểm tra sở hữu map", Description = "Check if current user owns the map (created or purchased). Returns mapExists, isOwned, isAuthor.", OperationId = "Learner_CheckMapOwnership", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> CheckMapOwnership(Guid id)
    {
        var result = await _mediator.Send(new CheckMapOwnershipQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Add a free map to current user's collection (MyMap). Only published free maps (price = 0 or null) can be added.
    /// </summary>
    /// <remarks>
    /// ThÃªm map free vÃ o bá»™ sÆ°u táº­p cá»§a user. Chá»‰ Ã¡p dá»¥ng cho map Ä‘Ã£ published vÃ  cÃ³ giÃ¡ = 0 hoáº·c null. Náº¿u Ä‘Ã£ cÃ³ trong bá»™ sÆ°u táº­p thÃ¬ tráº£ vá» success.
    /// **Route:** id (Guid): Map ID.
    /// **METHOD and path:** POST /api/learner/maps/{id}/add-to-my-maps
    /// </remarks>
    /// <response code="200">Map added or already in collection.</response>
    /// <response code="400">Map is paid or not published.</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Map not found</response>
    [HttpPost("{id:guid}/add-to-my-maps")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Thêm map free vào bộ sưu tập", Description = "Add a published free map to current user's collection (MyMap). Only free maps allowed.", OperationId = "Learner_AddMapToMyMaps", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> AddMapToMyMaps(Guid id)
    {
        var result = await _mediator.Send(new AddMapToMyMapsCommand(id));
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
    /// Get map info only (metadata, no MapDetail / hints)
    /// </summary>
    /// <remarks>
    /// Láº¥y chá»‰ thÃ´ng tin map theo ID: title, description, difficulty, type, timeLimitMs, isPublished, mapStatus, price, createdByUserId, createdAt, tagNames, winCondition, avatarUrl. KhÃ´ng tráº£ vá» MapDetail (JSON level), Hints, Editorial. DÃ¹ng khi chá»‰ cáº§n metadata.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **METHOD and path:** GET /api/learner/maps/{id}/info
    /// </remarks>
    /// <response code="200">Returns message and data (MapInfoDto).</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/info")]
    [ProducesResponseType(typeof(Result<MapInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MapInfoDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<MapInfoDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Thông tin map (metadata)", Description = "Returns map metadata only (no MapDetail JSON, no hints). Use for lightweight map info.", OperationId = "Learner_GetMapInfo", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> GetMapInfo(Guid id)
    {
        var result = await _mediator.Send(new GetMapInfoQuery(id));
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
    /// - difficulty (int, required): Difficulty level (1-5).
    /// - timeLimitMs (int, required): Time limit in milliseconds.
    /// - winCondition (int, required): Win condition value stored in Map metadata.
    /// - price (decimal?, optional): Price for paid map; null = free.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - mapDetailJson (object, required): Full JSON map detail payload (level/layers/start-goal/objects/metadata...).
    /// - hints (array of { orderNo: int, content: string }, optional): Ordered hints.
    /// - tagIds (array of Guid, optional): Tag IDs.
    /// - avatarUrl (string, optional): URL avatar map (Cloudinary). Hoáº·c upload sau qua POST /api/learner/maps/{id}/avatar.
    ///
    /// **METHOD and path:** POST /api/learner/maps
    ///
    /// **Body:** type (string, optional): Topdown | Platform | Snake. Máº·c Ä‘á»‹nh Topdown.
    /// **Example request body:** { "title": "My Map", "description": "Description", "difficulty": 1, "type": "Topdown", "timeLimitMs": 60000, "winCondition": 10, "mapDetailJson": { "id": "level-1", "layers": {} }, "hints": [], "tagIds": [] }
    /// </remarks>
    /// <response code="201">Map created. Returns message and data (mapId).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("application/json")]
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

    /// <summary>Táº¡o map (nhÃ¡p) kÃ¨m avatar + gallery (multipart). Route riÃªng Ä‘á»ƒ Swagger khÃ´ng trÃ¹ng POST /maps.</summary>
    [HttpPost("with-files")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Tạo map (nháp) + avatar/gallery", Description = "POST .../maps/with-files. Field `data`: JSON (CreateMapRequest). Optional: avatarFile, galleryFiles.", OperationId = "Learner_CreateMapMultipart", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> CreateMapMultipart([FromForm] CreateMapMultipartForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Data))
            return BadRequest(Result<Guid>.Failure("Trường 'dữ liệu' (chuỗi JSON, giống như nội dung CreateMapRequest) là bắt buộc.", ErrorCodeEnum.ValidationFailed));
        CreateMapRequest? req;
        try
        {
            req = JsonSerializer.Deserialize<CreateMapRequest>(form.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return BadRequest(Result<Guid>.Failure("Trường 'dữ liệu' không phải là JSON hợp lệ.", ErrorCodeEnum.ValidationFailed));
        }

        if (req == null)
            return BadRequest(Result<Guid>.Failure("Không thể giải tuần tự hóa trường 'dữ liệu' thành CreateMapRequest.", ErrorCodeEnum.ValidationFailed));

        var result = await _mediator.Send(new CreateMapCommand(req, GalleryFiles: form.GalleryFiles, AvatarFile: form.AvatarFile));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create new map from uploaded JSON file (draft)
    /// </summary>
    /// <remarks>
    /// Táº¡o map má»›i tá»« file JSON (multipart/form-data). Map Ä‘Æ°á»£c táº¡o á»Ÿ tráº¡ng thÃ¡i Draft; sau Ä‘Ã³ cáº­p nháº­t vÃ  gá»­i duyá»‡t qua Submit. YÃªu cáº§u Bearer token (Learner/Admin/Moderator).
    ///
    /// **METHOD and path:** POST /api/learner/maps/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title (string, required): TiÃªu Ä‘á» map.
    /// - description (string, required): MÃ´ táº£.
    /// - difficulty (int, required): Äá»™ khÃ³ (1-5).
    /// - timeLimitMs (int, required): Thá»i gian giá»›i háº¡n (ms).
    /// - winCondition (int, required): Äiá»u kiá»‡n tháº¯ng (metadata).
    /// - price (decimal?, optional): GiÃ¡; null = miá»…n phÃ­.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - tagIdsCsv (string, optional): Danh sÃ¡ch tag ID cÃ¡ch nhau báº±ng dáº¥u pháº©y.
    /// - mapDetailFiles (files, required): Má»™t hoáº·c nhiá»u file JSON. Má»™t file: cÃ³ thá»ƒ lÃ  object má»™t level, máº£ng cÃ¡c level, hoáº·c `{ "levels": [...] }`. Nhiá»u file: má»—i file = má»™t level (0,1,2â€¦).
    /// - avatarFile (file, optional): áº¢nh avatar map; upload lÃªn Cloudinary khi táº¡o.
    ///
    /// **Example:** multipart/form-data; láº·p field `mapDetailFiles` hoáº·c chá»n nhiá»u file (Postman/Swagger).
    /// </remarks>
    /// <response code="201">Map created. Returns message and data (mapId).</response>
    /// <response code="400">Validation error or mapDetailFiles is required</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("upload-json")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Tạo map từ file JSON", Description = "multipart/form-data: mapDetailFiles - một file (nhiều level trong file) hoặc nhiều file (mỗi file một level). Requires Bearer token.", OperationId = "Learner_CreateMapFromJsonFile", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> CreateMapFromJsonFile([FromForm] CreateMapFromJsonFileRequest request)
    {
        var (input, formErr) = await MapJsonUploadFormReader.BuildCreateInputAsync(request, Request);
        if (formErr != null || input == null)
            return BadRequest(Result<Guid>.Failure(formErr ?? "Đầu vào tệp bản đồ không hợp lệ.", ErrorCodeEnum.ValidationFailed));

        var result = await _mediator.Send(new CreateMapFromJsonFileCommand(
            input,
            AutoPublish: false,
            AvatarFile: request.AvatarFile,
            GalleryFiles: request.GalleryFiles));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update challenge map (draft only)
    /// </summary>
    /// <remarks>
    /// Updates a map in Draft status. Chá»‰ tÃ¡c giáº£ (Learner) hoáº·c Admin/Moderator má»›i Ä‘Æ°á»£c sá»­a. Sau khi update, map sáº½ quay láº¡i tráº¡ng thÃ¡i Draft (MapStatus=Draft, IsPublished=false) vÃ  cáº§n submit láº¡i Ä‘á»ƒ duyá»‡t. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **Body (JSON):**
    /// - title (string, required): Map title.
    /// - description (string, required): Map description.
    /// - difficulty (int, required): Difficulty (1-5).
    /// - timeLimitMs (int, required): Time limit in ms.
    /// - winCondition (int, required): Win condition value stored in Map metadata.
    /// - price (decimal?, optional): Price; null = free.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
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
    [SwaggerOperation(Summary = "Update map", Description = "Ghi đè nội dung cùng listing (cùng MapId): cập nhật map; sau publish lại người đã mua vẫn dùng cùng MapId. Để giữ map gốc không đổi và tạo bản mới (MapId mới, lịch sử map cũ không đổi), dùng POST .../{id}/duplicate-as-new. Author or Admin/Moderator.", OperationId = "Learner_UpdateMap", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> UpdateMap(Guid id, [FromBody] UpdateMapRequest request)
    {
        var result = await _mediator.Send(new UpdateMapCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Táº¡o map má»›i tá»« map nguá»“n (map gá»‘c khÃ´ng bá»‹ sá»­a).</summary>
    /// <remarks>
    /// Clone toÃ n bá»™ level, hint, gallery (cÃ¹ng URL), tag (hoáº·c gá»­i tagIds), metadata. Map má»›i máº·c Ä‘á»‹nh Draft; optional autoPublish.
    /// NgÆ°á»i gá»i trá»Ÿ thÃ nh tÃ¡c giáº£ báº£n sao (MyMap IsAuthor). NgÆ°á»i chÆ¡i muá»‘n báº£n má»›i cáº§n sá»Ÿ há»¯u map má»›i (MapId khÃ¡c).
    /// </remarks>
    [HttpPost("{id:guid}/duplicate-as-new")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Duplicate map as new listing", Description = "Creates a new map (new MapId). Source map is not modified. Optional body: DuplicateMapAsNewRequest (title, description, difficulty, price, tagIds, learnedTags, editorial, autoPublish, ...). Empty body = copy with title \"(Copy)\".", OperationId = "Learner_DuplicateMapAsNew", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> DuplicateMapAsNew(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DuplicateMapAsNewRequest? request = null)
    {
        var result = await _mediator.Send(new DuplicateMapAsNewCommand(id, request));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Tạo version mới (Draft) từ map đã duyệt/xuất bản trong cùng game line.
    /// </summary>
    [HttpPost("{id:guid}/create-version")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Tạo version mới từ map đã duyệt", Description = "Creates a new draft version in the same game line for approved/published map.", OperationId = "Learner_CreateMapVersionFromApproved", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> CreateMapVersion(Guid id)
    {
        var result = await _mediator.Send(new CreateMapVersionFromApprovedCommand(id));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update challenge map from uploaded JSON file (draft only)
    /// </summary>
    /// <remarks>
    /// Cáº­p nháº­t map (á»Ÿ tráº¡ng thÃ¡i Draft) tá»« file JSON (multipart/form-data). Author hoáº·c Admin/Moderator. YÃªu cáº§u Bearer token.
    ///
    /// **Route:** id (Guid, required): Map ID cáº§n update.
    ///
    /// **METHOD and path:** PUT /api/learner/maps/{id}/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title (string, required): TiÃªu Ä‘á» map.
    /// - description (string, required): MÃ´ táº£.
    /// - difficulty (int, required): Äá»™ khÃ³ (1-5).
    /// - timeLimitMs (int, required): Thá»i gian giá»›i háº¡n (ms).
    /// - winCondition (int, required): Äiá»u kiá»‡n tháº¯ng (metadata).
    /// - price (decimal?, optional): GiÃ¡; null = miá»…n phÃ­.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - tagIdsCsv (string, optional): Danh sÃ¡ch tag ID cÃ¡ch nhau báº±ng dáº¥u pháº©y.
    /// - mapDetailFiles: Giá»‘ng API táº¡o map (má»™t hoáº·c nhiá»u file JSON).
    ///
    /// **LÆ°u Ã½:** API nÃ y chá»‰ cáº­p nháº­t ná»™i dung map (spec, hints, tags, metadata) dá»±a trÃªn file JSON; avatar map cáº­p nháº­t qua API riÃªng `/api/learner/maps/{id}/avatar`.
    /// </remarks>
    /// <response code="200">Map updated. Returns message only.</response>
    /// <response code="400">Validation error or mapDetailFiles is required</response>
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
    [SwaggerOperation(Summary = "Update map từ file JSON", Description = "Giống tạo map: mapDetailFiles (một hoặc nhiều file). Requires Bearer token.", OperationId = "Learner_UpdateMapFromJsonFile", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> UpdateMapFromJsonFile(Guid id, [FromForm] CreateMapFromJsonFileRequest request)
    {
        var (input, formErr) = await MapJsonUploadFormReader.BuildCreateInputAsync(request, Request);
        if (formErr != null || input == null)
            return BadRequest(Result.Failure(formErr ?? "Đầu vào tệp bản đồ không hợp lệ.", ErrorCodeEnum.ValidationFailed));

        var result = await _mediator.Send(new UpdateMapFromJsonFileCommand(id, input));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Upload map avatar (image) to Cloudinary
    /// </summary>
    /// <remarks>
    /// Upload avatar cho map. Author hoáº·c Admin/Moderator. Body: multipart/form-data, field "avatar" (file áº£nh).
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
            return BadRequest(Result<string>.Failure("Cần có tập tin Avatar.", ErrorCodeEnum.ValidationFailed));
        var result = await _mediator.Send(new UploadMapAvatarCommand(id, avatar));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Upload má»™t hoáº·c nhiá»u áº£nh/video mÃ´ táº£ map (gallery, Cloudinary).</summary>
    [HttpPost("{id:guid}/gallery")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<List<MapMediaItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Upload map gallery (images/videos)", Description = "Author or Admin/Moderator. Form: files (one or more image/video files). Max 20 per request.", OperationId = "Learner_AddMapGalleryMedia", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> AddMapGalleryMedia(Guid id, [FromForm] List<IFormFile>? files)
    {
        var result = await _mediator.Send(new AddMapGalleryMediaCommand(id, files ?? new List<IFormFile>()));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>XÃ³a má»™t má»¥c trong gallery map.</summary>
    [HttpDelete("{id:guid}/gallery/{mediaId:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete map gallery item", Description = "Author or Admin/Moderator.", OperationId = "Learner_DeleteMapGalleryMedia", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> DeleteMapGalleryMedia(Guid id, Guid mediaId)
    {
        var result = await _mediator.Send(new DeleteMapGalleryMediaCommand(id, mediaId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Submit map for review
    /// </summary>
    /// <remarks>
    /// Submits a Draft map for moderator review. Author only (CreatedBy = current user). Bearer: Learner, Admin, or Moderator.
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
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Submit map for review", Description = "Submits draft map for moderator review. Author only. Bearer: Learner, Admin, or Moderator.", OperationId = "Learner_SubmitMapForReview", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> SubmitMapForReview(Guid id)
    {
        var result = await _mediator.Send(new SubmitMapForReviewCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Publish map (Approved â†’ Published) â€“ tÃ¡c giáº£ (Learner) hoáº·c Admin/Moderator
    /// </summary>
    /// <remarks>
    /// Publishes an Approved map so it appears in learner catalog.
    /// - **Learner:** chá»‰ Ä‘Æ°á»£c publish map do chÃ­nh mÃ¬nh táº¡o (CreatedBy).
    /// - **Admin/Moderator:** publish map Ä‘Ã£ Approved (báº¥t ká»³).
    ///
    /// **Route:** id (Guid, required): Map ID.
    ///
    /// **METHOD and path:** POST /api/learner/maps/{id}/publish
    /// </remarks>
    /// <response code="200">Map published. Returns message only.</response>
    /// <response code="400">Invalid status (map not Approved) hoáº·c lá»—i khÃ¡c.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author / not staff)</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/publish")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Publish map (Learner API)", Description = "Publishes an Approved map. Author (Learner) or Admin/Moderator. Route: id.", OperationId = "Learner_PublishMap", Tags = new[] { "Learner - Maps" })]
    public async Task<IActionResult> PublishMap(Guid id)
    {
        var result = await _mediator.Send(new PublishMapCommand(id));
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
}
