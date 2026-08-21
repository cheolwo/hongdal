using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class Nongsaro감자ProfileArchiveConfiguration
    : IEntityTypeConfiguration<Nongsaro감자ProfileArchive>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<Nongsaro감자ProfileArchive> builder)
    {
        builder.ToTable("agri_nongsaro_potato_profiles");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.StableId, item.Revision }).IsUnique();
        builder.HasIndex(item => new
        {
            item.CanonicalProductStableId,
            item.ApprovedForSimulationContext,
            item.RetrievedAtUtc
        });
        builder.Property(item => item.StableId).HasMaxLength(160).IsRequired();
        builder.Property(item => item.CanonicalProductStableId).HasMaxLength(120).IsRequired();
        builder.Property(item => item.WorkScheduleGroupCode).HasMaxLength(40).IsRequired();
        builder.Property(item => item.WorkScheduleContentNo).HasMaxLength(40).IsRequired();
        builder.Property(item => item.ProductRelationStatusCode).HasMaxLength(40).IsRequired();
        builder.Property(item => item.ReviewStatusCode).HasMaxLength(40).IsRequired();
        builder.Property(item => item.ProfileJson).HasColumnType("longtext").IsRequired();
        builder.Property(item => item.SourceSetHashSha256).HasMaxLength(64).IsRequired();
        builder.Property(item => item.DisasterPreventionHashSha256).HasMaxLength(64).IsRequired();
    }
}
