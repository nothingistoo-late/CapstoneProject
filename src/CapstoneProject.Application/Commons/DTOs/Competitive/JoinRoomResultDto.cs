namespace CapstoneProject.Application.Commons.DTOs.Competitive;

public class JoinRoomResultDto
{
    public Guid RoomId { get; set; }
    public Guid MatchId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
}
