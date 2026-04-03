using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
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
        builder.Entity<MapMedia>(ConfigureMapMedia);
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
        builder.Entity<LevelThreshold>(ConfigureLevelThreshold);
        builder.Entity<XpPolicyConfig>(ConfigureXpPolicyConfig);
        builder.Entity<XpSourceConfig>(ConfigureXpSourceConfig);
        builder.Entity<MapSolveScoreConfig>(ConfigureMapSolveScoreConfig);
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
        builder.Entity<Complaint>(ConfigureComplaint);
        builder.Entity<ComplaintMessage>(ConfigureComplaintMessage);
        builder.Entity<ComplaintStatusHistory>(ConfigureComplaintStatusHistory);
        builder.Entity<MyMap>(ConfigureMyMap);
        builder.Entity<LearningGoal>(ConfigureLearningGoal);
        builder.Entity<Concept>(ConfigureConcept);
        builder.Entity<LearningPathItem>(ConfigureLearningPathItem);
        builder.Entity<UserLearningGoal>(ConfigureUserLearningGoal);
        builder.Entity<UserConceptProgress>(ConfigureUserConceptProgress);
        builder.Entity<UserMapPlayHistory>(ConfigureUserMapPlayHistory);
    }

    static void ConfigureLearningGoal(EntityTypeBuilder<LearningGoal> e)
    {
        e.HasIndex(x => x.SortOrder);
        e.HasMany(x => x.Concepts).WithOne(x => x.LearningGoal).HasForeignKey(x => x.LearningGoalId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.LearningPathItems).WithOne(x => x.LearningGoal).HasForeignKey(x => x.LearningGoalId).OnDelete(DeleteBehavior.Restrict);
    }

    static void ConfigureConcept(EntityTypeBuilder<Concept> e)
    {
        e.HasIndex(x => new { x.LearningGoalId, x.SortOrder });
        e.HasOne(x => x.LearningGoal).WithMany(x => x.Concepts).HasForeignKey(x => x.LearningGoalId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureLearningPathItem(EntityTypeBuilder<LearningPathItem> e)
    {
        e.Property(x => x.ItemType).HasConversion<int>();
        e.HasIndex(x => new { x.LearningGoalId, x.SortOrder });
        e.HasOne(x => x.LearningGoal).WithMany(x => x.LearningPathItems).HasForeignKey(x => x.LearningGoalId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Concept).WithMany().HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.SetNull);
        e.HasOne(x => x.Map).WithMany().HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.SetNull);
    }

    static void ConfigureUserLearningGoal(EntityTypeBuilder<UserLearningGoal> e)
    {
        e.HasIndex(x => x.UserId).IsUnique();
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.LearningGoal).WithMany(x => x.UserLearningGoals).HasForeignKey(x => x.LearningGoalId).OnDelete(DeleteBehavior.Restrict);
    }

    static void ConfigureUserConceptProgress(EntityTypeBuilder<UserConceptProgress> e)
    {
        e.HasIndex(x => new { x.UserId, x.ConceptId }).IsUnique();
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Concept).WithMany(x => x.UserConceptProgresses).HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.Cascade);
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
        e.Property(x => x.LearnedTags)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<Guid>()
                    : (JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>())
            );
        e.HasIndex(x => x.CreatedBy);
        e.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatedBy).IsRequired(false);
        e.HasIndex(x => x.MapStatus);
        e.HasIndex(x => x.IsPublished);
        e.Property(x => x.ContentVersion).HasDefaultValue(1);
        e.HasMany(x => x.MapTags).WithOne(x => x.Map).HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.MapDetails).WithOne(x => x.Map).HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.MapMedias).WithOne(x => x.Map).HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureMapMedia(EntityTypeBuilder<MapMedia> e)
    {
        e.Property(x => x.Kind).HasConversion<int>();
        e.HasIndex(x => new { x.MapId, x.SortOrder });
        e.HasOne(x => x.Map)
            .WithMany(x => x.MapMedias)
            .HasForeignKey(x => x.MapId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureMapDetail(EntityTypeBuilder<MapDetail> e)
    {
        e.Property(x => x.Type).HasConversion<int>();
        e.HasIndex(x => new { x.MapId, x.LevelOrder }).IsUnique();
        e.HasOne(x => x.Map)
            .WithMany(x => x.MapDetails)
            .HasForeignKey(x => x.MapId)
            .OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.Hints).WithOne(x => x.MapDetail).HasForeignKey(x => x.MapDetailId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureHint(EntityTypeBuilder<Hint> e)
    {
        e.HasIndex(x => new { x.MapDetailId, x.OrderNo });
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
        e.HasIndex(x => x.MapDetailId);
        e.HasIndex(x => x.MatchId);
        e.HasOne(x => x.MapDetail).WithMany().HasForeignKey(x => x.MapDetailId).OnDelete(DeleteBehavior.Restrict);
        e.HasMany(x => x.ExecutionsResults).WithOne(x => x.Submission).HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureExecutionsResult(EntityTypeBuilder<ExecutionsResult> e)
    {
        e.HasIndex(x => x.SubmissionId);
    }

    static void ConfigureUserMapResult(EntityTypeBuilder<UserMapResult> e)
    {
        e.HasIndex(x => x.MapId);
        e.HasIndex(x => new { x.UserId, x.MapDetailId });
        e.HasOne(x => x.MapDetail).WithMany().HasForeignKey(x => x.MapDetailId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureUserMapPlayHistory(EntityTypeBuilder<UserMapPlayHistory> e)
    {
        e.Property(x => x.PlayMode).HasConversion<int>();
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.MapId);
        e.HasIndex(x => x.MapDetailId);
        e.HasIndex(x => new { x.UserId, x.MapId, x.StartTime });
        e.HasIndex(x => x.SubmissionId);
        e.HasIndex(x => x.ExecutionsResultId);
    }

    static void ConfigureXpTransaction(EntityTypeBuilder<XpTransaction> e)
    {
        e.Property(x => x.SourceType).HasConversion<int>();
        e.Property(x => x.IdempotencyKey).HasMaxLength(200);
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.MapId);
        e.HasIndex(x => x.SourceType);
        e.HasIndex(x => x.IdempotencyKey).IsUnique();
    }

    static void ConfigureLevelThreshold(EntityTypeBuilder<LevelThreshold> e)
    {
        e.HasIndex(x => x.Level).IsUnique();
        e.HasIndex(x => x.RequiredTotalXp);
    }

    static void ConfigureXpPolicyConfig(EntityTypeBuilder<XpPolicyConfig> e)
    {
        e.Property(x => x.PolicyKey).HasMaxLength(100);
        e.HasIndex(x => x.PolicyKey).IsUnique();
        e.HasIndex(x => new { x.IsEnabled, x.Priority });
    }

    static void ConfigureXpSourceConfig(EntityTypeBuilder<XpSourceConfig> e)
    {
        e.Property(x => x.SourceType).HasConversion<int>();
        e.HasIndex(x => x.SourceType).IsUnique();
        e.HasIndex(x => x.IsEnabled);
    }

    static void ConfigureMapSolveScoreConfig(EntityTypeBuilder<MapSolveScoreConfig> e)
    {
        e.Property(x => x.ConfigKey).HasMaxLength(64);
        e.HasIndex(x => x.ConfigKey).IsUnique();
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
        e.HasIndex(x => x.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
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

    static void ConfigureComplaint(EntityTypeBuilder<Complaint> e)
    {
        e.Property(x => x.ComplaintStatus).HasConversion<int>();

        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.ComplaintStatus);
        e.HasIndex(x => x.CreatedAt);
        e.HasIndex(x => new { x.ComplaintStatus, x.CreatedAt });

        e.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasMany(x => x.Messages)
            .WithOne(x => x.Complaint)
            .HasForeignKey(x => x.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasMany(x => x.StatusHistories)
            .WithOne(x => x.Complaint)
            .HasForeignKey(x => x.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureComplaintMessage(EntityTypeBuilder<ComplaintMessage> e)
    {
        e.HasIndex(x => x.ComplaintId);
        e.HasIndex(x => x.SenderId);
        e.HasIndex(x => x.CreatedAt);
        e.HasIndex(x => new { x.ComplaintId, x.CreatedAt });

        e.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    static void ConfigureComplaintStatusHistory(EntityTypeBuilder<ComplaintStatusHistory> e)
    {
        e.Property(x => x.FromStatus).HasConversion<int>();
        e.Property(x => x.ToStatus).HasConversion<int>();

        e.HasIndex(x => x.ComplaintId);
        e.HasIndex(x => x.ChangedBy);
        e.HasIndex(x => x.ChangedAt);
        e.HasIndex(x => new { x.ComplaintId, x.ChangedAt });

        e.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
