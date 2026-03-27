using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetXpLeaderboard;

public record GetXpLeaderboardQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginationResult<XpLeaderboardItemDto>>>;

public class XpLeaderboardItemDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int CurrentXp { get; set; }
    public int CurrentLevel { get; set; }
}

