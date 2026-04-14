using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Commons.Helpers;

public static class LeaderboardPeriodHelper
{
    // Update these values in code to change leaderboard cycle boundaries.
    public static DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Monday;

    public static (DateTime DateFrom, DateTime DateTo) GetRange(LeaderboardPeriodTypeEnum periodType, DateTime? now = null)
    {
        var current = now ?? VietnamDateTime.DbNow;
        var dateTo = current;

        DateTime dateFrom;
        if (periodType == LeaderboardPeriodTypeEnum.Week)
        {
            var diff = (7 + (current.DayOfWeek - WeekStartsOn)) % 7;
            dateFrom = current.Date.AddDays(-diff);
        }
        else
        {
            dateFrom = new DateTime(current.Year, current.Month, 1);
        }

        return (dateFrom, dateTo);
    }
}
