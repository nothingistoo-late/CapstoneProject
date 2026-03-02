using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Competitive.Commands.CreateMatch;

public record CreateMatchCommand(Guid MapId, string? RulesSpec = null) : IRequest<Result<Guid>>;
