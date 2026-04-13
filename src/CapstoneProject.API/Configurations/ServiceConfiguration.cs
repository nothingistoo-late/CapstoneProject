using CapstoneProject.API.Hubs;
using CapstoneProject.API.Injection;
using CapstoneProject.Application;
using CapstoneProject.Infrastructure;
using CapstoneProject.Infrastructure.Configurations;
using CapstoneProject.Infrastructure.Filters;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;

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

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Cross-cutting concerns
        builder.AddLoggingConfiguration();
        
        // Security configurations
        builder.Services.AddJwtConfiguration(builder.Configuration);
        builder.Services.AddCorsConfiguration(builder.Configuration);
        builder.Services.AddHttpClient();

        // Application & Infrastructure layers
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

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
        app.UseForwardedHeaders();

        var swaggerInProd = app.Configuration.GetValue("Swagger:EnabledInProduction", false);
        if (app.Environment.IsDevelopment() || swaggerInProd)
        {
            app.UseSwaggerConfiguration(app.Environment);
        }

        if (app.Environment.IsDevelopment())
        {
            var hangfireEnabled = app.Configuration.GetValue("Hangfire:Enabled", true);
            if (hangfireEnabled)
            {
                // Hangfire Dashboard (Development only - no auth required)
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
                    StatsPollingInterval = 2000,
                    DisplayStorageConnectionString = false
                });
            }
        }

        app.UseCorsConfiguration();
        
        // Enable static files for Swagger custom CSS
        app.UseStaticFiles();
        
        // TLS terminates at Koyeb / reverse proxy; Kestrel only serves HTTP in the container.
        // UseHttpsRedirection() here caused "Failed to determine the https port" and is unnecessary.

        // API-specific middlewares (exception handling, JWT, etc.)
        app.UseApiConfiguration();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        // Map SignalR Hubs
        app.MapHub<ChatHub>("/hubs/chat");
        app.MapHub<GameLobbyHub>("/hubs/gamelobby");

        return app;
    }
}
