using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Maps.Queries.MapExists;
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

    public GameLobbyHub(
        IRoomManager roomManager,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<GameLobbyHub> logger)
    {
        _roomManager = roomManager;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _logger = logger;
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

    /// <summary>Create a new room. Creator becomes host and is added to the room. Optionally set map now or later via SetSelectedMap.</summary>
    public async Task CreateRoom(int maxPlayers = 8, Guid? selectedMapId = null)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }
        if (selectedMapId.HasValue && selectedMapId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(selectedMapId.Value));
            if (!mapExists.IsSuccess || mapExists.Data != true)
            {
                await Clients.Caller.SendAsync("Error", mapExists.Message ?? "Map not found or has been deleted.");
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
        var room = _roomManager.CreateRoom(userId, Context.ConnectionId, maxPlayers, selectedMapId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Failed to create room.");
            return;
        }

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

        var (success, errorMessage, updatedRoom) = _roomManager.LeaveRoom(roomId, userId);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not leave room.");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(roomId));
        await Clients.Caller.SendAsync("LeftRoom", roomId);

        if (updatedRoom != null)
            await BroadcastRoomUpdated(updatedRoom);
        else
            _roomManager.RemoveRoom(roomId);

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
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not toggle ready.");
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
        if (room?.SelectedMapId is { } selectedMapId && selectedMapId != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(selectedMapId));
            if (!mapExists.IsSuccess || mapExists.Data != true)
            {
                await Clients.Caller.SendAsync("Error", mapExists.Message ?? "Map not found or has been deleted. Choose another map.");
                return;
            }
        }
        var (success, errorMessage, gameInstance, updatedRoom) = _roomManager.StartGame(roomId, userId);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not start game.");
            return;
        }

        if (updatedRoom != null)
            await BroadcastRoomUpdated(updatedRoom);
        var state = gameInstance!.GameState as LobbyGameState;
        await Clients.Group(RoomGroup(roomId)).SendAsync("GameStarted", new
        {
            gameInstance.RoomId,
            gameInstance.RoomCode,
            gameInstance.MapId,
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
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not end game.");
            return;
        }

        if (room != null)
            await BroadcastRoomUpdated(room);
        await Clients.Group(RoomGroup(roomId)).SendAsync("GameEnded", new { RoomId = roomId });
        await BroadcastLobbyRoomList();
        _logger.LogInformation("Game ended in room {RoomId}", roomId);
    }

    /// <summary>Set or change the selected map for the room. Host only; room must be Waiting.</summary>
    public async Task SetSelectedMap(Guid roomId, Guid? mapId)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }
        if (mapId.HasValue && mapId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(mapId.Value));
            if (!mapExists.IsSuccess || mapExists.Data != true)
            {
                await Clients.Caller.SendAsync("Error", mapExists.Message ?? "Map not found or has been deleted.");
                return;
            }
        }
        var (success, errorMessage, room) = _roomManager.SetRoomMap(roomId, userId, mapId);
        if (!success || room == null)
        {
            await Clients.Caller.SendAsync("Error", errorMessage ?? "Could not set map.");
            return;
        }

        await BroadcastRoomUpdated(room);
        await BroadcastLobbyRoomList();
    }

    /// <summary>Submit solution for the current game. Server validates with room map, records score; when all have submitted, broadcasts RankingUpdated to the room.</summary>
    public async Task SubmitSolution(
        Guid roomId,
        string? astSpec,
        string? bytecodeSpec,
        string? language = null,
        bool? isWin = null,
        int? stepsUsed = null,
        int? blocksUsed = null,
        double? timeSeconds = null)
    {
        if (!TryGetUserId(out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authenticated.");
            return;
        }

        var room = _roomManager.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Room not found.");
            return;
        }
        if (room.Status != RoomStatusEnum.Playing)
        {
            await Clients.Caller.SendAsync("Error", "Game is not in progress.");
            return;
        }
        if (!room.Players.ContainsKey(userId))
        {
            await Clients.Caller.SendAsync("Error", "You are not in this room.");
            return;
        }

        var gameInstance = _roomManager.GetGameInstance(roomId);
        if (gameInstance == null || !gameInstance.MapId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "No map for this game.");
            return;
        }

        var validateRequest = new ValidateSolutionRequest
        {
            MapId = gameInstance.MapId.Value,
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
            await Clients.Caller.SendAsync("SubmissionResult", new { Success = false, Message = validateResult.Message ?? "Validation failed." });
            return;
        }

        var score = validateResult.Data.Score ?? 0;
        var status = validateResult.Data.Status.ToString();
        var (recordSuccess, recordError, ranking) = _roomManager.RecordSubmission(roomId, userId, score, status, validateResult.Data.SubmissionId);
        if (!recordSuccess)
        {
            await Clients.Caller.SendAsync("SubmissionResult", new { Success = false, Message = recordError ?? "Could not record submission." });
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
            await Clients.Group(RoomGroup(roomId)).SendAsync("RankingUpdated", ranking);

        _logger.LogInformation("User {UserId} submitted in room {RoomId}; score={Score}, ranking broadcast={HasRanking}", userId, roomId, score, ranking?.Count > 0);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var str = _currentUserService.UserId;
        return !string.IsNullOrEmpty(str) && Guid.TryParse(str, out userId);
    }

    private static string RoomGroup(Guid roomId) => $"{RoomGroupPrefix}{roomId}";

    private async Task LeaveAllRoomsForUser(Guid userId)
    {
        var rooms = _roomManager.GetLobbyRooms();
        foreach (var r in rooms)
        {
            var room = _roomManager.GetRoomById(r.RoomId);
            if (room?.Players.ContainsKey(userId) != true)
                continue;
            var (_, _, updatedRoom) = _roomManager.LeaveRoom(room.RoomId, userId);
            if (updatedRoom != null)
                await BroadcastRoomUpdated(updatedRoom);
            else
                _roomManager.RemoveRoom(room.RoomId);
            await BroadcastLobbyRoomList();
            break;
        }
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
            r.SelectedMapId
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
            r.SelectedMapId
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
            room.SelectedMapId,
            Players = room.Players.Values.Select(p => new { p.PlayerId, p.IsReady, p.IsHost }).ToList()
        };
    }
}
