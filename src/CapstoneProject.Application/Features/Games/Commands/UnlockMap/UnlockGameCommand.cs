using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.UnlockMap;

public record UnlockMapCommand(Guid GameId, bool RepublishIfPublishedStatus = true) : IRequest<Result>;
