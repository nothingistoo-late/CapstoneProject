using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.DeleteComplaintPolicyRuleConfig;

public record DeleteComplaintPolicyRuleConfigCommand(string CategoryKey, string RuleKey) : IRequest<Result>;
