using CapstoneProject.Infrastructure.Configurations;
using CapstoneProject.Infrastructure.Filters;

namespace CapstoneProject.API.Extensions;

/// <summary>
/// Extension methods for application startup configuration
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Configure application - ĐÃ DỌN DẸP, CHỈ CÒN DATABASE VÀ SERVICE ACCOUNT TEST
    /// </summary>
    public static async Task<WebApplication> ConfigureApplicationAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ApplicationStartup");
        
        try
        {
            // Step 1: Apply main database migrations
            logger.LogInformation("Applying main database migrations...");
            await app.ApplyMigrationsAsync(logger);
            
            var hangfireEnabled = app.Configuration.GetValue("Hangfire:Enabled", true);
            if (hangfireEnabled)
            {
                // Step 2: Configure Hangfire storage (uses main database)
                logger.LogInformation("Configuring Hangfire storage...");
                await app.Services.ConfigureHangfireStorageAsync(app.Configuration);
                
                // Step 3: Initialize Hangfire recurring jobs
                logger.LogInformation("Initializing Hangfire jobs...");
                app.Services.UseHangfireConfiguration(app.Configuration);
            }
            else
            {
                logger.LogInformation("Hangfire is disabled (Hangfire:Enabled=false).");
            }
            
            logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations.");
            throw;
        }
        
        var seedOnStartup = app.Configuration.GetValue("DataSeeding:RunOnStartup", false);
        if (app.Environment.IsDevelopment() || seedOnStartup)
        {
            // Seed initial data (development or explicitly enabled)
            logger.LogInformation("Seeding initial data...");
            await app.SeedInitialDataAsync(logger);
        }
        
        logger.LogInformation("Application configuration completed successfully");
        return app;
    }
}
