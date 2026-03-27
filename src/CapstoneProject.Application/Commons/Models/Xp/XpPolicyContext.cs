using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Models.Xp;

public class XpPolicyContext
{
    public Guid UserId { get; set; }
    public int RequestedXp { get; set; }
    public int CurrentXpBeforeGrant { get; set; }
    public XpSourceTypeEnum SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public DateTime CurrentTime { get; set; }
    public XpSourceConfig? SourceConfig { get; set; }
    public Dictionary<string, XpPolicyConfig> PolicyConfigs { get; set; } = new();
}

