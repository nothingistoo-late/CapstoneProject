using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.SubmitMapForReview;

public record SubmitMapForReviewCommand(Guid GameId) : IRequest<Result>;
