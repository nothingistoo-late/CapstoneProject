namespace CapstoneProject.Application.Common.Models;

public class OtpSettings
{
    public int Length { get; set; }
    public int ExpirationMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public OtpRateLimitingConfiguration RateLimiting { get; set; } = new();
}

public class OtpRateLimitingConfiguration
{
    public OtpRateLimitingSettings Registration { get; set; } = new();
    public OtpRateLimitingSettings PasswordReset { get; set; } = new();
}

public class OtpRateLimitingSettings
{
    public int CooldownMinutes { get; set; } = 1;
    public int MaxRequestsPerWindow { get; set; } = 3;
    public int WindowMinutes { get; set; } = 5;
    public int BlockDurationMinutes { get; set; } = 15;
}

public class OtpRequestTracker
{
    public string Contact { get; set; } = string.Empty;
    public List<DateTime> RequestTimes { get; set; } = new();
    public DateTime? BlockedUntil { get; set; }
    public DateTime LastRequestTime { get; set; }
}