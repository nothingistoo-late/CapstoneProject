using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.ApproveMap;

public record ApproveMapCommand(Guid GameId, string? ReviewNote = null) : IRequest<Result>;
