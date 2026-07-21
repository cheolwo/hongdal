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
    bool IsFreshForPublication);

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
