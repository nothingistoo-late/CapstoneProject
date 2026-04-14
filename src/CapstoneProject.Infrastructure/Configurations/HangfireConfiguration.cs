using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models.Leaderboards;
using CapstoneProject.Infrastructure.Services;

namespace CapstoneProject.Infrastructure.Configurations;

public static class HangfireConfiguration
{
    /// <summary>
    /// Add Hangfire services to the dependency container
    /// Uses the main database (DefaultConnection)
    /// </summary>
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging to suppress Hangfire logs in production
        services.AddLogging(builder =>
        {
            builder.AddFilter("Hangfire", LogLevel.Warning);
            builder.AddFilter("Hangfire.PostgreSql", LogLevel.Warning);
            builder.AddFilter("Hangfire.Processing", LogLevel.Warning);
        });

        // Get connection string - use main database (DefaultConnection)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection string is required for Hangfire");
        }

        var prepareSchemaConfigured = configuration.GetValue<bool?>("Hangfire:PrepareSchemaIfNecessary");
        var prepareSchemaIfNecessary = prepareSchemaConfigured ?? true;
        if (!prepareSchemaConfigured.HasValue)
        {
            try
            {
                var b = new NpgsqlConnectionStringBuilder(connectionString);
                if (b.Port == 6543 || (b.Host ?? string.Empty).Contains("pooler", StringComparison.OrdinalIgnoreCase))
                {
                    // Default behavior only: on poolers, avoid DDL unless explicitly enabled via config.
                    prepareSchemaIfNecessary = false;
                }
            }
            catch
            {
                // ignore parsing errors; fallback to default value
            }
        }

        // Get Hangfire settings from configuration
        var slidingInvisibilityTimeout = configuration.GetValue("Hangfire:SlidingInvisibilityTimeout", 300);
        var queuePollInterval = configuration.GetValue("Hangfire:QueuePollInterval", 0);

        // Get retry settings
        var retryAttempts = configuration.GetValue("Hangfire:Retry:Attempts", 3);
        var retryDelayFirst = configuration.GetValue("Hangfire:Retry:DelayInSeconds:First", 60);
        var retryDelaySecond = configuration.GetValue("Hangfire:Retry:DelayInSeconds:Second", 300);
        var retryDelayThird = configuration.GetValue("Hangfire:Retry:DelayInSeconds:Third", 600);

        var storageOptions = new PostgreSqlStorageOptions
        {
            PrepareSchemaIfNecessary = prepareSchemaIfNecessary,
            QueuePollInterval = queuePollInterval > 0
                ? TimeSpan.FromSeconds(queuePollInterval)
                : TimeSpan.FromSeconds(15),
            InvisibilityTimeout = slidingInvisibilityTimeout > 0
                ? TimeSpan.FromSeconds(slidingInvisibilityTimeout)
                : TimeSpan.FromMinutes(30),
            UseSlidingInvisibilityTimeout = slidingInvisibilityTimeout > 0,
        };

        // Add Hangfire services
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                bootstrap => bootstrap.UseNpgsqlConnection(connectionString),
                storageOptions)
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
    /// Cấu hình Hangfire storage sử dụng main database
    /// Database đã được tạo trong bước migration trước đó
    /// </summary>
    public static async Task ConfigureHangfireStorageAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HangfireConfiguration");

        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogError("No connection string found for Hangfire storage");
                throw new InvalidOperationException("DefaultConnection string is required for Hangfire");
            }

            var databaseName = ExtractDatabaseName(connectionString);
            
            // Database should already exist from migration step, but check anyway
            if (!await CheckHangfireDatabaseExistsAsync(connectionString, databaseName, logger))
            {
                logger.LogWarning("Main database '{DatabaseName}' does not exist yet.", databaseName);
            }
            else
            {
                logger.LogInformation("Hangfire will use main database {DatabaseName}", databaseName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error configuring Hangfire storage");
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra database có tồn tại không
    /// </summary>
    private static async Task<bool> CheckHangfireDatabaseExistsAsync(string connectionString, string databaseName, ILogger logger)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "postgres"
            };
            
            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            
            await using var command = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @name",
                connection);
            command.Parameters.AddWithValue("name", databaseName);
            var result = await command.ExecuteScalarAsync();
            
            return result != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking database existence");
            return false;
        }
    }

    /// <summary>
    /// Trích xuất tên database từ connection string
    /// </summary>
    private static string ExtractDatabaseName(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return builder.Database ?? throw new InvalidOperationException("Database name missing in connection string.");
    }

    private static void CleanupObsoleteQuickLoginRecurringRecords(IConfiguration configuration, ILogger logger)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
