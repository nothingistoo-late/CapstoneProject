using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.CreateTemporaryGroupConversation;

public class CreateTemporaryGroupConversationCommand : IRequest<Result<ChatRoomResponse>>
{
    public string Name { get; set; } = string.Empty;
}
