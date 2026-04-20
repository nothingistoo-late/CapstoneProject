namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class PlayerLevelScoreDetailDto
{
    public Guid? MapDetailId { get; set; }
    public int LevelIndex { get; set; }
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }
    public double? TimeSeconds { get; set; }
}
