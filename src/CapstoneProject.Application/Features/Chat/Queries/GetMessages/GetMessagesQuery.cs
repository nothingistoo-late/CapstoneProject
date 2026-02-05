using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Queries.GetMessages;

public class GetMessagesQuery : IRequest<Result<PaginationResult<MessageResponse>>>
{
    public GetMessagesRequest Request { get; set; } = null!;
}
