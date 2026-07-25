using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class 국내농산물경락가격수집RunConfiguration
    : IEntityTypeConfiguration<국내농산물경락가격수집Run>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<국내농산물경락가격수집Run> builder)
    {
        builder.ToTable("agri_domestic_auction_price_collection_runs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RunKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.SettlementDate).HasColumnType("date");
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000).IsRequired();

        builder.HasIndex(x => x.RunKey).IsUnique();
        builder.HasIndex(x => new { x.SourceKey, x.SettlementDate, x.StatusCode });
    }
}

internal sealed class 국내농산물경락가격관측Configuration
    : IEntityTypeConfiguration<국내농산물경락가격관측>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<국내농산물경락가격관측> builder)
    {
        builder.ToTable("agri_domestic_auction_price_observations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SettlementDate).HasColumnType("date");
        builder.Property(x => x.WholesaleMarketCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CorporationCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.SlipNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.AuctionSequence1).HasMaxLength(30).IsRequired();
        builder.Property(x => x.AuctionSequence2).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TradingMethodCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.LargeCategoryCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.MiddleCategoryCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SmallCategoryCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CorporationItemCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.VarietyName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.UnitWeight).HasPrecision(20, 6);
        builder.Property(x => x.UnitCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PackageCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SizeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.GradeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(24, 6);
        builder.Property(x => x.AuctionPriceKrw).HasPrecision(24, 4);
        builder.Property(x => x.OriginCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.OriginName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TotalQuantity).HasPrecision(24, 6);
        builder.Property(x => x.TotalAmountKrw).HasPrecision(24, 4);
        builder.Property(x => x.AwardedTime).HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.FirstCollectionRun)
            .WithMany(x => x.NewObservations)
            .HasForeignKey(x => x.FirstCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RecordKey).IsUnique();
        builder.HasIndex(x => new
        {
            x.SettlementDate,
            x.WholesaleMarketCode,
            x.CorporationCode,
            x.ItemName
        });
        builder.HasIndex(x => new { x.ItemName, x.VarietyName, x.SettlementDate });
        builder.HasIndex(x => new { x.ItemName, x.SettlementDate, x.OriginName })
            .HasDatabaseName("IX_AuctionPrice_Item_Date_Origin");
        builder.HasIndex(x => new
        {
            x.ItemName,
            x.SettlementDate,
            x.WholesaleMarketCode
        }).HasDatabaseName("IX_AuctionPrice_Item_Date_Market");
        builder.HasIndex(x => x.LastSeenAtUtc);
    }
}
