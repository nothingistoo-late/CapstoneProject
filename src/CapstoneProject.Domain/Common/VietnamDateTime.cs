namespace CapstoneProject.Domain.Common;

public static class VietnamDateTime
{
    private static readonly TimeZoneInfo VietnamTimeZone = CreateVietnamTimeZone();

    private static TimeZoneInfo CreateVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                return TimeZoneInfo.CreateCustomTimeZone(
                    "Vietnam Time",
                    TimeSpan.FromHours(7),
                    "Vietnam Time",
                    "Vietnam Time");
            }
        }
    }

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(CapstoneProject.Domain.Common.VietnamDateTime.Now, VietnamTimeZone);

    public static DateTimeOffset NowOffset => new(Now, TimeSpan.FromHours(7));
}

