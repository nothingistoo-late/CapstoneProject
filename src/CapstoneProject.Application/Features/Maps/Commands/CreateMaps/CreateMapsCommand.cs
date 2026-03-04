using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMaps;

public record CreateMapsCommand(CreateMapsRequest Request) : IRequest<Result<MapsResponseDto>>;
