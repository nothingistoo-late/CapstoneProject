using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models.Leaderboards;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CapstoneProject.Infrastructure.Services;

public class LeaderboardRewardSettlementJob
{
    private const string RelatedEntityType = "LeaderboardReward";

    private readonly IUnitOfWork _unitOfWork;
    private readonly CapstoneProjectDbContext _dbContext;
    private readonly IXpEngineService _xpEngineService;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly INotificationPersistenceService _notificationPersistenceService;
    private readonly IOptions<LeaderboardRewardsOptions> _options;
    private readonly ILogger<LeaderboardRewardSettlementJob> _logger;

    public LeaderboardRewardSettlementJob(
        IUnitOfWork unitOfWork,
        CapstoneProjectDbContext dbContext,
        IXpEngineService xpEngineService,
        IOrbitCoinService orbitCoinService,
        INotificationPersistenceService notificationPersistenceService,
        IOptions<LeaderboardRewardsOptions> options,
        ILogger<LeaderboardRewardSettlementJob> logger)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _xpEngineService = xpEngineService;
        _orbitCoinService = orbitCoinService;
        _notificationPersistenceService = notificationPersistenceService;
        _options = options;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteWeeklyAsync()
    {
        var now = VietnamDateTime.DbNow;
        var (dateFrom, dateTo) = LeaderboardPeriodHelper.GetRange(Application.Common.Enums.LeaderboardPeriodTypeEnum.Week, now);
        await SettleAllLeaderboardsAsync("weekly", dateFrom, dateTo, now);
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteMonthlyAsync()
    {
        var now = VietnamDateTime.DbNow;
        var (dateFrom, dateTo) = LeaderboardPeriodHelper.GetRange(Application.Common.Enums.LeaderboardPeriodTypeEnum.Month, now);
        await SettleAllLeaderboardsAsync("monthly", dateFrom, dateTo, now);
    }

    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteMinuteTestAsync(int windowMinutes)
    {
        var now = VietnamDateTime.DbNow;
        var minutes = Math.Max(1, windowMinutes);
        var dateFrom = now.AddMinutes(-minutes);
        await SettleAllLeaderboardsAsync($"test-{minutes}m", dateFrom, now, now);
    }

    private async Task SettleAllLeaderboardsAsync(string periodKey, DateTime dateFrom, DateTime dateTo, DateTime now)
    {
        _logger.LogInformation("Leaderboard settlement started. Period={PeriodKey}, From={From}, To={To}", periodKey, dateFrom, dateTo);

        var options = _options.Value;
        await SettleTopLevelAsync(periodKey, dateFrom, dateTo, now, options.TopLevelTiers);
        await SettleXpGainAsync(periodKey, dateFrom, dateTo, now, options.XpGainTiers);
        await SettleMostPlayedCreatedMapsAsync(periodKey, dateFrom, dateTo, now, options.MostPlayedCreatedMapsTiers);

        _logger.LogInformation("Leaderboard settlement finished. Period={PeriodKey}", periodKey);
    }

    private async Task SettleTopLevelAsync(string periodKey, DateTime dateFrom, DateTime dateTo, DateTime now, List<LeaderboardRewardTier> tiers)
    {
        var orderedTiers = NormalizeTiers(tiers);
        if (orderedTiers.Count == 0) return;

        var maxTopN = orderedTiers.Max(t => t.TopN);
        var winners = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => u.Status == EntityStatusEnum.Active)
            .OrderByDescending(u => u.CurrentLevel)
            .ThenByDescending(u => u.CurrentXp)
            .ThenBy(u => u.JoiningAt)
            .Take(maxTopN)
            .Select(u => new WinnerCandidate(u.Id, $"{u.FirstName} {u.LastName}".Trim(), null))
            .ToListAsync();

        await GrantRewardsForWinnersAsync("top-level", periodKey, dateFrom, dateTo, now, orderedTiers, winners, BuildLeaderboardActionUrl("top-level", periodKey));
    }

    private async Task SettleXpGainAsync(string periodKey, DateTime dateFrom, DateTime dateTo, DateTime now, List<LeaderboardRewardTier> tiers)
    {
        var orderedTiers = NormalizeTiers(tiers);
        if (orderedTiers.Count == 0) return;

        var maxTopN = orderedTiers.Max(t => t.TopN);

        var xpByUserQuery = _unitOfWork.Repository<XpTransaction>().GetQueryable()
            .Where(tx => tx.CreatedAt.HasValue
                         && tx.CreatedAt.Value >= dateFrom
                         && tx.CreatedAt.Value <= dateTo)
            .GroupBy(tx => tx.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                XpGained = g.Sum(x => x.Delta > 0 ? x.Delta : 0),
                LastGainAt = g.Max(x => x.CreatedAt)
            });

