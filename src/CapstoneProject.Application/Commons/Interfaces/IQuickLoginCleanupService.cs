namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Service for cleaning up inactive QuickLogin users
/// </summary>
public interface IQuickLoginCleanupService
{
    /// <summary>
    /// Clean up QuickLogin users that haven't logged in for a specified number of days
    /// </summary>
    /// <param name="daysInactive">Number of days of inactivity before deletion (default: 7)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of users deleted</returns>
    Task<int> CleanupInactiveUsersAsync(int daysInactive = 7, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up all QuickLogin users older than specified days (regardless of last login)
    /// </summary>
    /// <param name="daysOld">Number of days old before deletion (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of users deleted</returns>
    Task<int> CleanupOldUsersAsync(int daysOld = 30, CancellationToken cancellationToken = default);
}
