using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Commands.UpdateGameSolveScoreConfig;

public record UpdateGameSolveScoreConfigCommand(
    int BaseScore,
    int TimeScore,
    int StepsScore,
    int BlocksScore) : IRequest<Result>;
