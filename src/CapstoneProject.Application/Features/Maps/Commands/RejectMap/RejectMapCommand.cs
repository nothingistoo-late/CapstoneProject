using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.RejectMap;

public record RejectMapCommand(Guid MapId, string? RejectReason = null) : IRequest<Result>;
