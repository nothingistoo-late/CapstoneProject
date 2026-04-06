using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintPolicyRuleConfigs;

public record GetComplaintPolicyRuleConfigsQuery(string? CategoryKey = null) : IRequest<Result<List<ComplaintPolicyRuleConfigDto>>>;

public class ComplaintPolicyRuleConfigDto
{
    public string CategoryKey { get; set; } = string.Empty;
    public string RuleKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string? ConfigJson { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
}
