using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetXpSourceConfigs;

public record GetXpSourceConfigsQuery() : IRequest<Result<List<XpSourceConfigDto>>>;

public class XpSourceConfigDto
{
    public string SourceType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int BaseXp { get; set; }
    public int DailyCap { get; set; }
    public double BonusMultiplier { get; set; }
    public string? ConfigJson { get; set; }
}

