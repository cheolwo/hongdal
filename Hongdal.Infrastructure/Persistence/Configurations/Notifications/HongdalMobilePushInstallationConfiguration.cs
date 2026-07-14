using Hongdal.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Notifications;

public sealed class HongdalMobilePushInstallationConfiguration
    : IEntityTypeConfiguration<HongdalMobilePushInstallation>
{
    public void Configure(EntityTypeBuilder<HongdalMobilePushInstallation> builder)
    {
        builder.ToTable("hongdal_mobile_push_installations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.InstallationId).HasColumnName("installation_id").HasMaxLength(120).IsRequired();
        builder.Property(x => x.AppKey).HasColumnName("app_key").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Platform).HasColumnName("platform").HasMaxLength(20).IsRequired();
        builder.Property(x => x.PushToken).HasColumnName("push_token").HasMaxLength(4096).IsRequired();
        builder.Property(x => x.PushTokenHash).HasColumnName("push_token_hash").HasMaxLength(64).IsRequired();
        builder.Property(x => x.AppVersion).HasColumnName("app_version").HasMaxLength(40);
        builder.Property(x => x.DeviceModel).HasColumnName("device_model").HasMaxLength(200);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.LastSeenAtUtc).HasColumnName("last_seen_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => new { x.AppKey, x.InstallationId })
            .HasDatabaseName("UX_mobile_push_app_installation")
            .IsUnique();
        builder.HasIndex(x => x.PushTokenHash)
            .HasDatabaseName("IX_mobile_push_token_hash");
        builder.HasIndex(x => new { x.UserId, x.IsActive })
            .HasDatabaseName("IX_mobile_push_user_active");
    }
}
