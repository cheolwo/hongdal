using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class Bls평균소매가격수집RunConfiguration
    : IEntityTypeConfiguration<Bls평균소매가격수집Run>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<Bls평균소매가격수집Run> builder)
    {
        builder.ToTable("agri_bls_average_retail_price_collection_runs");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.RunKey).HasMaxLength(40).IsRequired();
        builder.Property(item => item.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.QuerySummary).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SourceMessagesJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.ErrorMessage).HasMaxLength(2000).IsRequired();

        builder.HasIndex(item => item.RunKey).IsUnique();
        builder.HasIndex(item => new { item.StatusCode, item.StartedAtUtc });
        builder.HasIndex(item => new { item.YearFrom, item.YearTo });
    }
}

internal sealed class Bls평균소매가격관측Configuration
    : IEntityTypeConfiguration<Bls평균소매가격관측>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<Bls평균소매가격관측> builder)
    {
        builder.ToTable("agri_bls_average_retail_price_observations");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.RecordKey).HasMaxLength(80).IsRequired();
        builder.Property(item => item.SeriesId).HasMaxLength(30).IsRequired();
        builder.Property(item => item.ItemCode).HasMaxLength(20).IsRequired();
        builder.Property(item => item.CanonicalProductKey).HasMaxLength(80).IsRequired();
        builder.Property(item => item.ProductNameKo).HasMaxLength(100).IsRequired();
        builder.Property(item => item.ItemNameEn).HasMaxLength(300).IsRequired();
        builder.Property(item => item.AreaCode).HasMaxLength(10).IsRequired();
        builder.Property(item => item.AreaName).HasMaxLength(100).IsRequired();
        builder.Property(item => item.PeriodCode).HasMaxLength(10).IsRequired();
        builder.Property(item => item.PeriodName).HasMaxLength(30).IsRequired();
        builder.Property(item => item.ValueRaw).HasMaxLength(64).IsRequired();
        builder.Property(item => item.PriceUsd).HasPrecision(20, 6);
        builder.Property(item => item.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(item => item.OriginalUnit).HasMaxLength(60).IsRequired();
        builder.Property(item => item.Footnote).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.RawJson).HasColumnType("json").IsRequired();

        builder.HasOne(item => item.FirstCollectionRun)
            .WithMany(run => run.NewObservations)
            .HasForeignKey(item => item.FirstCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.RecordKey).IsUnique();
        builder.HasIndex(item => new { item.SeriesId, item.ReferenceMonth });
        builder.HasIndex(item => new { item.CanonicalProductKey, item.ReferenceMonth });
        builder.HasIndex(item => item.LastSeenAtUtc);
    }
}
