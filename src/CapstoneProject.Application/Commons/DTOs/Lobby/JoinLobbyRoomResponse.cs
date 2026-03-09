namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class JoinLobbyRoomResponse
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int CurrentPlayerCount { get; set; }
    public int MaxPlayers { get; set; }
}
