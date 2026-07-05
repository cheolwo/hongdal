using Hongdal.Domain.HsCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.HsCodes;

public sealed class HsCodeClassificationCaseConfiguration : IEntityTypeConfiguration<HsCodeClassificationCase>
{
    public void Configure(EntityTypeBuilder<HsCodeClassificationCase> builder)
    {
        builder.ToTable("hs_code_classification_cases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.HsCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.SourceType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceReferenceNo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IssuingAuthority).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.GoodsDescription).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.DecisionReason).HasMaxLength(4000).IsRequired();

        builder.HasOne(x => x.HsCodeEntry)
            .WithMany()
            .HasForeignKey(x => x.HsCodeEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.CountryCode, x.HsCode, x.DecidedAt });
        builder.HasIndex(x => new { x.SourceType, x.SourceReferenceNo });
        builder.HasIndex(x => x.ProductName);
    }
}
