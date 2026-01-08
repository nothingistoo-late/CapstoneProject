using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Infrastructure.Services;

namespace CapstoneProject.Infrastructure.Configurations;

public static class HangfireConfiguration
{
    /// <summary>
    /// Add Hangfire services to the dependency container
    /// Uses the same OuterDbConnection as token management for consistency
    /// </summary>
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging to suppress Hangfire logs in production
        services.AddLogging(builder =>
        {
            builder.AddFilter("Hangfire", LogLevel.Warning);
            builder.AddFilter("Hangfire.SqlServer", LogLevel.Warning);
            builder.AddFilter("Hangfire.Processing", LogLevel.Warning);
        });

        // Get connection string - use OuterDbConnection for consistency with token management
        var useOuterDatabase = configuration.GetValue("Hangfire:UseOuterDatabase", true);
        var connectionString = useOuterDatabase 
            ? configuration.GetConnectionString("OuterDbConnection")
            : configuration["Hangfire:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                useOuterDatabase 
                    ? "OuterDbConnection string is required for Hangfire when UseOuterDatabase is true"
                    : "Hangfire database connection string is required");
        }

        // Get Hangfire settings from configuration
        var commandBatchMaxTimeout = configuration.GetValue("Hangfire:CommandBatchMaxTimeout", 300);
        var slidingInvisibilityTimeout = configuration.GetValue("Hangfire:SlidingInvisibilityTimeout", 300);
        var queuePollInterval = configuration.GetValue("Hangfire:QueuePollInterval", 0);
        var useRecommendedIsolationLevel = configuration.GetValue("Hangfire:UseRecommendedIsolationLevel", true);
        var disableGlobalLocks = configuration.GetValue("Hangfire:DisableGlobalLocks", true);

        // Get retry settings
        var retryAttempts = configuration.GetValue("Hangfire:Retry:Attempts", 3);
        var retryDelayFirst = configuration.GetValue("Hangfire:Retry:DelayInSeconds:First", 60);
        var retryDelaySecond = configuration.GetValue("Hangfire:Retry:DelayInSeconds:Second", 300);
        var retryDelayThird = configuration.GetValue("Hangfire:Retry:DelayInSeconds:Third", 600);

        // Add Hangfire services
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromSeconds(commandBatchMaxTimeout),
                SlidingInvisibilityTimeout = TimeSpan.FromSeconds(slidingInvisibilityTimeout),
                QueuePollInterval = TimeSpan.FromSeconds(queuePollInterval),
                UseRecommendedIsolationLevel = useRecommendedIsolationLevel,
                DisableGlobalLocks = disableGlobalLocks,
                PrepareSchemaIfNecessary = true,
            })
            .UseFilter(new AutomaticRetryAttribute 
            { 
                Attempts = retryAttempts,
                DelaysInSeconds = new[] { retryDelayFirst, retryDelaySecond, retryDelayThird }
                    .Take(retryAttempts)
                    .ToArray()
            }));

        // Get server settings from configuration
        var heartbeatInterval = configuration.GetValue("Hangfire:HeartbeatInterval", 30);
        var workerCount = configuration.GetValue("Hangfire:WorkerCount", 0);
        
        // Get queues from configuration with order preserved
        var queues = configuration.GetSection("Hangfire:Queues").Get<string[]>() ?? 
            new[] { "token-management", "email-sending", "cleanup", "default" };

        // Add Hangfire server with ordered queues
        services.AddHangfireServer(options =>
        {
            options.HeartbeatInterval = TimeSpan.FromSeconds(heartbeatInterval);
            options.Queues = queues;
            if (workerCount > 0)
            {
                options.WorkerCount = workerCount;
            }
        });

        return services;
    }

    /// <summary>
    /// Cấu hình Hangfire storage sử dụng Outer Database đã có
    /// </summary>
    public static async Task ConfigureHangfireStorageAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HangfireConfiguration");

        try
        {
            var useOuterDatabase = configuration.GetValue("Hangfire:UseOuterDatabase", true);
            var connectionString = useOuterDatabase 
                ? configuration.GetConnectionString("OuterDbConnection")
                : configuration["Hangfire:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogError("No connection string found for Hangfire storage");
                throw new InvalidOperationException("Hangfire connection string is required");
            }

            var databaseName = ExtractDatabaseName(connectionString);
            
            if (!await CheckHangfireDatabaseExistsAsync(connectionString, databaseName, logger))
            {
                var errorMessage = useOuterDatabase
                    ? $"Outer database '{databaseName}' does not exist. Please ensure migrations are applied before Hangfire initialization."
                    : $"Hangfire database '{databaseName}' does not exist. Please create it manually.";
                
                logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
            else
            {
                logger.LogInformation("Hangfire will use database {DatabaseName} (shared with token management)", databaseName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error configuring Hangfire storage");
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra database Hangfire có tồn tại không
    /// </summary>
    private static async Task<bool> CheckHangfireDatabaseExistsAsync(string connectionString, string databaseName, ILogger logger)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var masterConnectionString = builder.ConnectionString.Replace(builder.InitialCatalog, "master");
            
            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", connection);
            var result = await command.ExecuteScalarAsync();
            var count = result != null ? (int)result : 0;
            
            return count > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Hangfire database existence");
            return false;
        }
    }

    /// <summary>
    /// Trích xuất tên database từ connection string
    /// </summary>
    private static string ExtractDatabaseName(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        return builder.InitialCatalog;
    }

    /// <summary>
    /// Configure Hangfire dashboard and initialize recurring jobs
    /// Token management jobs are disabled since we're using Service Account authentication
    /// </summary>
    public static void UseHangfireConfiguration(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HangfireConfiguration");
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        // Only log detailed configuration in development
        if (environment.IsDevelopment())
        {
            logger.LogInformation("=== HANGFIRE CONFIGURATION ===");
            logger.LogInformation("🔧 DEVELOPMENT MODE: Dashboard accessible without authentication");
            logger.LogInformation("🗄️ Database: Shared OuterDb");
            logger.LogInformation("🌐 Dashboard: /hangfire (no auth required)");
            logger.LogInformation("� Email: Using Service Account (no token management needed)");
        }

        // All OAuth token management jobs are disabled
        // Using Service Account authentication instead
        
        logger.LogInformation("Hangfire configured successfully (Service Account mode - no token jobs needed)");
    }
}
