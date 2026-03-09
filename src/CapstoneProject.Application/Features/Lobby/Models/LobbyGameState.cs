namespace CapstoneProject.Application.Features.Lobby.Models;

/// <summary>
/// In-memory state for an active game (turn, current player). Expand for your game rules.
/// </summary>
public class LobbyGameState
{
    /// <summary>Zero-based index into TurnOrder.</summary>
    public int CurrentTurnIndex { get; set; }

    /// <summary>PlayerId whose turn it is (TurnOrder[CurrentTurnIndex]).</summary>
    public Guid CurrentPlayerId { get; set; }

    /// <summary>Round number (e.g. 1-based).</summary>
    public int RoundNumber { get; set; } = 1;
}
