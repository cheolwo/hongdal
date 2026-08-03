using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.PublicData;

namespace Ssalddel.Infrastructure.Persistence;

public sealed class 공동주택공공정보수집RunConfiguration
    : IEntityTypeConfiguration<공동주택공공정보수집Run>
{
    public void Configure(EntityTypeBuilder<공동주택공공정보수집Run> builder)
    {
        builder.ToTable("public_apartment_collection_runs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RunKey).HasMaxLength(220).IsRequired();
        builder.Property(x => x.ScopeKey).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ComplexCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ComplexName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TargetMonth).HasMaxLength(6).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.HasIndex(x => x.RunKey).IsUnique();
        builder.HasIndex(x => new { x.ComplexCode, x.TargetMonth, x.StatusCode });
    }
}

public sealed class 공동주택공공정보SnapshotConfiguration
    : IEntityTypeConfiguration<공동주택공공정보Snapshot>
{
    public void Configure(EntityTypeBuilder<공동주택공공정보Snapshot> builder)
    {
        builder.ToTable("public_apartment_snapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SourceVersion).HasMaxLength(40).IsRequired();
        builder.Property(x => x.SpatialKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ComplexCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ComplexName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TargetMonth).HasMaxLength(6).IsRequired();
        builder.Property(x => x.ContentSha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NormalizedJson).HasColumnType("longtext").IsRequired();
        builder.Property(x => x.FreshnessStatusCode).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.ComplexCode, x.TargetMonth }).IsUnique();
        builder.HasIndex(x => x.ContentSha256);
    }
}
