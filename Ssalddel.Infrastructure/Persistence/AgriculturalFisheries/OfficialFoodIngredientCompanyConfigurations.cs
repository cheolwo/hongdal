using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.FoodCulture;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class OfficialFoodIngredientCompanyResearchRunConfiguration
    : IEntityTypeConfiguration<OfficialFoodIngredientCompanyResearchRun>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodIngredientCompanyResearchRun> builder)
    {
        builder.ToTable("food_ingredient_company_research_runs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RunKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.TriggerCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ErrorMessage).HasColumnType("text").IsRequired();

        builder.HasIndex(x => x.RunKey).IsUnique();
        builder.HasIndex(x => new { x.TriggerCode, x.StatusCode, x.StartedAtUtc });
    }
}

internal sealed class OfficialFoodIngredientCompanyProfileConfiguration
    : IEntityTypeConfiguration<OfficialFoodIngredientCompanyProfile>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodIngredientCompanyProfile> builder)
    {
        builder.ToTable("food_ingredient_company_profiles");
        builder.HasKey(x => x.IngredientId);

        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ResearchQueryTerm).HasMaxLength(300).IsRequired();

        builder.HasOne(x => x.Ingredient)
            .WithOne(x => x.CompanyResearchProfile)
            .HasForeignKey<OfficialFoodIngredientCompanyProfile>(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.LastResearchRun)
            .WithMany()
            .HasForeignKey(x => x.LastResearchRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.StatusCode, x.LastResearchedAtUtc });
        builder.HasIndex(x => x.LastResearchRunId);
    }
}

internal sealed class OfficialFoodIngredientCompanyEvidenceConfiguration
    : IEntityTypeConfiguration<OfficialFoodIngredientCompanyEvidence>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodIngredientCompanyEvidence> builder)
    {
        builder.ToTable("food_ingredient_company_evidence");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CandidateKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OrganizationKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OrganizationName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.NormalizedOrganizationName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.CountryName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.RelationCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.EvidenceCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EvidenceSummary).HasColumnType("text").IsRequired();
        builder.Property(x => x.RelatedProductName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ProductCategory).HasMaxLength(300).IsRequired();
        builder.Property(x => x.OfficialIdentifier).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EvidenceRecordIdentifier).HasMaxLength(200).IsRequired();
        builder.Property(x => x.VerificationStatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RawIngredientText).HasColumnType("text").IsRequired();
        builder.Property(x => x.EvidenceDate).HasMaxLength(40).IsRequired();
        builder.Property(x => x.EvidenceLastChangedDate).HasMaxLength(40).IsRequired();
        builder.Property(x => x.EvidenceSequence).HasMaxLength(80).IsRequired();
        builder.Property(x => x.AttentionReason).HasColumnType("text").IsRequired();
        builder.Property(x => x.SourceKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ResearchQueryTerm).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ManufacturerRegionCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ManufacturerRegionName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ManufacturerRegionScope).HasMaxLength(800).IsRequired();
        builder.Property(x => x.ManufacturerRegionClassificationMethod)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.ManufacturerRegionEvidence).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ManufacturerRegionConfidence).HasPrecision(5, 4);

        builder.HasOne(x => x.Ingredient)
            .WithMany(x => x.CompanyEvidence)
            .HasForeignKey(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.LastResearchRun)
            .WithMany()
            .HasForeignKey(x => x.LastResearchRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.IngredientId, x.CandidateKey }).IsUnique();
        builder.HasIndex(x => new { x.IngredientId, x.IsCurrent, x.RelationCode });
        builder.HasIndex(x => new { x.IngredientId, x.OrganizationKey, x.IsCurrent });
        builder.HasIndex(x => new { x.CountryCode, x.RelationCode, x.IsCurrent });
        builder.HasIndex(x => new { x.CountryCode, x.ManufacturerRegionCode, x.IsCurrent });
        builder.HasIndex(x => new { x.SourceKey, x.IsCurrent, x.LastObservedAtUtc });
        builder.HasIndex(x => x.LastResearchRunId);
    }
}

internal sealed class OfficialFoodIngredientCompanySourceObservationConfiguration
    : IEntityTypeConfiguration<OfficialFoodIngredientCompanySourceObservation>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodIngredientCompanySourceObservation> builder)
    {
        builder.ToTable("food_ingredient_company_source_observations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(300).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.CountryScope).HasMaxLength(300).IsRequired();
        builder.Property(x => x.OfficialUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StatusMessage).HasColumnType("text").IsRequired();

        builder.HasOne(x => x.ResearchRun)
            .WithMany()
            .HasForeignKey(x => x.ResearchRunId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Ingredient)
            .WithMany(x => x.CompanySourceObservations)
            .HasForeignKey(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ResearchRunId, x.IngredientId, x.SourceKey }).IsUnique();
        builder.HasIndex(x => new { x.IngredientId, x.ObservedAtUtc });
        builder.HasIndex(x => new { x.SourceKey, x.StatusCode, x.ObservedAtUtc });
    }
}
