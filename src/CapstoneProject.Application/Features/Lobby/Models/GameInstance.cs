using System.Collections.Concurrent;
using CapstoneProject.Application.Commons.DTOs.Lobby;

namespace CapstoneProject.Application.Features.Lobby.Models;

/// <summary>
/// Represents an active game instance after the match has started.
/// PlayerResults: score/status per player after they submit solution.
/// </summary>
public class GameInstance
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public Guid? GameId { get; set; }
    /// <summary>Danh sách người chơi còn trong phòng (cập nhật khi có người thoát giữa game).</summary>
    public List<LobbyPlayer> Players { get; set; } = new();

    public object? GameState { get; set; }

    public List<Guid> TurnOrder { get; set; } = new();
    public DateTime StartedAt { get; set; } = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
    /// <summary>Per-player submission result (score, status). Filled when player submits.</summary>
    public ConcurrentDictionary<Guid, PlayerGameResult> PlayerResults { get; } = new();
    /// <summary>Per-level submissions for multiplayer campaign flow.</summary>
    public ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, PlayerGameResult>> LevelResults { get; } = new();
    /// <summary>Total score across submitted levels by player.</summary>
    public ConcurrentDictionary<Guid, int> TotalScores { get; } = new();
}



