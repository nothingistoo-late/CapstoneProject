using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMaps;

public record UpdateMapsCommand(Guid Id, UpdateMapsRequest Request) : IRequest<Result<MapsResponseDto>>;
