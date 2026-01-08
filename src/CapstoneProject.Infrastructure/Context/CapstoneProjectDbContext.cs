using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Infrastructure.Context;

public class CapstoneProjectDbContext : IdentityDbContext<AppUser, AppRole, Guid>, ICapstoneProjectDbContext
{
    public CapstoneProjectDbContext(DbContextOptions<CapstoneProjectDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Đổi tên bảng Identity
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // AppUser
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => x.Id).IsUnique();

            // Performance indexes
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.JoiningAt);
            entity.HasIndex(x => x.LastLoginAt).HasFilter("LastLoginAt IS NOT NULL");
            entity.HasIndex(x => new { x.Status, x.JoiningAt });
        });

        // AppRole
        builder.Entity<AppRole>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<int>();
        });

        // Configure the base IdentityUserRole<Guid> key
        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        });

        // Gọi cấu hình chung cho BaseEntity (if any BaseEntity-derived entities are added later)
        BaseEntityConfigurationHelper.ConfigureBaseEntities(builder);
    }
}