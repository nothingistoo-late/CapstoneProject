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
    public Guid? MapId { get; set; }
    public IReadOnlyList<LobbyPlayer> Players { get; set; } = Array.Empty<LobbyPlayer>();
    public object? GameState { get; set; }
    public IReadOnlyList<Guid> TurnOrder { get; set; } = Array.Empty<Guid>();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Per-player submission result (score, status). Filled when player submits.</summary>
    public ConcurrentDictionary<Guid, PlayerGameResult> PlayerResults { get; } = new();
}
