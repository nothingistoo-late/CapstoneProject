using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Chat.Queries.GetChatRooms;

public class GetChatRoomsQuery : IRequest<Result<PaginationResult<ChatRoomResponse>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}
