using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Challenge.Commands.BatchApproveMaps;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.BatchPublishMaps;

public record BatchPublishMapsCommand(List<Guid> MapIds) : IRequest<Result<BatchMapResultDto>>;
