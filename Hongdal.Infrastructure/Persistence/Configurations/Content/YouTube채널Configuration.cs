using Hongdal.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Content;

public sealed class YouTube감시채널Configuration : IEntityTypeConfiguration<YouTube감시채널>
{
    public void Configure(EntityTypeBuilder<YouTube감시채널> builder)
    {
        builder.ToTable("youtube_watched_channels");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChannelId).HasColumnName("channel_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.채널명).HasColumnName("channel_name").HasMaxLength(300).IsRequired();
        builder.Property(x => x.UploadsPlaylistId).HasColumnName("uploads_playlist_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.썸네일Url).HasColumnName("thumbnail_url").HasMaxLength(1000);
        builder.Property(x => x.활성화여부).HasColumnName("is_active");
        builder.Property(x => x.초기동기화완료여부).HasColumnName("initial_sync_completed");
        builder.Property(x => x.마지막동기화일시Utc).HasColumnName("last_synced_at_utc");
        builder.Property(x => x.마지막영상Id).HasColumnName("latest_video_id").HasMaxLength(100);
        builder.Property(x => x.마지막영상게시일시Utc).HasColumnName("latest_video_published_at_utc");
        builder.Property(x => x.생성일시Utc).HasColumnName("created_at_utc");
        builder.Property(x => x.수정일시Utc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.ChannelId).IsUnique();
        builder.HasIndex(x => new { x.활성화여부, x.마지막동기화일시Utc });
    }
}

public sealed class YouTube채널영상Configuration : IEntityTypeConfiguration<YouTube채널영상>
{
    public void Configure(EntityTypeBuilder<YouTube채널영상> builder)
    {
        builder.ToTable("youtube_channel_videos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.YouTube감시채널Id).HasColumnName("watched_channel_id");
        builder.Property(x => x.VideoId).HasColumnName("video_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChannelId).HasColumnName("channel_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.제목).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(x => x.설명).HasColumnName("description").HasColumnType("text").IsRequired();
        builder.Property(x => x.게시일시Utc).HasColumnName("published_at_utc");
        builder.Property(x => x.썸네일Url).HasColumnName("thumbnail_url").HasMaxLength(1000);
        builder.Property(x => x.신규업로드여부).HasColumnName("is_new_upload");
        builder.Property(x => x.공유상태).HasColumnName("sharing_status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.최초감지일시Utc).HasColumnName("first_detected_at_utc");

        builder.HasOne(x => x.감시채널)
            .WithMany(x => x.영상)
            .HasForeignKey(x => x.YouTube감시채널Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.VideoId).IsUnique();
        builder.HasIndex(x => new { x.ChannelId, x.게시일시Utc });
        builder.HasIndex(x => new { x.신규업로드여부, x.공유상태, x.최초감지일시Utc });
    }
}
