using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.JoinLobbyRoom;

public record JoinLobbyRoomCommand(JoinLobbyRoomRequest Request) : IRequest<Result<JoinLobbyRoomResponse>>;
