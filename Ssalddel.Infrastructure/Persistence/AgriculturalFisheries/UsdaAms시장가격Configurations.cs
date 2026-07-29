using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class UsdaAms시장가격수집RunConfiguration
    : IEntityTypeConfiguration<UsdaAms시장가격수집Run>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<UsdaAms시장가격수집Run> builder)
    {
        builder.ToTable("agri_usda_ams_market_price_collection_runs");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.RunKey).HasMaxLength(40).IsRequired();
        builder.Property(item => item.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.RequestedMarketTypesJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SourceMessagesJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.ErrorMessage).HasMaxLength(2000).IsRequired();
        builder.HasIndex(item => item.RunKey).IsUnique();
        builder.HasIndex(item => new { item.StatusCode, item.StartedAtUtc });
        builder.HasIndex(item => new { item.DateFrom, item.DateTo });
    }
}

internal sealed class UsdaAms시장가격관측Configuration
    : IEntityTypeConfiguration<UsdaAms시장가격관측>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<UsdaAms시장가격관측> builder)
    {
        builder.ToTable("agri_usda_ams_market_price_observations");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.RecordKey).HasMaxLength(64).IsRequired();
        builder.Property(item => item.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(item => item.MarketStageCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.SlugId).HasMaxLength(20).IsRequired();
        builder.Property(item => item.SlugName).HasMaxLength(80).IsRequired();
        builder.Property(item => item.ReportTitle).HasMaxLength(500).IsRequired();
        builder.Property(item => item.PublishedDateRaw).HasMaxLength(50).IsRequired();
        builder.Property(item => item.OfficeName).HasMaxLength(120).IsRequired();
        builder.Property(item => item.OfficeState).HasMaxLength(20).IsRequired();
        builder.Property(item => item.OfficeCity).HasMaxLength(120).IsRequired();
        builder.Property(item => item.MarketType).HasMaxLength(80).IsRequired();
        builder.Property(item => item.MarketLocationName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.MarketLocationState).HasMaxLength(20).IsRequired();
        builder.Property(item => item.MarketLocationCity).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Community).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Group).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Category).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Commodity).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Variety).HasMaxLength(300).IsRequired();
        builder.Property(item => item.Repack).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Package).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Storage).HasMaxLength(120).IsRequired();
        builder.Property(item => item.TransportationMode).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Grade).HasMaxLength(160).IsRequired();
        builder.Property(item => item.UnitSales).HasMaxLength(160).IsRequired();
        builder.Property(item => item.ItemSize).HasMaxLength(300).IsRequired();
        builder.Property(item => item.Appearance).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Quality).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Condition).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Organic).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Crop).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Origin).HasMaxLength(200).IsRequired();
        builder.Property(item => item.District).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Environment).HasMaxLength(120).IsRequired();
        builder.Property(item => item.LowPrice).HasPrecision(24, 8);
        builder.Property(item => item.HighPrice).HasPrecision(24, 8);
        builder.Property(item => item.MostlyLowPrice).HasPrecision(24, 8);
        builder.Property(item => item.MostlyHighPrice).HasPrecision(24, 8);
        builder.Property(item => item.WeightedAveragePrice).HasPrecision(24, 8);
        builder.Property(item => item.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(item => item.OriginalUnit).HasMaxLength(500).IsRequired();
        builder.Property(item => item.RawJson).HasColumnType("json").IsRequired();
        builder.HasOne(item => item.FirstCollectionRun)
            .WithMany(run => run.NewObservations)
            .HasForeignKey(item => item.FirstCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.RecordKey).IsUnique();
        builder.HasIndex(item => new
        {
            item.SourceKey,
            item.ReportBeginDate
        });
        builder.HasIndex(item => new
        {
            item.Commodity,
            item.ReportBeginDate
        });
        builder.HasIndex(item => new
        {
            item.Commodity,
            item.MarketStageCode,
            item.ReportBeginDate
        });
        builder.HasIndex(item => new
        {
            item.MarketLocationState,
            item.ReportBeginDate
        });
        builder.HasIndex(item => item.LastSeenAtUtc);
    }
}

internal sealed class UsdaAms연도상품CatalogConfiguration
    : IEntityTypeConfiguration<UsdaAms연도상품Catalog>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<UsdaAms연도상품Catalog> builder)
    {
        builder.ToTable("agri_usda_ams_year_commodity_catalog");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Commodity).HasMaxLength(200).IsRequired();
        builder.HasIndex(item => new
        {
            item.Year,
            item.Commodity
        }).IsUnique();
        builder.HasIndex(item => new
        {
            item.Commodity,
            item.Year
        });
    }
}
