using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.ReportMap;

public record ReportMapCommand(Guid MapId, string Reason, string? Details = null) : IRequest<Result<Guid>>;
