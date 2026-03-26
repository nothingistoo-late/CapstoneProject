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

    // For persistence to PostgreSQL "timestamp without time zone" (local Vietnam time).
    // Npgsql expects DateTime.Kind == Unspecified for this PostgreSQL type.
    public static DateTime DbNow => DateTime.SpecifyKind(VietnamNow, DateTimeKind.Unspecified);

    // For display/presentation in Vietnam timezone.
    public static DateTime VietnamNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

    // Keep UTC helpers for cases that truly need UTC.
    public static DateTime UtcNow => DateTime.UtcNow;

    public static DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;

    public static DateTimeOffset VietnamNowOffset => new(VietnamNow, TimeSpan.FromHours(7));
}

