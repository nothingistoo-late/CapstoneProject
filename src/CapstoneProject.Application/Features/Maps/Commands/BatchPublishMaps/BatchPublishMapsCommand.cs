using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchPublishMaps;

public record BatchPublishMapsCommand(List<Guid> MapIds) : IRequest<Result<BatchMapResultDto>>;
