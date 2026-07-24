using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Community;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Community;

public sealed class CommunityPostEmailNotificationOutboxConfiguration
    : IEntityTypeConfiguration<CommunityPostEmailNotificationOutbox>
{
    public void Configure(EntityTypeBuilder<CommunityPostEmailNotificationOutbox> builder)
    {
        builder.ToTable("community_post_email_notification_outbox");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired().IsConcurrencyToken();
        builder.Property(x => x.ProcessingToken).HasMaxLength(64).IsConcurrencyToken();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.HasIndex(x => x.PostId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        builder.HasIndex(x => x.LockedUntilUtc);
    }
}
