using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetMapInfo;

public record GetMapInfoQuery(Guid GameId) : IRequest<Result<MapInfoDto>>;
