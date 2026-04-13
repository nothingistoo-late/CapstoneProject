namespace CapstoneProject.Application.Features.Lobby.Models;

/// <summary>
/// Represents a player in an in-memory lobby room.
/// PlayerId identifies the user, and ConnectionId is SignalR-specific runtime state.
/// </summary>
public class LobbyPlayer
{
    public Guid PlayerId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    /// <summary>Indicates whether this player is the room host.</summary>
    public bool IsHost { get; set; }
}
