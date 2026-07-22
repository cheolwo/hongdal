using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Domain.FoodCulture;
using Microsoft.EntityFrameworkCore;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

public sealed class AgriculturalFisheriesDbContext : DbContext
{
    public AgriculturalFisheriesDbContext(DbContextOptions<AgriculturalFisheriesDbContext> options)
        : base(options)
    {
    }

    public DbSet<UsdaNassPriceCollectionRun> CollectionRuns => Set<UsdaNassPriceCollectionRun>();

    public DbSet<UsdaNassPriceObservation> PriceObservations => Set<UsdaNassPriceObservation>();

    public DbSet<HsUsdaCommodityMapping> HsCommodityMappings => Set<HsUsdaCommodityMapping>();

    public DbSet<KamisPriceCollectionRun> KamisCollectionRuns => Set<KamisPriceCollectionRun>();

    public DbSet<KamisPriceObservation> KamisPriceObservations => Set<KamisPriceObservation>();

    public DbSet<OfficialFoodRecipeSource> OfficialFoodRecipeSources => Set<OfficialFoodRecipeSource>();

    public DbSet<OfficialFoodDish> OfficialFoodDishes => Set<OfficialFoodDish>();

    public DbSet<OfficialFoodRecipeVariant> OfficialFoodRecipeVariants => Set<OfficialFoodRecipeVariant>();

    public DbSet<OfficialFoodRecipeCollectionRun> OfficialFoodRecipeCollectionRuns =>
        Set<OfficialFoodRecipeCollectionRun>();

    public DbSet<OfficialFoodIngredientCategory> OfficialFoodIngredientCategories =>
        Set<OfficialFoodIngredientCategory>();

    public DbSet<OfficialFoodIngredient> OfficialFoodIngredients => Set<OfficialFoodIngredient>();

    public DbSet<OfficialFoodIngredientPriceMapping> OfficialFoodIngredientPriceMappings =>
        Set<OfficialFoodIngredientPriceMapping>();

    public DbSet<OfficialFoodIngredientHsMapping> OfficialFoodIngredientHsMappings =>
        Set<OfficialFoodIngredientHsMapping>();

    public DbSet<OfficialFoodRecipeIngredient> OfficialFoodRecipeIngredients =>
        Set<OfficialFoodRecipeIngredient>();

    public DbSet<OfficialFoodIngredientCompanyResearchRun>
        OfficialFoodIngredientCompanyResearchRuns =>
        Set<OfficialFoodIngredientCompanyResearchRun>();

    public DbSet<OfficialFoodIngredientCompanyProfile> OfficialFoodIngredientCompanyProfiles =>
        Set<OfficialFoodIngredientCompanyProfile>();

    public DbSet<OfficialFoodIngredientCompanyEvidence> OfficialFoodIngredientCompanyEvidence =>
        Set<OfficialFoodIngredientCompanyEvidence>();

    public DbSet<OfficialFoodIngredientCompanySourceObservation>
        OfficialFoodIngredientCompanySourceObservations =>
        Set<OfficialFoodIngredientCompanySourceObservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsdaNassPriceCollectionRunConfiguration());
        modelBuilder.ApplyConfiguration(new UsdaNassPriceObservationConfiguration());
        modelBuilder.ApplyConfiguration(new HsUsdaCommodityMappingConfiguration());
        modelBuilder.ApplyConfiguration(new KamisPriceCollectionRunConfiguration());
        modelBuilder.ApplyConfiguration(new KamisPriceObservationConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodRecipeSourceConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodDishConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodRecipeVariantConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodRecipeCollectionRunConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodIngredientCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodIngredientConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodIngredientPriceMappingConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodIngredientHsMappingConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodRecipeIngredientConfiguration());
        modelBuilder.ApplyConfiguration(
            new OfficialFoodIngredientCompanyResearchRunConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodIngredientCompanyProfileConfiguration());
        modelBuilder.ApplyConfiguration(new OfficialFoodIngredientCompanyEvidenceConfiguration());
        modelBuilder.ApplyConfiguration(
            new OfficialFoodIngredientCompanySourceObservationConfiguration());
    }
}
