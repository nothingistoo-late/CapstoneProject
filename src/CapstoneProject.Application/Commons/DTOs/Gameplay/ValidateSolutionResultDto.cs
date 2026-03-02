using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class ValidateSolutionResultDto
{
    public Guid SubmissionId { get; set; }
    public SubmissionStatusEnum Status { get; set; }
    public int? Score { get; set; }
    public int? Stars { get; set; }
    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }
    public string? Message { get; set; }
}
