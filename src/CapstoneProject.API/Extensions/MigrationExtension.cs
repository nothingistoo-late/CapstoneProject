using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
using CapstoneProject.Infrastructure.Context;

namespace CapstoneProject.API.Extensions;

public static class MigrationExtension
{
    /// <summary>
    /// Tự động apply các migration pending cho CapstoneProject database
    /// Tự động tạo database nếu chưa tồn tại
    /// </summary>
    /// <param name="app">IApplicationBuilder</param>
    /// <param name="logger">ILogger</param>
    /// <returns>Task</returns>
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            using var CapstoneProjectContext = scope.ServiceProvider.GetRequiredService<CapstoneProjectDbContext>();

            logger.LogInformation("Starting CapstoneProject database migrations...");

            var runMigrationsOnStartup = configuration.GetValue("Database:RunMigrationsOnStartup", true);
            if (!runMigrationsOnStartup)
            {
                logger.LogInformation("Skipping database migrations on startup (Database:RunMigrationsOnStartup=false).");
                return;
            }

            // Step 1: Đảm bảo database được tạo nếu chưa tồn tại
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var autoCreateDatabase = configuration.GetValue("Database:AutoCreate", false);
            if (autoCreateDatabase && !string.IsNullOrEmpty(connectionString))
            {
                await EnsureDatabaseExistsAsync(connectionString, logger);
            }

            // Step 2: Kiểm tra kết nối database với retry logic (sau khi đã đảm bảo database tồn tại)
            try
            {
                await RetryDatabaseConnectionAsync(CapstoneProjectContext, "CapstoneProject", logger);
                logger.LogInformation("Successfully connected to CapstoneProject database.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to CapstoneProject database after multiple attempts!");
                throw;
            }

            // Step 3: Apply pending migrations cho CapstoneProject
            try
            {
                var pendingMigrations = await CapstoneProjectContext.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await CapstoneProjectContext.Database.GetAppliedMigrationsAsync();

                logger.LogInformation(
                    "CapstoneProject DB: Found {PendingCount} pending migrations and {AppliedCount} previously applied migrations",
                    pendingMigrations.Count(),
                    appliedMigrations.Count());

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying pending CapstoneProject migrations: {Migrations}",
                        string.Join(", ", pendingMigrations));

                    await CapstoneProjectContext.Database.MigrateAsync();
                    logger.LogInformation("Successfully applied all pending CapstoneProject migrations.");
                }
                else
                {
                    logger.LogInformation("No pending migrations found for CapstoneProject database.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying CapstoneProject migrations!");
                throw;
            }

            logger.LogInformation("CapstoneProject database migrations completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A problem occurred during CapstoneProject database migrations!");
            throw;
        }
    }

    /// <summary>
    /// Đảm bảo database tồn tại, tạo nếu chưa có (PostgreSQL)
    /// </summary>
    private static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger logger)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("Database name is missing in the connection string.");
            }

            // Hosted Postgres poolers (Supabase 6543, Neon pooler host, …) typically cannot CREATE DATABASE.
            // CREATE DATABASE is typically not allowed and can hang/fail.
            if (builder.Port == 6543 || (builder.Host ?? string.Empty).Contains("pooler", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation(
                    "Skipping database auto-create check (pooler detected). Using existing database '{DatabaseName}'.",
                    databaseName);
                return;
            }

            // Add explicit timeouts to avoid startup hangs
            builder.Timeout = Math.Max(builder.Timeout, 10);
            builder.CommandTimeout = Math.Max(builder.CommandTimeout, 10);

            builder.Database = "postgres";
            var adminConnectionString = builder.ConnectionString;

            await using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync();

            await using (var checkCmd = new NpgsqlCommand(
                             "SELECT 1 FROM pg_database WHERE datname = @name",
                             connection))
            {
                checkCmd.Parameters.AddWithValue("name", databaseName);
                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists != null)
                {
                    logger.LogInformation("Database '{DatabaseName}' already exists", databaseName);
                    return;
                }
            }

            var escaped = databaseName.Replace("\"", "\"\"");
            await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{escaped}\"", connection);
            await createCmd.ExecuteNonQueryAsync();
            logger.LogInformation("Database '{DatabaseName}' created successfully", databaseName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring database exists");
            throw;
        }
    }

    /// <summary>
    /// Thử kết nối database với retry logic
    /// </summary>
    private static async Task RetryDatabaseConnectionAsync(DbContext context, string contextName,
        ILogger logger, int maxRetries = 3, int delaySeconds = 5)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting to connect to {ContextName} database (attempt {Attempt}/{MaxRetries})...",
                    contextName, attempt, maxRetries);

                // Test connection
                await context.Database.CanConnectAsync();
                logger.LogInformation("Successfully connected to {ContextName} database on attempt {Attempt}",
                    contextName, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex,
                    "Failed to connect to {ContextName} database on attempt {Attempt}. Retrying in {DelaySeconds} seconds...",
                    contextName, attempt, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to {ContextName} database after {MaxRetries} attempts",
                    contextName, maxRetries);
                throw;
            }
        }
    }

    /// <summary>
    /// Đảm bảo database được tạo nếu chưa tồn tại
    /// </summary>
    public static void EnsureDatabaseCreated(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var CapstoneProjectDbContext = scope.ServiceProvider.GetRequiredService<CapstoneProjectDbContext>();

            logger.LogInformation("Checking if CapstoneProject database exists...");

            if (CapstoneProjectDbContext.Database.EnsureCreated())
            {
                logger.LogInformation("CapstoneProject database was created successfully.");
            }
            else
            {
                logger.LogInformation("CapstoneProject database already exists.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring CapstoneProject database exists!");
            throw;
        }
    }
}
