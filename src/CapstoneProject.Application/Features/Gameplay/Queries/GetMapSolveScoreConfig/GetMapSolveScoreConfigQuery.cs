using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetMapSolveScoreConfig;

public record GetMapSolveScoreConfigQuery() : IRequest<Result<MapSolveScoreConfigDto>>;

public class MapSolveScoreConfigDto
{
    public string ConfigKey { get; set; } = string.Empty;
    public int BaseScore { get; set; }
    public int TimeScore { get; set; }
    public int StepsScore { get; set; }
    public int BlocksScore { get; set; }
}
