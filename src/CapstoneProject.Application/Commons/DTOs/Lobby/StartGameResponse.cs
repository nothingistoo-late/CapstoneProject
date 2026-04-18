namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class StartGameResponse
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public Guid? GameId { get; set; }
    public List<Guid> TurnOrder { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public int CurrentTurnIndex { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public int RoundNumber { get; set; }
}
