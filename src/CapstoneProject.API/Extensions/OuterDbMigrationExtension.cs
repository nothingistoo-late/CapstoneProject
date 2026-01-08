using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CapstoneProject.Infrastructure.Context;

namespace CapstoneProject.API.Extensions;

public static class OuterDbMigrationExtension
{
    /// <summary>
    /// Apply pending migrations for Outer Database (External services, Hangfire)
    /// </summary>
    public static async Task ApplyOuterDbMigrationsAsync(this IApplicationBuilder app, ILogger logger)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var outerDbContext = scope.ServiceProvider.GetService<CapstoneProjectOuterDbContext>();
        
        if (outerDbContext == null)
        {
            logger.LogWarning("CapstoneProjectOuterDbContext is not registered. Skipping outer database migrations.");
            return;
        }

        try
        {
            var pendingMigrations = await outerDbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations to Outer Database...", pendingMigrations.Count());
                await outerDbContext.Database.MigrateAsync();
                logger.LogInformation("Outer Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations for Outer Database.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying Outer Database migrations");
            throw;
        }
    }
}
