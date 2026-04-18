using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Games.Commands.BatchApproveMaps;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.BatchPublishMaps;

public record BatchPublishMapsCommand(List<Guid> GameIds) : IRequest<Result<BatchMapResultDto>>;
