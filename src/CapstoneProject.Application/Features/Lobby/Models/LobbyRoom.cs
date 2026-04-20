using System.Collections.Concurrent;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Lobby.Models;

/// <summary>
/// In-memory game lobby room (Gunny/GunBound style).
/// Contains runtime room state for matching players before a game starts.
/// </summary>
public class LobbyRoom
{
    public Guid RoomId { get; set; }
    /// <summary>Human-readable join code for players entering the room.</summary>
    public string RoomCode { get; set; } = string.Empty;
    public Guid HostId { get; set; }
    public ConcurrentDictionary<Guid, LobbyPlayer> Players { get; } = new();
    public int MaxPlayers { get; set; } = 8;
    public RoomStatusEnum Status { get; set; } = RoomStatusEnum.Waiting;
    public bool IsLocked { get; set; }
    /// <summary>Game chosen by host for this room (optional; host sets before start).</summary>
    public Guid? SelectedGameId { get; set; }
    /// <summary>Creation timestamp for temporary in-memory room lifecycle.</summary>
    public DateTime CreatedAt { get; set; } = VietnamDateTime.DbNow;
    /// <summary>Last time room received an activity (join/leave/ready/start/end/update).</summary>
    public DateTime LastActivityAt { get; set; } = VietnamDateTime.DbNow;

    public int PlayerCount => Players.Count;
    public bool IsWaiting => Status == RoomStatusEnum.Waiting;
    public bool IsFull => PlayerCount >= MaxPlayers;
}
