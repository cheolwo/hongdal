using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class 주간국가농수산물비교SnapshotConfiguration
    : IEntityTypeConfiguration<주간국가농수산물비교Snapshot>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<주간국가농수산물비교Snapshot> builder)
    {
        builder.ToTable("agri_weekly_country_product_comparison_snapshots");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PeriodKey).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.PeriodKey).IsUnique();
        builder.HasIndex(x => new { x.WeekStartDate, x.WeekEndDate });

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Snapshot)
            .HasForeignKey(x => x.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class 주간국가농수산물비교항목Configuration
    : IEntityTypeConfiguration<주간국가농수산물비교항목>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<주간국가농수산물비교항목> builder)
    {
        builder.ToTable("agri_weekly_country_product_comparison_items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductKey).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProductNameKo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(x => x.CountryNameKo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.SourceKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.OriginalProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MarketStage).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Price).HasPrecision(20, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ComparisonNote).HasMaxLength(1000).IsRequired();

        builder.HasIndex(x => new { x.SnapshotId, x.ProductKey, x.CountryCode }).IsUnique();
        builder.HasIndex(x => new { x.CountryCode, x.ReferenceDate });
    }
}
