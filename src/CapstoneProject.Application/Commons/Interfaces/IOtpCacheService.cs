using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Common.Interfaces;

public interface IOtpCacheService
{
    // Core OTP Operations
    string GenerateAndStoreOtp(string contact, OtpTypeEnum type, object userData, NotificationChannelEnum channel = NotificationChannelEnum.Email);
    OtpCacheItem? GetOtpData(string contact, OtpTypeEnum type);
    OtpResult VerifyOtp(string contact, string otpCode, OtpTypeEnum type, NotificationChannelEnum channel = NotificationChannelEnum.Email);
    void RemoveOtp(string contact, OtpTypeEnum type);

    //Utility methods
    int GetRemainingAttempts(string contact, OtpTypeEnum type);
    bool IsOtpExpired(string contact, OtpTypeEnum type);
    TimeSpan GetRemainingTime(string contact, OtpTypeEnum type);

    // Management Operations
    int CleanUpExpriredOtp();
    int GetActiveCacheCount();
    void ClearAllCache();

    // Rate Limiting Operations
    void ClearRateLimitTracker(string contact);
    (bool IsBlocked, TimeSpan? RemainingTime) GetRateLimitStatus(string contact);

    // Settings
    int ExpirationMinutes { get; }
}