using CapstoneProject.API.Attributes;
using CapstoneProject.API.Middlewares;
using CapstoneProject.API.Services;
using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.API.Injection;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HttpContextAccessor
        services.AddHttpContextAccessor();
        
        // Register FileService with factory pattern (based on appsettings.json configuration)
        // The factory automatically selects the appropriate provider based on FileStorage:ProviderType
        services.AddScoped<IFileService>(provider =>
        {
            var factory = provider.GetRequiredService<IFileServiceFactory>();
            return factory.CreateFileService();
        });
        
        // Register role-based access filters
        services.AddScoped<LearnerRoleAccessFilter>();
        services.AddScoped<ModeratorRoleAccessFilter>();
        services.AddScoped<AdminRoleAccessFilter>();

        // Register validation configuration
        services.AddValidationConfiguration();
        
        // Register chat broadcast service for SignalR
        services.AddScoped<IChatBroadcastService, ChatBroadcastService>();


        return services;
    }

    public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
    {
        // Use global exception handling
        app.UseGlobalExceptionHandling();

        // Use JWT middleware
        app.UseJwtMiddleware();

        return app;
    }
} 