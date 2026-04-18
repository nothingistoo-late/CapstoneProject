using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.RejectMap;

public record RejectMapCommand(Guid GameId, string? RejectReason = null) : IRequest<Result>;
