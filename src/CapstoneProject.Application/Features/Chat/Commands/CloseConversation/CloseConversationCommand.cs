using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.CloseConversation;

public class CloseConversationCommand : IRequest<Result<bool>>
{
    public Guid ConversationId { get; set; }
}
