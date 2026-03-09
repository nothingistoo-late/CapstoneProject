using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.ToggleLobbyReady;

public record ToggleLobbyReadyCommand(Guid RoomId) : IRequest<Result<LobbyRoomDetailResponse>>;
