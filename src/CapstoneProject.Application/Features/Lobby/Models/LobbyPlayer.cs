namespace CapstoneProject.Application.Features.Lobby.Models;

/// <summary>
/// Represents a player in an in-memory lobby room.
/// Aligned with Domain RoomParticipant: PlayerId (UserId), IsReady, IsHost (IsOwner).
/// ConnectionId is lobby-only for SignalR.
/// </summary>
public class LobbyPlayer
{
    public Guid PlayerId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    /// <summary>Room host; equivalent to Domain RoomParticipant.IsOwner.</summary>
    public bool IsHost { get; set; }
}
