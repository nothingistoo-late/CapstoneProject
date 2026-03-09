using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.CreateLobbyRoom;

public record CreateLobbyRoomCommand(CreateLobbyRoomRequest? Request) : IRequest<Result<CreateLobbyRoomResponse>>;
