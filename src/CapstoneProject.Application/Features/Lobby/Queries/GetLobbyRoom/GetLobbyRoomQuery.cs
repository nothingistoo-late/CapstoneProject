using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRoom;

public record GetLobbyRoomQuery(Guid RoomId) : IRequest<Result<LobbyRoomDetailResponse>>;
