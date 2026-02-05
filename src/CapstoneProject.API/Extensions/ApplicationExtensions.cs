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
            
            // Step 2: Configure Hangfire storage (uses main database)
            logger.LogInformation("Configuring Hangfire storage...");
            await app.Services.ConfigureHangfireStorageAsync(app.Configuration);
            
            // Step 3: Initialize Hangfire recurring jobs
            logger.LogInformation("Initializing Hangfire jobs...");
            app.Services.UseHangfireConfiguration(app.Configuration);
            
            logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations.");
            throw;
        }
        
        if (app.Environment.IsDevelopment())
        {
            // Step 3: Seed initial data (only in development)
            logger.LogInformation("Seeding initial data...");
            await app.SeedInitialDataAsync(logger);
        }
        
        logger.LogInformation("Application configuration completed successfully");
        return app;
    }
}
