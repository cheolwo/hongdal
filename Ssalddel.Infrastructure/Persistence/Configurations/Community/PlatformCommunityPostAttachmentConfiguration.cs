using Ssalddel.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Community;

public sealed class PlatformCommunityPostAttachmentConfiguration : IEntityTypeConfiguration<PlatformCommunityPostAttachment>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostAttachment> builder)
    {
        builder.ToTable("platform_community_post_attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BucketName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ObjectName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(120).IsRequired();

        builder.HasIndex(x => new { x.PostId, x.UploadedAtUtc });

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Attachment)
            .HasForeignKey(x => x.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
