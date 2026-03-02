using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.SubmitMapForReview;

public record SubmitMapForReviewCommand(Guid MapId) : IRequest<Result>;
