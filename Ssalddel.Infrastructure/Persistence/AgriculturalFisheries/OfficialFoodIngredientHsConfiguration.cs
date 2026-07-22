using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.FoodCulture;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class OfficialFoodIngredientHsMappingConfiguration
    : IEntityTypeConfiguration<OfficialFoodIngredientHsMapping>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodIngredientHsMapping> builder)
    {
        builder.ToTable("food_official_ingredient_hs_mappings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.JurisdictionUseCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StandardCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CatalogRevision).HasMaxLength(50).IsRequired();
        builder.Property(x => x.HsCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.NormalizedHsCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.KoreanName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.EnglishName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text").IsRequired();
        builder.Property(x => x.MatchMethod).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MatchQualityCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MatchConfidence).HasPrecision(5, 4);
        builder.Property(x => x.MappingState).HasMaxLength(30).IsRequired();
        builder.Property(x => x.MatchBasis).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ReviewReason).HasColumnType("text").IsRequired();
        builder.Property(x => x.RequiredProductDetailsJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.SourceName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();

        builder.HasOne(x => x.Ingredient)
            .WithMany()
            .HasForeignKey(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.IngredientId,
            x.HsCodeCatalogVersionId,
            x.HsCodeEntryId
        }).IsUnique();
        builder.HasIndex(x => new { x.IngredientId, x.CountryCode, x.IsActive });
        builder.HasIndex(x => new { x.MappingState, x.IsActive, x.LastCheckedAtUtc });
        builder.HasIndex(x => new { x.NormalizedHsCode, x.CountryCode, x.IsActive });
    }
}
