using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Commands.DeleteMessage;

public class DeleteMessageCommand : IRequest<Result<bool>>
{
    public Guid MessageId { get; set; }
}
