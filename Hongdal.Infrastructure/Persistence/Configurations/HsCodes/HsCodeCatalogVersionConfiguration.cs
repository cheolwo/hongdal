using Hongdal.Domain.HsCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.HsCodes;

public sealed class HsCodeCatalogVersionConfiguration : IEntityTypeConfiguration<HsCodeCatalogVersion>
{
    public void Configure(EntityTypeBuilder<HsCodeCatalogVersion> builder)
    {
        builder.ToTable("hs_code_catalog_versions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StandardCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Revision).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SourceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000).IsRequired();

        builder.HasIndex(x => new { x.StandardCode, x.CountryCode, x.Revision, x.CodeDigits })
            .IsUnique();

        builder.HasIndex(x => new { x.CountryCode, x.IsActive, x.EffectiveFrom });
    }
}
