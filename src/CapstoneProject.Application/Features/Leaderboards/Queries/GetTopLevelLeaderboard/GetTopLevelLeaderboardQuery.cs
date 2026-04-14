using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Leaderboards.Queries.GetTopLevelLeaderboard;

public record GetTopLevelLeaderboardQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginationResult<TopLevelLeaderboardItemDto>>>;

public class TopLevelLeaderboardItemDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int CurrentXp { get; set; }
}
