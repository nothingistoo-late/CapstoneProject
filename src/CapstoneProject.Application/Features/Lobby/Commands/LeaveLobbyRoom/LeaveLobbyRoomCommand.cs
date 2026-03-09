using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.LeaveLobbyRoom;

public record LeaveLobbyRoomCommand(Guid RoomId) : IRequest<Result>;
