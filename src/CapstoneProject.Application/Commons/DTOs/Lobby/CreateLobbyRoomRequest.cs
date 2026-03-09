namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class CreateLobbyRoomRequest
{
    public int MaxPlayers { get; set; } = 8;
    public Guid? SelectedMapId { get; set; }
}
