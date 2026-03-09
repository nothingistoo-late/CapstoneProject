using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRooms;

public record GetLobbyRoomsQuery : IRequest<Result<IReadOnlyList<LobbyRoomListItemDto>>>;
