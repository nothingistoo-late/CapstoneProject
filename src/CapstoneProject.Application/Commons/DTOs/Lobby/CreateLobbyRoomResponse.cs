namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class CreateLobbyRoomResponse
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public Guid? SelectedMapId { get; set; }
}
