using Ssalddel.Domain.TraditionalMarkets;
using Ssalddel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets;

public sealed class TraditionalMarketLogisticsHubConfiguration
    : IEntityTypeConfiguration<TraditionalMarketLogisticsHub>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<TraditionalMarketLogisticsHub> builder)
    {
        builder.ToTable("traditional_market_logistics_hubs");
        builder.HasKey(x => x.MarketCode);
        builder.Property(x => x.MarketCode).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OperatorOrganizationName).HasMaxLength(160);
        builder.Property(x => x.ServiceRadiusKm).HasPrecision(8, 2);
        builder.Property(x => x.ReceivingWindow).HasMaxLength(160);
        builder.Property(x => x.PickupWindow).HasMaxLength(160);
        builder.Property(x => x.OperatingNotes).HasMaxLength(2000);
        builder.Property(x => x.SiteVerifiedByUserId).HasMaxLength(450);
        builder.Property(x => x.MapLatitude).HasPrecision(10, 7);
        builder.Property(x => x.MapLongitude).HasPrecision(10, 7);
        builder.Property(x => x.MapLocationPrecisionCode).HasMaxLength(40);
        builder.Property(x => x.MapLocationSourceName).HasMaxLength(160);
        builder.Property(x => x.MapLocationSourceHref).HasMaxLength(1000);
        builder.Property(x => x.MapLocationVerifiedByUserId).HasMaxLength(450);
        builder.Property(x => x.StatusReason).HasMaxLength(500);
        builder.Property(x => x.UpdatedByUserId).HasMaxLength(450);
        builder.Property(x => x.Revision).IsConcurrencyToken();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.UpdatedAtUtc });
        builder.HasIndex(x => new { x.Status, x.MapLocationVerifiedAtUtc });

        builder.HasOne<TraditionalMarket>()
            .WithOne()
            .HasForeignKey<TraditionalMarketLogisticsHub>(x => x.MarketCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
