using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Community;

public sealed class PlatformCommunityPostAttachmentCommentConfiguration : IEntityTypeConfiguration<PlatformCommunityPostAttachmentComment>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostAttachmentComment> builder)
    {
        builder.ToTable("platform_community_post_attachment_comments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nickname).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => new { x.AttachmentId, x.IsDeleted, x.IsOperatorHidden, x.CreatedAtUtc });
    }
}
