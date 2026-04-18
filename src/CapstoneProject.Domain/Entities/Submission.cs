using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Bài nộp (block strategy) của người chơi cho một game.
/// Bảng Submissions: PK Id (SubmissionId), FK UserId, FK GameId, AstSpec, BytecodeSpec, Status (ResultStatus), Score, StepsUsed, BlocksUsed, CreatedAt (BaseEntity); thêm Language, MatchId.
/// </summary>
public class Submission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    /// <summary>Level đang nộp (GameDetails.Id). Cần sau migration DB.</summary>
    public Guid? GameDetailId { get; set; }
    public virtual GameDetail? GameDetail { get; set; }
    public string Language { get; set; } = "Blockly";
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }
    /// <summary>Status bài nộp (Accepted, WrongAnswer...). Trong DB/diagram gọi là Status.</summary>
    public SubmissionStatusEnum ResultStatus { get; set; }
    public int? Score { get; set; }
    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }
    public Guid? MatchId { get; set; }

    public virtual ICollection<ExecutionsResult> ExecutionsResults { get; set; } = new List<ExecutionsResult>();
}
