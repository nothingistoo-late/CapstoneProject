using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Competitive.Commands.CreateRoom;

public record CreateRoomCommand(Guid MatchId, int MaxPlayers = 8) : IRequest<Result<CreateRoomResultDto>>;

public class CreateRoomResultDto
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
}
