namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class SetRoomMapRequest
{
    public Guid? GameId { get; set; }
    public int? MaxPlayers { get; set; }
}
