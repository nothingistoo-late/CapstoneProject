using System.Text.Json.Serialization;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class ValidateSolutionResultDto
{
    public Guid SubmissionId { get; set; }
    /// <summary>Trạng thái bài nộp (enum trong code, serialize ra JSON dạng string "Accepted" | "WrongAnswer" cho client dễ đọc).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SubmissionStatusEnum Status { get; set; }
    public int? Score { get; set; }
    public int? Stars { get; set; }
    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }
    public string? Message { get; set; }
}
