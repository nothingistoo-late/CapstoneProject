namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>
/// Helper class for DateTime operations, especially Vietnam timezone conversions
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Vietnam timezone identifier - supports both Windows and Linux
    /// </summary>
    private static readonly TimeZoneInfo VietnamTimeZone = GetVietNamTimeZoneInfo();

    /// <summary>
    /// Get Vietnam timezone info - handles both Windows and Linux
    /// </summary>
    private static TimeZoneInfo GetVietNamTimeZoneInfo()
    {
        try
        {
            // Try Windows timezone ID first
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            try
            {
                // Try Linux timezone ID
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                try
                {
                    // Try alternative Linux timezone ID
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Saigon");
                }
                catch
                {
                    // Fallback to UTC+7 offset if timezone not found
                    return TimeZoneInfo.CreateCustomTimeZone("Vietnam Time", TimeSpan.FromHours(7), "Vietnam Time", "Vietnam Time");
                }
            }
        }
    }

    /// <summary>
    /// Get current Vietnam time
    /// </summary>
    /// <returns>Current DateTime in Vietnam timezone</returns>
    public static DateTime GetVietNamTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
    }

    /// <summary>
    /// Convert UTC DateTime to Vietnam time
    /// </summary>
    /// <param name="utcDateTime">UTC DateTime to convert</param>
    /// <returns>DateTime in Vietnam timezone</returns>
    public static DateTime GetVietNamTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Unspecified)
        {
            // Assume it's UTC if unspecified
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.ToUniversalTime(), VietnamTimeZone);
    }

    /// <summary>
    /// Convert Vietnam time to UTC DateTime
    /// </summary>
    /// <param name="vietnamDateTime">Vietnam DateTime to convert</param>
    /// <returns>DateTime in UTC</returns>
    public static DateTime GetUtcTime(DateTime vietnamDateTime)
    {
        if (vietnamDateTime.Kind == DateTimeKind.Unspecified)
        {
            // Assume it's Vietnam time if unspecified
            vietnamDateTime = DateTime.SpecifyKind(vietnamDateTime, DateTimeKind.Unspecified);
        }
        
        return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
    }

    /// <summary>
    /// Get current Vietnam time as nullable DateTime
    /// </summary>
    /// <returns>Current DateTime in Vietnam timezone, or null if error</returns>
    public static DateTime? GetVietNamTimeNullable()
    {
        try
        {
            return GetVietNamTime();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get Vietnam timezone info
    /// </summary>
    /// <returns>TimeZoneInfo for Vietnam</returns>
    public static TimeZoneInfo GetVietNamTimeZone()
    {
        return VietnamTimeZone;
    }
}

