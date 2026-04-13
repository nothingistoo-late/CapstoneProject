using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery() : IRequest<Result<GetUnreadCountResponse>>;

public class GetUnreadCountResponse
{
    public int UnreadCount { get; set; }
}
