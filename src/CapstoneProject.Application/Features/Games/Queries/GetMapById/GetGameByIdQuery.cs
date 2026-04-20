using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetMapById;

public record GetMapByIdQuery(
    Guid GameId,
    bool IncludeEditorialForUser = false,
    bool IncludeInactive = false) : IRequest<Result<GameDetailDto>>;
