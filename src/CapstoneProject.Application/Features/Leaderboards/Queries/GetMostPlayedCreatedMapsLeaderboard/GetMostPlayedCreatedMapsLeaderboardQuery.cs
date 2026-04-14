using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Leaderboards.Queries.GetMostPlayedCreatedMapsLeaderboard;

public record GetMostPlayedCreatedMapsLeaderboardQuery(
    LeaderboardPeriodTypeEnum PeriodType = LeaderboardPeriodTypeEnum.Week,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginationResult<MostPlayedCreatedMapLeaderboardItemDto>>>;

public class MostPlayedCreatedMapLeaderboardItemDto
{
    public int Rank { get; set; }
    public Guid MapId { get; set; }
    public string MapTitle { get; set; } = string.Empty;
    public Guid CreatorUserId { get; set; }
    public string CreatorDisplayName { get; set; } = string.Empty;
    public int PlayCount { get; set; }
    public int UniquePlayerCount { get; set; }
    public DateTime? LastPlayedAt { get; set; }
}
