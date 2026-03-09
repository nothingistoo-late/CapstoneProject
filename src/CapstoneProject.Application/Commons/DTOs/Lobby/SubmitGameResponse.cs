namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class SubmitGameResponse
{
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid SubmissionId { get; set; }
    public List<PlayerRankingDto>? RankingIfAllSubmitted { get; set; }
}
