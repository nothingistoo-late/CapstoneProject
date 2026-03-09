using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.StartLobbyGame;

public record StartLobbyGameCommand(Guid RoomId) : IRequest<Result<StartGameResponse>>;
