using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateGameReviewCriterion;

public record UpdateGameReviewCriterionCommand(Guid Id, UpdateGameReviewCriterionRequest Request) : IRequest<Result<GameReviewCriterionDto>>;
