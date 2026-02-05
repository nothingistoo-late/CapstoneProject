using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapstoneProject.Infrastructure.Services;

public class QuickLoginCleanupService : IQuickLoginCleanupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QuickLoginCleanupService> _logger;
    private const string QuickLoginEmailDomain = "@quicklogin.test";

    public QuickLoginCleanupService(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IConfiguration configuration,
        ILogger<QuickLoginCleanupService> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<int> CleanupInactiveUsersAsync(int daysInactive = 7, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cleanup of inactive QuickLogin users (inactive for {Days} days)", daysInactive);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysInactive);
            
            // Get all QuickLogin users that haven't logged in for the specified days
            // Or never logged in and created before cutoff date
            var quickLoginUsers = await _unitOfWork.Repository<AppUser>()
                .FindAsync(
                    predicate: u => u.Email != null && 
                                   u.Email.EndsWith(QuickLoginEmailDomain) &&
                                   u.Status == EntityStatusEnum.Active &&
                                   (u.LastLoginAt == null || u.LastLoginAt < cutoffDate)
                );

            var usersToDelete = quickLoginUsers.ToList();
            var deleteCount = 0;

            foreach (var user in usersToDelete)
            {
                try
                {
                    // Deactivate user (soft delete)
                    user.DeactivateUser(user.Id);
                    var updateResult = await _identityService.UpdateUserAsync(user);
                    
                    if (updateResult.Succeeded)
                    {
                        deleteCount++;
                        _logger.LogDebug("Deactivated QuickLogin user: {Email} (LastLogin: {LastLogin}, Created: {Created})", 
                            user.Email, user.LastLoginAt, user.CreatedAt);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deactivate QuickLogin user: {Email}", user.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deactivating QuickLogin user: {Email}", user.Email);
                }
            }

            if (deleteCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Cleanup completed: {Count} inactive QuickLogin users deactivated", deleteCount);
            }
            else
            {
                _logger.LogInformation("Cleanup completed: No inactive QuickLogin users found");
            }

            return deleteCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during QuickLogin users cleanup");
            throw;
        }
    }

    public async Task<int> CleanupOldUsersAsync(int daysOld = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cleanup of old QuickLogin users (older than {Days} days)", daysOld);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            
            // Get all QuickLogin users created before the cutoff date
            var quickLoginUsers = await _unitOfWork.Repository<AppUser>()
                .FindAsync(
                    predicate: u => u.Email != null && 
                                   u.Email.EndsWith(QuickLoginEmailDomain) &&
                                   u.Status == EntityStatusEnum.Active &&
                                   u.CreatedAt.HasValue &&
                                   u.CreatedAt.Value < cutoffDate
                );

            var usersToDelete = quickLoginUsers.ToList();
            var deleteCount = 0;

            foreach (var user in usersToDelete)
            {
                try
                {
                    // Deactivate user (soft delete)
                    user.DeactivateUser(user.Id);
                    var updateResult = await _identityService.UpdateUserAsync(user);
                    
                    if (updateResult.Succeeded)
                    {
                        deleteCount++;
                        _logger.LogDebug("Deactivated old QuickLogin user: {Email} (Created: {Created})", 
                            user.Email, user.CreatedAt);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deactivate QuickLogin user: {Email}", user.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deactivating QuickLogin user: {Email}", user.Email);
                }
            }

            if (deleteCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Cleanup completed: {Count} old QuickLogin users deactivated", deleteCount);
            }
            else
            {
                _logger.LogInformation("Cleanup completed: No old QuickLogin users found");
            }

            return deleteCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during QuickLogin users cleanup");
            throw;
        }
    }
}
