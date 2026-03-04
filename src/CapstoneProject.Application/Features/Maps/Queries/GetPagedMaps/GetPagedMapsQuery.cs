using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetPagedMaps;

public record GetPagedMapsQuery(MapsFilter Filter) : IRequest<PaginationResult<MapsListItemDto>>;
