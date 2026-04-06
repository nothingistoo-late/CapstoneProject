using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class UpdateComplaintPolicyRuleConfigRequest
{
    [Required]
    [MaxLength(100)]
    public string RuleKey { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 0;
    public string? ConfigJson { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
}
