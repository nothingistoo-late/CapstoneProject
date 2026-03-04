using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapsById;

public record GetMapsByIdQuery(Guid Id) : IRequest<Result<MapsResponseDto>>;
