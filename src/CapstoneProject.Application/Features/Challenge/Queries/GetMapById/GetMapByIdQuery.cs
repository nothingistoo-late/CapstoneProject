using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Queries.GetMapById;

public record GetMapByIdQuery(Guid MapId, bool IncludeEditorialForUser = false) : IRequest<Result<MapDetailDto>>;
