using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Infrastructure.Context.Configurations;

/// <summary>
/// Cấu hình Fluent API cho các entity QuackOrbit (Map, Challenge, Match, Package, ...).
/// </summary>
public static class QuackOrbitEntityConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<Map>(ConfigureMap);
        builder.Entity<MapDetail>(ConfigureMapDetail);
        builder.Entity<Hint>(ConfigureHint);
        builder.Entity<Tag>(ConfigureTag);
        builder.Entity<MapTag>(ConfigureMapTag);
        builder.Entity<Achievement>(ConfigureAchievement);
        builder.Entity<UserAchievement>(ConfigureUserAchievement);
        builder.Entity<Submission>(ConfigureSubmission);
        builder.Entity<ExecutionsResult>(ConfigureExecutionsResult);
        builder.Entity<UserMapResult>(ConfigureUserMapResult);
        builder.Entity<XpTransaction>(ConfigureXpTransaction);
        builder.Entity<Package>(ConfigurePackage);
        builder.Entity<UserPackage>(ConfigureUserPackage);
        builder.Entity<Payment>(ConfigurePayment);
        builder.Entity<PaymentRecord>(ConfigurePaymentRecord);
        builder.Entity<Match>(ConfigureMatch);
        builder.Entity<Room>(ConfigureRoom);
        builder.Entity<RoomParticipant>(ConfigureRoomParticipant);
        builder.Entity<UserMatchResult>(ConfigureUserMatchResult);
        builder.Entity<MapRating>(ConfigureMapRating);
        builder.Entity<MapReport>(ConfigureMapReport);
        builder.Entity<MyMap>(ConfigureMyMap);
    }

    static void ConfigureMyMap(EntityTypeBuilder<MyMap> e)
    {
        e.HasIndex(x => new { x.UserId, x.MapId }).IsUnique();
        e.HasIndex(x => x.MapId);
        e.HasOne(x => x.Map).WithMany().HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureMap(EntityTypeBuilder<Map> e)
    {
        e.Property(x => x.MapStatus).HasConversion<int>();
        e.HasIndex(x => x.CreatedBy);
        e.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatedBy).IsRequired(false);
        e.HasIndex(x => x.MapStatus);
        e.HasIndex(x => x.IsPublished);
        e.HasMany(x => x.Hints).WithOne(x => x.Map).HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.MapTags).WithOne(x => x.Map).HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureMapDetail(EntityTypeBuilder<MapDetail> e)
    {
        e.HasIndex(x => x.MapId);
        e.HasOne(x => x.Map)
            .WithOne(x => x.MapDetail)
            .HasForeignKey<MapDetail>(x => x.MapId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureHint(EntityTypeBuilder<Hint> e)
    {
        e.HasIndex(x => new { x.MapId, x.OrderNo });
    }

    static void ConfigureTag(EntityTypeBuilder<Tag> e)
    {
        e.HasIndex(x => x.Name).IsUnique();
    }

    static void ConfigureMapTag(EntityTypeBuilder<MapTag> e)
    {
        e.HasIndex(x => new { x.MapId, x.TagId }).IsUnique();
    }


    static void ConfigureAchievement(EntityTypeBuilder<Achievement> e)
    {
        e.HasIndex(x => x.Code).IsUnique();
        e.HasMany(x => x.UserAchievements).WithOne(x => x.Achievement).HasForeignKey(x => x.AchievementId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureUserAchievement(EntityTypeBuilder<UserAchievement> e)
    {
        e.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();
    }

    static void ConfigureSubmission(EntityTypeBuilder<Submission> e)
    {
        e.Property(x => x.ResultStatus).HasConversion<int>();
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.MapId);
        e.HasIndex(x => x.MatchId);
        e.HasMany(x => x.ExecutionsResults).WithOne(x => x.Submission).HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureExecutionsResult(EntityTypeBuilder<ExecutionsResult> e)
    {
        e.HasIndex(x => x.SubmissionId);
    }

    static void ConfigureUserMapResult(EntityTypeBuilder<UserMapResult> e)
    {
        e.HasIndex(x => new { x.UserId, x.MapId }).IsUnique();
    }

    static void ConfigureXpTransaction(EntityTypeBuilder<XpTransaction> e)
    {
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.MapId);
    }

    static void ConfigurePackage(EntityTypeBuilder<Package> e)
    {
        e.HasMany(x => x.UserPackages).WithOne(x => x.Package).HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
        e.HasMany(x => x.PaymentRecords).WithOne(x => x.Package).HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
    }

    static void ConfigureUserPackage(EntityTypeBuilder<UserPackage> e)
    {
        e.HasIndex(x => new { x.UserId, x.PackageId });
    }

    static void ConfigurePayment(EntityTypeBuilder<Payment> e)
    {
        e.HasIndex(x => x.Code).IsUnique();
        e.HasMany(x => x.PaymentRecords).WithOne(x => x.Payment).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }

    static void ConfigurePaymentRecord(EntityTypeBuilder<PaymentRecord> e)
    {
        e.Property(x => x.PaymentStatus).HasConversion<int>();
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.PaidAt);
    }

    static void ConfigureMatch(EntityTypeBuilder<Match> e)
    {
        e.HasIndex(x => x.MapId);
        e.HasMany(x => x.Rooms).WithOne(x => x.Match).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.UserMatchResults).WithOne(x => x.Match).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureRoom(EntityTypeBuilder<Room> e)
    {
        e.Property(x => x.RoomStatus).HasConversion<int>();
        e.HasIndex(x => x.MatchId);
        e.HasIndex(x => x.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        e.HasMany(x => x.RoomParticipants).WithOne(x => x.Room).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureRoomParticipant(EntityTypeBuilder<RoomParticipant> e)
    {
        e.HasIndex(x => new { x.RoomId, x.UserId }).IsUnique();
    }

    static void ConfigureUserMatchResult(EntityTypeBuilder<UserMatchResult> e)
    {
        e.HasIndex(x => new { x.MatchId, x.UserId }).IsUnique();
    }

    static void ConfigureMapRating(EntityTypeBuilder<MapRating> e)
    {
        e.HasIndex(x => new { x.UserId, x.MapId }).IsUnique();
    }

    static void ConfigureMapReport(EntityTypeBuilder<MapReport> e)
    {
        e.Property(x => x.ReportStatus).HasConversion<int>();
        e.HasIndex(x => x.MapId);
        e.HasIndex(x => x.ReportStatus);
        e.HasOne(r => r.Map).WithMany().HasForeignKey(r => r.MapId).OnDelete(DeleteBehavior.Restrict);
    }
}
