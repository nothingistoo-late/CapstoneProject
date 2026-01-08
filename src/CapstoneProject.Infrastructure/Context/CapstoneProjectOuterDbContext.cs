using Microsoft.EntityFrameworkCore;


namespace CapstoneProject.Infrastructure.Context;

/// <summary>
/// Database context for external services, token management and Hangfire
/// This database contains:
/// - Token management (RefreshTokens, AccessTokens)
/// - Hangfire job storage (Hangfire will auto-create its tables)
/// Separated from main context to isolate external dependencies
/// </summary>
public class CapstoneProjectOuterDbContext : DbContext
{
    //     public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    //     public DbSet<AccessToken> AccessTokens { get; set; } = null!;

    public CapstoneProjectOuterDbContext(DbContextOptions<CapstoneProjectOuterDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ExternalToken entity (legacy - keep for backward compatibility)


        // Configure RefreshToken entity
        // modelBuilder.Entity<RefreshToken>(entity =>
        // {
        //     entity.HasKey(e => e.Id);

        //     entity.Property(e => e.Provider)
        //         .IsRequired()
        //         .HasMaxLength(50);

        //     entity.Property(e => e.TokenValue)
        //         .IsRequired()
        //         .HasColumnType("nvarchar(max)");

        //     entity.Property(e => e.Scope)
        //         .HasMaxLength(500);

        //     entity.Property(e => e.ExpiresAt)
        //         .HasColumnType("datetime2");

        //     entity.Property(e => e.LastUsedAt)
        //         .HasColumnType("datetime2");

        //     // Indexes for performance
        //     entity.HasIndex(e => e.Provider)
        //         .HasDatabaseName("IX_RefreshTokens_Provider");

        //     entity.HasIndex(e => e.ExpiresAt)
        //         .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
        // });

        // Configure AccessToken entity
        // modelBuilder.Entity<AccessToken>(entity =>
        // {
        //     entity.HasKey(e => e.Id);

        //     entity.Property(e => e.TokenValue)
        //         .IsRequired()
        //         .HasColumnType("nvarchar(max)");

        //     entity.Property(e => e.TokenType)
        //         .IsRequired()
        //         .HasMaxLength(20)
        //         .HasDefaultValue("Bearer");

        //     entity.Property(e => e.Scope)
        //         .HasMaxLength(500);

        //     entity.Property(e => e.ExpiresAt)
        //         .HasColumnType("datetime2");

        //     entity.Property(e => e.LastUsedAt)
        //         .HasColumnType("datetime2");

        //     // Foreign key relationship
        //     entity.HasOne(e => e.RefreshToken)
        //         .WithMany(r => r.AccessTokens)
        //         .HasForeignKey(e => e.RefreshTokenId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     // Indexes for performance
        //     entity.HasIndex(e => e.RefreshTokenId)
        //         .HasDatabaseName("IX_AccessTokens_RefreshTokenId");

        //     entity.HasIndex(e => e.ExpiresAt)
        //         .HasDatabaseName("IX_AccessTokens_ExpiresAt");

        // });

        // Apply shared BaseEntity configuration to all entities
        BaseEntityConfigurationHelper.ConfigureBaseEntities(modelBuilder);
    }
}
