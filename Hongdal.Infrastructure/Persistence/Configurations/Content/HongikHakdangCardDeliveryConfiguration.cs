using Hongdal.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Content;

public sealed class HongikHakdangCardImageVariantConfiguration
    : IEntityTypeConfiguration<HongikHakdangCardImageVariant>
{
    public void Configure(EntityTypeBuilder<HongikHakdangCardImageVariant> builder)
    {
        builder.ToTable("hongik_hakdang_card_image_variants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardId).HasColumnName("card_id");
        builder.Property(x => x.VariantKind).HasColumnName("variant_kind").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.LocalImagePath).HasColumnName("local_image_path").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes");
        builder.Property(x => x.Sha256).HasColumnName("sha256").HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceImageSha256).HasColumnName("source_image_sha256").HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(x => x.Card)
            .WithMany(x => x.ImageVariants)
            .HasForeignKey(x => x.CardId)
            .HasConstraintName("FK_hh_card_variants_cards")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CardId, x.VariantKind })
            .HasDatabaseName("UX_hh_card_variants_card_kind")
            .IsUnique();
        builder.HasIndex(x => x.Sha256)
            .HasDatabaseName("IX_hh_card_variants_sha256");
    }
}

public sealed class HongikHakdangCardDeliveryPreferenceConfiguration
    : IEntityTypeConfiguration<HongikHakdangCardDeliveryPreference>
{
    public void Configure(EntityTypeBuilder<HongikHakdangCardDeliveryPreference> builder)
    {
        builder.ToTable("hongik_hakdang_card_delivery_preferences");
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(450);
        builder.Property(x => x.Enabled).HasColumnName("enabled");
        builder.Property(x => x.DeliveryMode).HasColumnName("delivery_mode").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PushEnabled).HasColumnName("push_enabled");
        builder.Property(x => x.LocalDeliveryMinute).HasColumnName("local_delivery_minute");
        builder.Property(x => x.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ShuffleWithoutRepeats).HasColumnName("shuffle_without_repeats");
        builder.Property(x => x.PreferredCollectionKey).HasColumnName("preferred_collection_key").HasMaxLength(200);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => new { x.Enabled, x.PushEnabled })
            .HasDatabaseName("IX_hh_card_preferences_delivery");
    }
}

public sealed class HongikHakdangDailyCardSelectionConfiguration
    : IEntityTypeConfiguration<HongikHakdangDailyCardSelection>
{
    public void Configure(EntityTypeBuilder<HongikHakdangDailyCardSelection> builder)
    {
        builder.ToTable("hongik_hakdang_daily_card_selections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SelectionDate).HasColumnName("selection_date").HasColumnType("date");
        builder.Property(x => x.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CardId).HasColumnName("card_id");
        builder.Property(x => x.SelectedAtUtc).HasColumnName("selected_at_utc");

        builder.HasOne(x => x.Card)
            .WithMany()
            .HasForeignKey(x => x.CardId)
            .HasConstraintName("FK_hh_daily_cards_cards")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SelectionDate, x.TimeZoneId })
            .HasDatabaseName("UX_hh_daily_cards_date_zone")
            .IsUnique();
        builder.HasIndex(x => x.CardId)
            .HasDatabaseName("IX_hh_daily_cards_card");
    }
}

public sealed class HongikHakdangCardDeliveryOutboxConfiguration
    : IEntityTypeConfiguration<HongikHakdangCardDeliveryOutbox>
{
    public void Configure(EntityTypeBuilder<HongikHakdangCardDeliveryOutbox> builder)
    {
        builder.ToTable("hongik_hakdang_card_delivery_outbox");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(240).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.InstallationId).HasColumnName("installation_id");
        builder.Property(x => x.CardId).HasColumnName("card_id");
        builder.Property(x => x.SelectionDate).HasColumnName("selection_date").HasColumnType("date");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count");
        builder.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(1000);
        builder.Property(x => x.SentAtUtc).HasColumnName("sent_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(x => x.Card)
            .WithMany()
            .HasForeignKey(x => x.CardId)
            .HasConstraintName("FK_hh_card_outbox_cards")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Installation)
            .WithMany()
            .HasForeignKey(x => x.InstallationId)
            .HasConstraintName("FK_hh_card_outbox_installations")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.IdempotencyKey)
            .HasDatabaseName("UX_hh_card_outbox_idempotency")
            .IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc })
            .HasDatabaseName("IX_hh_card_outbox_due");
        builder.HasIndex(x => x.InstallationId)
            .HasDatabaseName("IX_hh_card_outbox_installation");
        builder.HasIndex(x => x.CardId)
            .HasDatabaseName("IX_hh_card_outbox_card");
    }
}
