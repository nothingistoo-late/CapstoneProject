using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.MapExists;

/// <summary>Check if a map exists and is not deleted (for lobby room map validation).</summary>
public record MapExistsQuery(Guid MapId) : IRequest<Result<bool>>;
