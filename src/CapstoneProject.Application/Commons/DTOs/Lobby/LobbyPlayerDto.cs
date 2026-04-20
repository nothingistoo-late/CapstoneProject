namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class LobbyPlayerDto
{
    public Guid PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public bool IsReady { get; set; }
    public bool IsHost { get; set; }
}
