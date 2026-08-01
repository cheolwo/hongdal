using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Geography;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Geography;

public sealed class 지역농수산Map행정구역Configuration
    : IEntityTypeConfiguration<지역농수산Map행정구역>
{
    public void Configure(EntityTypeBuilder<지역농수산Map행정구역> builder)
    {
        builder.HasIndex(item => item.PublicRegionKey).IsUnique();
        builder.HasIndex(item => new { item.CountryCode, item.RegionTypeCode });
        builder.HasIndex(item => item.ParentRegionId);
        builder.HasOne(item => item.ParentRegion)
            .WithMany(item => item.ChildRegions)
            .HasForeignKey(item => item.ParentRegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class 지역농수산Map행정구역CodeAssignmentConfiguration
    : IEntityTypeConfiguration<지역농수산Map행정구역CodeAssignment>
{
    public void Configure(EntityTypeBuilder<지역농수산Map행정구역CodeAssignment> builder)
    {
        builder.HasIndex(item => new
        {
            item.SchemeCode,
            item.ExternalCode,
            item.SourceVintage
        }).IsUnique();
        builder.HasIndex(item => new { item.SchemeCode, item.ExternalCode });
        builder.HasIndex(item => item.RegionId);
        builder.HasOne(item => item.Region)
            .WithMany(item => item.CodeAssignments)
            .HasForeignKey(item => item.RegionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 지역농수산Map행정구역BoundaryConfiguration
    : IEntityTypeConfiguration<지역농수산Map행정구역Boundary>
{
    public void Configure(EntityTypeBuilder<지역농수산Map행정구역Boundary> builder)
    {
        builder.HasIndex(item => new
        {
            item.RegionId,
            item.BoundarySourceCode,
            item.BoundaryVintage,
            item.SimplificationLevel
        }).IsUnique();
        builder.HasOne(item => item.Region)
            .WithMany(item => item.Boundaries)
            .HasForeignKey(item => item.RegionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 지역농수산Map지역CrosswalkConfiguration
    : IEntityTypeConfiguration<지역농수산Map지역Crosswalk>
{
    public void Configure(EntityTypeBuilder<지역농수산Map지역Crosswalk> builder)
    {
        builder.HasIndex(item => new
        {
            item.SourceSchemeCode,
            item.SourceCode,
            item.SourceVintage
        }).IsUnique();
        builder.HasIndex(item => new { item.SourceSchemeCode, item.SourceCode });
        builder.HasOne(item => item.TargetRegion)
            .WithMany(item => item.IncomingCrosswalks)
            .HasForeignKey(item => item.TargetRegionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
