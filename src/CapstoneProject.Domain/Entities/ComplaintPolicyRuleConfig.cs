using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class ComplaintPolicyRuleConfig : BaseEntity
{
    public string CategoryKey { get; set; } = string.Empty;
    public string RuleKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 0;
    public string? ConfigJson { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
}
