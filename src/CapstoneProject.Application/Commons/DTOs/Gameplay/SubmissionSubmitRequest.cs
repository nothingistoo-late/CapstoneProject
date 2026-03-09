namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

/// <summary>
/// DTO submit bài — client gửi: AstSpec, BytecodeSpec, Score, StepsUsed, BlocksUsed, Time. UserId/MapId do server điền từ auth và room.
/// </summary>
public class SubmissionSubmitRequest
{
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }
    public int? Score { get; set; }
    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }
    /// <summary>Thời gian (giây) hoặc milliseconds tùy client — thời gian chơi / thời gian nộp.</summary>
    public double? Time { get; set; }
}
