using Ssalddel.Domain.HsCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.Configurations.HsCodes;

public sealed class HsCodeEntryRiskTagConfiguration : IEntityTypeConfiguration<HsCodeEntryRiskTag>
{
    public void Configure(EntityTypeBuilder<HsCodeEntryRiskTag> builder)
    {
        builder.ToTable("hs_code_entry_risk_tags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TagType).HasConversion<int>();
        builder.Property(x => x.Source).HasConversion<int>();
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();

        builder.HasOne(x => x.HsCodeEntry)
            .WithMany(x => x.RiskTags)
            .HasForeignKey(x => x.HsCodeEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.HsCodeEntryId, x.TagType, x.IsActive });
        builder.HasIndex(x => new { x.TagType, x.IsActive });
    }
}
