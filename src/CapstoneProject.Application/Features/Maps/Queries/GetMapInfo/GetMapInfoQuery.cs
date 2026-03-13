using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapInfo;

public record GetMapInfoQuery(Guid MapId) : IRequest<Result<MapInfoDto>>;
