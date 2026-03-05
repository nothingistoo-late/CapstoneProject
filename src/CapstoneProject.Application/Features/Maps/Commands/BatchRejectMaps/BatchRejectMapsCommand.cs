using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchRejectMaps;

public record BatchRejectMapsCommand(List<Guid> MapIds, string? RejectReason = null) : IRequest<Result<BatchMapResultDto>>;
