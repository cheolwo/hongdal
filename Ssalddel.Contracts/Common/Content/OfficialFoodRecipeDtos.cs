namespace Ssalddel.Contracts.Common.Content;

public static class OfficialFoodRecipeSourceKeys
{
    public const string MfdsCookRecipe = "mfds-cookrcp01";
    public const string RdaLocalFood = "rda-local-food";
    public const string MaffRegionalCuisine = "maff-regional-cuisines";
    public const string NhsHealthierFamilies = "nhs-healthier-families-recipes";
    public const string UsdaMyPlate = "usda-myplate-recipes";
    public const string HealthCanada = "health-canada-recipes";
    public const string FranceAgriculture = "france-agriculture-recipes";
}

public static class OfficialFoodRecipeAutomationStates
{
    public const string EnabledWhenConfigured = "EnabledWhenConfigured";
    public const string Enabled = "Enabled";
    public const string MetadataOnly = "MetadataOnly";
}

public static class OfficialFoodRecipeRepresentationStates
{
    public const string Candidate = "Candidate";
    public const string Representative = "Representative";
    public const string Excluded = "Excluded";
}

public static class OfficialFoodRecipeReviewStates
{
    public const string PendingReview = "PendingReview";
    public const string Approved = "Approved";
    public const string Excluded = "Excluded";
}

public static class OfficialFoodRecipeImagePolicies
{
    public const string ReferenceOnly = "ReferenceOnly";
    public const string Blocked = "Blocked";
}

public static class OfficialFoodIngredientCategoryCodes
{
    public const string GrainAndStarch = "grain-starch";
    public const string LegumeAndSoy = "legume-soy";
    public const string Vegetable = "vegetable";
    public const string Fruit = "fruit";
    public const string Mushroom = "mushroom";
    public const string Seaweed = "seaweed";
    public const string Meat = "meat";
    public const string PoultryAndEgg = "poultry-egg";
    public const string Seafood = "seafood";
    public const string Dairy = "dairy";
    public const string NutAndSeed = "nut-seed";
    public const string OilAndFat = "oil-fat";
    public const string SeasoningAndSpice = "seasoning-spice";
    public const string SauceAndFermented = "sauce-fermented";
    public const string ProcessedFood = "processed-food";
    public const string BeverageAndAlcohol = "beverage-alcohol";
    public const string WaterAndStock = "water-stock";
    public const string Other = "other";
}

public static class OfficialFoodIngredientClassificationStates
{
    public const string AutoClassified = "AutoClassified";
    public const string PendingReview = "PendingReview";
    public const string Confirmed = "Confirmed";
}

public static class OfficialFoodIngredientPublicPriceSourceKeys
{
    public const string Kamis = "kamis-price-observations";

    public const string UsdaNass = "usda-nass-price-observations";
}

public static class OfficialFoodIngredientPriceMappingStates
{
    public const string AutoMatched = "AutoMatched";

    public const string Confirmed = "Confirmed";
}

public static class OfficialFoodIngredientPriceMarketStages
{
    public const string Retail = "Retail";

    public const string Wholesale = "Wholesale";

    public const string ProducerReceived = "ProducerReceived";
}

public sealed record OfficialFoodRecipeSourceDto(
    string SourceKey,
    string Provider,
    string DisplayName,
    string CountryCode,
    string LanguageCode,
    string AccessMethod,
    string DocumentationUrl,
    string TermsUrl,
    string LicenseCode,
    string TextReusePolicy,
    string ImageReusePolicy,
    string AttributionTemplate,
    string UpdateCycle,
    string AutomationState,
    bool FullTextStorageAllowed,
    bool ImageBinaryStorageAllowed,
    bool RequiresEditorialReview,
    DateTime RightsVerifiedAtUtc,
    DateTime? LastCollectedAtUtc);

public sealed class OfficialFoodRecipeQuery
{
    public string? SourceKey { get; init; }

    public string? CountryCode { get; init; }

    public string? RegionName { get; init; }

    public string? ReviewState { get; init; }

    public string? SearchText { get; init; }

    public int Take { get; init; } = 50;
}

public sealed record OfficialFoodRecipeDishDto(
    string DishKey,
    string CountryCode,
    string RegionName,
    string Name,
    string OriginalName,
    string EnglishName,
    string Category,
    string Summary,
    string RepresentationState,
    string ReviewState,
    int VariantCount,
    DateTime UpdatedAtUtc);

