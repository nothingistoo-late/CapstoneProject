namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class PlayerRankingDto
{
    public Guid PlayerId { get; set; }
    public int Score { get; set; }
    public int Rank { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PlayerLevelScoreDetailDto> LevelDetails { get; set; } = new();
}
