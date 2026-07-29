using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class 농수산물포장Fcl분석SnapshotConfiguration
    : IEntityTypeConfiguration<농수산물포장Fcl분석Snapshot>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<농수산물포장Fcl분석Snapshot> builder)
    {
        builder.ToTable("agri_packaging_fcl_analysis_snapshots");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.AnalysisKey).HasMaxLength(100).IsRequired();
        builder.Property(item => item.ProfileVersion).HasMaxLength(30).IsRequired();
        builder.Property(item => item.CategoryCode).HasMaxLength(10).IsRequired();
        builder.Property(item => item.CategoryName).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ItemCode).HasMaxLength(20).IsRequired();
        builder.Property(item => item.ItemName).HasMaxLength(100).IsRequired();
        builder.Property(item => item.KamisPriceComparisonUnitsJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.KamisKindNamesJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.PackageTypeCode).HasMaxLength(40).IsRequired();
        builder.Property(item => item.PackageUnitLabel).HasMaxLength(40).IsRequired();
        builder.Property(item => item.NetContentWeightKg).HasPrecision(12, 3);
        builder.Property(item => item.GrossWeightKg).HasPrecision(12, 3);
        builder.Property(item => item.UnitCountLabel).HasMaxLength(30).IsRequired();
        builder.Property(item => item.TemperatureCode).HasMaxLength(20).IsRequired();
        builder.Property(item => item.PackingMethodCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.EvidenceLevelCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ConfidenceScore).HasPrecision(5, 4);
        builder.Property(item => item.AssumptionNote).HasMaxLength(3000).IsRequired();
        builder.Property(item => item.EvidenceJson).HasColumnType("json").IsRequired();
        builder.Property(item => item.ContainerEstimatesJson).HasColumnType("json").IsRequired();

        builder.HasIndex(item => item.AnalysisKey).IsUnique();
        builder.HasIndex(item => new { item.SourceYear, item.CategoryCode, item.ItemCode });
        builder.HasIndex(item => new { item.SourceYear, item.EvidenceLevelCode });
        builder.HasIndex(item => item.AnalyzedAtUtc);
    }
}
