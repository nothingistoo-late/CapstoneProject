using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.AddMemberToRoom;

public class AddMemberToRoomCommand : IRequest<Result<ChatRoomMemberResponse>>
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
}
