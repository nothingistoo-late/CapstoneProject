using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.EndLobbyGame;

public record EndLobbyGameCommand(Guid RoomId) : IRequest<Result<LobbyRoomDetailResponse>>;
