using System.Text.Json;
using CapstoneProject.API.Helpers;
using CapstoneProject.API.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Features.Games.Commands.CreateMap;
using CapstoneProject.Application.Features.Games.Commands.CreateMapFromJsonFile;
using CapstoneProject.Application.Features.Games.Commands.DeleteMap;
using CapstoneProject.Application.Features.Games.Commands.DuplicateMapAsNew;
using CapstoneProject.Application.Features.Games.Commands.SubmitMapForReview;
using CapstoneProject.Application.Features.Games.Commands.UpdateMap;
using CapstoneProject.Application.Features.Games.Commands.UploadMapAvatar;
using CapstoneProject.Application.Features.Games.Commands.AddMapGalleryMedia;
using CapstoneProject.Application.Features.Games.Commands.DeleteMapGalleryMedia;
using CapstoneProject.Application.Features.Games.Queries.GetMapById;
using CapstoneProject.Application.Features.Games.Queries.GetMapInfo;
using CapstoneProject.Application.Features.Games.Queries.GetMaps;
using CapstoneProject.Application.Features.Games.Queries.GetMyGames;
using CapstoneProject.Application.Features.Games.Queries.GetMyGameList;
using CapstoneProject.Application.Features.Games.Queries.GetTags;
using CapstoneProject.Application.Features.Games.Queries.CheckMapOwnership;
using CapstoneProject.Application.Features.Games.Commands.UpdateMapFromJsonFile;
using CapstoneProject.Application.Features.Games.Commands.PublishMap;
using CapstoneProject.Application.Features.Games.Commands.AddMapToMyGames;
using CapstoneProject.Application.Features.Games.Commands.CreateMapVersionFromApproved;
using CapstoneProject.Application.Features.Leaderboards.Queries.GetMostPlayedCreatedMapsLeaderboard;
using CapstoneProject.Application.Common.Enums;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API thá»­ thÃ¡ch dÃ nh cho Learner: catalog, táº¡o/sá»­a game (UGC), gá»­i duyá»‡t. Tags/Concepts chá»‰ Ä‘á»c.
/// </summary>
[ApiController]
[Route("api/learner/games")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Games")]
[SwaggerTag("Learner - Games (catalog, create, update, submit), tags (read-only)")]
public class LearnerGameController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerGameController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get list of challenge games (catalog)
    /// </summary>
    /// <remarks>
    /// Returns paginated challenge games for the learner catalog. Use filters for difficulty, type, tag, and search. When publishedOnly=true only published games are returned.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - publishedOnly (bool?, optional): true = only published games (catalog). Ignored when mapStatus is set. Default true.
    /// - mapStatus (int?, optional): Filter by game status: 0=Draft, 1=PendingReview, 2=Approved, 3=Rejected, 4=Published. When set, publishedOnly is ignored.
    /// - difficulty (int?, optional): Filter by difficulty level (1-5).
    /// - type (int?, optional): Filter by game type: 0=Topdown, 1=Platform, 2=Snake.
    /// - tagId (Guid?, optional): Filter by tag ID.
    /// - search (string, optional): Search in title and description.
    /// - sortBy (string, optional): Sort by: CreatedAt, Title, Difficulty, TimeLimitMs.
    /// - sortAscending (bool, optional): true = ascending, false = descending. Default false.
    ///
    /// **METHOD and path:** GET /api/learner/games
    ///
    /// **Example request:** GET /api/learner/games?pageNumber=1&amp;pageSize=10&amp;mapStatus=4&amp;difficulty=1&amp;search=abc&amp;sortBy=Title&amp;sortAscending=true
    /// </remarks>
    /// <response code="200">Returns message and data (paginated list of games).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách game (catalog)", Description = "Returns paginated challenge games for catalog. Filter by mapStatus (0-4) or publishedOnly, difficulty, tagId, search, sortBy.", OperationId = "Learner_GetGames", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetMaps([FromQuery] GetMapsQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get leaderboard of user-created games by play count (week/month)
    /// </summary>
    /// <remarks>
    /// Returns ranked games created by users, ordered by play count in selected period.
    ///
    /// **METHOD and path:** GET /api/learner/games/leaderboard/most-played-created
    ///
    /// **Query:**
    /// - periodType (LeaderboardPeriodTypeEnum, optional): Week | Month. Default Week.
    /// - pageNumber, pageSize.
    /// </remarks>
    /// <response code="200">Returns message and data (paginated game leaderboard).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("leaderboard/most-played-created")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Leaderboard game được chơi nhiều nhất", Description = "Get leaderboard for user-created games by play count in Week/Month period.", OperationId = "Learner_GetMostPlayedCreatedMapsLeaderboard", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetMostPlayedCreatedMapsLeaderboard(
        [FromQuery] LeaderboardPeriodTypeEnum periodType = LeaderboardPeriodTypeEnum.Week,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetMostPlayedCreatedMapsLeaderboardQuery(periodType, pageNumber, pageSize));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get all games owned by the current user (created by user + purchased with OrbitCoin)
    /// </summary>
    /// <remarks>
    /// Returns paginated list of games the user owns: games they created and games they purchased. Requires Bearer token.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - sortBy (string, optional): CreatedAt, Title, Difficulty, TimeLimitMs.
    /// - sortAscending (bool, optional): Default false.
    /// - isAuthorOnly (bool, optional): true = chá»‰ láº¥y game do chÃ­nh user táº¡o; false (máº·c Ä‘á»‹nh) = bao gá»“m cáº£ game Ä‘Ã£ mua.
    ///
    /// **Response item fields (MapListItemDto):**
    /// - id, title, description, difficulty, type, timeLimitMs, isPublished, mapStatus, price, createdByUserId, createdAt, tagNames, winCondition, avatarUrl
    /// - isAuthor (bool): true = game do chÃ­nh user Ä‘ang gá»­i request táº¡o ra (Game.CreatedBy); false = user chá»‰ sá»Ÿ há»¯u (mua/thÃªm). DÃ¹ng Ä‘á»ƒ phÃ¢n biá»‡t tÃ¡c giáº£, khÃ´ng pháº£i kiá»ƒm tra sá»Ÿ há»¯u.
    ///
    /// **METHOD and path:** GET /api/learner/games/my-games
    /// </remarks>
    /// <response code="200">Returns message and data (paginated list of owned games).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("my-games")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Danh sách game của tôi", Description = "Returns games owned by current user: created by user + purchased with OrbitCoin. Requires Bearer token.", OperationId = "Learner_GetMyGames", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetMyGames([FromQuery] GetMyGamesQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get list of games from báº£ng MyGame (tá»± táº¡o, mua, thÃªm free). Filter isAuthor: null = láº¥y háº¿t, true = chá»‰ game tá»± táº¡o, false = chá»‰ game mua/thÃªm vÃ o.
    /// </summary>
    /// <remarks>
    /// API má»›i láº¥y dá»¯ liá»‡u tá»« báº£ng MyGame. KhÃ´ng gá»­i isAuthor = láº¥y háº¿t; isAuthor=true = chá»‰ game tá»± táº¡o (author); isAuthor=false = chá»‰ game Ä‘Ã£ mua hoáº·c thÃªm vÃ o.
    /// **Query:** pageNumber, pageSize, sortBy (CreatedAt, Title, Difficulty, TimeLimitMs), sortAscending, isAuthor (bool?, optional).
    /// **METHOD and path:** GET /api/learner/games/my-game-list
    /// </remarks>
    /// <response code="200">Paginated list of games (MapListItemDto, isAuthor tá»« báº£ng MyGame).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("my-game-list")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<MapListItemDto>>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Danh sách game từ bảng MyGame", Description = "Returns games from MyGame table with filter isAuthor. null=all, true=author only, false=purchased/added only.", OperationId = "Learner_GetMyGameList", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetMyGameList([FromQuery] GetMyGameListQuery query)
    {
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Check if current user owns a game (created or purchased)
    /// </summary>
    /// <remarks>
    /// Nháº­p game ID, tráº£ vá» game cÃ³ tá»“n táº¡i khÃ´ng vÃ  user hiá»‡n táº¡i Ä‘Ã£ sá»Ÿ há»¯u game chÆ°a (tá»± táº¡o hoáº·c Ä‘Ã£ mua báº±ng OrbitCoin). Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **Response (CheckMapOwnershipDto):**
    /// - mapExists (bool): Game cÃ³ tá»“n táº¡i vÃ  active.
    /// - isOwned (bool): User cÃ³ sá»Ÿ há»¯u (tÃ¡c giáº£ hoáº·c Ä‘Ã£ mua).
    /// - isAuthor (bool): true náº¿u user lÃ  tÃ¡c giáº£; false náº¿u chá»‰ mua hoáº·c khÃ´ng sá»Ÿ há»¯u.
    ///
    /// **METHOD and path:** GET /api/learner/games/{id}/check-ownership
    /// </remarks>
    /// <response code="200">Returns message and data (mapExists, isOwned, isAuthor).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id:guid}/check-ownership")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CheckMapOwnershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CheckMapOwnershipDto>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Kiểm tra sở hữu game", Description = "Check if current user owns the game (created or purchased). Returns mapExists, isOwned, isAuthor.", OperationId = "Learner_CheckMapOwnership", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> CheckMapOwnership(Guid id)
    {
        var result = await _mediator.Send(new CheckMapOwnershipQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Add a free game to current user's collection (MyGame). Only published free games (price = 0 or null) can be added.
    /// </summary>
    /// <remarks>
    /// ThÃªm game free vÃ o bá»™ sÆ°u táº­p cá»§a user. Chá»‰ Ã¡p dá»¥ng cho game Ä‘Ã£ published vÃ  cÃ³ giÃ¡ = 0 hoáº·c null. Náº¿u Ä‘Ã£ cÃ³ trong bá»™ sÆ°u táº­p thÃ¬ tráº£ vá» success.
    /// **Route:** id (Guid): Game ID.
    /// **METHOD and path:** POST /api/learner/games/{id}/add-to-my-games
    /// </remarks>
    /// <response code="200">Game added or already in collection.</response>
    /// <response code="400">Game is paid or not published.</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Game not found</response>
    [HttpPost("{id:guid}/add-to-my-games")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Thêm game free vào bộ sưu tập", Description = "Add a published free game to current user's collection (MyGame). Only free games allowed.", OperationId = "Learner_AddMapToMyGames", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> AddMapToMyGames(Guid id)
    {
        var result = await _mediator.Send(new AddMapToMyGamesCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get challenge game detail by ID
    /// </summary>
    /// <remarks>
    /// Returns full game detail (spec, hints, constraints). Set includeEditorialForUser=true to get editorial when the user has earned enough stars.
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **Query:**
    /// - includeEditorialForUser (bool, optional): If true, includes editorial content when user has sufficient stars. Default false.
    ///
    /// **METHOD and path:** GET /api/learner/games/{id}
    ///
    /// **Example request:** GET /api/learner/games/3fa85f64-5717-4562-b3fc-2c963f66afa6?includeEditorialForUser=false
    /// </remarks>
    /// <response code="200">Returns message and data (game detail).</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Chi tiết game theo ID", Description = "Returns game detail (spec, hints, constraints). Optional includeEditorialForUser for editorial when user has enough stars.", OperationId = "Learner_GetMapById", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetMapById(Guid id, [FromQuery] bool includeEditorialForUser = false)
    {
        var result = await _mediator.Send(new GetMapByIdQuery(id, includeEditorialForUser));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get latest game version detail by ID (including draft/pending)
    /// </summary>
    /// <remarks>
    /// Returns the newest version in the same game line for playtest. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Game ID (any version in the line).
    ///
    /// **Query:**
    /// - includeEditorialForUser (bool, optional): Include editorial when user has enough stars.
    ///
    /// **METHOD and path:** GET /api/learner/games/{id}/latest
    /// </remarks>
    /// <response code="200">Returns message and data (latest game detail).</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden (not author)</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/latest")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<GameDetailDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Chi tiết game phiên bản mới nhất", Description = "Returns latest version in the game line for playtest (draft/pending included).", OperationId = "Learner_GetLatestMapById", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetLatestMapById(Guid id, [FromQuery] bool includeEditorialForUser = false)
    {
        var result = await _mediator.Send(new GetMapByIdQuery(
            id,
            includeEditorialForUser,
            PreferLatestVersion: true,
            RequireOwnershipForUnpublished: true));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get game info only (metadata, no GameDetail / hints)
    /// </summary>
    /// <remarks>
    /// Láº¥y chá»‰ thÃ´ng tin game theo ID: title, description, difficulty, type, timeLimitMs, isPublished, mapStatus, price, createdByUserId, createdAt, tagNames, winCondition, avatarUrl. KhÃ´ng tráº£ vá» GameDetail (JSON level), Hints, Editorial. DÃ¹ng khi chá»‰ cáº§n metadata.
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **METHOD and path:** GET /api/learner/games/{id}/info
    /// </remarks>
    /// <response code="200">Returns message and data (MapInfoDto).</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/info")]
    [ProducesResponseType(typeof(Result<MapInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MapInfoDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<MapInfoDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Thông tin game (metadata)", Description = "Returns game metadata only (no GameDetail JSON, no hints). Use for lightweight game info.", OperationId = "Learner_GetMapInfo", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetMapInfo(Guid id)
    {
        var result = await _mediator.Send(new GetMapInfoQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create new challenge game (draft)
    /// </summary>
    /// <remarks>
    /// Creates a new challenge game in Draft status. Then Update and Submit for moderator review. Requires Bearer token (Learner/Admin/Moderator).
    ///
    /// **Body (JSON):**
    /// - title (string, required): Game title.
    /// - description (string, required): Game description.
    /// - difficulty (int, required): Difficulty level (1-5).
    /// - timeLimitMs (int, required): Time limit in milliseconds.
    /// - winCondition (int, required): Win condition value stored in Game metadata.
    /// - price (decimal?, optional): Price for paid game; null = free.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - mapDetailJson (object, required): Full JSON game detail payload (level/layers/start-goal/objects/metadata...).
    /// - hints (array of { orderNo: int, content: string }, optional): Ordered hints.
    /// - tagIds (array of Guid, optional): Tag IDs.
    /// - avatarUrl (string, optional): URL avatar game (Cloudinary). Hoáº·c upload sau qua POST /api/learner/games/{id}/avatar.
    ///
    /// **METHOD and path:** POST /api/learner/games
    ///
    /// **Body:** type (string, optional): Topdown | Platform | Snake. Máº·c Ä‘á»‹nh Topdown.
    /// **Example request body:** { "title": "My Game", "description": "Description", "difficulty": 1, "type": "Topdown", "timeLimitMs": 60000, "winCondition": 10, "mapDetailJson": { "id": "level-1", "layers": {} }, "hints": [], "tagIds": [] }
    /// </remarks>
    /// <response code="201">Game created. Returns message and data (gameId).</response>
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
    [SwaggerOperation(Summary = "Tạo game (nháp)", Description = "Creates new challenge game as Draft. Returns gameId. Then Update and Submit for review. Requires Bearer token.", OperationId = "Learner_CreateMap", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> CreateMap([FromBody] CreateMapRequest request)
    {
        var result = await _mediator.Send(new CreateMapCommand(request));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Táº¡o game (nhÃ¡p) kÃ¨m avatar + gallery (multipart). Route riÃªng Ä‘á»ƒ Swagger khÃ´ng trÃ¹ng POST /games.</summary>
    [HttpPost("with-files")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Tạo game (nháp) + avatar/gallery", Description = "POST .../games/with-files. Field `data`: JSON (CreateMapRequest). Optional: avatarFile, galleryFiles.", OperationId = "Learner_CreateMapMultipart", Tags = new[] { "Learner - Games" })]
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
    /// Create new game from uploaded JSON file (draft)
    /// </summary>
    /// <remarks>
    /// Táº¡o game má»›i tá»« file JSON (multipart/form-data). Game Ä‘Æ°á»£c táº¡o á»Ÿ tráº¡ng thÃ¡i Draft; sau Ä‘Ã³ cáº­p nháº­t vÃ  gá»­i duyá»‡t qua Submit. YÃªu cáº§u Bearer token (Learner/Admin/Moderator).
    ///
    /// **METHOD and path:** POST /api/learner/games/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title (string, required): TiÃªu Ä‘á» game.
    /// - description (string, required): MÃ´ táº£.
    /// - difficulty (int, required): Äá»™ khÃ³ (1-5).
    /// - timeLimitMs (int, required): Thá»i gian giá»›i háº¡n (ms).
    /// - winCondition (int, required): Äiá»u kiá»‡n tháº¯ng (metadata).
    /// - price (decimal?, optional): GiÃ¡; null = miá»…n phÃ­.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - tagIdsCsv (string, optional): Danh sÃ¡ch tag ID cÃ¡ch nhau báº±ng dáº¥u pháº©y.
    /// - mapDetailFiles (files, required): Má»™t hoáº·c nhiá»u file JSON. Má»™t file: cÃ³ thá»ƒ lÃ  object má»™t level, máº£ng cÃ¡c level, hoáº·c `{ "levels": [...] }`. Nhiá»u file: má»—i file = má»™t level (0,1,2â€¦).
    /// - avatarFile (file, optional): áº¢nh avatar game; upload lÃªn Cloudinary khi táº¡o.
    ///
    /// **Example:** multipart/form-data; láº·p field `mapDetailFiles` hoáº·c chá»n nhiá»u file (Postman/Swagger).
    /// </remarks>
    /// <response code="201">Game created. Returns message and data (gameId).</response>
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
    [SwaggerOperation(Summary = "Tạo game từ file JSON", Description = "multipart/form-data: mapDetailFiles - một file (nhiều level trong file) hoặc nhiều file (mỗi file một level). Requires Bearer token.", OperationId = "Learner_CreateMapFromJsonFile", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> CreateMapFromJsonFile([FromForm] CreateMapFromJsonFileRequest request)
    {
        var (input, formErr) = await MapJsonUploadFormReader.BuildCreateInputAsync(request, Request);
        if (formErr != null || input == null)
            return BadRequest(Result<Guid>.Failure(formErr ?? "Đầu vào tệp trò chơi không hợp lệ.", ErrorCodeEnum.ValidationFailed));

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
    /// Update challenge game (draft only)
    /// </summary>
    /// <remarks>
    /// Updates a game in Draft status. Chá»‰ tÃ¡c giáº£ (Learner) hoáº·c Admin/Moderator má»›i Ä‘Æ°á»£c sá»­a. Sau khi update, game sáº½ quay láº¡i tráº¡ng thÃ¡i Draft (GameStatus=Draft, IsPublished=false) vÃ  cáº§n submit láº¡i Ä‘á»ƒ duyá»‡t. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **Body (JSON):**
    /// - title (string, required): Game title.
    /// - description (string, required): Game description.
    /// - difficulty (int, required): Difficulty (1-5).
    /// - timeLimitMs (int, required): Time limit in ms.
    /// - winCondition (int, required): Win condition value stored in Game metadata.
    /// - price (decimal?, optional): Price; null = free.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - mapDetailJson (object, optional): Full JSON game detail payload.
    /// - editorialContent (string, optional): Editorial text.
    /// - unlockEditorialAfterStars (int?, optional): Stars required to unlock editorial.
    /// - hints (array, optional): Hint items.
    /// - tagIds (array of Guid, optional): Tag IDs.
    ///
    /// **METHOD and path:** PUT /api/learner/games/{id}
    ///
    /// **Example request body:** { "title": "Updated Game", "description": "Desc", "difficulty": 1, "timeLimitMs": 60000, "winCondition": 10, "mapDetailJson": { "id": "level-1", "layers": {} }, "tagIds": [] }
    /// </remarks>
    /// <response code="200">Game updated. Returns message and data (gameId).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author or admin)</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Update game", Description = "Ghi đè nội dung cùng listing (cùng GameId): cập nhật game; sau publish lại người đã mua vẫn dùng cùng GameId. Để giữ game gốc không đổi và tạo bản mới (GameId mới, lịch sử game cũ không đổi), dùng POST .../{id}/duplicate-as-new. Author or Admin/Moderator.", OperationId = "Learner_UpdateMap", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> UpdateMap(Guid id, [FromBody] UpdateMapRequest request)
    {
        var result = await _mediator.Send(new UpdateMapCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Táº¡o game má»›i tá»« game nguá»“n (game gá»‘c khÃ´ng bá»‹ sá»­a).</summary>
    /// <remarks>
    /// Clone toÃ n bá»™ level, hint, gallery (cÃ¹ng URL), tag (hoáº·c gá»­i tagIds), metadata. Game má»›i máº·c Ä‘á»‹nh Draft; optional autoPublish.
    /// NgÆ°á»i gá»i trá»Ÿ thÃ nh tÃ¡c giáº£ báº£n sao (MyGame IsAuthor). NgÆ°á»i chÆ¡i muá»‘n báº£n má»›i cáº§n sá»Ÿ há»¯u game má»›i (GameId khÃ¡c).
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
    [SwaggerOperation(Summary = "Duplicate game as new listing", Description = "Creates a new game (new GameId). Source game is not modified. Optional body: DuplicateMapAsNewRequest (title, description, difficulty, price, tagIds, learnedTags, editorial, autoPublish, ...). Empty body = copy with title \"(Copy)\".", OperationId = "Learner_DuplicateMapAsNew", Tags = new[] { "Learner - Games" })]
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
    /// Tạo version mới (Draft) từ game đã duyệt/xuất bản trong cùng game line.
    /// </summary>
    [HttpPost("{id:guid}/create-version")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Tạo version mới từ game đã duyệt", Description = "Creates a new draft version in the same game line for approved/published game.", OperationId = "Learner_CreateMapVersionFromApproved", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> CreateMapVersion(Guid id)
    {
        var result = await _mediator.Send(new CreateMapVersionFromApprovedCommand(id));
        if (result.IsSuccess && result.Data != default)
            return CreatedAtAction(nameof(GetMapById), new { id = result.Data }, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update challenge game from uploaded JSON file (draft only)
    /// </summary>
    /// <remarks>
    /// Cáº­p nháº­t game (á»Ÿ tráº¡ng thÃ¡i Draft) tá»« file JSON (multipart/form-data). Author hoáº·c Admin/Moderator. YÃªu cáº§u Bearer token.
    ///
    /// **Route:** id (Guid, required): Game ID cáº§n update.
    ///
    /// **METHOD and path:** PUT /api/learner/games/{id}/upload-json
    ///
    /// **Body (multipart/form-data):**
    /// - title (string, required): TiÃªu Ä‘á» game.
    /// - description (string, required): MÃ´ táº£.
    /// - difficulty (int, required): Äá»™ khÃ³ (1-5).
    /// - timeLimitMs (int, required): Thá»i gian giá»›i háº¡n (ms).
    /// - winCondition (int, required): Äiá»u kiá»‡n tháº¯ng (metadata).
    /// - price (decimal?, optional): GiÃ¡; null = miá»…n phÃ­.
    /// - freeTrialAttemptLimit (int, optional): Sá»‘ lÆ°á»£t chÆ¡i thá»­ miá»…n phÃ­ cho má»—i ngÆ°á»i chÆ¡i. 0 = khÃ´ng cÃ³ trial.
    /// - tagIdsCsv (string, optional): Danh sÃ¡ch tag ID cÃ¡ch nhau báº±ng dáº¥u pháº©y.
    /// - mapDetailFiles: Giá»‘ng API táº¡o game (má»™t hoáº·c nhiá»u file JSON).
    ///
    /// **LÆ°u Ã½:** API nÃ y chá»‰ cáº­p nháº­t ná»™i dung game (spec, hints, tags, metadata) dá»±a trÃªn file JSON; avatar game cáº­p nháº­t qua API riÃªng `/api/learner/games/{id}/avatar`.
    /// </remarks>
    /// <response code="200">Game updated. Returns message only.</response>
    /// <response code="400">Validation error or mapDetailFiles is required</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author or admin)</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/upload-json")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Update game từ file JSON", Description = "Giống tạo game: mapDetailFiles (một hoặc nhiều file). Requires Bearer token.", OperationId = "Learner_UpdateMapFromJsonFile", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> UpdateMapFromJsonFile(Guid id, [FromForm] CreateMapFromJsonFileRequest request)
    {
        var (input, formErr) = await MapJsonUploadFormReader.BuildCreateInputAsync(request, Request);
        if (formErr != null || input == null)
            return BadRequest(Result<Guid>.Failure(formErr ?? "Đầu vào tệp trò chơi không hợp lệ.", ErrorCodeEnum.ValidationFailed));

        var result = await _mediator.Send(new UpdateMapFromJsonFileCommand(id, input));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Upload game avatar (image) to Cloudinary
    /// </summary>
    /// <remarks>
    /// Upload avatar cho game. Author hoáº·c Admin/Moderator. Body: multipart/form-data, field "avatar" (file áº£nh).
    /// **METHOD and path:** POST /api/learner/games/{id}/avatar
    /// </remarks>
    [HttpPost("{id:guid}/avatar")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Upload avatar game", Description = "Upload avatar image for game (Cloudinary). Author or Admin/Moderator. Form: avatar (file).", OperationId = "Learner_UploadMapAvatar", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> UploadMapAvatar(Guid id, IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
            return BadRequest(Result<string>.Failure("Cần có tập tin Avatar.", ErrorCodeEnum.ValidationFailed));
        var result = await _mediator.Send(new UploadMapAvatarCommand(id, avatar));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Upload má»™t hoáº·c nhiá»u áº£nh/video mÃ´ táº£ game (gallery, Cloudinary).</summary>
    [HttpPost("{id:guid}/gallery")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<List<GameMediaItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Upload game gallery (images/videos)", Description = "Author or Admin/Moderator. Form: files (one or more image/video files). Max 20 per request.", OperationId = "Learner_AddMapGalleryMedia", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> AddMapGalleryMedia(Guid id, [FromForm] List<IFormFile>? files)
    {
        var result = await _mediator.Send(new AddMapGalleryMediaCommand(id, files ?? new List<IFormFile>()));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>XÃ³a má»™t má»¥c trong gallery game.</summary>
    [HttpDelete("{id:guid}/gallery/{mediaId:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete game gallery item", Description = "Author or Admin/Moderator.", OperationId = "Learner_DeleteMapGalleryMedia", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> DeleteMapGalleryMedia(Guid id, Guid mediaId)
    {
        var result = await _mediator.Send(new DeleteMapGalleryMediaCommand(id, mediaId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Submit game for review
    /// </summary>
    /// <remarks>
    /// Submits a Draft game for moderator review. Author only (CreatedBy = current user). Bearer: Learner, Admin, or Moderator.
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **Body:** None. Headers only (Authorization: Bearer &lt;token&gt;).
    ///
    /// **METHOD and path:** POST /api/learner/games/{id}/submit
    ///
    /// **Example request:** POST /api/learner/games/3fa85f64-5717-4562-b3fc-2c963f66afa6/submit
    /// </remarks>
    /// <response code="200">Game submitted for review. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/submit")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Submit game for review", Description = "Submits draft game for moderator review. Author only. Bearer: Learner, Admin, or Moderator.", OperationId = "Learner_SubmitMapForReview", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> SubmitMapForReview(Guid id)
    {
        var result = await _mediator.Send(new SubmitMapForReviewCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Publish game (Approved â†’ Published) â€“ tÃ¡c giáº£ (Learner) hoáº·c Admin/Moderator
    /// </summary>
    /// <remarks>
    /// Publishes an Approved game so it appears in learner catalog.
    /// - **Learner:** chá»‰ Ä‘Æ°á»£c publish game do chÃ­nh mÃ¬nh táº¡o (CreatedBy).
    /// - **Admin/Moderator:** publish game Ä‘Ã£ Approved (báº¥t ká»³).
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **METHOD and path:** POST /api/learner/games/{id}/publish
    /// </remarks>
    /// <response code="200">Game published. Returns message only.</response>
    /// <response code="400">Invalid status (game not Approved) hoáº·c lá»—i khÃ¡c.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden (not author / not staff)</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/publish")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Publish game (Learner API)", Description = "Publishes an Approved game. Author (Learner) or Admin/Moderator. Route: id.", OperationId = "Learner_PublishMap", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> PublishMap(Guid id)
    {
        var result = await _mediator.Send(new PublishMapCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Delete challenge game (soft delete)
    /// </summary>
    /// <remarks>
    /// Soft-deletes the game. Author or Admin/Moderator only. Requires Bearer token.
    ///
    /// **Route:** id (Guid, required): Game ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/learner/games/{id}
    ///
    /// **Example request:** DELETE /api/learner/games/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    /// <response code="200">Game deleted. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Xóa game", Description = "Soft-deletes game. Author or Admin/Moderator only. Requires Bearer token.", OperationId = "Learner_DeleteMap", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> DeleteMap(Guid id)
    {
        var result = await _mediator.Send(new DeleteMapCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get list of tags
    /// </summary>
    /// <remarks>
    /// Returns all tags (read-only). Use for dropdown when creating/editing games.
    ///
    /// **Query:**
    /// - search (string, optional): Filter tags by name.
    ///
    /// **METHOD and path:** GET /api/learner/games/tags
    ///
    /// **Example request:** GET /api/learner/games/tags?search=logic
    /// </remarks>
    /// <response code="200">Returns message and data (list of tags).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<TagDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Danh sách tag", Description = "Returns all tags. Optional query: search. Read-only, for game create/edit dropdown.", OperationId = "Learner_GetTags", Tags = new[] { "Learner - Games" })]
    public async Task<IActionResult> GetTags([FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetTagsQuery(search));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
