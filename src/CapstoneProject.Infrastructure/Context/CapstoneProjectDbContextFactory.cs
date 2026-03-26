using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CapstoneProject.Infrastructure.Context;

public class CapstoneProjectDbContextFactory : IDesignTimeDbContextFactory<CapstoneProjectDbContext>
{
    public CapstoneProjectDbContext CreateDbContext(string[] args)
    {
       var optionsBuilder = new DbContextOptionsBuilder<CapstoneProjectDbContext>();
        
        // Design-time : lire la connection string depuis le projet API (chemin relatif au répertoire courant du tool EF).
        var webAppPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CapstoneProject.API");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(webAppPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Set ConnectionStrings__DefaultConnection or add appsettings.json in CapstoneProject.API.");
        }

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.MigrationsAssembly(typeof(CapstoneProjectDbContext).Assembly.FullName);
            });

        return new CapstoneProjectDbContext(optionsBuilder.Options);
    }
}
