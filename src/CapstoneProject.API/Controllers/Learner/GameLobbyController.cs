using System.Linq;
using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CapstoneProject.API.Hubs;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Features.Lobby.Commands.CreateLobbyRoom;
using CapstoneProject.Application.Features.Lobby.Commands.EndLobbyGame;
using CapstoneProject.Application.Features.Lobby.Commands.JoinLobbyRoom;
using CapstoneProject.Application.Features.Lobby.Commands.LeaveLobbyRoom;
using CapstoneProject.Application.Features.Lobby.Commands.SetLobbyRoomMap;
using CapstoneProject.Application.Features.Lobby.Commands.StartLobbyGame;
using CapstoneProject.Application.Features.Lobby.Commands.SubmitLobbySolution;
using CapstoneProject.Application.Features.Lobby.Commands.ToggleLobbyReady;
using CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRoom;
using CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRooms;
using CapstoneProject.Application.Features.Chat.Commands.CreateTemporaryGroupConversation;
using CapstoneProject.Application.Features.Chat.Commands.AddMemberToRoom;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/lobby")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Game Lobby")]
[SwaggerTag("Game Lobby: list/create/join rooms. Real-time via SignalR /hubs/gamelobby")]
public class GameLobbyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<GameLobbyHub> _hubContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoomManager _roomManager;

    public GameLobbyController(
        IMediator mediator,
        IHubContext<GameLobbyHub> hubContext,
        IUnitOfWork unitOfWork,
        IRoomManager roomManager)
    {
        _mediator = mediator;
        _hubContext = hubContext;
        _unitOfWork = unitOfWork;
        _roomManager = roomManager;
    }

    /// <summary>Danh sách phòng lobby (REST).</summary>
    /// <remarks>
    /// Trả về danh sách tất cả phòng lobby đang mở. Real-time cập nhật qua SignalR hub /hubs/gamelobby. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** GET /api/learner/lobby/rooms
    ///
    /// **Body:** None. Headers: Authorization Bearer &lt;token&gt;.
    /// </remarks>
    /// <response code="200">Returns message and data (list of rooms: roomId, roomCode, hostId, playerCount, maxPlayers, status, selectedGameId).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("rooms")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<IReadOnlyList<LobbyRoomListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "List rooms", Description = "Returns all lobby rooms. Real-time updates via SignalR /hubs/gamelobby. Requires Bearer token.", OperationId = "Lobby_GetRooms", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> GetRooms()
    {
        var result = await _mediator.Send(new GetLobbyRoomsQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo phòng lobby mới.</summary>
    /// <remarks>
    /// Tạo phòng mới; người gọi trở thành host. Body tùy chọn: maxPlayers (mặc định 8), selectedGameId (Guid?). Sau khi tạo có thể set game qua SetRoomMap hoặc SignalR. Real-time: kết nối SignalR và join room. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms
    ///
    /// **Body (JSON, optional):**
    /// - maxPlayers (int, optional): Số người tối đa. Default 8.
    /// - selectedGameId (Guid?, optional): Game đã chọn ngay khi tạo; null = chọn sau.
    ///
    /// **Example request body:** { "maxPlayers": 8, "selectedGameId": null }
    /// </remarks>
    /// <response code="201">Room created. Returns message and data (roomId, roomCode, hostId, maxPlayers, selectedGameId).</response>
    /// <response code="400">Đã ở trong một phòng: isSuccess=false, message báo lỗi, data chứa thông tin phòng hiện tại (roomId, roomCode, maxPlayers, selectedGameId).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CreateLobbyRoomResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CreateLobbyRoomResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Create room", Description = "Creates new lobby room; caller becomes host. Optional body: maxPlayers, selectedGameId. Requires Bearer token.", OperationId = "Lobby_CreateRoom", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> CreateRoom([FromBody] CreateLobbyRoomRequest? request = null)
    {
        var result = await _mediator.Send(new CreateLobbyRoomCommand(request));
        if (result.IsSuccess && result.Data != null)
        {
            await EnsureRoomConversationSynchronizedAsync(result.Data.RoomId);
            await BroadcastLobbyListToAllAsync();
            await BroadcastRoomUpdatedToGroupAsync(result.Data.RoomId);
            return Created(string.Empty, result);
        }
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Vào phòng lobby theo RoomId hoặc RoomCode.</summary>
    /// <remarks>
    /// Gửi một trong hai: roomId (Guid) hoặc roomCode (string, ví dụ "AB12CD"). Phòng phải tồn tại và chưa đầy. Sau khi join nên kết nối SignalR và join group Room_&lt;roomId&gt; để nhận cập nhật real-time. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/join
    ///
    /// **Body (JSON):**
    /// - roomId (Guid?, optional): ID phòng. Bắt buộc nếu không gửi roomCode.
    /// - roomCode (string?, optional): Mã phòng (vd AB12CD). Bắt buộc nếu không gửi roomId.
    ///
    /// **Example by code:** { "roomCode": "AB12CD" }
    /// **Example by id:** { "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
    /// </remarks>
    /// <response code="200">Joined. Returns message and data (roomId, roomCode, hostId, playerCount, maxPlayers, status, selectedGameId, players).</response>
    /// <response code="400">Missing roomId/roomCode or room full</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/join")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<JoinLobbyRoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Join room by RoomId OR RoomCode", Description = "Body: provide either roomId or roomCode. Example by code: { \"roomCode\": \"AB12CD\" }. Example by id: { \"roomId\": \"guid\" }. Requires Bearer token.", OperationId = "Lobby_JoinRoom", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> JoinRoom([FromBody] JoinLobbyRoomRequest request)
    {
        var result = await _mediator.Send(new JoinLobbyRoomCommand(request));
        if (result.IsSuccess && result.Data != null)
        {
            await EnsureRoomConversationSynchronizedAsync(result.Data.RoomId);
            await BroadcastLobbyListToAllAsync();
            await BroadcastRoomUpdatedToGroupAsync(result.Data.RoomId);
        }
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Chi tiết phòng lobby theo roomId.</summary>
    /// <remarks>
    /// Trả về thông tin phòng: roomId, roomCode, hostId, số người, maxPlayers, status, selectedGameId, danh sách players (playerId, isReady, isHost). Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** GET /api/learner/lobby/rooms/{roomId}
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Example request:** GET /api/learner/lobby/rooms/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    /// <response code="200">Returns message and data (room detail with players).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("rooms/{roomId:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<LobbyRoomDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get room detail", Description = "Returns room detail: roomId, roomCode, hostId, players, status, selectedGameId. Requires Bearer token.", OperationId = "Lobby_GetRoom", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> GetRoom(Guid roomId)
    {
        var result = await _mediator.Send(new GetLobbyRoomQuery(roomId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Bắt đầu game trong phòng (chỉ host).</summary>
    /// <remarks>
    /// Chỉ host mới gọi được. Phòng phải có ít nhất 2 người và đã chọn game (selectedGameId). Trạng thái phòng chuyển sang InGame. Real-time: gửi event qua SignalR. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/{roomId}/start
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Body:** None.
    /// </remarks>
    /// <response code="200">Game started. Returns message and data (roomId, gameId, startedAt).</response>
    /// <response code="400">Not host / not enough players / game not set</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/{roomId:guid}/start")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<StartGameResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Start game", Description = "Host starts the game. Room must have game set and at least 2 players. Requires Bearer token.", OperationId = "Lobby_StartGame", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> StartGame(Guid roomId)
    {
        var result = await _mediator.Send(new StartLobbyGameCommand(roomId));
        if (result.IsSuccess && result.Data != null)
        {
            var groupName = $"{GameLobbyHub.RoomGroupPrefix}{roomId}";
            await _hubContext.Clients.Group(groupName).SendAsync("GameStarted", new
            {
                result.Data.RoomId,
                result.Data.GameId,
                result.Data.RoomCode,
                result.Data.TurnOrder,
                result.Data.StartedAt,
                result.Data.CurrentTurnIndex,
                result.Data.CurrentPlayerId,
                result.Data.RoundNumber
            });
        }
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Kết thúc game trong phòng (host hoặc bất kỳ khi đang InGame).</summary>
    /// <remarks>
    /// Chuyển trạng thái phòng từ InGame về Waiting. Trả về chi tiết phòng sau khi kết thúc. Real-time: broadcast qua SignalR. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/{roomId}/end
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Body:** None.
    /// </remarks>
    /// <response code="200">Game ended. Returns message and data (room detail).</response>
    /// <response code="400">Room not in InGame state</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/{roomId:guid}/end")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<LobbyRoomDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "End game", Description = "Ends the game in room; status returns to Waiting. Requires Bearer token.", OperationId = "Lobby_EndGame", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> EndGame(Guid roomId)
    {
        var result = await _mediator.Send(new EndLobbyGameCommand(roomId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Nộp lời giải trong game (khi phòng đang InGame).</summary>
    /// <remarks>
    /// Gửi lời giải (astSpec hoặc bytecodeSpec, language). Server validate và cập nhật ranking. Nếu tất cả đã nộp, trả về ranking và broadcast RankingUpdated qua SignalR. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/{roomId}/submit
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Body (JSON):**
    /// - language (string, optional): Ngôn ngữ solution. Default "Blockly".
    /// - astSpec (string, optional): AST spec (JSON). Dùng astSpec hoặc bytecodeSpec.
    /// - bytecodeSpec (string, optional): Bytecode spec.
    ///
    /// **Example request body:** { "language": "Blockly", "astSpec": "{}" }
    /// </remarks>
    /// <response code="200">Submitted. Returns message and data (accepted, stars, rankingIfAllSubmitted).</response>
    /// <response code="400">Validation error / room not InGame</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room or game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/{roomId:guid}/submit")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<SubmitGameResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Submit solution in game", Description = "Submits solution for current game. Returns ranking when all players submitted. Requires Bearer token.", OperationId = "Lobby_SubmitSolution", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> SubmitSolution(Guid roomId, [FromBody] SubmissionSubmitRequest request)
    {
        var result = await _mediator.Send(new SubmitLobbySolutionCommand(roomId, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Rời phòng lobby.</summary>
    /// <remarks>
    /// User hiện tại rời khỏi phòng. Nếu là host rời thì phòng có thể bị đóng hoặc chuyển host tùy logic. Real-time: broadcast qua SignalR. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/{roomId}/leave
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Body:** None.
    /// </remarks>
    /// <response code="200">Left room. Returns message only.</response>
    /// <response code="400">User not in room</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/{roomId:guid}/leave")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Leave room", Description = "Current user leaves the room. Requires Bearer token.", OperationId = "Lobby_LeaveRoom", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> LeaveRoom(Guid roomId)
    {
        string? roomCode = null;
        var roomBeforeLeave = await _mediator.Send(new GetLobbyRoomQuery(roomId));
        if (roomBeforeLeave.IsSuccess && roomBeforeLeave.Data != null)
        {
            roomCode = roomBeforeLeave.Data.RoomCode?.Trim();
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;
        Guid.TryParse(userIdClaim, out var userId);
        var leftPlayerName = userId != Guid.Empty ? await ResolveUserDisplayNameAsync(userId) : "Unknown";

        var result = await _mediator.Send(new LeaveLobbyRoomCommand(roomId));
        if (result.IsSuccess)
        {
            await BroadcastLobbyListToAllAsync();
            await BroadcastRoomUpdatedToGroupAsync(roomId);
            await _hubContext.Clients.Group($"{GameLobbyHub.RoomGroupPrefix}{roomId}")
                .SendAsync("PlayerLeftRoom", new
                {
                    RoomId = roomId,
                    PlayerId = userId,
                    PlayerName = leftPlayerName
                });
            await CleanupRoomConversationIfEmptyAsync(roomId, roomCode, userId);
        }
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Bật/tắt trạng thái ready trong phòng.</summary>
    /// <remarks>
    /// Chỉ khi phòng đang Waiting. Host có thể start game khi tất cả đã ready. Trả về chi tiết phòng sau khi toggle. Real-time: broadcast qua SignalR. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/{roomId}/ready
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Body:** None.
    /// </remarks>
    /// <response code="200">Ready toggled. Returns message and data (room detail).</response>
    /// <response code="400">Room not in Waiting state</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/{roomId:guid}/ready")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<LobbyRoomDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Toggle ready", Description = "Toggles ready state in room (Waiting only). Returns updated room detail. Requires Bearer token.", OperationId = "Lobby_ToggleReady", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> ToggleReady(Guid roomId)
    {
        var result = await _mediator.Send(new ToggleLobbyReadyCommand(roomId));
        if (result.IsSuccess && result.Data != null)
        {
            await BroadcastRoomUpdatedToGroupAsync(result.Data.RoomId);
        }
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Đặt game cho phòng (chỉ khi Waiting, thường do host).</summary>
    /// <remarks>
    /// Chỉ khi phòng đang Waiting. Game phải tồn tại và đã publish. Trả về chi tiết phòng sau khi set. Real-time: broadcast qua SignalR. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/lobby/rooms/{roomId}/game
    ///
    /// **Route:** roomId (Guid, required): ID phòng.
    ///
    /// **Body (JSON):**
    /// - gameId (Guid, required): ID game đã publish.
    ///
    /// **Example request body:** { "gameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
    /// </remarks>
    /// <response code="200">Game set. Returns message and data (room detail).</response>
    /// <response code="400">Room not Waiting / game not found or not published</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">Room or game not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rooms/{roomId:guid}/game")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<LobbyRoomDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Set room game", Description = "Sets selected game for room (Waiting only). Game must exist and be published. Requires Bearer token.", OperationId = "Lobby_SetRoomMap", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> SetRoomMap(Guid roomId, [FromBody] SetRoomMapRequest request)
    {
        var result = await _mediator.Send(new SetLobbyRoomMapCommand(roomId, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Dọn phòng rác và room chat tạm đã bỏ trống.</summary>
    /// <remarks>
    /// Dùng để cleanup thủ công:
    /// - Xóa lobby room in-memory không hợp lệ (không còn room hoặc 0 người).
    /// - Đóng + soft-delete room chat temporary group đã đóng hoặc không còn thành viên active.
    /// Yêu cầu Bearer token.
    /// </remarks>
    [HttpPost("rooms/cleanup")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Cleanup garbage rooms", Description = "Cleanup invalid in-memory lobby rooms and orphan temporary chat rooms.", OperationId = "Lobby_CleanupGarbageRooms", Tags = new[] { "Learner - Game Lobby" })]
    public async Task<IActionResult> CleanupGarbageRooms()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;
        Guid.TryParse(userIdClaim, out var actorId);

        var removedLobbyRooms = 0;
        var lobbyRoomsSnapshot = _roomManager.GetLobbyRooms().ToList();
        foreach (var roomItem in lobbyRoomsSnapshot)
        {
            var room = _roomManager.GetRoomById(roomItem.RoomId);
            if (room != null && room.PlayerCount > 0) continue;
            if (_roomManager.RemoveRoom(roomItem.RoomId))
            {
                removedLobbyRooms++;
            }
        }

        var deletedTemporaryChats = 0;
        var closedTemporaryChats = 0;
        var chatRoomRepo = _unitOfWork.Repository<ChatRoom>();
        var temporaryChats = await chatRoomRepo.GetQueryable()
            .Where(c => !c.IsDeleted && c.RoomType == ChatRoomTypeEnum.TemporaryGroup)
            .Select(c => new
            {
                Room = c,
                ActiveMemberCount = c.Members.Count(m => !m.IsDeleted && m.LeftAt == null)
            })
            .ToListAsync();

        var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        var changed = false;
        foreach (var item in temporaryChats)
        {
            if (!item.Room.IsClosed && item.ActiveMemberCount <= 0)
            {
                item.Room.Close(actorId == Guid.Empty ? Guid.Empty : actorId);
                closedTemporaryChats++;
                changed = true;
            }

            if (!item.Room.IsDeleted && (item.Room.IsClosed || item.ActiveMemberCount <= 0))
            {
                item.Room.IsDeleted = true;
                item.Room.DeletedAt = now;
                item.Room.DeletedBy = actorId == Guid.Empty ? Guid.Empty : actorId;
                chatRoomRepo.Update(item.Room);
                deletedTemporaryChats++;
                changed = true;
            }
        }

        if (changed)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        if (removedLobbyRooms > 0)
        {
            await BroadcastLobbyListToAllAsync();
        }

        var payload = new
        {
            RemovedLobbyRooms = removedLobbyRooms,
            ClosedTemporaryChats = closedTemporaryChats,
            DeletedTemporaryChats = deletedTemporaryChats
        };
        var result = Result<object>.Success(payload, "Đã dọn phòng rác thành công.");
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Đồng bộ danh sách phòng mở với mọi client đang xem lobby (REST không gọi Hub trước đây).</summary>
    private async Task BroadcastLobbyListToAllAsync()
    {
        var list = await _mediator.Send(new GetLobbyRoomsQuery());
        if (!list.IsSuccess || list.Data == null) return;
        var payload = list.Data.Select(r => new
        {
            r.RoomId,
            r.RoomCode,
            r.HostId,
            r.HostName,
            r.CurrentPlayerCount,
            r.MaxPlayers,
            Status = r.Status.ToString(),
            r.IsLocked,
            r.SelectedGameId,
            r.SelectedGameTitle
        }).ToList();
        await _hubContext.Clients.Group(GameLobbyHub.LobbyGroupName).SendAsync("LobbyRoomList", payload);
    }

    private async Task BroadcastRoomUpdatedToGroupAsync(Guid roomId)
    {
        var roomResult = await _mediator.Send(new GetLobbyRoomQuery(roomId));
        if (!roomResult.IsSuccess || roomResult.Data == null) return;
        var r = roomResult.Data;
        var dto = new
        {
            r.RoomId,
            r.RoomCode,
            r.HostId,
            CurrentPlayerCount = r.CurrentPlayerCount,
            r.MaxPlayers,
            Status = r.Status.ToString(),
            r.IsLocked,
            r.SelectedGameId,
            Players = r.Players.Select(p => new { p.PlayerId, p.PlayerName, p.IsReady, p.IsHost }).ToList()
        };
        await _hubContext.Clients.Group($"{GameLobbyHub.RoomGroupPrefix}{roomId}").SendAsync("RoomUpdated", dto);
    }

    /// <summary>
    /// Best-effort sync: ensure temporary room chat exists and all room players are members.
    /// This keeps room chat auto-created on room creation and auto-joined on room join.
    /// </summary>
    private async Task EnsureRoomConversationSynchronizedAsync(Guid roomId)
    {
        var roomResult = await _mediator.Send(new GetLobbyRoomQuery(roomId));
        if (!roomResult.IsSuccess || roomResult.Data == null) return;

        var room = roomResult.Data;
        var roomCode = room.RoomCode?.Trim();
        if (string.IsNullOrWhiteSpace(roomCode)) return;
        var conversationName = $"Lobby {roomCode}";

        var chatRoomRepo = _unitOfWork.Repository<ChatRoom>();
        var chatRoom = await chatRoomRepo.GetQueryable()
            .FirstOrDefaultAsync(c =>
                !c.IsDeleted &&
                c.RoomType == ChatRoomTypeEnum.TemporaryGroup &&
                c.Name != null &&
                c.Name.ToLower() == conversationName.ToLower());

        if (chatRoom == null)
        {
            var createChat = await _mediator.Send(new CreateTemporaryGroupConversationCommand
            {
                Name = conversationName
            });
            if (createChat.IsSuccess && createChat.Data != null)
            {
                chatRoom = await chatRoomRepo.GetQueryable()
                    .FirstOrDefaultAsync(c => c.Id == createChat.Data.Id);
            }
            if (chatRoom == null)
            {
                chatRoom = await chatRoomRepo.GetQueryable()
                    .FirstOrDefaultAsync(c =>
                        !c.IsDeleted &&
                        c.RoomType == ChatRoomTypeEnum.TemporaryGroup &&
                        c.Name != null &&
                        c.Name.ToLower() == conversationName.ToLower());
            }
        }

        if (chatRoom == null) return;

        var memberRepo = _unitOfWork.Repository<ChatRoomMember>();
        var existingMemberIds = await memberRepo.GetQueryable()
            .Where(m => !m.IsDeleted && m.ChatRoomId == chatRoom.Id && m.LeftAt == null)
            .Select(m => m.UserId)
            .ToListAsync();
        var existingSet = existingMemberIds.ToHashSet();

        foreach (var player in room.Players)
        {
            if (existingSet.Contains(player.PlayerId)) continue;
            await _mediator.Send(new AddMemberToRoomCommand
            {
                ChatRoomId = chatRoom.Id,
                UserId = player.PlayerId
            });
        }
    }

    private async Task<string> ResolveUserDisplayNameAsync(Guid userId)
    {
        var user = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName, u.UserName })
            .FirstOrDefaultAsync();
        if (user == null) return userId.ToString("N")[..8];
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
        if (!string.IsNullOrWhiteSpace(user.UserName)) return user.UserName;
        return userId.ToString("N")[..8];
    }

    private async Task CleanupRoomConversationIfEmptyAsync(Guid roomId, string? roomCode, Guid closedByUserId)
    {
        if (string.IsNullOrWhiteSpace(roomCode)) return;

        var roomAfterLeave = await _mediator.Send(new GetLobbyRoomQuery(roomId));
        if (roomAfterLeave.IsSuccess && roomAfterLeave.Data != null && roomAfterLeave.Data.CurrentPlayerCount > 0)
            return;

        var conversationName = $"Lobby {roomCode.Trim()}";
        var chatRoomRepo = _unitOfWork.Repository<ChatRoom>();
        var chatRoom = await chatRoomRepo.GetQueryable()
            .FirstOrDefaultAsync(c =>
                !c.IsDeleted &&
                c.RoomType == ChatRoomTypeEnum.TemporaryGroup &&
                c.Name != null &&
                c.Name.ToLower() == conversationName.ToLower());
        if (chatRoom == null) return;

        var actor = closedByUserId == Guid.Empty ? Guid.Empty : closedByUserId;
        if (!chatRoom.IsClosed)
        {
            chatRoom.Close(actor);
        }

        chatRoom.IsDeleted = true;
        chatRoom.DeletedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        chatRoom.DeletedBy = actor;
        chatRoomRepo.Update(chatRoom);
        await _unitOfWork.SaveChangesAsync();
    }
}
