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

    public DbSet<공통식품품목Identity> CommonFoodProductIdentities =>
        Set<공통식품품목Identity>();

    public DbSet<공통식품품목Code관계> CommonFoodProductCodeRelations =>
        Set<공통식품품목Code관계>();

    public DbSet<공통식품품목Code관계검토이력> CommonFoodProductCodeRelationReviews =>
        Set<공통식품품목Code관계검토이력>();

    public DbSet<KamisPriceCollectionRun> KamisCollectionRuns => Set<KamisPriceCollectionRun>();

    public DbSet<KamisPriceObservation> KamisPriceObservations => Set<KamisPriceObservation>();

    public DbSet<Bls평균소매가격수집Run> BlsAverageRetailPriceCollectionRuns =>
        Set<Bls평균소매가격수집Run>();

    public DbSet<Bls평균소매가격관측> BlsAverageRetailPriceObservations =>
        Set<Bls평균소매가격관측>();

    public DbSet<국제농수산가격수집Run> InternationalPriceCollectionRuns =>
        Set<국제농수산가격수집Run>();

    public DbSet<국제농수산가격관측> InternationalPriceObservations =>
        Set<국제농수산가격관측>();

    public DbSet<UsdaAms시장가격수집Run> UsdaAmsMarketPriceCollectionRuns =>
        Set<UsdaAms시장가격수집Run>();

    public DbSet<UsdaAms시장가격관측> UsdaAmsMarketPriceObservations =>
        Set<UsdaAms시장가격관측>();

    public DbSet<UsdaAms연도상품Catalog> UsdaAmsYearCommodityCatalog =>
        Set<UsdaAms연도상품Catalog>();

    public DbSet<UsdaAms공개사업체수집Run> UsdaAmsPublicBusinessCollectionRuns =>
        Set<UsdaAms공개사업체수집Run>();

    public DbSet<UsdaAms공개사업체Profile> UsdaAmsPublicBusinessProfiles =>
        Set<UsdaAms공개사업체Profile>();

    public DbSet<UsdaAms공개사업체취급품목> UsdaAmsPublicBusinessProducts =>
        Set<UsdaAms공개사업체취급품목>();

    public DbSet<주간국가농수산물비교Snapshot> WeeklyCountryProductComparisonSnapshots =>
        Set<주간국가농수산물비교Snapshot>();

    public DbSet<주간국가농수산물비교항목> WeeklyCountryProductComparisonItems =>
        Set<주간국가농수산물비교항목>();

    public DbSet<농수산물포장Fcl분석Snapshot> PackagingFclAnalysisSnapshots =>
        Set<농수산물포장Fcl분석Snapshot>();

    public DbSet<국내농산물경락가격수집Run> DomesticAuctionPriceCollectionRuns =>
        Set<국내농산물경락가격수집Run>();

    public DbSet<국내농산물경락가격관측> DomesticAuctionPriceObservations =>
        Set<국내농산물경락가격관측>();

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
        modelBuilder.ApplyConfiguration(new 공통식품품목IdentityConfiguration());
        modelBuilder.ApplyConfiguration(new 공통식품품목Code관계Configuration());
        modelBuilder.ApplyConfiguration(new 공통식품품목Code관계검토이력Configuration());
        modelBuilder.ApplyConfiguration(new KamisPriceCollectionRunConfiguration());
        modelBuilder.ApplyConfiguration(new KamisPriceObservationConfiguration());
        modelBuilder.ApplyConfiguration(new Bls평균소매가격수집RunConfiguration());
        modelBuilder.ApplyConfiguration(new Bls평균소매가격관측Configuration());
        modelBuilder.ApplyConfiguration(new 국제농수산가격수집RunConfiguration());
        modelBuilder.ApplyConfiguration(new 국제농수산가격관측Configuration());
        modelBuilder.ApplyConfiguration(new UsdaAms시장가격수집RunConfiguration());
        modelBuilder.ApplyConfiguration(new UsdaAms시장가격관측Configuration());
        modelBuilder.ApplyConfiguration(new UsdaAms연도상품CatalogConfiguration());
        modelBuilder.ApplyConfiguration(new UsdaAms공개사업체수집RunConfiguration());
        modelBuilder.ApplyConfiguration(new UsdaAms공개사업체ProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UsdaAms공개사업체취급품목Configuration());
        modelBuilder.ApplyConfiguration(new 주간국가농수산물비교SnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new 주간국가농수산물비교항목Configuration());
        modelBuilder.ApplyConfiguration(new 농수산물포장Fcl분석SnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new 국내농산물경락가격수집RunConfiguration());
        modelBuilder.ApplyConfiguration(new 국내농산물경락가격관측Configuration());
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
