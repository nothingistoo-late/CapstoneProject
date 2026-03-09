using System.Collections.Concurrent;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Lobby.Models;

/// <summary>
/// In-memory game lobby room (Gunny/GunBound style).
/// Aligned with Domain Room: Id (RoomId), Code (RoomCode), RoomStatusEnum (Status), MaxPlayers.
/// HostId/IsHost align with Domain RoomParticipant.IsOwner.
/// </summary>
public class LobbyRoom
{
    public Guid RoomId { get; set; }
    /// <summary>Same as Domain Room.Code; human-readable join code.</summary>
    public string RoomCode { get; set; } = string.Empty;
    public Guid HostId { get; set; }
    public ConcurrentDictionary<Guid, LobbyPlayer> Players { get; } = new();
    public int MaxPlayers { get; set; } = 8;
    public RoomStatusEnum Status { get; set; } = RoomStatusEnum.Waiting;
    public bool IsLocked { get; set; }
    /// <summary>Map chosen by host for this room (optional; host sets before start).</summary>
    public Guid? SelectedMapId { get; set; }

    public int PlayerCount => Players.Count;
    public bool IsWaiting => Status == RoomStatusEnum.Waiting;
    public bool IsFull => PlayerCount >= MaxPlayers;
}
