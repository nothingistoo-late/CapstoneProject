using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;

public record ValidateSolutionCommand(ValidateSolutionRequest Request) : IRequest<Result<ValidateSolutionResultDto>>;
