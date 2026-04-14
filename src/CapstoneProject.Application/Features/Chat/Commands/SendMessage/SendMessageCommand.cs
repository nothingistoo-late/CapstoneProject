using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommand : IRequest<Result<MessageResponse>>
{
    public Guid ChatRoomId { get; set; }
    public string? Content { get; set; }           // nullable – image-only messages have no text
    public MessageTypeEnum MessageType { get; set; } = MessageTypeEnum.Text;
    public Guid? ReplyToMessageId { get; set; }
    public IFormFile? ImageFile { get; set; }
}
