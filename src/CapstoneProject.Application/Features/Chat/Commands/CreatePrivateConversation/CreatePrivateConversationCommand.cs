using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.CreatePrivateConversation;

public class CreatePrivateConversationCommand : IRequest<Result<ChatRoomResponse>>
{
    public Guid OtherUserId { get; set; }
}
