using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Extensions;

public static class SeedingExtension
{
    public static async Task SeedInitialDataAsync(this IApplicationBuilder app, ILogger logger)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var enableSeeding = configuration.GetSection("DataSeeding").GetValue<bool>("EnableSeeding");
        if (!enableSeeding)
        {
            logger.LogInformation("Data seeding is disabled.");
            return;
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // Seed roles from RoleEnum
        foreach (var roleName in Enum.GetNames(typeof(RoleEnum)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Status = EntityStatusEnum.Active,
                    CreatedAt = DateTime.UtcNow
                };
                var roleResult = await roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create role {Role}: {Errors}", roleName, errors);
                }
                else
                {
                    logger.LogInformation("Created role: {Role}", roleName);
                }
            }
        }

        // Seed admin user
        var adminEmail = configuration.GetSection("AdminUser").GetValue<string>("Email")?.Trim();
        var adminPassword = configuration.GetSection("AdminUser").GetValue<string>("DefaultPassword");
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("AdminUser configuration is missing. Skipping admin seeding.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                JoiningAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatusEnum.Active
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to create admin user: {Errors}", errors);
                return;
            }
            existingAdmin = admin;
            logger.LogInformation("Created admin user {Email}", adminEmail);
        }

        // Ensure admin is in Admin role
        var adminRoleName = RoleEnum.Admin.ToString();

        if (!await userManager.IsInRoleAsync(existingAdmin, adminRoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(existingAdmin, adminRoleName);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to add admin user to role {Role}: {Errors}", adminRoleName, errors);
            }
            else
            {
                logger.LogInformation("Added admin user to role {Role}", adminRoleName);
            }
        }

        // Final verification
        var roles = await userManager.GetRolesAsync(existingAdmin);
        logger.LogInformation("Admin user {Email} roles: {Roles}", existingAdmin.Email, string.Join(", ", roles));

        // Seed demo user for quick login
        var demoUserEmail = configuration.GetSection("QuickLogin").GetValue<string>("DemoUserEmail");
        var demoUserPassword = configuration.GetSection("QuickLogin").GetValue<string>("DemoUserPassword") ?? "Demo@123";
        
        if (!string.IsNullOrWhiteSpace(demoUserEmail))
        {
            var existingDemoUser = await userManager.FindByEmailAsync(demoUserEmail);
            if (existingDemoUser == null)
            {
                var demoUser = new AppUser
                {
                    UserName = demoUserEmail,
                    Email = demoUserEmail,
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "User",
                    JoiningAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };

                var createDemoResult = await userManager.CreateAsync(demoUser, demoUserPassword);
                if (!createDemoResult.Succeeded)
                {
                    var errors = string.Join(", ", createDemoResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create demo user: {Errors}", errors);
                }
                else
                {
                    existingDemoUser = demoUser;
                    logger.LogInformation("Created demo user {Email}", demoUserEmail);
                }
            }

            // Ensure demo user is in Learner role
            if (existingDemoUser != null)
            {
                var learnerRoleName = RoleEnum.Learner.ToString();
                if (!await userManager.IsInRoleAsync(existingDemoUser, learnerRoleName))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(existingDemoUser, learnerRoleName);
                    if (!addRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                        logger.LogWarning("Failed to add demo user to role {Role}: {Errors}", learnerRoleName, errors);
                    }
                    else
                    {
                        logger.LogInformation("Added demo user to role {Role}", learnerRoleName);
                    }
                }
            }
        }

        logger.LogInformation("Data seeding completed.");
    }
}


