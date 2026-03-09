using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class LobbyRoomListItemDto
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public Guid HostId { get; set; }
    public int CurrentPlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public RoomStatusEnum Status { get; set; }
    public bool IsLocked { get; set; }
    public Guid? SelectedMapId { get; set; }
}
