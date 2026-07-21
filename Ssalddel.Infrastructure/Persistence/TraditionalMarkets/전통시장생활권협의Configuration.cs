using Ssalddel.Domain.TraditionalMarkets;
using Ssalddel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets;

public sealed class 전통시장생활권협의체Configuration
    : IEntityTypeConfiguration<전통시장생활권협의체>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<전통시장생활권협의체> builder)
    {
        builder.ToTable("traditional_market_neighborhood_councils");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.시장Code).HasColumnName("MarketCode").HasMaxLength(20).IsRequired();
        builder.Property(x => x.협의체명).HasColumnName("CouncilName").HasMaxLength(160).IsRequired();
        builder.Property(x => x.아파트단지명).HasColumnName("ApartmentCommunityName").HasMaxLength(160).IsRequired();
        builder.Property(x => x.아파트주소).HasColumnName("ApartmentAddress").HasMaxLength(500);
        builder.Property(x => x.아파트대표UserId).HasColumnName("ApartmentRepresentativeUserId").HasMaxLength(450).IsRequired();
        builder.Property(x => x.아파트대표명).HasColumnName("ApartmentRepresentativeName").HasMaxLength(100).IsRequired();
        builder.Property(x => x.아파트대표수락AtUtc).HasColumnName("ApartmentRepresentativeAcceptedAtUtc");
        builder.Property(x => x.상인회명).HasColumnName("MerchantAssociationName").HasMaxLength(160).IsRequired();
        builder.Property(x => x.상인회대표UserId).HasColumnName("MerchantRepresentativeUserId").HasMaxLength(450).IsRequired();
        builder.Property(x => x.상인회대표명).HasColumnName("MerchantRepresentativeName").HasMaxLength(100).IsRequired();
        builder.Property(x => x.상인회대표수락AtUtc).HasColumnName("MerchantRepresentativeAcceptedAtUtc");
        builder.Property(x => x.협의목적).HasColumnName("Purpose").HasMaxLength(2000);
        builder.Property(x => x.상태).HasColumnName("Status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Revision).IsConcurrencyToken();

        builder.HasIndex(x => new { x.시장Code, x.상태 });
        builder.HasIndex(x => new { x.아파트대표UserId, x.상태 });
        builder.HasIndex(x => new { x.상인회대표UserId, x.상태 });

        builder.HasOne<TraditionalMarket>()
            .WithMany()
            .HasForeignKey(x => x.시장Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class 전통시장교역안건Configuration
    : IEntityTypeConfiguration<전통시장교역안건>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<전통시장교역안건> builder)
    {
        builder.ToTable("traditional_market_trade_agendas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.협의체Id).HasColumnName("CouncilId");
        builder.Property(x => x.교역방향).HasColumnName("TradeDirection").HasMaxLength(20).IsRequired();
        builder.Property(x => x.품목명).HasColumnName("ItemName").HasMaxLength(200).IsRequired();
        builder.Property(x => x.품목설명).HasColumnName("ItemDescription").HasMaxLength(2000);
        builder.Property(x => x.희망수량).HasColumnName("RequestedQuantity").HasPrecision(18, 3);
        builder.Property(x => x.수량단위).HasColumnName("QuantityUnit").HasMaxLength(40).IsRequired();
        builder.Property(x => x.원산지국가).HasColumnName("OriginCountry").HasMaxLength(100);
        builder.Property(x => x.목적지국가).HasColumnName("DestinationCountry").HasMaxLength(100);
        builder.Property(x => x.희망시작일).HasColumnName("DesiredStartDate");
        builder.Property(x => x.희망종료일).HasColumnName("DesiredEndDate");
        builder.Property(x => x.물류조건).HasColumnName("LogisticsTerms").HasMaxLength(2000);
        builder.Property(x => x.예상금액).HasColumnName("EstimatedAmount").HasPrecision(18, 2);
        builder.Property(x => x.통화Code).HasColumnName("CurrencyCode").HasMaxLength(3).IsRequired();
        builder.Property(x => x.통관검토필요여부).HasColumnName("RequiresCustomsReview");
        builder.Property(x => x.제안내용).HasColumnName("ProposalText").HasMaxLength(4000);
        builder.Property(x => x.상태).HasColumnName("Status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.아파트측결정).HasColumnName("ApartmentDecision").HasMaxLength(30).IsRequired();
        builder.Property(x => x.아파트측의견).HasColumnName("ApartmentDecisionMemo").HasMaxLength(2000);
        builder.Property(x => x.아파트측결정AtUtc).HasColumnName("ApartmentDecidedAtUtc");
        builder.Property(x => x.상인회측결정).HasColumnName("MerchantDecision").HasMaxLength(30).IsRequired();
        builder.Property(x => x.상인회측의견).HasColumnName("MerchantDecisionMemo").HasMaxLength(2000);
        builder.Property(x => x.상인회측결정AtUtc).HasColumnName("MerchantDecidedAtUtc");
        builder.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Revision).IsConcurrencyToken();

        builder.HasIndex(x => new { x.협의체Id, x.상태, x.UpdatedAtUtc });
        builder.HasOne(x => x.협의체)
            .WithMany(x => x.안건)
            .HasForeignKey(x => x.협의체Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
