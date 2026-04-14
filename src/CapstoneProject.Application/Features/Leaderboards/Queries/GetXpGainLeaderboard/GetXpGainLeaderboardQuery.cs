using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Leaderboards.Queries.GetXpGainLeaderboard;

public record GetXpGainLeaderboardQuery(
    LeaderboardPeriodTypeEnum PeriodType = LeaderboardPeriodTypeEnum.Week,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginationResult<XpGainLeaderboardItemDto>>>;

public class XpGainLeaderboardItemDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int XpGained { get; set; }
    public int CurrentLevel { get; set; }
    public DateTime? LastGainAt { get; set; }
}
