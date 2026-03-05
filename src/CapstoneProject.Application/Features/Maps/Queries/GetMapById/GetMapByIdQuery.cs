using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapById;

public record GetMapByIdQuery(Guid MapId, bool IncludeEditorialForUser = false) : IRequest<Result<MapDetailDto>>;
