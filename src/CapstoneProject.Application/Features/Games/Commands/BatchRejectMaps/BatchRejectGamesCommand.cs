using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Games.Commands.BatchApproveMaps;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.BatchRejectMaps;

public record BatchRejectMapsCommand(List<Guid> GameIds, string? RejectReason = null) : IRequest<Result<BatchMapResultDto>>;
