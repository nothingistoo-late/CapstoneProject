namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

/// <summary>
/// DTO submit bài — client gửi: AstSpec, BytecodeSpec, Score, StepsUsed, BlocksUsed, Time. UserId/GameId do server điền từ auth và room.
/// </summary>
public class SubmissionSubmitRequest
{
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }

    /// <summary>Không dùng để override điểm server — giữ tương thích JSON cũ; điểm luôn từ ValidateSolution.</summary>
    public int? Score { get; set; }

    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }

    /// <summary>Thắng/thua theo engine — bắt buộc cho chấm điểm lobby đúng.</summary>
    public bool? IsWin { get; set; }
    /// <summary>Thời gian (giây) hoặc milliseconds tùy client — thời gian chơi / thời gian nộp.</summary>
    public double? Time { get; set; }

    /// <summary>Level đang nộp (GameDetails.Id). Null = game một level.</summary>
    public Guid? GameDetailId { get; set; }
}
