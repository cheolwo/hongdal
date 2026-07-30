using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class UsdaAms공개사업체수집RunConfiguration
    : IEntityTypeConfiguration<UsdaAms공개사업체수집Run>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<UsdaAms공개사업체수집Run> builder)
    {
        builder.ToTable("agri_usda_ams_public_business_collection_runs");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.RunKey).HasMaxLength(40).IsRequired();
        builder.Property(item => item.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.RequestedDirectoryTypesJson)
            .HasColumnType("json")
            .IsRequired();
        builder.Property(item => item.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SourceMessagesJson)
            .HasColumnType("json")
            .IsRequired();
        builder.Property(item => item.ErrorMessage).HasMaxLength(2000).IsRequired();
        builder.HasIndex(item => item.RunKey).IsUnique();
        builder.HasIndex(item => new
        {
            item.StatusCode,
            item.StartedAtUtc
        });
    }
}

internal sealed class UsdaAms공개사업체ProfileConfiguration
    : IEntityTypeConfiguration<UsdaAms공개사업체Profile>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<UsdaAms공개사업체Profile> builder)
    {
        builder.ToTable("agri_usda_ams_public_business_profiles");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProfileKey).HasMaxLength(64).IsRequired();
        builder.Property(item => item.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(item => item.DirectoryTypeCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.ExternalListingId).HasMaxLength(80).IsRequired();
        builder.Property(item => item.BusinessName).HasMaxLength(500).IsRequired();
        builder.Property(item => item.BusinessNameNormalized).HasMaxLength(500).IsRequired();
        builder.Property(item => item.CityName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.StateCode).HasMaxLength(10).IsRequired();
        builder.Property(item => item.LocationPrecisionCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.LegalStatus).HasMaxLength(300).IsRequired();
        builder.Property(item => item.ProductSummary).HasMaxLength(4000).IsRequired();
        builder.Property(item => item.OfficialListingUrl).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SourceFingerprint).HasMaxLength(64).IsRequired();
        builder.HasOne(item => item.FirstCollectionRun)
            .WithMany()
            .HasForeignKey(item => item.FirstCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.LastCollectionRun)
            .WithMany()
            .HasForeignKey(item => item.LastCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.ProfileKey).IsUnique();
        builder.HasIndex(item => new
        {
            item.SourceKey,
            item.DirectoryTypeCode,
            item.ExternalListingId
        }).IsUnique();
        builder.HasIndex(item => new
        {
            item.DirectoryTypeCode,
            item.StateCode,
            item.BusinessNameNormalized
        });
        builder.HasIndex(item => new
        {
            item.StateCode,
            item.IsCurrentlyListed,
            item.DirectoryTypeCode
        });
        builder.HasIndex(item => item.LastSeenAtUtc);
    }
}

internal sealed class UsdaAms공개사업체취급품목Configuration
    : IEntityTypeConfiguration<UsdaAms공개사업체취급품목>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<UsdaAms공개사업체취급품목> builder)
    {
        builder.ToTable("agri_usda_ams_public_business_products");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductKey).HasMaxLength(200).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(300).IsRequired();
        builder.HasOne(item => item.Profile)
            .WithMany(profile => profile.Products)
            .HasForeignKey(item => item.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(item => new
        {
            item.ProfileId,
            item.ProductKey
        }).IsUnique();
        builder.HasIndex(item => new
        {
            item.ProductKey,
            item.ProfileId
        });
    }
}
