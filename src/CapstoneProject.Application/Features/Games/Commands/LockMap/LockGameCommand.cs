using CapstoneProject.Application.Common.Models;
using MediatR;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Games.Commands.LockMap;

public record LockMapCommand(Guid GameId, string? Note = null) : IRequest<Result>;
