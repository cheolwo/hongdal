using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Community;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Community;

public sealed class 커뮤니티활동공개ProjectionConfiguration
    : IEntityTypeConfiguration<커뮤니티활동공개Projection>
{
    public void Configure(EntityTypeBuilder<커뮤니티활동공개Projection> builder)
    {
        builder.ToTable("community_activity_public_projections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AggregateKey).HasColumnName("aggregate_key").HasMaxLength(64).IsRequired();
        builder.Property(x => x.AppKey).HasColumnName("app_key").HasMaxLength(80).IsRequired();
        builder.Property(x => x.CommunityScope).HasColumnName("community_scope").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ActivityKind).HasColumnName("activity_kind").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(160).IsRequired();
        builder.Property(x => x.PublicSummary).HasColumnName("public_summary").HasMaxLength(500).IsRequired();
        builder.Property(x => x.TopicTagsJson).HasColumnName("topic_tags_json").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.TimeBucketStartUtc).HasColumnName("time_bucket_start_utc");
        builder.Property(x => x.TimeBucketEndUtc).HasColumnName("time_bucket_end_utc");
        builder.Property(x => x.ActivityCount).HasColumnName("activity_count");
        builder.Property(x => x.VisibilityScope).HasColumnName("visibility_scope").HasMaxLength(40).IsRequired();
        builder.Property(x => x.PrivacyPolicyVersion).HasColumnName("privacy_policy_version").HasMaxLength(40).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.AggregateKey)
            .HasDatabaseName("UX_community_activity_public_projection")
            .IsUnique();
        builder.HasIndex(x => new
            {
                x.VisibilityScope,
                x.TimeBucketStartUtc,
                x.ActivityCount
            })
            .HasDatabaseName("IX_community_activity_public_projection_browse");
        builder.HasIndex(x => new { x.AppKey, x.CommunityScope })
            .HasDatabaseName("IX_community_activity_public_projection_scope");
    }
}

public sealed class 커뮤니티활동처리기록Configuration
    : IEntityTypeConfiguration<커뮤니티활동처리기록>
{
    public void Configure(EntityTypeBuilder<커뮤니티활동처리기록> builder)
    {
        builder.ToTable("community_activity_processing_receipts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OccurrenceKey).HasColumnName("occurrence_key").HasMaxLength(64).IsRequired();
        builder.Property(x => x.AggregateKey).HasColumnName("aggregate_key").HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceKind).HasColumnName("source_kind").HasMaxLength(20).IsRequired();
        builder.Property(x => x.SourceName).HasColumnName("source_name").HasMaxLength(160).IsRequired();
        builder.Property(x => x.RecordedAtUtc).HasColumnName("recorded_at_utc");

        builder.HasIndex(x => x.OccurrenceKey)
            .HasDatabaseName("UX_community_activity_processing_receipt")
            .IsUnique();
        builder.HasIndex(x => x.AggregateKey)
            .HasDatabaseName("IX_community_activity_processing_receipt_aggregate");
    }
}
