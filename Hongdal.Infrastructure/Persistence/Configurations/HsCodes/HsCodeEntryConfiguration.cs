using Hongdal.Domain.HsCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.HsCodes;

public sealed class HsCodeEntryConfiguration : IEntityTypeConfiguration<HsCodeEntry>
{
    public void Configure(EntityTypeBuilder<HsCodeEntry> builder)
    {
        builder.ToTable("hs_code_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.NormalizedCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ParentNormalizedCode).HasMaxLength(30);
        builder.Property(x => x.Level).HasConversion<int>();
        builder.Property(x => x.KoreanName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.EnglishName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.SearchKeywords).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.BusinessCategory).HasConversion<int>();
        builder.Property(x => x.BusinessCategoryReason).HasMaxLength(1000).IsRequired();

        builder.HasOne(x => x.CatalogVersion)
            .WithMany(x => x.Entries)
            .HasForeignKey(x => x.CatalogVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CatalogVersionId, x.NormalizedCode })
            .IsUnique();

        builder.HasIndex(x => new { x.CatalogVersionId, x.ParentNormalizedCode });
        builder.HasIndex(x => new { x.CatalogVersionId, x.BusinessCategory, x.IsActive });
        builder.HasIndex(x => new { x.NormalizedCode, x.IsActive });
        builder.HasIndex(x => x.KoreanName);
    }
}
