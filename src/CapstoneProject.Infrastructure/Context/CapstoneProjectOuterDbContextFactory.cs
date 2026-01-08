using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CapstoneProject.Infrastructure.Context;

public class CapstoneProjectOuterDbContextFactory : IDesignTimeDbContextFactory<CapstoneProjectOuterDbContext>
{
    public CapstoneProjectOuterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CapstoneProjectOuterDbContext>();
        
        // Đọc connection string từ appsettings.json của WebApp
        var webAppPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CapstoneProject.API");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(webAppPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("OuterDbConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'OuterDbConnection' not found. Make sure appsettings.json exists in CapstoneProject.API project.");
        }

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.MigrationsAssembly(typeof(CapstoneProjectOuterDbContext).Assembly.FullName);
            });

        return new CapstoneProjectOuterDbContext(optionsBuilder.Options);
    }
}
