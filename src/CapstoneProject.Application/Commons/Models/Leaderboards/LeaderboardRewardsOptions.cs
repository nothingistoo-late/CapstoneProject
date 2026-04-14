namespace CapstoneProject.Application.Commons.Models.Leaderboards;

public class LeaderboardRewardsOptions
{
    public const string SectionName = "LeaderboardRewards";

    public LeaderboardCycleOptions Cycle { get; set; } = new();

    public List<LeaderboardRewardTier> TopLevelTiers { get; set; } = new();

    public List<LeaderboardRewardTier> XpGainTiers { get; set; } = new();

    public List<LeaderboardRewardTier> MostPlayedCreatedMapsTiers { get; set; } = new();
}

public class LeaderboardCycleOptions
{
    public bool EnableWeeklySettlement { get; set; } = true;

    public bool EnableMonthlySettlement { get; set; } = true;

    // Week boundary used by scheduler/settlement. Keep in code config for easy tuning.
    public DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Monday;

    public string TimeZoneId { get; set; } = "SE Asia Standard Time";

    // Suggested recurring schedules for settlement jobs.
    public string WeeklyCron { get; set; } = "0 0 * * 1";

    public string MonthlyCron { get; set; } = "0 0 1 * *";

    // Test mode: run settlement by minute with rolling minute window.
    public bool EnableMinuteTestMode { get; set; } = false;

    public string MinuteTestCron { get; set; } = "*/1 * * * *";

    public int MinuteTestWindowMinutes { get; set; } = 30;
}

public class LeaderboardRewardTier
{
    // Inclusive top-N cutoff for this tier.
    public int TopN { get; set; }

    public int RewardXp { get; set; }

    public decimal RewardOrbitCoin { get; set; }
}
