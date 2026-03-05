using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.SubmitMapForReview;

public record SubmitMapForReviewCommand(Guid MapId) : IRequest<Result>;
