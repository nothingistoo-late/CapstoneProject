using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetXpPolicyConfigs;

public record GetXpPolicyConfigsQuery() : IRequest<Result<List<XpPolicyConfigDto>>>;

public class XpPolicyConfigDto
{
    public string PolicyKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string? ConfigJson { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
}

