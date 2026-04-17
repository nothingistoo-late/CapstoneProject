using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.MapExists;

/// <summary>Check if a game exists and is not deleted (for lobby room game validation).</summary>
public record MapExistsQuery(Guid GameId) : IRequest<Result<bool>>;
