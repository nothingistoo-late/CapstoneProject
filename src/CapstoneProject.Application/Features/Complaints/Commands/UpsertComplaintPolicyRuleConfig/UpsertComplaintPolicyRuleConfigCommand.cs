using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.UpsertComplaintPolicyRuleConfig;

public record UpsertComplaintPolicyRuleConfigCommand(
    string CategoryKey,
    string RuleKey,
    bool IsEnabled,
    int Priority,
    string? ConfigJson,
    DateTime? ActiveFrom,
    DateTime? ActiveTo) : IRequest<Result<UpsertComplaintPolicyRuleConfigDto>>;

public class UpsertComplaintPolicyRuleConfigDto
{
    public string CategoryKey { get; set; } = string.Empty;
    public string RuleKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string? ConfigJson { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
}