DELETE FROM hangfire.hash WHERE ""key"" = 'recurring-job:quicklogin-cleanup-inactive';
DELETE FROM hangfire.set WHERE ""key"" = 'recurring-jobs' AND value = 'quicklogin-cleanup-inactive';";

            command.ExecuteNonQuery();
            logger.LogInformation("Removed obsolete Hangfire recurring records for quicklogin-cleanup-inactive");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not purge obsolete quicklogin recurring records from Hangfire tables");
        }
    }

    private static void TryScheduleRecurringJob(Action scheduleAction, ILogger logger, string jobId)
    {
        try
        {
            scheduleAction();
        }
        catch (PostgreSqlDistributedLockException ex)
        {
            logger.LogWarning(ex, "Skipped registering recurring job {JobId} due to distributed lock timeout. App will continue to run.", jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register recurring job {JobId}", jobId);
        }
    }

    private static void TryRemoveRecurringJob(IRecurringJobManager recurringJobManager, ILogger logger, string jobId)
    {
        try
        {
            recurringJobManager.RemoveIfExists(jobId);
            logger.LogInformation("Recurring job removed: {JobId}", jobId);
        }
        catch (PostgreSqlDistributedLockException ex)
        {
            logger.LogWarning(ex, "Skipped removing recurring job {JobId} due to distributed lock timeout.", jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove recurring job {JobId}", jobId);
        }
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
            logger.LogInformation("🗄️ Database: Main Database");
            logger.LogInformation("🌐 Dashboard: /hangfire (no auth required)");
            logger.LogInformation("Email: Using Service Account (no token management needed)");
        }

        // All OAuth token management jobs are disabled
        // Using Service Account authentication instead
        CleanupObsoleteQuickLoginRecurringRecords(configuration, logger);
        
        // Setup leaderboard reward settlement jobs (weekly + monthly + optional minute test)
        var lbOptions = configuration.GetSection(LeaderboardRewardsOptions.SectionName).Get<LeaderboardRewardsOptions>()
                        ?? new LeaderboardRewardsOptions();
        var cycle = lbOptions.Cycle ?? new LeaderboardCycleOptions();

        TimeZoneInfo settlementTimeZone;
        try
        {
            settlementTimeZone = TimeZoneInfo.FindSystemTimeZoneById(cycle.TimeZoneId);
        }
        catch
        {
            settlementTimeZone = TimeZoneInfo.Utc;
        }

        if (cycle.EnableWeeklySettlement)
        {
            TryScheduleRecurringJob(() =>
                recurringJobManager.AddOrUpdate(
                    "leaderboard-reward-settlement-weekly",
                    (LeaderboardRewardSettlementJob job) => job.ExecuteWeeklyAsync(),
                    cycle.WeeklyCron,
                    new RecurringJobOptions { TimeZone = settlementTimeZone }),
                logger,
                "leaderboard-reward-settlement-weekly");

            logger.LogInformation("✅ Leaderboard weekly settlement job scheduled with cron: {Cron}", cycle.WeeklyCron);
        }
        else
        {
            TryRemoveRecurringJob(recurringJobManager, logger, "leaderboard-reward-settlement-weekly");
        }

        if (cycle.EnableMonthlySettlement)
        {
            TryScheduleRecurringJob(() =>
                recurringJobManager.AddOrUpdate(
                    "leaderboard-reward-settlement-monthly",
                    (LeaderboardRewardSettlementJob job) => job.ExecuteMonthlyAsync(),
                    cycle.MonthlyCron,
                    new RecurringJobOptions { TimeZone = settlementTimeZone }),
                logger,
                "leaderboard-reward-settlement-monthly");

            logger.LogInformation("✅ Leaderboard monthly settlement job scheduled with cron: {Cron}", cycle.MonthlyCron);
        }
        else
        {
            TryRemoveRecurringJob(recurringJobManager, logger, "leaderboard-reward-settlement-monthly");
        }

        if (cycle.EnableMinuteTestMode)
        {
            var minuteWindow = Math.Max(1, cycle.MinuteTestWindowMinutes);
            TryScheduleRecurringJob(() =>
                recurringJobManager.AddOrUpdate(
                    "leaderboard-reward-settlement-minute-test",
                    (LeaderboardRewardSettlementJob job) => job.ExecuteMinuteTestAsync(minuteWindow),
                    cycle.MinuteTestCron,
                    new RecurringJobOptions { TimeZone = settlementTimeZone }),
                logger,
                "leaderboard-reward-settlement-minute-test");

            logger.LogInformation(
                "✅ Leaderboard minute-test settlement job scheduled with cron: {Cron}; window={Window} minutes",
                cycle.MinuteTestCron,
                minuteWindow);
        }
            else
            {
                TryRemoveRecurringJob(recurringJobManager, logger, "leaderboard-reward-settlement-minute-test");
            }
        
        logger.LogInformation("Hangfire configured successfully (Service Account mode - no token jobs needed)");
    }
}
