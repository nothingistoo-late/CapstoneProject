using CapstoneProject.Application.Commons.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CapstoneProject.Infrastructure.Services;

/// <summary>
/// Hangfire job class for cleaning up inactive QuickLogin users
/// Hangfire will automatically resolve this class and its dependencies from DI container
/// </summary>
public class QuickLoginCleanupJob
{
    private readonly IQuickLoginCleanupService _cleanupService;
    private readonly ILogger<QuickLoginCleanupJob> _logger;

    public QuickLoginCleanupJob(
        IQuickLoginCleanupService cleanupService,
        ILogger<QuickLoginCleanupJob> logger)
    {
        _cleanupService = cleanupService;
        _logger = logger;
    }

    /// <summary>
    /// Hangfire job method to cleanup inactive QuickLogin users
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task Execute(int daysInactive)
    {
        try
        {
            _logger.LogInformation("Starting QuickLogin cleanup job (inactive for {Days} days)", daysInactive);
            var deletedCount = await _cleanupService.CleanupInactiveUsersAsync(daysInactive);
            _logger.LogInformation("QuickLogin cleanup job completed: {Count} users deactivated", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QuickLogin cleanup job");
            throw;
        }
    }
}
