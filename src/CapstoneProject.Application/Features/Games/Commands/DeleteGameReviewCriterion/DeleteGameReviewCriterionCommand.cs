using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteGameReviewCriterion;

public record DeleteGameReviewCriterionCommand(Guid Id) : IRequest<Result>;
