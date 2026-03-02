using CapstoneProject.API.Hubs;
using CapstoneProject.API.Injection;
using CapstoneProject.Application;
using CapstoneProject.Infrastructure;
using CapstoneProject.Infrastructure.Configurations;
using CapstoneProject.Infrastructure.Filters;
using Hangfire;

namespace CapstoneProject.API.Configurations;

public static class ServiceConfiguration
{
    /// <summary>
    /// Register all application services and infrastructure to keep Program.cs minimal
    /// </summary>
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // Core ASP.NET services
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Disable automatic 400 response for model validation errors
                options.SuppressModelStateInvalidFilter = true;
            });
        
        builder.Services.AddEndpointsApiExplorer();        // Custom Swagger configuration with tagging and styling
        builder.Services.AddSwaggerConfiguration();

        // Cross-cutting concerns
        builder.AddLoggingConfiguration();
        
        // Security configurations
        builder.Services.AddJwtConfiguration(builder.Configuration);
        builder.Services.AddCorsConfiguration(builder.Configuration);
        builder.Services.AddHttpClient();

        // Application & Infrastructure layers
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        // Hangfire background jobs
        builder.Services.AddHangfireServices(builder.Configuration);

        // API layer services (filters, middlewares, validation, etc.)
        builder.Services.AddApiServices(builder.Configuration);

        // SignalR for real-time chat
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB for file uploads
        });

        return builder;
    }

    /// <summary>
    /// Configure the middleware pipeline
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Use custom Swagger configuration with styling and tagging
            app.UseSwaggerConfiguration(app.Environment);
            
            // Hangfire Dashboard (Development only - no auth required)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
                StatsPollingInterval = 2000,
                DisplayStorageConnectionString = false
            });
        }

        // Enable CORS FIRST - Must be before UseHttpsRedirection to avoid preflight redirect issues
        app.UseCorsConfiguration();
        
        // Enable static files for Swagger custom CSS
        app.UseStaticFiles();
        
        // HTTPS Redirection - Only in production or when HTTPS is configured
        // In development, this can cause CORS preflight issues
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // API-specific middlewares (exception handling, JWT, etc.)
        app.UseApiConfiguration();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        // Map SignalR Hubs
        app.MapHub<ChatHub>("/hubs/chat");
        app.MapHub<CompetitiveHub>("/hubs/competitive");

        return app;
    }
}
