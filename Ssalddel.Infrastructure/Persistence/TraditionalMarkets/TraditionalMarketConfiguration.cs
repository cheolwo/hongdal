using Ssalddel.Domain.TraditionalMarkets;
using Ssalddel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets;

public sealed class TraditionalMarketConfiguration
    : IEntityTypeConfiguration<TraditionalMarket>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<TraditionalMarket> builder)
    {
        builder.ToTable("public_data_traditional_markets");
        builder.HasKey(x => x.MarketCode);
        builder.Property(x => x.MarketCode).HasMaxLength(20);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MarketType).HasMaxLength(60);
        builder.Property(x => x.LotNumberAddress).HasMaxLength(500);
        builder.Property(x => x.RoadAddress).HasMaxLength(500);
        builder.Property(x => x.Province).HasMaxLength(60);
        builder.Property(x => x.CityCounty).HasMaxLength(100);
        builder.Property(x => x.SourceDatasetKey).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SourceHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => new { x.Province, x.CityCounty, x.IsActive });
        builder.HasIndex(x => new { x.SourceDatasetKey, x.SourceReferenceDate });

        builder.OwnsOne(x => x.Facilities, facilities =>
        {
            facilities.Property(x => x.HasArcade).HasColumnName("has_arcade");
            facilities.Property(x => x.HasElevatorOrEscalator).HasColumnName("has_elevator_or_escalator");
            facilities.Property(x => x.HasCustomerSupportCenter).HasColumnName("has_customer_support_center");
            facilities.Property(x => x.HasSprinkler).HasColumnName("has_sprinkler");
            facilities.Property(x => x.HasFireDetector).HasColumnName("has_fire_detector");
            facilities.Property(x => x.HasChildrenPlayroom).HasColumnName("has_children_playroom");
            facilities.Property(x => x.HasCallCenter).HasColumnName("has_call_center");
            facilities.Property(x => x.HasCustomerLounge).HasColumnName("has_customer_lounge");
            facilities.Property(x => x.HasNursingCenter).HasColumnName("has_nursing_center");
            facilities.Property(x => x.HasLocker).HasColumnName("has_locker");
            facilities.Property(x => x.HasBicycleStorage).HasColumnName("has_bicycle_storage");
            facilities.Property(x => x.HasSportsFacility).HasColumnName("has_sports_facility");
            facilities.Property(x => x.HasLibrary).HasColumnName("has_library");
            facilities.Property(x => x.HasShoppingCart).HasColumnName("has_shopping_cart");
            facilities.Property(x => x.HasForeignVisitorCenter).HasColumnName("has_foreign_visitor_center");
            facilities.Property(x => x.HasCustomerPath).HasColumnName("has_customer_path");
            facilities.Property(x => x.HasBroadcastCenter).HasColumnName("has_broadcast_center");
            facilities.Property(x => x.HasCultureClassroom).HasColumnName("has_culture_classroom");
            facilities.Property(x => x.HasSharedLogisticsWarehouse).HasColumnName("has_shared_logistics_warehouse");
            facilities.Property(x => x.HasDedicatedParking).HasColumnName("has_dedicated_parking");
            facilities.Property(x => x.HasTrainingRoom).HasColumnName("has_training_room");
            facilities.Property(x => x.HasMeetingRoom).HasColumnName("has_meeting_room");
            facilities.Property(x => x.HasAed).HasColumnName("has_aed");
        });
        builder.Navigation(x => x.Facilities).IsRequired();
    }
}
