using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context.Configurations;

namespace CapstoneProject.Infrastructure.Context;

public class CapstoneProjectDbContext : IdentityDbContext<AppUser, AppRole, Guid>, ICapstoneProjectDbContext
{
    public CapstoneProjectDbContext(DbContextOptions<CapstoneProjectDbContext> options) : base(options)
    {
    }

    // Chat entities
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRead> MessageReads { get; set; }

    // QuackOrbit: Challenge Management
    public DbSet<Map> Maps { get; set; }
    public DbSet<MapDetail> MapDetails { get; set; }
    public DbSet<Hint> Hints { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<MapTag> MapTags { get; set; }

    // QuackOrbit: Gameplay & Progress
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<ExecutionsResult> ExecutionsResults { get; set; }
    public DbSet<UserMapResult> UserMapResults { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }
    public DbSet<XpTransaction> XpTransactions { get; set; }

    // QuackOrbit: Competitive
    public DbSet<Match> Matches { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomParticipant> RoomParticipants { get; set; }
    public DbSet<UserMatchResult> UserMatchResults { get; set; }

    // QuackOrbit: Marketplace
    public DbSet<Package> Packages { get; set; }
    public DbSet<UserPackage> UserPackages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }

    // QuackOrbit: Community & Safety
    public DbSet<MapRating> MapRatings { get; set; }
    public DbSet<MapReport> MapReports { get; set; }

    // OrbitCoin: virtual currency
    public DbSet<UserWallet> UserWallets { get; set; }
    public DbSet<OrbitCoinTransaction> OrbitCoinTransactions { get; set; }

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

        // Configure ChatRoom
        builder.Entity<ChatRoom>(entity =>
        {
            entity.Property(x => x.RoomType).HasConversion<int>();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => x.RoomType);
            entity.HasIndex(x => x.LastMessageAt);
            
            entity.HasMany(x => x.Members)
                .WithOne(x => x.ChatRoom)
                .HasForeignKey(x => x.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(x => x.Messages)
                .WithOne(x => x.ChatRoom)
                .HasForeignKey(x => x.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ChatRoomMember
        builder.Entity<ChatRoomMember>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => new { x.ChatRoomId, x.UserId, x.LeftAt });
            entity.HasIndex(x => x.UserId);
            
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Message
        builder.Entity<Message>(entity =>
        {
            entity.Property(x => x.MessageType).HasConversion<int>();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => new { x.ChatRoomId, x.CreatedAt });
            entity.HasIndex(x => x.SenderId);
            entity.HasIndex(x => x.ReplyToMessageId);
            
            entity.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(x => x.ReplyToMessage)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(x => x.MessageReads)
                .WithOne(x => x.Message)
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MessageRead
        builder.Entity<MessageRead>(entity =>
        {
            entity.HasIndex(x => new { x.MessageId, x.UserId }).IsUnique();
            entity.HasIndex(x => x.UserId);
            
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QuackOrbit entities
        QuackOrbitEntityConfiguration.Configure(builder);

        // OrbitCoin virtual currency
        OrbitCoinEntityConfiguration.Configure(builder);

        // Gọi cấu hình chung cho BaseEntity (if any BaseEntity-derived entities are added later)
        BaseEntityConfigurationHelper.ConfigureBaseEntities(builder);
    }
}