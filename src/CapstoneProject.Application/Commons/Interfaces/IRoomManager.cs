using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Features.Lobby.Models;

namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// In-memory lobby room management (Gunny/GunBound style).
/// Thread-safe; no database persistence for rooms.
/// </summary>
public interface IRoomManager
{
    /// <summary>Get all rooms for lobby listing (only waiting rooms are joinable).</summary>
    IReadOnlyList<LobbyRoomListItemDto> GetLobbyRooms();

    /// <summary>Get room by id, or null if not found.</summary>
    LobbyRoom? GetRoomById(Guid roomId);

    /// <summary>Get room by room code (case-insensitive), or null if not found.</summary>
    LobbyRoom? GetRoomByCode(string roomCode);

    /// <summary>Create a new room; creator becomes host. Returns the created room or null on failure. hostConnectionId can be empty when creating via API.</summary>
    LobbyRoom? CreateRoom(Guid hostPlayerId, string hostConnectionId, int maxPlayers = 8, Guid? selectedMapId = null);

    /// <summary>Set or change the selected map for the room. Host only; room must be Waiting.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) SetRoomMap(Guid roomId, Guid hostPlayerId, Guid? mapId);

    /// <summary>Join room by roomId (e.g. from lobby). Validates room exists, waiting, not full, and lock/code if locked.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) JoinRoom(Guid roomId, Guid playerId, string connectionId, string? roomCode = null);

    /// <summary>Join room by roomCode only. Same validation as JoinRoom.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) JoinRoomByCode(string roomCode, Guid playerId, string connectionId);

    /// <summary>Remove player from room. If host leaves and status is Waiting, migrates host to next player.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? UpdatedRoom) LeaveRoom(Guid roomId, Guid playerId);

    /// <summary>Toggle ready state for a player in the room.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) ToggleReady(Guid roomId, Guid playerId);

    /// <summary>Start game: requires at least 2 players, all ready, caller is host. Creates GameInstance and sets room to Playing.</summary>
    (bool Success, string? ErrorMessage, GameInstance? GameInstance, LobbyRoom? Room) StartGame(Guid roomId, Guid hostPlayerId);

    /// <summary>Kick a player from the room. Only host can kick.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) KickPlayer(Guid roomId, Guid hostPlayerId, Guid targetPlayerId);

    /// <summary>Lock or unlock the room. Only host can change.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) SetRoomLocked(Guid roomId, Guid hostPlayerId, bool isLocked);

    /// <summary>Get the active game instance for a room, if any.</summary>
    GameInstance? GetGameInstance(Guid roomId);

    /// <summary>Record a player's submission (score, status). Returns ranking when all players have submitted.</summary>
    (bool Success, string? ErrorMessage, IReadOnlyList<PlayerRankingDto>? RankingIfAllSubmitted) RecordSubmission(Guid roomId, Guid playerId, int score, string status, Guid? submissionId = null);

    /// <summary>End the current game: remove GameInstance, set room back to Waiting so room can start again.</summary>
    (bool Success, string? ErrorMessage, LobbyRoom? Room) EndGame(Guid roomId, Guid requestedByPlayerId);

    /// <summary>Remove room from memory (e.g. when game ends).</summary>
    bool RemoveRoom(Guid roomId);

    /// <summary>Update player's SignalR connectionId (e.g. after joining via API and then connecting to hub).</summary>
    bool UpdatePlayerConnectionId(Guid roomId, Guid playerId, string connectionId);
}
