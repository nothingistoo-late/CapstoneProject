using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Commands.UpdateMapSolveScoreConfig;

public record UpdateMapSolveScoreConfigCommand(
    int BaseScore,
    int TimeScore,
    int StepsScore,
    int BlocksScore) : IRequest<Result>;
