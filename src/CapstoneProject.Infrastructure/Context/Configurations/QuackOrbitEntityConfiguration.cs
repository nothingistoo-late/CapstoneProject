using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Infrastructure.Context.Configurations;

/// <summary>
/// Cấu hình Fluent API cho các entity QuackOrbit (Game, Challenge, Package, ...).
/// </summary>
public static class QuackOrbitEntityConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<Game>(ConfigureMap);
        builder.Entity<GameMedia>(ConfigureGameMedia);
        builder.Entity<GameDetail>(ConfigureGameDetail);
        builder.Entity<Hint>(ConfigureHint);
        builder.Entity<Tag>(ConfigureTag);
        builder.Entity<GameTag>(ConfigureGameTag);
        builder.Entity<Achievement>(ConfigureAchievement);
        builder.Entity<UserAchievement>(ConfigureUserAchievement);
        builder.Entity<Submission>(ConfigureSubmission);
        builder.Entity<ExecutionsResult>(ConfigureExecutionsResult);
        builder.Entity<UserGameResult>(ConfigureUserGameResult);
        builder.Entity<XpTransaction>(ConfigureXpTransaction);
        builder.Entity<LevelThreshold>(ConfigureLevelThreshold);
        builder.Entity<XpPolicyConfig>(ConfigureXpPolicyConfig);
        builder.Entity<XpSourceConfig>(ConfigureXpSourceConfig);
        builder.Entity<GameSolveScoreConfig>(ConfigureGameSolveScoreConfig);
        builder.Entity<Package>(ConfigurePackage);
        builder.Entity<UserPackage>(ConfigureUserPackage);
        builder.Entity<Payment>(ConfigurePayment);
        builder.Entity<PaymentRecord>(ConfigurePaymentRecord);
        builder.Entity<GameRating>(ConfigureGameRating);
        builder.Entity<GameReport>(ConfigureGameReport);
        builder.Entity<Complaint>(ConfigureComplaint);
        builder.Entity<ComplaintCategoryCatalog>(ConfigureComplaintCategoryCatalog);
        builder.Entity<ComplaintPolicyRuleConfig>(ConfigureComplaintPolicyRuleConfig);
        builder.Entity<ComplaintMessage>(ConfigureComplaintMessage);
        builder.Entity<ComplaintMessageAttachment>(ConfigureComplaintMessageAttachment);
        builder.Entity<ComplaintStatusHistory>(ConfigureComplaintStatusHistory);
        builder.Entity<Notification>(ConfigureNotification);
        builder.Entity<UserNotification>(ConfigureUserNotification);
        builder.Entity<MyGame>(ConfigureMyGame);
        builder.Entity<LearningGoal>(ConfigureLearningGoal);
        builder.Entity<Concept>(ConfigureConcept);
        builder.Entity<LearningPathItem>(ConfigureLearningPathItem);
        builder.Entity<UserLearningGoal>(ConfigureUserLearningGoal);
        builder.Entity<UserConceptProgress>(ConfigureUserConceptProgress);
        builder.Entity<UserGamePlayHistory>(ConfigureUserGamePlayHistory);
        builder.Entity<UserMonthlyHintUsage>(ConfigureUserMonthlyHintUsage);
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
        e.Property(x => x.GameId);
        e.Property(x => x.ItemType).HasConversion<int>();
        e.HasIndex(x => new { x.LearningGoalId, x.SortOrder });
        e.HasOne(x => x.LearningGoal).WithMany(x => x.LearningPathItems).HasForeignKey(x => x.LearningGoalId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Concept).WithMany().HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.SetNull);
        e.HasOne(x => x.Game).WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.SetNull);
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

    static void ConfigureMyGame(EntityTypeBuilder<MyGame> e)
    {
        e.ToTable("MyGames");
        e.Property(x => x.GameId);
        e.HasIndex(x => new { x.UserId, x.GameId }).IsUnique();
        e.HasIndex(x => x.GameId);
        e.HasOne(x => x.Game).WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureMap(EntityTypeBuilder<Game> e)
    {
        e.ToTable("Games");
        e.Property(x => x.GameStatus).HasConversion<int>();
        e.Property(x => x.RootGameId).HasColumnName("RootGameId");
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
        e.HasIndex(x => x.GameStatus);
        e.HasIndex(x => x.IsPublished);
        e.Property(x => x.ContentVersion).HasDefaultValue(1);
        e.Property(x => x.IsActiveVersion).HasDefaultValue(true);
        e.HasIndex(x => x.RootGameId);
        e.HasIndex(x => new { x.RootGameId, x.IsActiveVersion });
        e.HasMany(x => x.GameTags).WithOne(x => x.Game).HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.GameDetails).WithOne(x => x.Game).HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.GameMedias).WithOne(x => x.Game).HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureGameMedia(EntityTypeBuilder<GameMedia> e)
    {
        e.ToTable("GameMedias");
        e.Property(x => x.GameId);
        e.Property(x => x.Kind).HasConversion<int>();
        e.HasIndex(x => new { x.GameId, x.SortOrder });
        e.HasOne(x => x.Game)
            .WithMany(x => x.GameMedias)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureGameDetail(EntityTypeBuilder<GameDetail> e)
    {
        e.ToTable("GameDetails");
        e.Property(x => x.GameId);
        e.Property(x => x.Type).HasConversion<int>();
        e.HasIndex(x => new { x.GameId, x.LevelOrder }).IsUnique();
        e.HasOne(x => x.Game)
            .WithMany(x => x.GameDetails)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        e.HasMany(x => x.Hints).WithOne(x => x.GameDetail).HasForeignKey(x => x.GameDetailId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureHint(EntityTypeBuilder<Hint> e)
    {
        e.Property(x => x.GameDetailId);
        e.HasIndex(x => new { x.GameDetailId, x.OrderNo });
    }

    static void ConfigureTag(EntityTypeBuilder<Tag> e)
    {
        e.HasIndex(x => x.Name).IsUnique();
    }

    static void ConfigureGameTag(EntityTypeBuilder<GameTag> e)
    {
        e.ToTable("GameTags");
        e.Property(x => x.GameId);
        e.HasIndex(x => new { x.GameId, x.TagId }).IsUnique();
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
        e.Property(x => x.GameId);
        e.Property(x => x.GameDetailId);
        e.Property(x => x.ResultStatus).HasConversion<int>();
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.GameId);
        e.HasIndex(x => x.GameDetailId);
        e.HasIndex(x => x.MatchId);
        e.HasOne(x => x.GameDetail).WithMany().HasForeignKey(x => x.GameDetailId).OnDelete(DeleteBehavior.Restrict);
        e.HasMany(x => x.ExecutionsResults).WithOne(x => x.Submission).HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureExecutionsResult(EntityTypeBuilder<ExecutionsResult> e)
    {
        e.HasIndex(x => x.SubmissionId);
    }

    static void ConfigureUserGameResult(EntityTypeBuilder<UserGameResult> e)
    {
        e.ToTable("UserGameResults");
        e.Property(x => x.GameId);
        e.Property(x => x.GameDetailId);
        e.HasIndex(x => x.GameId);
        e.HasIndex(x => new { x.UserId, x.GameDetailId });
        e.HasOne(x => x.GameDetail).WithMany().HasForeignKey(x => x.GameDetailId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureUserGamePlayHistory(EntityTypeBuilder<UserGamePlayHistory> e)
    {
        e.ToTable("UserGamePlayHistories");
        e.Property(x => x.GameId);
        e.Property(x => x.GameDetailId);
        e.Property(x => x.PlayMode).HasConversion<int>();
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.GameId);
        e.HasIndex(x => x.GameDetailId);
        e.HasIndex(x => new { x.UserId, x.GameId, x.StartTime });
        e.HasIndex(x => x.SubmissionId);
        e.HasIndex(x => x.ExecutionsResultId);
    }

    static void ConfigureUserMonthlyHintUsage(EntityTypeBuilder<UserMonthlyHintUsage> e)
    {
        e.ToTable("UserMonthlyHintUsages");
        e.HasIndex(x => new { x.UserId, x.MonthKey }).IsUnique();
        e.HasIndex(x => x.MonthKey);
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureXpTransaction(EntityTypeBuilder<XpTransaction> e)
    {
        e.Property(x => x.GameId);
        e.Property(x => x.SourceType).HasConversion<int>();
        e.Property(x => x.IdempotencyKey).HasMaxLength(200);
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.GameId);
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

    static void ConfigureGameSolveScoreConfig(EntityTypeBuilder<GameSolveScoreConfig> e)
    {
        e.ToTable("GameSolveScoreConfigs");
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

    static void ConfigureGameRating(EntityTypeBuilder<GameRating> e)
    {
        e.ToTable("GameRatings");
        e.Property(x => x.GameId);
        e.HasIndex(x => new { x.UserId, x.GameId }).IsUnique();
    }

    static void ConfigureGameReport(EntityTypeBuilder<GameReport> e)
    {
        e.ToTable("GameReports");
        e.Property(x => x.GameId);
        e.Property(x => x.ReportStatus).HasConversion<int>();
        e.HasIndex(x => x.GameId);
        e.HasIndex(x => x.ReportStatus);
        e.HasOne(r => r.Game).WithMany().HasForeignKey(r => r.GameId).OnDelete(DeleteBehavior.Restrict);
    }

    static void ConfigureComplaintCategoryCatalog(EntityTypeBuilder<ComplaintCategoryCatalog> e)
    {
        e.Property(x => x.CategoryKey).HasMaxLength(100);
        e.Property(x => x.DisplayName).HasMaxLength(150);
        e.HasIndex(x => x.CategoryKey).IsUnique();
        e.HasIndex(x => new { x.IsEnabled, x.SortOrder });
    }

    static void ConfigureComplaintPolicyRuleConfig(EntityTypeBuilder<ComplaintPolicyRuleConfig> e)
    {
        e.Property(x => x.CategoryKey).HasMaxLength(100);
        e.Property(x => x.RuleKey).HasMaxLength(100);
        e.HasIndex(x => new { x.CategoryKey, x.RuleKey }).IsUnique();
        e.HasIndex(x => new { x.IsEnabled, x.Priority });
    }

    static void ConfigureComplaint(EntityTypeBuilder<Complaint> e)
    {
        e.Property(x => x.ComplaintStatus).HasConversion<int>();
        e.Property(x => x.CategoryKey).HasMaxLength(100);
        e.Property(x => x.ContextType).HasMaxLength(100);
        e.Property(x => x.ContextKey).HasMaxLength(250);

        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.ComplaintStatus);
        e.HasIndex(x => x.CreatedAt);
        e.HasIndex(x => x.CategoryKey);
        e.HasIndex(x => x.ContextKey);
        e.HasIndex(x => new { x.ComplaintStatus, x.CreatedAt });
        e.HasIndex(x => new { x.UserId, x.CategoryKey, x.ContextKey, x.ComplaintStatus });

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

        e.HasMany(x => x.Attachments)
            .WithOne(x => x.ComplaintMessage)
            .HasForeignKey(x => x.ComplaintMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureComplaintMessageAttachment(EntityTypeBuilder<ComplaintMessageAttachment> e)
    {
        e.Property(x => x.FileName).HasMaxLength(260);
        e.Property(x => x.Url).HasMaxLength(2000);
        e.Property(x => x.MimeType).HasMaxLength(120);

        e.HasIndex(x => x.ComplaintMessageId);
        e.HasIndex(x => new { x.ComplaintMessageId, x.SortOrder });
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

    static void ConfigureNotification(EntityTypeBuilder<Notification> e)
    {
        e.Property(x => x.NotificationType).HasConversion<int>();
        e.HasIndex(x => x.CreatedAt).IsUnique(false);
        e.HasIndex(x => x.ActorUserId);
        e.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.SetNull);
        e.HasMany(x => x.UserNotifications).WithOne(x => x.Notification).HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
    }

    static void ConfigureUserNotification(EntityTypeBuilder<UserNotification> e)
    {
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => new { x.UserId, x.IsRead });
        e.HasIndex(x => new { x.UserId, x.CreatedAt }).IsUnique(false);
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Notification).WithMany(x => x.UserNotifications).HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
    }
}