public sealed record OfficialFoodRecipeVariantDto(
    string RecordKey,
    string DishKey,
    string SourceKey,
    string Provider,
    string ExternalId,
    string Title,
    string Summary,
    string RegionName,
    string Category,
    string ServingText,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> Instructions,
    IReadOnlyDictionary<string, string> Nutrition,
    IReadOnlyList<string> Tags,
    string Tips,
    string OriginalUrl,
    string? ImageReferenceUrl,
    string ImageReusePolicy,
    string LicenseCode,
    string AttributionText,
    DateTime CollectedAtUtc,
    DateTime? ContentExpiresAtUtc,
    bool IsFreshForPublication,
    IReadOnlyList<OfficialFoodRecipeIngredientDto>? StructuredIngredients = null);

public sealed record OfficialFoodIngredientCategoryDto(
    string CategoryCode,
    string KoreanName,
    string EnglishName,
    string Description,
    int SortOrder,
    int IngredientCount);

public sealed class OfficialFoodIngredientQuery
{
    public string? CategoryCode { get; init; }

    public string? LanguageCode { get; init; }

    public string? ClassificationState { get; init; }

    public string? SearchText { get; init; }

    public int Take { get; init; } = 100;
}

public sealed record OfficialFoodIngredientDto(
    string IngredientKey,
    string LanguageCode,
    string CanonicalName,
    string NormalizedName,
    string CategoryCode,
    string CategoryName,
    string ClassificationMethod,
    decimal ClassificationConfidence,
    string ClassificationState,
    int RecipeVariantCount,
    DateTime UpdatedAtUtc,
    IReadOnlyList<OfficialFoodIngredientPublicPriceDto>? PublicPrices = null);

public sealed record OfficialFoodRecipeIngredientDto(
    string IngredientKey,
    string CanonicalName,
    string CategoryCode,
    string CategoryName,
    string GroupName,
    string OriginalText,
    string SourceName,
    string QuantityText,
    decimal? QuantityValue,
    decimal? QuantityMaxValue,
    string UnitCode,
    string UnitText,
    string HouseholdMeasureText,
    string PreparationNote,
    int DisplayOrder,
    string ParserVersion,
    decimal ParseConfidence,
    bool RequiresReview,
    IReadOnlyList<OfficialFoodIngredientPublicPriceDto>? PublicPrices = null);

public sealed record OfficialFoodIngredientPublicPriceDto(
    string CountryCode,
    string CountryName,
    string SourceKey,
    string Provider,
    string MarketStageCode,
    string MarketStageName,
    string CommodityName,
    string VarietyOrClass,
    decimal AveragePrice,
    decimal MinimumPrice,
    decimal MaximumPrice,
    string CurrencyCode,
    string Unit,
    DateOnly ReferenceDate,
    string ReferencePeriod,
    string RegionName,
    string FrequencyCode,
    int SampleCount,
    string MatchQualityCode,
    string MappingNote,
    string SourceUrl,
    DateTime DataCollectedAtUtc,
    bool IsDirectlyComparableAcrossCountries = false);

public sealed record OfficialFoodRecipeCollectionRequest(
    string SourceKey,
    int MaxPages = 1,
    int MaxItems = 100);

public sealed record OfficialFoodRecipeCollectionResponse(
    long CollectionRunId,
    string SourceKey,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc);

public sealed record OfficialFoodIngredientIndexRequest(
    string? SourceKey = null,
    int MaxItems = 5000,
    bool Force = false);

public sealed record OfficialFoodIngredientIndexResponse(
    string? SourceKey,
    int ProcessedRecipeVariantCount,
    int RecipeIngredientCount,
    int CatalogIngredientCount,
    int PendingReviewIngredientCount,
    IReadOnlyDictionary<string, int> CategoryCounts,
    DateTime CompletedAtUtc);

public sealed record OfficialFoodIngredientPriceIndexRequest(
    int MaxItems = 5000,
    bool Force = false);

public sealed record OfficialFoodIngredientPriceIndexResponse(
    int ProcessedIngredientCount,
    int MappedIngredientCount,
    int MappingCount,
    int KoreanMappingCount,
    int UnitedStatesMappingCount,
    int UnmappedIngredientCount,
    int PricedIngredientCount,
    int KoreanPriceCount,
    int UnitedStatesPriceCount,
    DateTime CompletedAtUtc);
