using MediatR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Games.Queries.MapExists;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Hubs;

/// <summary>
/// SignalR hub for multiplayer lobby and room management (Gunny/GunBound style).
/// All IDs are Guid; client sends Guid as string in JSON.
/// </summary>
[Authorize]
public class GameLobbyHub : Hub
{
    public const string LobbyGroupName = "Lobby";
    public const string RoomGroupPrefix = "Room_";

    private readonly IRoomManager _roomManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<GameLobbyHub> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private static readonly ConcurrentDictionary<Guid, ConcurrentQueue<RoomChatMessageDto>> RoomChatStore = new();
    private const int MaxRoomChatMessages = 200;

    public GameLobbyHub(
        IRoomManager roomManager,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<GameLobbyHub> logger,
        IUnitOfWork unitOfWork)
    {
        _roomManager = roomManager;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdStr = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdStr))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userIdStr}");
        await Groups.AddToGroupAsync(Context.ConnectionId, LobbyGroupName);
        await base.OnConnectedAsync();
        await SendLobbyRoomListToClient(Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetUserId(out var userId))
            await LeaveAllRoomsForUser(userId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Create a new room. Creator becomes host and is added to the room. Optionally set game now or later via SetSelectedMap.</summary>
    public async Task CreateRoom(int maxPlayers = 8, Guid? selectedGameId = null)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }
        if (selectedGameId.HasValue && selectedGameId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(selectedGameId.Value));
            if (!mapExists.IsSuccess || mapExists.Data != true)
            {
                await Clients.Caller.SendAsync("Error", mapExists.Message ?? "Bản đồ không được tìm thấy hoặc đã bị xóa.");
                return;
            }
        }
        var existingRoom = _roomManager.GetRoomContainingPlayer(userId);
        if (existingRoom != null)
        {
            await Clients.Caller.SendAsync("Error", "Không thể tạo phòng. Bạn đã ở trong một phòng rồi. Vui lòng rời phòng hiện tại trước khi tạo phòng mới.");
            await Clients.Caller.SendAsync("AlreadyInRoom", ToRoomDto(existingRoom));
            return;
        }
        var room = _roomManager.CreateRoom(userId, Context.ConnectionId, maxPlayers, selectedGameId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Không tạo được phòng.");
            return;
        }

        EnsureRoomChat(room.RoomId);

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(room.RoomId));
        await Clients.Caller.SendAsync("RoomCreated", ToRoomDto(room));
        await BroadcastLobbyRoomList();
        _logger.LogInformation("User {UserId} created room {RoomId} ({RoomCode})", userId, room.RoomId, room.RoomCode);
    }

    /// <summary>Join a room by roomId (Guid). Optionally pass roomCode if room is locked.</summary>
    public async Task JoinRoom(Guid roomId, string? roomCode = null)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var roomSnapshot = _roomManager.GetRoomById(roomId);
        if (roomSnapshot?.SelectedGameId is { } selectedGameId && selectedGameId != Guid.Empty)
        {
            var canJoinSelectedMap = await EnsureUserOwnsPaidGame(selectedGameId, userId);
            if (!canJoinSelectedMap.Success)
            {
                await Clients.Caller.SendAsync("Error", canJoinSelectedMap.ErrorMessage ?? "Khong the vao phong.");
                return;
            }
        }

        var (success, errorMessage, room) = _roomManager.JoinRoom(roomId, userId, Context.ConnectionId, roomCode);
        if (!success)
        {
            if (errorMessage?.Contains("Already", StringComparison.OrdinalIgnoreCase) == true)
            {
                var existingRoom = _roomManager.GetRoomById(roomId);
                if (existingRoom != null && _roomManager.UpdatePlayerConnectionId(roomId, userId, Context.ConnectionId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomId));
                    await Clients.Caller.SendAsync("JoinedRoom", ToRoomDto(existingRoom));
                    _logger.LogInformation("User {UserId} re-registered connection in room {RoomId}", userId, roomId);
                    return;
                }
            }
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not join room.");
            return;
        }

        if (room == null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(room.RoomId));
        await BroadcastRoomUpdated(room);
        await Clients.Caller.SendAsync("JoinedRoom", ToRoomDto(room));
        await BroadcastLobbyRoomList();
        _logger.LogInformation("User {UserId} joined room {RoomId}", userId, room.RoomId);
    }

    /// <summary>Join a room using only the room code.</summary>
    public async Task JoinRoomByCode(string roomCode)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var roomByCode = _roomManager.GetRoomByCode(roomCode.Trim());
        if (roomByCode?.SelectedGameId is { } selectedGameId && selectedGameId != Guid.Empty)
        {
            var canJoinSelectedMap = await EnsureUserOwnsPaidGame(selectedGameId, userId);
            if (!canJoinSelectedMap.Success)
            {
                await Clients.Caller.SendAsync("Error", canJoinSelectedMap.ErrorMessage ?? "Khong the vao phong.");
                return;
            }
        }

        var (success, errorMessage, room) = _roomManager.JoinRoomByCode(roomCode.Trim(), userId, Context.ConnectionId);
        if (!success || room == null)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not join room.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(room.RoomId));
        await BroadcastRoomUpdated(room);
        await Clients.Caller.SendAsync("JoinedRoom", ToRoomDto(room));
        await BroadcastLobbyRoomList();
        _logger.LogInformation("User {UserId} joined room {RoomId} by code {RoomCode}", userId, room.RoomId, room.RoomCode);
    }

    /// <summary>Leave the current room.</summary>
    public async Task LeaveRoom(Guid roomId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var leftPlayerName = await GetUserDisplayNameAsync(userId);
        var (success, errorMessage, updatedRoom) = _roomManager.LeaveRoom(roomId, userId);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Không thể rời khỏi phòng.");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(roomId));
        await Clients.Caller.SendAsync("LeftRoom", roomId);

        if (updatedRoom != null)
            await BroadcastRoomUpdated(updatedRoom);
        else
        {
            _roomManager.RemoveRoom(roomId);
            RemoveRoomChat(roomId);
        }

        await Clients.Group(RoomGroup(roomId)).SendAsync("PlayerLeftRoom", new
        {
            RoomId = roomId,
            PlayerId = userId,
            PlayerName = leftPlayerName
        });
        await BroadcastLobbyRoomList();
        _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
    }

    /// <summary>Toggle ready state in the room.</summary>
    public async Task ToggleReady(Guid roomId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var (success, errorMessage, room) = _roomManager.ToggleReady(roomId, userId);
        if (!success || room == null)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Không thể chuyển đổi sẵn sàng.");
            return;
        }

        await BroadcastRoomUpdated(room);
    }

    /// <summary>Start the game. Requires host, at least 2 players, and all ready.</summary>
    public async Task StartGame(Guid roomId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }
        var room = _roomManager.GetRoomById(roomId);
        if (room?.SelectedGameId is { } selectedGameId && selectedGameId != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(selectedGameId));
            if (!mapExists.IsSuccess || mapExists.Data != true)
            {
                await Clients.Caller.SendAsync("Error", mapExists.Message ?? "Bản đồ không được tìm thấy hoặc đã bị xóa. Chọn bản đồ khác.");
                return;
            }
            var playerIds = room.Players.Keys.ToList();
            var ownershipCheck = await EnsurePlayersCanPlayGame(selectedGameId, playerIds);
            if (!ownershipCheck.Success)
            {
                await Clients.Caller.SendAsync("Error", ownershipCheck.ErrorMessage ?? "Tất cả người chơi trong phòng phải sở hữu bản đồ.");
                return;
            }
        }
        var (success, errorMessage, gameInstance, updatedRoom) = _roomManager.StartGame(roomId, userId);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Không thể bắt đầu trò chơi.");
            return;
        }

        if (updatedRoom != null)
            await BroadcastRoomUpdated(updatedRoom);
        var state = gameInstance!.GameState as LobbyGameState;
        await Clients.Group(RoomGroup(roomId)).SendAsync("GameStarted", new
        {
            gameInstance.RoomId,
            gameInstance.RoomCode,
            gameInstance.GameId,
            Players = gameInstance.Players.Select(p => new { p.PlayerId, p.IsReady, p.IsHost }).ToList(),
            gameInstance.TurnOrder,
            GameState = state != null ? new { state.CurrentTurnIndex, state.CurrentPlayerId, state.RoundNumber } : null,
            gameInstance.StartedAt
        });
        await BroadcastLobbyRoomList();
        _logger.LogInformation("Game started in room {RoomId}", roomId);
    }

    /// <summary>Kick a player from the room. Host only.</summary>
    public async Task KickPlayer(Guid roomId, Guid targetPlayerId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var (success, errorMessage, room) = _roomManager.KickPlayer(roomId, userId, targetPlayerId);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not kick player.");
            return;
        }

        if (room != null)
        {
            await BroadcastRoomUpdated(room);
            await Clients.Group($"User_{targetPlayerId}").SendAsync("KickedFromRoom", new { RoomId = roomId });
        }

        await BroadcastLobbyRoomList();
    }

    /// <summary>Lock or unlock the room. Host only.</summary>
    public async Task SetRoomLocked(Guid roomId, bool isLocked)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var (success, errorMessage, room) = _roomManager.SetRoomLocked(roomId, userId, isLocked);
        if (!success || room == null)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not update room lock.");
            return;
        }

        await BroadcastRoomUpdated(room);
        await BroadcastLobbyRoomList();
    }

    /// <summary>Request current lobby room list (e.g. on tab focus).</summary>
    public async Task GetLobbyRooms()
    {
        await SendLobbyRoomListToClient(Context.ConnectionId);
    }

    /// <summary>End the current game. Room returns to Waiting; everyone unready. Any player in the room can call.</summary>
    public async Task EndGame(Guid roomId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var (success, errorMessage, room) = _roomManager.EndGame(roomId, userId);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Không thể kết thúc trò chơi.");
            return;
        }

        if (room != null)
            await BroadcastRoomUpdated(room);
        await Clients.Group(RoomGroup(roomId)).SendAsync("GameEnded", new { RoomId = roomId });
        await BroadcastLobbyRoomList();
        _logger.LogInformation("Game ended in room {RoomId}", roomId);
    }

    /// <summary>Set or change the selected game for the room. Host only; room must be Waiting.</summary>
    public async Task SetSelectedMap(Guid roomId, Guid? gameId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }
        if (gameId.HasValue && gameId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(gameId.Value));
            if (!mapExists.IsSuccess || mapExists.Data != true)
            {
                await Clients.Caller.SendAsync("Error", mapExists.Message ?? "Bản đồ không được tìm thấy hoặc đã bị xóa.");
                return;
            }
            var roomForOwnership = _roomManager.GetRoomById(roomId);
            if (roomForOwnership == null)
            {
                await Clients.Caller.SendAsync("Error", "Không tìm thấy phòng.");
                return;
            }
            var ownershipCheck = await EnsurePlayersCanPlayGame(gameId.Value, roomForOwnership.Players.Keys.ToList());
            if (!ownershipCheck.Success)
            {
                await Clients.Caller.SendAsync("Error", ownershipCheck.ErrorMessage ?? "Tất cả người chơi trong phòng phải sở hữu bản đồ.");
                return;
            }
        }
        var (success, errorMessage, room) = _roomManager.SetRoomMap(roomId, userId, gameId);
        if (!success || room == null)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Không thể thiết lập bản đồ.");
            return;
        }

        await BroadcastRoomUpdated(room);
        await BroadcastLobbyRoomList();
    }

    /// <summary>Submit solution for the current game. Server validates with room game, records score; when all have submitted, broadcasts RankingUpdated to the room.</summary>
    public async Task SubmitSolution(
        Guid roomId,
        string? astSpec,
        string? bytecodeSpec,
        string? language = null,
        bool? isWin = null,
        int? stepsUsed = null,
        int? blocksUsed = null,
        double? timeSeconds = null,
        Guid? mapDetailId = null)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var room = _roomManager.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Không tìm thấy phòng.");
            return;
        }
        if (room.Status != RoomStatusEnum.Playing)
        {
            await Clients.Caller.SendAsync("Error", "Trò chơi không được tiến hành.");
            return;
        }
        if (!room.Players.ContainsKey(userId))
        {
            await Clients.Caller.SendAsync("Error", "Bạn không ở trong phòng này.");
            return;
        }

        var gameInstance = _roomManager.GetGameInstance(roomId);
        if (gameInstance == null || !gameInstance.GameId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "No game for this game.");
            return;
        }

        var validateRequest = new ValidateSolutionRequest
        {
            GameId = gameInstance.GameId.Value,
            GameDetailId = mapDetailId,
            Language = language ?? "Blockly",
            AstSpec = astSpec,
            BytecodeSpec = bytecodeSpec,
            PlayMode = PlayModeEnum.Lobby,
            RoomId = roomId,
            IsWin = isWin,
            ClientStepsUsed = stepsUsed,
            ClientBlocksUsed = blocksUsed,
            ClientElapsedSeconds = timeSeconds
        };
        var validateResult = await _mediator.Send(new ValidateSolutionCommand(validateRequest));
        if (!validateResult.IsSuccess || validateResult.Data == null)
        {
            await Clients.Caller.SendAsync("SubmissionResult", new { Success = false, Message = validateResult.Message ?? "Xác thực không thành công." });
            return;
        }

        var score = validateResult.Data.Score ?? 0;
        var status = validateResult.Data.Status.ToString();
        var (recordSuccess, recordError, ranking) = _roomManager.RecordSubmission(
            roomId,
            userId,
            score,
            status,
            validateResult.Data.SubmissionId,
            mapDetailId,
            stepsUsed,
            blocksUsed,
            timeSeconds);
        if (!recordSuccess)
        {
            await Clients.Caller.SendAsync("SubmissionResult", new { Success = false, Message = recordError ?? "Không thể ghi lại bài nộp." });
            return;
        }

        await Clients.Caller.SendAsync("SubmissionResult", new
        {
            Success = true,
            Score = score,
            Status = status,
            SubmissionId = validateResult.Data.SubmissionId,
            Message = validateResult.Data.Message
        });
        if (ranking != null && ranking.Count > 0)
        {
            var rankingPayload = new
            {
                RoomId = roomId,
                Ranking = ranking
            };

            await Clients.Group(RoomGroup(roomId)).SendAsync("RankingUpdated", rankingPayload);

            // Also push to per-user groups so result screens still receive realtime updates
            // even when the connection is temporarily not in the room SignalR group.
            var roomPlayerGroupNames = room.Players.Keys
                .Select(playerId => $"User_{playerId}")
                .ToList();
            if (roomPlayerGroupNames.Count > 0)
            {
                await Clients.Groups(roomPlayerGroupNames).SendAsync("RankingUpdated", rankingPayload);
            }
        }

        _logger.LogInformation("User {UserId} submitted in room {RoomId}; score={Score}, ranking prepared={HasRanking}", userId, roomId, score, ranking?.Count > 0);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var str = _currentUserService.UserId;
        return !string.IsNullOrEmpty(str) && Guid.TryParse(str, out userId);
    }

    private static string RoomGroup(Guid roomId) => $"{RoomGroupPrefix}{roomId}";

    private async Task<(bool Success, string? ErrorMessage)> EnsurePlayersCanPlayGame(Guid gameId, List<Guid> playerIds)
    {
        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game == null || game.IsDeleted)
            return (false, "Bản đồ không được tìm thấy hoặc đã bị xóa.");

        var price = game.Price.GetValueOrDefault();
        if (price <= 0)
            return (true, null);

        var paidUserIds = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(p => !p.IsDeleted
                        && p.GameId == game.Id
                        && p.PaymentStatus == PaymentStatusEnum.Completed
                        && playerIds.Contains(p.UserId))
            .Select(p => p.UserId)
            .Distinct()
            .ToListAsync();

        var myGameUserIds = await _unitOfWork.Repository<MyGame>().GetQueryable()
            .Where(mg => !mg.IsDeleted
                         && mg.GameId == game.Id
                         && playerIds.Contains(mg.UserId))
            .Select(mg => mg.UserId)
            .Distinct()
            .ToListAsync();

        var ownedUserIds = paidUserIds.Concat(myGameUserIds).ToHashSet();
        foreach (var playerId in playerIds)
        {
            if (game.CreatedBy == playerId || ownedUserIds.Contains(playerId))
                continue;
            return (false, "Tất cả người chơi trong phòng phải sở hữu bản đồ trước khi chơi.");
        }
        return (true, null);
    }

    private async Task<(bool Success, string? ErrorMessage)> EnsureUserOwnsPaidGame(Guid gameId, Guid userId)
    {
        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (game == null || game.IsDeleted)
            return (false, "Khong the vao phong: tro choi da chon khong con ton tai.");

        if (game.CreatedBy == userId || game.Price.GetValueOrDefault() <= 0)
            return (true, null);

        var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .AnyAsync(p => !p.IsDeleted
                           && p.UserId == userId
                           && p.GameId == game.Id
                           && p.PaymentStatus == PaymentStatusEnum.Completed);
        if (purchased)
            return (true, null);

        var inMyGame = await _unitOfWork.Repository<MyGame>().GetQueryable()
            .AnyAsync(mg => !mg.IsDeleted && mg.UserId == userId && mg.GameId == game.Id);
        if (inMyGame)
            return (true, null);

        return (false, "Khong the vao phong: ban chua so huu tro choi dang duoc chon.");
    }

    private async Task LeaveAllRoomsForUser(Guid userId)
    {
        var rooms = _roomManager.GetLobbyRooms();
        var leftPlayerName = await GetUserDisplayNameAsync(userId);
        foreach (var r in rooms)
        {
            var room = _roomManager.GetRoomById(r.RoomId);
            if (room?.Players.ContainsKey(userId) != true)
                continue;
            var (_, _, updatedRoom) = _roomManager.LeaveRoom(room.RoomId, userId);
            if (updatedRoom != null)
                await BroadcastRoomUpdated(updatedRoom);
            else
            {
                _roomManager.RemoveRoom(room.RoomId);
                RemoveRoomChat(room.RoomId);
            }
            await Clients.Group(RoomGroup(room.RoomId)).SendAsync("PlayerLeftRoom", new
            {
                RoomId = room.RoomId,
                PlayerId = userId,
                PlayerName = leftPlayerName
            });
            await BroadcastLobbyRoomList();
            break;
        }
    }

    private async Task<string> GetUserDisplayNameAsync(Guid userId)
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

    public async Task<List<object>> GetRoomChatMessages(Guid roomId, int take = 50)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return new List<object>();
        }

        var room = _roomManager.GetRoomById(roomId);
        if (room?.Players.ContainsKey(userId) != true)
        {
            await Clients.Caller.SendAsync("Error", "Bạn không ở trong phòng này.");
            return new List<object>();
        }

        var queue = EnsureRoomChat(roomId);

        return queue.ToArray()
            .TakeLast(Math.Clamp(take, 1, 200))
            .Select(m => (object)m)
            .ToList();
    }

    public async Task SendRoomChatMessage(Guid roomId, string content)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var room = _roomManager.GetRoomById(roomId);
        if (room?.Players.ContainsKey(userId) != true)
        {
            await Clients.Caller.SendAsync("Error", "Bạn không ở trong phòng này.");
            return;
        }

        var text = (content ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        var message = new RoomChatMessageDto
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = userId,
            SenderName = await GetUserDisplayNameAsync(userId),
            Content = text.Length > 1000 ? text[..1000] : text,
            CreatedAt = DateTime.UtcNow
        };

        var queue = EnsureRoomChat(roomId);
        queue.Enqueue(message);
        while (queue.Count > MaxRoomChatMessages && queue.TryDequeue(out _))
        {
            // trim old messages in-memory
        }

        await Clients.Group(RoomGroup(roomId)).SendAsync("RoomChatMessage", message);
    }

    private async Task SendLobbyRoomListToClient(string connectionId)
    {
        var list = _roomManager.GetLobbyRooms().Select(r => new
        {
            r.RoomId,
            r.RoomCode,
            r.HostId,
            r.CurrentPlayerCount,
            r.MaxPlayers,
            Status = r.Status.ToString(),
            r.IsLocked,
            r.SelectedGameId
        }).ToList();
        await Clients.Client(connectionId).SendAsync("LobbyRoomList", list);
    }

    private async Task BroadcastLobbyRoomList()
    {
        var list = _roomManager.GetLobbyRooms().Select(r => new
        {
            r.RoomId,
            r.RoomCode,
            r.HostId,
            r.CurrentPlayerCount,
            r.MaxPlayers,
            Status = r.Status.ToString(),
            r.IsLocked,
            r.SelectedGameId
        }).ToList();
        await Clients.Group(LobbyGroupName).SendAsync("LobbyRoomList", list);
    }

    private async Task BroadcastRoomUpdated(LobbyRoom room)
    {
        var dto = ToRoomDto(room);
        await Clients.Group(RoomGroup(room.RoomId)).SendAsync("RoomUpdated", dto);
    }

    private static object ToRoomDto(LobbyRoom room)
    {
        return new
        {
            room.RoomId,
            room.RoomCode,
            room.HostId,
            CurrentPlayerCount = room.PlayerCount,
            room.MaxPlayers,
            Status = room.Status.ToString(),
            room.IsLocked,
            room.SelectedGameId,
            Players = room.Players.Values.Select(p => new { p.PlayerId, p.IsReady, p.IsHost }).ToList()
        };
    }

    public static ConcurrentQueue<RoomChatMessageDto> EnsureRoomChat(Guid roomId)
        => RoomChatStore.GetOrAdd(roomId, _ => new ConcurrentQueue<RoomChatMessageDto>());

    public static void RemoveRoomChat(Guid roomId)
        => RoomChatStore.TryRemove(roomId, out _);
}

public class RoomChatMessageDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
