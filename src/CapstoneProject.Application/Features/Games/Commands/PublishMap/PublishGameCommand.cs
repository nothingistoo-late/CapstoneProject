using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.PublishMap;

public record PublishMapCommand(Guid GameId) : IRequest<Result>;
