using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

public class MapReport : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public ReportStatusEnum ReportStatus { get; set; } = ReportStatusEnum.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
    public virtual Map Map { get; set; } = null!;
}
