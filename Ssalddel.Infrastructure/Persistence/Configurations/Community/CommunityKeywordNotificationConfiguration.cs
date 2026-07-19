using Ssalddel.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Community;

public sealed class CommunityKeywordSubscriptionConfiguration
    : IEntityTypeConfiguration<CommunityKeywordSubscription>
{
    public void Configure(EntityTypeBuilder<CommunityKeywordSubscription> builder)
    {
        builder.ToTable("community_keyword_subscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.AppKey).HasColumnName("app_key").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Keyword).HasColumnName("keyword").HasMaxLength(40).IsRequired();
        builder.Property(x => x.NormalizedKeyword).HasColumnName("normalized_keyword").HasMaxLength(40).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => new { x.UserId, x.AppKey, x.NormalizedKeyword })
            .HasDatabaseName("UX_community_keyword_subscription")
            .IsUnique();
        builder.HasIndex(x => new { x.AppKey, x.IsActive })
            .HasDatabaseName("IX_community_keyword_subscription_match");
    }
}

public sealed class PlatformCommunityPostKeywordScanConfiguration
    : IEntityTypeConfiguration<PlatformCommunityPostKeywordScan>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostKeywordScan> builder)
    {
        builder.ToTable("platform_community_post_keyword_scans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PostId).HasColumnName("post_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count");
        builder.Property(x => x.ProcessingToken).HasColumnName("processing_token").HasMaxLength(32);
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
        builder.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");

        builder.HasOne(x => x.Post)
            .WithOne(x => x.KeywordNotificationScan)
            .HasForeignKey<PlatformCommunityPostKeywordScan>(x => x.PostId)
            .HasConstraintName("FK_community_keyword_scan_post")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PostId)
            .HasDatabaseName("UX_community_keyword_scan_post")
            .IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc })
            .HasDatabaseName("IX_community_keyword_scan_due");
    }
}

public sealed class CommunityKeywordNotificationConfiguration
    : IEntityTypeConfiguration<CommunityKeywordNotification>
{
    public void Configure(EntityTypeBuilder<CommunityKeywordNotification> builder)
    {
        builder.ToTable("community_keyword_notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.PostId).HasColumnName("post_id");
        builder.Property(x => x.PostAppKey).HasColumnName("post_app_key").HasMaxLength(80).IsRequired();
        builder.Property(x => x.PostCategory).HasColumnName("post_category").HasMaxLength(60).IsRequired();
        builder.Property(x => x.PostTitle).HasColumnName("post_title").HasMaxLength(160).IsRequired();
        builder.Property(x => x.PostExcerpt).HasColumnName("post_excerpt").HasMaxLength(300).IsRequired();
        builder.Property(x => x.PostAuthorNickname).HasColumnName("post_author_nickname").HasMaxLength(40).IsRequired();
        builder.Property(x => x.MatchedKeywordsJson).HasColumnName("matched_keywords_json").HasMaxLength(4096).IsRequired();
        builder.Property(x => x.IsRead).HasColumnName("is_read");
        builder.Property(x => x.ReadAtUtc).HasColumnName("read_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(x => x.Post)
            .WithMany()
            .HasForeignKey(x => x.PostId)
            .HasConstraintName("FK_community_keyword_notification_post")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Deliveries)
            .WithOne(x => x.Notification)
            .HasForeignKey(x => x.NotificationId)
            .HasConstraintName("FK_community_keyword_delivery_notification")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.PostId })
            .HasDatabaseName("UX_community_keyword_notification_user_post")
            .IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAtUtc })
            .HasDatabaseName("IX_community_keyword_notification_inbox");
    }
}

public sealed class CommunityKeywordNotificationDeliveryConfiguration
    : IEntityTypeConfiguration<CommunityKeywordNotificationDelivery>
{
    public void Configure(EntityTypeBuilder<CommunityKeywordNotificationDelivery> builder)
    {
        builder.ToTable("community_keyword_notification_deliveries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NotificationId).HasColumnName("notification_id");
        builder.Property(x => x.InstallationId).HasColumnName("installation_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count");
        builder.Property(x => x.ProcessingToken).HasColumnName("processing_token").HasMaxLength(32);
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(1000);
        builder.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");
        builder.Property(x => x.SentAtUtc).HasColumnName("sent_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(x => x.Installation)
            .WithMany()
            .HasForeignKey(x => x.InstallationId)
            .HasConstraintName("FK_community_keyword_delivery_installation")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.NotificationId, x.InstallationId })
            .HasDatabaseName("UX_community_keyword_delivery_target")
            .IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc })
            .HasDatabaseName("IX_community_keyword_delivery_due");
        builder.HasIndex(x => x.InstallationId)
            .HasDatabaseName("IX_community_keyword_delivery_installation");
    }
}
