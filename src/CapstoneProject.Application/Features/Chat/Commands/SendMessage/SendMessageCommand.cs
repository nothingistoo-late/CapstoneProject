using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommand : IRequest<Result<MessageResponse>>
{
    public SendMessageRequest Request { get; set; } = null!;
    public string? FilePath { get; set; }
}
