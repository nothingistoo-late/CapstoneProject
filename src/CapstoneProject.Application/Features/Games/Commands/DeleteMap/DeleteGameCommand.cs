using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteMap;

public record DeleteMapCommand(Guid GameId) : IRequest<Result>;
