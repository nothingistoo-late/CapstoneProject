using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Infrastructure.Services;

/// <summary>
/// In-memory, thread-safe lobby room manager (Gunny/GunBound style).
/// </summary>
public class RoomManager : IRoomManager
{
    private static readonly ConcurrentDictionary<Guid, LobbyRoom> _rooms = new();
    private static readonly ConcurrentDictionary<Guid, GameInstance> _gameInstances = new();
    private static readonly ConcurrentDictionary<string, Guid> _roomIdByCode = new(StringComparer.OrdinalIgnoreCase);
    private const int RoomCodeLength = 6;
    private static readonly Random _random = new();

    public IReadOnlyList<LobbyRoomListItemDto> GetLobbyRooms()
    {
        return _rooms.Values
            .Select(r => new LobbyRoomListItemDto
            {
                RoomId = r.RoomId,
                RoomCode = r.RoomCode,
                HostId = r.HostId,
                CurrentPlayerCount = r.PlayerCount,
                MaxPlayers = r.MaxPlayers,
                Status = r.Status,
                IsLocked = r.IsLocked,
                SelectedGameId = r.SelectedGameId
            })
            .ToList();
    }

    public LobbyRoom? GetRoomById(Guid roomId)
    {
        return _rooms.TryGetValue(roomId, out var room) ? room : null;
    }

