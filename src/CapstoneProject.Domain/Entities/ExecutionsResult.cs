using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Kết quả một lần chạy mô phỏng cho submission (replay / validation).
/// </summary>
public class ExecutionsResult : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool IsDeterministic { get; set; }
    public string? ServerSimVersion { get; set; }
    public string? ResultSpec { get; set; }

    public virtual Submission Submission { get; set; } = null!;
}
