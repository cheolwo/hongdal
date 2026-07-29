using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class 국제농수산가격수집RunConfiguration
    : IEntityTypeConfiguration<국제농수산가격수집Run>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<국제농수산가격수집Run> builder)
    {
        builder.ToTable("agri_international_price_collection_runs");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.RunKey).HasMaxLength(40).IsRequired();
        builder.Property(item => item.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(item => item.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.QuerySummary).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SourceMessagesJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.ErrorMessage).HasMaxLength(2000).IsRequired();

        builder.HasIndex(item => item.RunKey).IsUnique();
        builder.HasIndex(item => new { item.SourceKey, item.StatusCode, item.StartedAtUtc });
        builder.HasIndex(item => new { item.SourceKey, item.YearFrom, item.YearTo });
    }
}

internal sealed class 국제농수산가격관측Configuration
    : IEntityTypeConfiguration<국제농수산가격관측>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<국제농수산가격관측> builder)
    {
        builder.ToTable("agri_international_price_observations");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.RecordKey).HasMaxLength(180).IsRequired();
        builder.Property(item => item.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(item => item.DatasetCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.CountryCode).HasMaxLength(20).IsRequired();
        builder.Property(item => item.CountryName).HasMaxLength(120).IsRequired();
        builder.Property(item => item.GeographyCode).HasMaxLength(80).IsRequired();
        builder.Property(item => item.GeographyName).HasMaxLength(160).IsRequired();
        builder.Property(item => item.MarketStageCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.OfficialSeriesCode).HasMaxLength(80).IsRequired();
        builder.Property(item => item.OfficialProductCode).HasMaxLength(80).IsRequired();
        builder.Property(item => item.ProductNameOriginal).HasMaxLength(500).IsRequired();
        builder.Property(item => item.CanonicalProductKey).HasMaxLength(100).IsRequired();
        builder.Property(item => item.FrequencyCode).HasMaxLength(20).IsRequired();
        builder.Property(item => item.ValueRaw).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Price).HasPrecision(24, 8);
        builder.Property(item => item.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(item => item.OriginalUnit).HasMaxLength(160).IsRequired();
        builder.Property(item => item.BasePeriod).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ObservationStatus).HasMaxLength(200).IsRequired();
        builder.Property(item => item.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.RawJson).HasColumnType("json").IsRequired();

        builder.HasOne(item => item.FirstCollectionRun)
            .WithMany(run => run.NewObservations)
            .HasForeignKey(item => item.FirstCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.RecordKey).IsUnique();
        builder.HasIndex(item => new
        {
            item.SourceKey,
            item.CountryCode,
            item.ReferenceDate
        });
        builder.HasIndex(item => new
        {
            item.SourceKey,
            item.OfficialProductCode,
            item.ReferenceDate
        });
        builder.HasIndex(item => item.LastSeenAtUtc);
    }
}
