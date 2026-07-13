using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Community;

public sealed class PlatformCommunityPostAudioConfiguration : IEntityTypeConfiguration<PlatformCommunityPostAudio>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostAudio> builder)
    {
        builder.ToTable("platform_community_post_audio");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PostId).HasColumnName("post_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(30).IsRequired();
        builder.Property(x => x.VoiceId).HasColumnName("voice_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModelVersion).HasColumnName("model_version").HasMaxLength(30).IsRequired();
        builder.Property(x => x.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
        builder.Property(x => x.AudioFormat).HasColumnName("audio_format").HasMaxLength(20).IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count");
        builder.Property(x => x.ProcessingToken)
            .HasColumnName("processing_token")
            .HasMaxLength(50)
            .IsConcurrencyToken();
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
        builder.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");

        builder.HasOne(x => x.Post)
            .WithOne(x => x.Audio)
            .HasForeignKey<PlatformCommunityPostAudio>(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PostId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc, x.UpdatedAtUtc });
        builder.HasIndex(x => x.ProcessingToken);
    }
}

public sealed class PlatformCommunityPostAudioSegmentConfiguration : IEntityTypeConfiguration<PlatformCommunityPostAudioSegment>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostAudioSegment> builder)
    {
        builder.ToTable("platform_community_post_audio_segments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AudioId).HasColumnName("audio_id");
        builder.Property(x => x.Sequence).HasColumnName("sequence");
        builder.Property(x => x.CharacterCount).HasColumnName("character_count");
        builder.Property(x => x.BucketName).HasColumnName("bucket_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ObjectName).HasColumnName("object_name").HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(120).IsRequired();
        builder.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasOne(x => x.Audio)
            .WithMany(x => x.Segments)
            .HasForeignKey(x => x.AudioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AudioId, x.Sequence }).IsUnique();
    }
}

public sealed class PlatformCommunityPostAudioAccessLogConfiguration : IEntityTypeConfiguration<PlatformCommunityPostAudioAccessLog>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostAudioAccessLog> builder)
    {
        builder.ToTable("platform_community_post_audio_access_logs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AudioId).HasColumnName("audio_id");
        builder.Property(x => x.PostId).HasColumnName("post_id");
        builder.Property(x => x.SegmentSequence).HasColumnName("segment_sequence");
        builder.Property(x => x.AccessType).HasColumnName("access_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.RequesterUserId).HasColumnName("requester_user_id").HasMaxLength(450);
        builder.Property(x => x.TraceId).HasColumnName("trace_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.AccessedAtUtc).HasColumnName("accessed_at_utc");

        builder.HasOne(x => x.Audio)
            .WithMany(x => x.AccessLogs)
            .HasForeignKey(x => x.AudioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PostId, x.AccessedAtUtc });
        builder.HasIndex(x => new { x.RequesterUserId, x.AccessedAtUtc });
    }
}
