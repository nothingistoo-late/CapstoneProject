using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Context;
using CapstoneProject.Infrastructure.Repositories;
using CapstoneProject.Infrastructure.Repositories.Outer;
using CapstoneProject.Infrastructure.Services;
using CapstoneProject.Infrastructure.Configurations;
using CapstoneProject.Infrastructure.Factories;

namespace CapstoneProject.Infrastructure;

public static class InfrastructureDependencyInjection
{

    /// <summary>
    /// Add infrastructure services to the dependency container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please se configure it in appsettings.json");
        }

        // Configure database contexts
        services.AddDbContextPool<CapstoneProjectDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(CapstoneProjectDbContext).Assembly.FullName);
                sql.CommandTimeout(30);
            });
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning));
        });

        // Configure Outer Database Context for external services and Hangfire
        var outerConnectionString = configuration.GetConnectionString("OuterDbConnection");
        if (!string.IsNullOrEmpty(outerConnectionString))
        {
            services.AddDbContextPool<CapstoneProjectOuterDbContext>(options =>
            {
                options.UseSqlServer(outerConnectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(CapstoneProjectOuterDbContext).Assembly.FullName);
                    sql.CommandTimeout(30);
                });
            });
        }
        // Register contexts as interfaces
        services.AddScoped<ICapstoneProjectDbContext>(provider => provider.GetRequiredService<CapstoneProjectDbContext>());
        // Map DbContext for services that depend on base DbContext (e.g., UnitOfWork)
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<CapstoneProjectDbContext>());

        // Configure Identity
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;

            // User settings
            options.User.RequireUniqueEmail = true;  // Changed to true for email verification

            // SignIn settings  
            options.SignIn.RequireConfirmedEmail = true;   // Changed to true for email verification
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<CapstoneProjectDbContext>()
        .AddDefaultTokenProviders();

        // Configure email token lifespan (24 hours)
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        //config app settings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Configure OTP settings
        services.Configure<OtpSettings>(configuration.GetSection("OTPSettings"));

        // Configure Google settings
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));

        // Configure Hangfire - DISABLED vì không cần token management nữa
        // Service Account hoàn toàn tự động
        var useHangfire = false; // configuration.GetValue("Hangfire:UseOuterDatabase", true);
        if (useHangfire && !string.IsNullOrEmpty(outerConnectionString))
        {
            services.AddHangfireServices(configuration);
        }

        // Configure Storage settings
        //services.Configure<StorageSettings>(configuration.GetSection("Storage"));

        // Configure VNPay settings
        //services.Configure<VNPaySettings>(configuration.GetSection("VNPay"));

        // Configure Email settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Configure File Storage settings
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

        // Register repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Outer Unit of Work for external services
        services.AddScoped<IOuterUnitOfWork, OuterUnitOfWork>();
        services.AddScoped<OuterUnitOfWork>(); // For direct injection in handlers

        // Register services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IOtpCacheService, OtpCacheService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationFactory, NotificationFactory>();
        
        // File storage services
        services.AddScoped<LocalFileService>(); // Local storage implementation
        // services.AddScoped<S3FileService>(); // Uncomment when S3 is implemented
        // services.AddScoped<AzureBlobFileService>(); // Uncomment when Azure Blob is implemented
        // services.AddScoped<GoogleCloudFileService>(); // Uncomment when Google Cloud is implemented
        services.AddScoped<IFileServiceFactory, FileServiceFactory>(); // Register factory as scoped to resolve scoped services
        
        return services;
    }
}