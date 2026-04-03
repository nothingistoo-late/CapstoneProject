using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Bài nộp (block strategy) của người chơi cho một map - chơi đơn hoặc trong match.
/// Bảng Submissions: PK Id (SubmissionId), FK UserId, FK MapId, AstSpec, BytecodeSpec, Status (ResultStatus), Score, StepsUsed, BlocksUsed, CreatedAt (BaseEntity); thêm Language, MatchId.
/// </summary>
public class Submission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    /// <summary>Level đang nộp (MapDetails.Id). Cần sau migration DB.</summary>
    public Guid? MapDetailId { get; set; }
    public virtual MapDetail? MapDetail { get; set; }
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
