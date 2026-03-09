namespace CapstoneProject.Application.Commons.DTOs.Lobby;

public class PlayerGameResult
{
    public Guid PlayerId { get; set; }
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public Guid? SubmissionId { get; set; }
}
