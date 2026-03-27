using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Models.Xp;

public class XpGrantInput
{
    public Guid UserId { get; set; }
    public int RequestedXp { get; set; }
    public XpSourceTypeEnum SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}

