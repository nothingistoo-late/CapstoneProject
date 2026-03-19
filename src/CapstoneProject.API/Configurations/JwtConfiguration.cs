using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CapstoneProject.API.Configurations;

/// <summary>
/// Extension methods for JWT configuration
/// </summary>
public static class JwtConfiguration
{
    /// <summary>
    /// Configure JWT authentication
    /// </summary>
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, 
        IConfiguration configuration, bool requireHttps = false)
    {
        // Get JWT settings
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey))
            throw new ArgumentNullException(nameof(jwtKey), "JWT Key is not configured");

        if (string.IsNullOrEmpty(jwtIssuer))
            throw new ArgumentNullException(nameof(jwtIssuer), "JWT Issuer is not configured");

        if (string.IsNullOrEmpty(audience))
            throw new ArgumentNullException(nameof(audience), "JWT Audience is not configured");

        // Validate JWT key length (HS256 requires at least 256 bits = 32 bytes)
        var key = Encoding.UTF8.GetBytes(jwtKey);
        if (key.Length < 32)
        {
            throw new ArgumentException(
                $"JWT Key must be at least 32 bytes (256 bits) for HS256 algorithm. Current key is {key.Length} bytes. " +
                "Please generate a longer key (at least 32 characters).", 
                nameof(jwtKey));
        }
        const string AllowExpiredScheme = "JwtBearerAllowExpired";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = requireHttps;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            // Add event handlers for JWT bearer events
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var path = context.HttpContext.Request.Path;
                    
                    // Log authentication failures for SignalR (for debugging)
                    if (path.StartsWithSegments("/hubs"))
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                        var logger = loggerFactory?.CreateLogger("SignalR.Authentication");
                        logger?.LogWarning(
                            "SignalR authentication failed for path: {Path}. Error: {Error}",
                            path,
                            context.Exception.Message
                        );
                    }
                    
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                },
                // SignalR sends token via query string, not Authorization header
                // This is because WebSocket API in browsers doesn't support custom headers
                // Token must be sent as query parameter "access_token"
                OnMessageReceived = context =>
                {
                    var path = context.HttpContext.Request.Path;
                    
                    // Only process SignalR hub endpoints
                    if (!path.StartsWithSegments("/hubs"))
                    {
                        return Task.CompletedTask;
                    }
                    
                    // For SignalR connections, token is sent as query parameter "access_token"
                    var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                    
                    // Also check Authorization header for non-WebSocket transports (SSE, Long Polling)
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            accessToken = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        // Log for debugging (only in development)
                        var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                        var logger = loggerFactory?.CreateLogger("SignalR.Authentication");
                        logger?.LogDebug(
                            "SignalR token received for path: {Path}, Token length: {TokenLength}",
                            path,
                            accessToken.Length
                        );
                        
                        context.Token = accessToken;
                    }
                    else
                    {
                        // Log missing token
                        var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                        var logger = loggerFactory?.CreateLogger("SignalR.Authentication");
                        logger?.LogWarning(
                            "SignalR connection attempt without token for path: {Path}",
                            path
                        );
                    }
                    
                    return Task.CompletedTask;
                }
            };
        })
        .AddJwtBearer(AllowExpiredScheme, options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = requireHttps;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false, // Allow expired tokens for refresh-token endpoint only
                ClockSkew = TimeSpan.Zero
            };
        });
        
        return services;
    }
}