    public LobbyRoom? GetRoomByCode(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode)) return null;
        return _roomIdByCode.TryGetValue(roomCode.Trim(), out var roomId) && _rooms.TryGetValue(roomId, out var room)
            ? room
            : null;
    }

    public LobbyRoom? GetRoomContainingPlayer(Guid playerId)
    {
        if (playerId == Guid.Empty) return null;
        return _rooms.Values.FirstOrDefault(r => r.Players.ContainsKey(playerId));
    }

    public LobbyRoom? CreateRoom(Guid hostPlayerId, string hostConnectionId, int maxPlayers = 8, Guid? selectedGameId = null)
    {
        if (hostPlayerId == Guid.Empty) return null;
        maxPlayers = Math.Clamp(maxPlayers, 2, 16);

        var roomId = Guid.NewGuid();
        var roomCode = GenerateRoomCode();

        while (_roomIdByCode.ContainsKey(roomCode))
            roomCode = GenerateRoomCode();

        var room = new LobbyRoom
        {
            RoomId = roomId,
            RoomCode = roomCode,
            HostId = hostPlayerId,
            MaxPlayers = maxPlayers,
            Status = RoomStatusEnum.Waiting,
            IsLocked = false,
            SelectedGameId = selectedGameId
        };

        var host = new LobbyPlayer
        {
            PlayerId = hostPlayerId,
            ConnectionId = hostConnectionId ?? string.Empty,
            IsReady = false,
            IsHost = true
        };
        room.Players[hostPlayerId] = host;

        if (!_rooms.TryAdd(roomId, room))
            return null;
        _roomIdByCode[roomCode] = roomId;
        return room;
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) JoinRoom(Guid roomId, Guid playerId, string connectionId, string? roomCode = null)
    {
        if (playerId == Guid.Empty)
            return (false, "Invalid player.", null);

        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (room.Status != RoomStatusEnum.Waiting)
            return (false, "Room is not accepting players.", null);
        if (room.Players.ContainsKey(playerId))
            return (false, "Already in this room.", null);
        if (room.IsFull)
            return (false, "Room is full.", null);
        if (room.IsLocked)
        {
            if (string.IsNullOrWhiteSpace(roomCode) || !string.Equals(room.RoomCode, roomCode.Trim(), StringComparison.OrdinalIgnoreCase))
                return (false, "Room is locked. Provide the correct room code.", null);
        }

        var player = new LobbyPlayer
        {
            PlayerId = playerId,
            ConnectionId = connectionId ?? string.Empty,
            IsReady = false,
            IsHost = false
        };
        if (!room.Players.TryAdd(playerId, player))
            return (false, "Could not join room.", null);

        return (true, null, room);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) JoinRoomByCode(string roomCode, Guid playerId, string connectionId)
    {
        var room = GetRoomByCode(roomCode);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        return JoinRoom(room.RoomId, playerId, connectionId, roomCode);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? UpdatedRoom) LeaveRoom(Guid roomId, Guid playerId)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (!room.Players.TryRemove(playerId, out _))
            return (false, "Player not in room.", null);

        if (room.Players.IsEmpty)
        {
            _rooms.TryRemove(roomId, out _);
            _roomIdByCode.TryRemove(room.RoomCode, out _);
            _gameInstances.TryRemove(roomId, out _);
            return (true, null, null);
        }

        // Host rá»i (Waiting hoáº·c Ä‘ang chÆ¡i): chuyá»ƒn host cho ngÆ°á»i cÃ²n láº¡i
        if (room.HostId == playerId)
        {
            foreach (var p in room.Players.Values) p.IsHost = false;
            var nextHost = room.Players.Values.OrderBy(p => p.PlayerId).First();
            room.HostId = nextHost.PlayerId;
            nextHost.IsHost = true;
        }

        // Äang chÆ¡i: gá»¡ ngÆ°á»i rá»i khá»i GameInstance Ä‘á»ƒ ranking chá»‰ cáº§n ná»™p Ä‘á»§ sá»‘ ngÆ°á»i cÃ²n láº¡i
        if (room.Status == RoomStatusEnum.Playing && _gameInstances.TryGetValue(roomId, out var gi))
        {
            var stillIn = gi.Players.Where(p => room.Players.ContainsKey(p.PlayerId)).ToList();
            gi.Players = stillIn;
            gi.TurnOrder = stillIn.Select(p => p.PlayerId).ToList();
        }

        return (true, null, room);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) ToggleReady(Guid roomId, Guid playerId)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (room.Status != RoomStatusEnum.Waiting)
            return (false, "Game already started.", null);
        if (!room.Players.TryGetValue(playerId, out var player))
            return (false, "Player not in room.", null);

        player.IsReady = !player.IsReady;
        return (true, null, room);
    }

    public (bool Success, string? ErrorMessage, GameInstance? GameInstance, LobbyRoom? Room) StartGame(Guid roomId, Guid hostPlayerId)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null, null);
        if (room.Status != RoomStatusEnum.Waiting)
            return (false, "Game already started.", null, null);
        if (room.HostId != hostPlayerId)
            return (false, "Only the host can start the game.", null, null);
        if (room.PlayerCount < 2)
            return (false, "At least 2 players are required to start.", null, null);
        if (room.Players.Values.Any(p => !p.IsReady))
            return (false, "All players must be ready.", null, null);

        room.Status = RoomStatusEnum.Playing;
        var playersList = room.Players.Values.ToList();
        var turnOrder = playersList.Select(p => p.PlayerId).ToList();
        var firstPlayerId = turnOrder.Count > 0 ? turnOrder[0] : Guid.Empty;

        var gameInstance = new GameInstance
        {
            RoomId = room.RoomId,
            RoomCode = room.RoomCode,
            GameId = room.SelectedGameId,
            Players = playersList,
            TurnOrder = turnOrder,
            GameState = new LobbyGameState
            {
                CurrentTurnIndex = 0,
                CurrentPlayerId = firstPlayerId,
                RoundNumber = 1
            },
            StartedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };
        _gameInstances[roomId] = gameInstance;

        return (true, null, gameInstance, room);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) KickPlayer(Guid roomId, Guid hostPlayerId, Guid targetPlayerId)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (room.HostId != hostPlayerId)
            return (false, "Only the host can kick players.", null);
        if (room.Status != RoomStatusEnum.Waiting)
            return (false, "Cannot kick after game has started.", null);
        if (targetPlayerId == hostPlayerId)
            return (false, "Host cannot kick themselves.", null);
        if (!room.Players.TryRemove(targetPlayerId, out _))
            return (false, "Player not in room.", null);

        return (true, null, room);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) SetRoomLocked(Guid roomId, Guid hostPlayerId, bool isLocked)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (room.HostId != hostPlayerId)
            return (false, "Only the host can lock/unlock the room.", null);
        if (room.Status != RoomStatusEnum.Waiting)
            return (false, "Cannot change lock after game has started.", null);

        room.IsLocked = isLocked;
        return (true, null, room);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) SetRoomMap(Guid roomId, Guid hostPlayerId, Guid? gameId)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (room.HostId != hostPlayerId)
            return (false, "Only the host can set the game.", null);
        if (room.Status != RoomStatusEnum.Waiting)
            return (false, "Cannot change game after game has started.", null);

        room.SelectedGameId = gameId;
        return (true, null, room);
    }

    public GameInstance? GetGameInstance(Guid roomId)
    {
        return _gameInstances.TryGetValue(roomId, out var instance) ? instance : null;
    }

    public (bool Success, string? ErrorMessage, IReadOnlyList<PlayerRankingDto>? RankingIfAllSubmitted) RecordSubmission(
        Guid roomId,
        Guid playerId,
        int score,
        string status,
        Guid? submissionId = null,
        Guid? mapDetailId = null,
        int? stepsUsed = null,
        int? blocksUsed = null,
        double? timeSeconds = null)
    {
        var instance = GetGameInstance(roomId);
        if (instance == null)
            return (false, "Game not found or not started.", null);
        if (!instance.Players.Any(p => p.PlayerId == playerId))
            return (false, "You are not in this game.", null);

        var levelKey = mapDetailId ?? Guid.Empty;
        var levelResults = instance.LevelResults.GetOrAdd(levelKey, _ => new ConcurrentDictionary<Guid, PlayerGameResult>());
        if (levelResults.ContainsKey(playerId))
            return (false, "Already submitted for this level.", null);

        var playerResult = new PlayerGameResult
        {
            PlayerId = playerId,
            MapDetailId = mapDetailId,
            Score = score,
            Status = status ?? string.Empty,
            StepsUsed = stepsUsed,
            BlocksUsed = blocksUsed,
            TimeSeconds = timeSeconds,
            SubmittedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            SubmissionId = submissionId
        };
        levelResults[playerId] = playerResult;
        instance.PlayerResults[playerId] = playerResult;
        instance.TotalScores.AddOrUpdate(playerId, score, (_, current) => current + score);

        var rankingRows = new List<PlayerRankingDto>();
        foreach (var p in instance.Players.Select(x => x.PlayerId))
        {
            var hasSubmittedCurrentLevel = levelResults.TryGetValue(p, out var currentLevelResult);
            var totalScore = instance.TotalScores.TryGetValue(p, out var sum) ? sum : 0;
            rankingRows.Add(new PlayerRankingDto
            {
                PlayerId = p,
                Score = totalScore,
                Status = hasSubmittedCurrentLevel ? currentLevelResult!.Status : "Pending",
                Rank = 0,
                LevelDetails = instance.LevelResults
                    .OrderBy(x => x.Value.Values.Min(v => v.SubmittedAt))
                    .Select((x, idx) => new { x.Key, x.Value, Index = idx + 1 })
                    .Where(x => x.Value.TryGetValue(p, out _))
                    .Select(x =>
                    {
                        var levelResult = x.Value[p];
                        return new PlayerLevelScoreDetailDto
                        {
                            MapDetailId = x.Key == Guid.Empty ? null : x.Key,
                            LevelIndex = x.Index,
                            Score = levelResult.Score,
                            Status = levelResult.Status,
                            StepsUsed = levelResult.StepsUsed,
                            BlocksUsed = levelResult.BlocksUsed,
                            TimeSeconds = levelResult.TimeSeconds
                        };
                    })
                    .ToList()
            });
        }

        var ordered = rankingRows
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Status == "Pending" ? 1 : 0)
            .ThenBy(r => r.PlayerId)
            .ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Rank = i + 1;
        }
        return (true, null, ordered);
    }

    public (bool Success, string? ErrorMessage, LobbyRoom? Room) EndGame(Guid roomId, Guid requestedByPlayerId)
    {
        var room = GetRoomById(roomId);
        if (room == null)
            return (false, "Không tìm thấy phòng.", null);
        if (room.Status != RoomStatusEnum.Playing)
            return (false, "No game in progress.", null);
        if (!room.Players.ContainsKey(requestedByPlayerId))
            return (false, "Bạn không ở trong phòng này.", null);

        _gameInstances.TryRemove(roomId, out _);
        room.Status = RoomStatusEnum.Waiting;
        foreach (var p in room.Players.Values)
            p.IsReady = false;

        return (true, null, room);
    }

    public bool RemoveRoom(Guid roomId)
    {
        if (!_rooms.TryRemove(roomId, out var room))
            return false;
        _roomIdByCode.TryRemove(room.RoomCode, out _);
        _gameInstances.TryRemove(roomId, out _);
        return true;
    }

    public bool UpdatePlayerConnectionId(Guid roomId, Guid playerId, string connectionId)
    {
        var room = GetRoomById(roomId);
        if (room == null || !room.Players.TryGetValue(playerId, out var player))
            return false;
        player.ConnectionId = connectionId ?? string.Empty;
        return true;
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var sb = new StringBuilder(RoomCodeLength);
        for (var i = 0; i < RoomCodeLength; i++)
            sb.Append(chars[_random.Next(chars.Length)]);
        return sb.ToString();
    }
}



