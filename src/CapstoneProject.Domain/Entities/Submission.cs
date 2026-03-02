using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Bài nộp (block strategy) của người chơi cho một map - chơi đơn hoặc trong match.
/// </summary>
public class Submission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    public string Language { get; set; } = "Blockly";
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }
    /// <summary>Trạng thái kết quả chạy (Accepted, WrongAnswer...). Khác với BaseEntity.Status (Active/Inactive).</summary>
    public SubmissionStatusEnum ResultStatus { get; set; }
    public int? Score { get; set; }
    public int? StepsUsed { get; set; }
    public int? BlocksUsed { get; set; }
    public Guid? MatchId { get; set; }

    public virtual ICollection<ExecutionsResult> ExecutionsResults { get; set; } = new List<ExecutionsResult>();
}
