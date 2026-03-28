using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Infrastructure.Services
{
    public class OtpCacheService : IOtpCacheService
    {
        private readonly ILogger<OtpCacheService> _logger;
        private static readonly ConcurrentDictionary<string, OtpCacheItem> _cache = new();
        private static readonly ConcurrentDictionary<string, OtpRequestTracker> _requestTrackers = new();
        private readonly OtpSettings _otpSettings;
        //private readonly Random _random = new();

        public int ExpirationMinutes => _otpSettings.ExpirationMinutes;

        public OtpCacheService(ILogger<OtpCacheService> logger, IOptions<OtpSettings> otpSettings)
        {
            _logger = logger;
            _otpSettings = otpSettings.Value;
        }

        // Implementation of the OTP cache service
        public int CleanUpExpriredOtp()
        {
            try
            {
                var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                var expiredKeys = _cache.Where(exk => exk.Value.ExpiresAt <= now).Select(exk => exk.Key).ToList();
                var removedCount = 0;
                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                    removedCount++;
                    _logger.LogInformation("Removed expired OTP cache item with key {Key}", key);
                }
                return removedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of expired OTP cache items");
                return 0;
            }
        }

        public void ClearAllCache()
        {
            _cache.Clear();
            _logger.LogInformation("Cleared all OTP cache items");
        }

        public string GenerateAndStoreOtp(string contact, OtpTypeEnum type, object userData, NotificationChannelEnum channel = NotificationChannelEnum.Email)
        {
            try
            {
                // 1. Check rate limiting before generating OTP
                var rateLimitCheck = CheckRateLimiting(contact, type);
                if (!rateLimitCheck.IsAllowed)
                {
                    _logger.LogWarning("Rate limit exceeded for contact {Contact} with type {Type}: {Reason}", contact, type, rateLimitCheck.Reason);
                    throw new InvalidOperationException(rateLimitCheck.Reason);
                }

                // 2. Track this OTP request
                TrackOtpRequest(contact, type);

                //3. Generate OTP
                var otpCode = GenerateSecureOtp(_otpSettings.Length);
                //4. Generate Cache Key
                var cacheKey = GenerateCacheKey(contact, type);
                
                //5. Create OtpCacheItem
                var cacheItem = new OtpCacheItem
                {
                    Contact = contact,
                    OtpCode = otpCode,
                    Channel = channel,
                    ExpiresAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow.AddMinutes(_otpSettings.ExpirationMinutes),
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    AttemptCount = 0,
                    MaxAttempts = _otpSettings.MaxAttempts,
                    IsVerified = false,
                    Type = type,
                    userData = userData
                };
                //6. Store in cache with override if key existed
                _cache.AddOrUpdate(cacheKey, cacheItem, (key, existingItem) => cacheItem);
                // Clean up expired items (fire-and-forget)
                _ = Task.Run(() =>
                {
                    CleanUpExpriredOtp();
                    CleanUpExpiredRequestTrackers();
                });
                _logger.LogInformation("Generated and stored OTP for {Contact} with type {Type}, expires at {ExpiresAt}",
                    contact, type, cacheItem.ExpiresAt);
                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating or storing OTP for {Contact} with type {Type}", contact, type);
                throw;
            }
        }

        //helper method to generate OTP
        private string GenerateSecureOtp(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(bytes);
                int digit = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10;
                chars[i] = (char)('0' + digit); // chuyển số thành ký tự trực tiếp
            }

            return new string(chars);
        }

        //helper method to generate cache key
        private string GenerateCacheKey(string contact, OtpTypeEnum type)
        {
            return $"otp_{type.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
        }
        public int GetActiveCacheCount()
        {
            CleanUpExpriredOtp();
            return _cache.Count();
        }

        public OtpCacheItem? GetOtpData(string contact, OtpTypeEnum type)
        {
            var cacheKey = GenerateCacheKey(contact, type);
            if (_cache.TryGetValue(cacheKey, out var cacheItem))
            {
                if (cacheItem.ExpiresAt > CapstoneProject.Domain.Common.VietnamDateTime.DbNow)
                {
                    // Item is still valid
                    return cacheItem;
                }
                else
                {
                    //remove expired item
                    _cache.TryRemove(cacheKey, out _);
                    _logger.LogInformation("OTP for {Contact} with type {Type} has expired and was removed from cache", contact, type);
                }
            }
            return null; // Not found or expired;
        }

        public int GetRemainingAttempts(string contact, OtpTypeEnum type)
        {
            var cacheItem = GetOtpData(contact, type);
            if (cacheItem == null)
                return 0;

            return Math.Max(0, cacheItem.MaxAttempts - cacheItem.AttemptCount);
        }

        public TimeSpan GetRemainingTime(string contact, OtpTypeEnum type)
        {
            var cacheItem = GetOtpData(contact, type);
            if (cacheItem == null || cacheItem.ExpiresAt <= CapstoneProject.Domain.Common.VietnamDateTime.DbNow)
                return TimeSpan.Zero;

            return cacheItem.ExpiresAt - CapstoneProject.Domain.Common.VietnamDateTime.DbNow;

        }

        public bool IsOtpExpired(string contact, OtpTypeEnum type)
        {
            var cacheItem = GetOtpData(contact, type);
            return cacheItem == null || cacheItem.ExpiresAt <= CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        }

        public void RemoveOtp(string contact, OtpTypeEnum type)
        {
            var cacheKey = GenerateCacheKey(contact, type);
            _cache.TryRemove(cacheKey, out _);
            _logger.LogInformation("Removing OTP for {Contact} with type {Type} from cache", contact, type);
        }

        public OtpResult VerifyOtp(string contact, string otpCode, OtpTypeEnum type, NotificationChannelEnum channel = NotificationChannelEnum.Email)
        {
            try
            {
                //1. Generate Cache Key
                var cacheKey = GenerateCacheKey(contact, type);
                //2. Try get cache item
                var cacheItem = GetOtpData(contact, type);
                //3. If valid cache item found, increase attempt count and update
                if (cacheItem == null)
                {
                    _logger.LogWarning("No valid OTP found for {Contact} with type {Type}", contact, type);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "No valid OTP found or OTP has expired."
                    };
                }

                if (cacheItem.Channel != channel)
                {
                    _logger.LogWarning("OTP channel mismatch for {Contact} with type {Type}. Expected {ExpectedChannel}, got {ActualChannel}",
                        contact, type, cacheItem.Channel, channel);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "OTP channel mismatch."
                    };
                }

                if (cacheItem.IsVerified)
                {
                    _logger.LogWarning("OTP for {Contact} with type {Type} has already been verified", contact, type);
                    RemoveOtp(contact, type);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "OTP has already been verified."
                    };
                }

                // if (cacheItem.ExpiresAt <= CapstoneProject.Domain.Common.VietnamDateTime.DbNow)
                // {
                //     RemoveOtp(contact, type);
                //     _logger.LogWarning("OTP for {Contact} with type {Type} has expired", contact, type);
                //     return false;
                // }

                cacheItem.AttemptCount++;
                _cache.AddOrUpdate(cacheKey, cacheItem, (key, existingItem) => cacheItem);

                //4. check max attempts
                if (cacheItem.AttemptCount > cacheItem.MaxAttempts)
                {
                    _logger.LogWarning("Max OTP attempts exceeded for {Contact} with type {Type}", contact, type);
                    
                    // Background cleanup when max attempts exceeded
                    _ = Task.Run(() =>
                    {
                        RemoveOtp(contact, type);
                        ClearRateLimitTracker(contact);
                    });
                    
                    return new OtpResult
                    {
                        Success = false,
                        Message = "Max OTP attempts exceeded."
                    };
                }

                //5. If check max attempts passed, verify OTP code
                bool isValid = cacheItem.OtpCode.Equals(otpCode, StringComparison.Ordinal);

                //6. if verified, mark as verified and update cache item (do not remove yet for cqrs use case)
                if (!isValid)
                {
                    _logger.LogWarning("Invalid OTP code provided for {Contact} with type {Type}. Attempt {AttemptCount}/{MaxAttempts}",
                        contact, type, cacheItem.AttemptCount, cacheItem.MaxAttempts);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "Invalid OTP code.",
                        RemainingAttempts = GetRemainingAttempts(contact, type),
                        ExpiresIn = GetRemainingTime(contact, type)
                    };
                }
                cacheItem.IsVerified = true;
                _cache.AddOrUpdate(cacheKey, cacheItem, (key, existingItem) => cacheItem);
                _logger.LogInformation("OTP verified successfully for {Contact} with type {Type}", contact, type);
                return new OtpResult
                {
                    Success = true,
                    Message = "OTP verified successfully.",
                    UserData = cacheItem.userData,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Contact} with type {Type}", contact, type);
                return new OtpResult
                {
                    Success = false,
                    Message = "Error verifying OTP."
                };
            }
        }

        #region Rate Limiting Methods

        private (bool IsAllowed, string Reason) CheckRateLimiting(string contact, OtpTypeEnum type)
        {
            var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            var trackerKey = $"otp_requests_{type.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
            
            // Get rate limiting settings based on OTP type
            var rateLimitSettings = type switch
            {
                OtpTypeEnum.Registration => _otpSettings.RateLimiting.Registration,
                OtpTypeEnum.PasswordReset => _otpSettings.RateLimiting.PasswordReset,
                _ => _otpSettings.RateLimiting.Registration // fallback to registration settings
            };
            
            var tracker = _requestTrackers.GetOrAdd(trackerKey, new OtpRequestTracker 
            { 
                Contact = contact,
                RequestTimes = new List<DateTime>(),
                LastRequestTime = DateTime.MinValue
            });

            // Check if currently blocked
            if (tracker.BlockedUntil.HasValue && tracker.BlockedUntil > now)
            {
                var remainingTime = tracker.BlockedUntil.Value - now;
                return (false, $"Too many {type} OTP requests. Please try again in {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.");
            }

            // Check cooldown period
            var cooldownMinutes = rateLimitSettings.CooldownMinutes;
            if (tracker.LastRequestTime != DateTime.MinValue && 
                tracker.LastRequestTime.AddMinutes(cooldownMinutes) > now)
            {
                var remainingCooldown = tracker.LastRequestTime.AddMinutes(cooldownMinutes) - now;
                return (false, $"Please wait {remainingCooldown.Seconds} seconds before requesting another {type} OTP.");
            }

            // Clean up old requests outside the window
            var windowStart = now.AddMinutes(-rateLimitSettings.WindowMinutes);
            tracker.RequestTimes.RemoveAll(rt => rt < windowStart);

            // Check rate limit within window
            if (tracker.RequestTimes.Count >= rateLimitSettings.MaxRequestsPerWindow)
            {
                // Block the contact
                tracker.BlockedUntil = now.AddMinutes(rateLimitSettings.BlockDurationMinutes);
                _requestTrackers.AddOrUpdate(trackerKey, tracker, (key, existingTracker) => tracker);
                
                return (false, $"Too many {type} OTP requests. You are blocked for {rateLimitSettings.BlockDurationMinutes} minutes.");
            }

            return (true, string.Empty);
        }

        private void TrackOtpRequest(string contact, OtpTypeEnum type)
        {
            var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            var trackerKey = $"otp_requests_{type.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
            
            // Get rate limiting settings based on OTP type
            var rateLimitSettings = type switch
            {
                OtpTypeEnum.Registration => _otpSettings.RateLimiting.Registration,
                OtpTypeEnum.PasswordReset => _otpSettings.RateLimiting.PasswordReset,
                _ => _otpSettings.RateLimiting.Registration // fallback to registration settings
            };
            
            var tracker = _requestTrackers.GetOrAdd(trackerKey, new OtpRequestTracker 
            { 
                Contact = contact,
                RequestTimes = new List<DateTime>(),
                LastRequestTime = DateTime.MinValue
            });

            tracker.RequestTimes.Add(now);
            tracker.LastRequestTime = now;
            
            // Clean up old requests
            var windowStart = now.AddMinutes(-rateLimitSettings.WindowMinutes);
            tracker.RequestTimes.RemoveAll(rt => rt < windowStart);

            _requestTrackers.AddOrUpdate(trackerKey, tracker, (key, existingTracker) => tracker);
        }

        private int CleanUpExpiredRequestTrackers()
        {
            try
            {
                var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                var expiredKeys = _requestTrackers.Where(kvp =>
                {
                    var tracker = kvp.Value;
                    var key = kvp.Key;
                    
                    // Determine OTP type from key to get correct window settings
                    var otpType = OtpTypeEnum.Registration; // default
                    if (key.Contains("passwordreset"))
                        otpType = OtpTypeEnum.PasswordReset;
                    else if (key.Contains("registration"))
                        otpType = OtpTypeEnum.Registration;
                    
                    var rateLimitSettings = otpType switch
                    {
                        OtpTypeEnum.Registration => _otpSettings.RateLimiting.Registration,
                        OtpTypeEnum.PasswordReset => _otpSettings.RateLimiting.PasswordReset,
                        _ => _otpSettings.RateLimiting.Registration
                    };
                    
                    // Remove if no recent requests and not blocked, or block period has expired
                    var isExpired = (tracker.RequestTimes.Count == 0 || 
                                   tracker.RequestTimes.Max() < now.AddMinutes(-rateLimitSettings.WindowMinutes)) &&
                                   (!tracker.BlockedUntil.HasValue || tracker.BlockedUntil < now);
                    return isExpired;
                }).Select(kvp => kvp.Key).ToList();

                var removedCount = 0;
                foreach (var key in expiredKeys)
                {
                    if (_requestTrackers.TryRemove(key, out _))
                    {
                        removedCount++;
                        _logger.LogInformation("Removed expired OTP request tracker with key {Key}", key);
                    }
                }
                return removedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of expired OTP request trackers");
                return 0;
            }
        }

        public (bool IsBlocked, TimeSpan? RemainingTime) GetRateLimitStatus(string contact)
        {
            var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            
            // Check both Registration and PasswordReset trackers
            foreach (var otpType in new[] { OtpTypeEnum.Registration, OtpTypeEnum.PasswordReset })
            {
                var trackerKey = $"otp_requests_{otpType.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
                if (_requestTrackers.TryGetValue(trackerKey, out var tracker))
                {
                    var rateLimitSettings = otpType switch
                    {
                        OtpTypeEnum.Registration => _otpSettings.RateLimiting.Registration,
                        OtpTypeEnum.PasswordReset => _otpSettings.RateLimiting.PasswordReset,
                        _ => _otpSettings.RateLimiting.Registration
                    };
                    
                    if (tracker.BlockedUntil.HasValue && tracker.BlockedUntil > now)
                    {
                        return (true, tracker.BlockedUntil.Value - now);
                    }

                    var cooldownMinutes = rateLimitSettings.CooldownMinutes;
                    if (tracker.LastRequestTime != DateTime.MinValue && 
                        tracker.LastRequestTime.AddMinutes(cooldownMinutes) > now)
                    {
                        return (false, tracker.LastRequestTime.AddMinutes(cooldownMinutes) - now);
                    }
                }
            }
            
            return (false, null);
        }

        public void ClearRateLimitTracker(string contact)
        {
            // Clear all rate limiting trackers for this contact (both Registration and PasswordReset)
            var registrationKey = $"otp_requests_{OtpTypeEnum.Registration.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
            var passwordResetKey = $"otp_requests_{OtpTypeEnum.PasswordReset.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
            
            bool registrationRemoved = _requestTrackers.TryRemove(registrationKey, out _);
            bool passwordResetRemoved = _requestTrackers.TryRemove(passwordResetKey, out _);
            
            if (registrationRemoved || passwordResetRemoved)
            {
                _logger.LogInformation("Cleared rate limiting trackers for contact {Contact}", contact);
            }
        }

        #endregion
    }
}


