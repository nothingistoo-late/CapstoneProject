using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.SetLobbyRoomMap;

public record SetLobbyRoomMapCommand(Guid RoomId, SetRoomMapRequest Request) : IRequest<Result<LobbyRoomDetailResponse>>;
