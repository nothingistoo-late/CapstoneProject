using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Competitive.Commands.JoinRoom;

public record JoinRoomCommand(string RoomCode) : IRequest<Result<JoinRoomResultDto>>;

public class JoinRoomResultDto
{
    public Guid RoomId { get; set; }
    public Guid MatchId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
}
