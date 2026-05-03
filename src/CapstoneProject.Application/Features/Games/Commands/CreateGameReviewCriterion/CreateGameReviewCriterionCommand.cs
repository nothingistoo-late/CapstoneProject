using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.CreateGameReviewCriterion;

public record CreateGameReviewCriterionCommand(CreateGameReviewCriterionRequest Request) : IRequest<Result<GameReviewCriterionDto>>;
