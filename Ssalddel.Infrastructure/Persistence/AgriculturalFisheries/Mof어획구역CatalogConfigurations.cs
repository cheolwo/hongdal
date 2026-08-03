using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class Mof어획구역Catalog수집RunConfiguration
    : IEntityTypeConfiguration<Mof어획구역Catalog수집Run>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<Mof어획구역Catalog수집Run> builder)
    {
        builder.ToTable("agri_mof_fishing_area_collection_runs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RunKey).HasMaxLength(160).IsRequired();
        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.DatasetVersion).HasMaxLength(40).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ContentSha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => x.RunKey).IsUnique();
        builder.HasIndex(x => new { x.SourceKey, x.StatusCode, x.CompletedAtUtc });
        builder.HasOne(x => x.Snapshot)
            .WithMany(x => x.CollectionRuns)
            .HasForeignKey(x => x.SnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class Mof어획구역Catalog영속SnapshotConfiguration
    : IEntityTypeConfiguration<Mof어획구역Catalog영속Snapshot>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<Mof어획구역Catalog영속Snapshot> builder)
    {
        builder.ToTable("agri_mof_fishing_area_snapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.DatasetVersion).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ContentSha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FreshnessCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.NormalizedRecordsJson).IsRequired();
        builder.HasIndex(x => new { x.SourceKey, x.DatasetVersion, x.ContentSha256 }).IsUnique();
        builder.HasIndex(x => new { x.SourceKey, x.LastSeenAtUtc });
    }
}
