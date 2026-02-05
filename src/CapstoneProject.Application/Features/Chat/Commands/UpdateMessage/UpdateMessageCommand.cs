using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.UpdateMessage;

public class UpdateMessageCommand : IRequest<Result<MessageResponse>>
{
    public Guid MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
}