        var winners = await (
            from agg in xpByUserQuery
            join user in _unitOfWork.Repository<AppUser>().GetQueryable() on agg.UserId equals user.Id
            where agg.XpGained > 0 && user.Status == EntityStatusEnum.Active
            orderby agg.XpGained descending, agg.LastGainAt ascending, user.JoiningAt ascending
            select new WinnerCandidate(user.Id, $"{user.FirstName} {user.LastName}".Trim(), agg.XpGained))
            .Take(maxTopN)
            .ToListAsync();

        await GrantRewardsForWinnersAsync("xp-gain", periodKey, dateFrom, dateTo, now, orderedTiers, winners, BuildLeaderboardActionUrl("xp-gain", periodKey));
    }

    private async Task SettleMostPlayedCreatedMapsAsync(string periodKey, DateTime dateFrom, DateTime dateTo, DateTime now, List<LeaderboardRewardTier> tiers)
    {
        var orderedTiers = NormalizeTiers(tiers);
        if (orderedTiers.Count == 0)
        {
            _logger.LogInformation("Skip most-played-created-games settlement because no valid tiers are configured.");
            return;
        }

        var maxTopN = orderedTiers.Max(t => t.TopN);

        var mapPlayAggQuery = _unitOfWork.Repository<UserGamePlayHistory>().GetQueryable()
            .Where(p => !p.IsDeleted && p.StartTime >= dateFrom && p.StartTime <= dateTo)
            .GroupBy(p => p.GameId)
            .Select(g => new
            {
                GameId = g.Key,
                PlayCount = g.Count(),
                UniquePlayerCount = g.Select(x => x.UserId).Distinct().Count(),
                LastPlayedAt = g.Max(x => x.StartTime)
            });

        var winners = await (
            from agg in mapPlayAggQuery
            join game in _unitOfWork.Repository<Game>().GetQueryable() on agg.GameId equals game.Id
            join creator in _unitOfWork.Repository<AppUser>().GetQueryable() on game.CreatedBy equals creator.Id
            where !game.IsDeleted && game.CreatedBy.HasValue
            orderby agg.PlayCount descending, agg.UniquePlayerCount descending, agg.LastPlayedAt descending, game.CreatedAt ascending
            select new WinnerCandidate(creator.Id, $"{creator.FirstName} {creator.LastName}".Trim(), agg.PlayCount))
            .Take(maxTopN)
            .ToListAsync();

        if (winners.Count == 0)
        {
            _logger.LogInformation(
                "No winners for most-played-created-games. Period={PeriodKey}, From={From}, To={To}. Check UserGamePlayHistory data in this window and game creator status.",
                periodKey,
                dateFrom,
                dateTo);
            return;
        }

        _logger.LogInformation(
            "Most-played-created-games winners resolved: Count={Count}, TopN={TopN}, FirstWinnerUserId={FirstWinnerUserId}, FirstWinnerMetric={FirstWinnerMetric}",
            winners.Count,
            maxTopN,
            winners[0].UserId,
            winners[0].MetricValue);

        await GrantRewardsForWinnersAsync("most-played-created-games", periodKey, dateFrom, dateTo, now, orderedTiers, winners, BuildLeaderboardActionUrl("most-played-created-games", periodKey));
    }

    private async Task GrantRewardsForWinnersAsync(
        string leaderboardKey,
        string periodKey,
        DateTime dateFrom,
        DateTime dateTo,
        DateTime now,
        List<LeaderboardRewardTier> tiers,
        List<WinnerCandidate> winners,
        string actionUrl)
    {
        for (var i = 0; i < winners.Count; i++)
        {
            var rank = i + 1;
            var winner = winners[i];
            var tier = tiers.FirstOrDefault(t => rank <= t.TopN);
            if (tier == null) continue;

            var periodToken = $"{periodKey}:{dateFrom:yyyyMMddHHmm}-{dateTo:yyyyMMddHHmm}";

            if (tier.RewardXp > 0)
            {
                var xpKey = $"lb:{leaderboardKey}:{periodToken}:user:{winner.UserId}:rank:{rank}:xp";
                var sourceId = CreateDeterministicGuid(xpKey);

                await _xpEngineService.GrantXpAsync(new XpGrantInput
                {
                    UserId = winner.UserId,
                    RequestedXp = tier.RewardXp,
                    SourceType = XpSourceTypeEnum.XpBonus,
                    SourceId = sourceId,
                    IdempotencyKey = xpKey,
                    Reason = $"Leaderboard reward [{leaderboardKey}] rank #{rank}",
                    Metadata = JsonSerializer.Serialize(new
                    {
                        leaderboard = leaderboardKey,
                        period = periodKey,
                        dateFrom,
                        dateTo,
                        rank,
                        metric = winner.MetricValue
                    })
                });
            }

            if (tier.RewardOrbitCoin > 0)
            {
                var rewardEntityId = CreateDeterministicGuid($"lb:{leaderboardKey}:{periodToken}:user:{winner.UserId}:rank:{rank}:coin");

                var alreadyCredited = await _dbContext.OrbitCoinTransactions
                    .AnyAsync(t => t.UserId == winner.UserId
                                   && t.TransactionType == CoinTransactionTypeEnum.AdminAdjustment
                                   && t.RelatedEntityType == RelatedEntityType
                                   && t.RelatedEntityId == rewardEntityId);
                if (!alreadyCredited)
                {
                    await _orbitCoinService.CreditAsync(
                        winner.UserId,
                        tier.RewardOrbitCoin,
                        CoinTransactionTypeEnum.AdminAdjustment,
                        RelatedEntityType,
                        rewardEntityId,
                        feeAmount: 0,
                        note: $"Leaderboard reward [{leaderboardKey}] rank #{rank}",
                        createdBy: null);
                }
            }

            try
            {
                var leaderboardLabel = GetLeaderboardDisplayName(leaderboardKey);
                var periodLabel = GetPeriodDisplayName(periodKey);
                var rewardSummary = BuildRewardSummary(tier.RewardXp, tier.RewardOrbitCoin);

                var payloadJson = JsonSerializer.Serialize(new
                {
                    leaderboard = leaderboardKey,
                    period = periodKey,
                    dateFrom,
                    dateTo,
                    rank,
                    rewardXp = tier.RewardXp,
                    rewardOrbitCoin = tier.RewardOrbitCoin,
                    settledAt = now,
                    metric = winner.MetricValue
                });

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.SystemAnnouncement,
                    $"Chúc mừng! Bạn nhận thưởng BXH {leaderboardLabel}",
                    $"Bạn đạt hạng #{rank} ở BXH {leaderboardLabel} kỳ {periodLabel}. {rewardSummary}",
                    new List<Guid> { winner.UserId },
                    actorUserId: null,
                    payloadJson: payloadJson,
                    actionUrl: actionUrl);
            }
            catch
            {
                // Ignore notification failures to keep settlement robust.
            }
        }
    }

    private static List<LeaderboardRewardTier> NormalizeTiers(List<LeaderboardRewardTier>? tiers)
    {
        return (tiers ?? new List<LeaderboardRewardTier>())
            .Where(t => t.TopN > 0 && (t.RewardXp > 0 || t.RewardOrbitCoin > 0))
            .OrderBy(t => t.TopN)
            .ToList();
    }

    private static Guid CreateDeterministicGuid(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var guidBytes = new byte[16];
        Array.Copy(bytes, guidBytes, 16);
        return new Guid(guidBytes);
    }

    private static string BuildLeaderboardActionUrl(string leaderboardKey, string periodKey)
    {
        var periodType = periodKey.Equals("monthly", StringComparison.OrdinalIgnoreCase) ? "Month" : "Week";

        return leaderboardKey switch
        {
            "top-level" => "/learner/leaderboard?tab=top-level",
            "xp-gain" => $"/learner/leaderboard?tab=xp-gain&period={periodType}",
            "most-played-created-games" => $"/learner/leaderboard?tab=most-played&period={periodType}",
            _ => "/learner/leaderboard"
        };
    }

    private static string GetLeaderboardDisplayName(string leaderboardKey)
    {
        return leaderboardKey switch
        {
            "top-level" => "Cấp độ",
            "xp-gain" => "XP tăng trưởng",
            "most-played-created-games" => "Game được chơi nhiều",
            _ => "Leaderboard"
        };
    }

    private static string GetPeriodDisplayName(string periodKey)
    {
        return periodKey.Equals("monthly", StringComparison.OrdinalIgnoreCase)
            ? "tháng"
            : "tuần";
    }

    private static string BuildRewardSummary(int rewardXp, decimal rewardOrbitCoin)
    {
        var rewards = new List<string>();
        if (rewardXp > 0)
        {
            rewards.Add($"+{rewardXp} XP");
        }

        if (rewardOrbitCoin > 0)
        {
            rewards.Add($"+{rewardOrbitCoin:0.##} OrbitCoin");
        }

        if (rewards.Count == 0)
        {
            return "Phần thưởng đã được cộng vào tài khoản của bạn.";
        }

        return $"Thưởng nhận được: {string.Join(", ", rewards)}.";
    }

    private sealed record WinnerCandidate(Guid UserId, string DisplayName, int? MetricValue);
}
