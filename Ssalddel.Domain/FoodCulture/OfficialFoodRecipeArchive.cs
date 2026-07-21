namespace Ssalddel.Domain.FoodCulture;

public static class OfficialFoodRecipeCollectionStatuses
{
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public sealed class OfficialFoodRecipeSource
{
    public long Id { get; set; }

    public string SourceKey { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string LanguageCode { get; set; } = string.Empty;

    public string AccessMethod { get; set; } = string.Empty;

    public string DocumentationUrl { get; set; } = string.Empty;

    public string TermsUrl { get; set; } = string.Empty;

    public string LicenseCode { get; set; } = string.Empty;

    public string TextReusePolicy { get; set; } = string.Empty;

    public string ImageReusePolicy { get; set; } = string.Empty;

    public string AttributionTemplate { get; set; } = string.Empty;

    public string UpdateCycle { get; set; } = string.Empty;

    public string AutomationState { get; set; } = string.Empty;

    public bool FullTextStorageAllowed { get; set; }

    public bool ImageBinaryStorageAllowed { get; set; }

    public bool RequiresEditorialReview { get; set; } = true;

    public DateTime RightsVerifiedAtUtc { get; set; }

    public DateTime? LastCollectedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OfficialFoodRecipeVariant> RecipeVariants { get; set; } =
        new List<OfficialFoodRecipeVariant>();
}

public sealed class OfficialFoodDish
{
    public long Id { get; set; }

    public string DishKey { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string RegionName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string OriginalName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string RepresentationState { get; set; } = "Candidate";

    public string ReviewState { get; set; } = "PendingReview";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OfficialFoodRecipeVariant> RecipeVariants { get; set; } =
        new List<OfficialFoodRecipeVariant>();
}

public sealed class OfficialFoodRecipeVariant
{
    public long Id { get; set; }

    public long SourceId { get; set; }

    public OfficialFoodRecipeSource? Source { get; set; }

    public long DishId { get; set; }

    public OfficialFoodDish? Dish { get; set; }

    public long FirstCollectionRunId { get; set; }

    public OfficialFoodRecipeCollectionRun? FirstCollectionRun { get; set; }

    public string RecordKey { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string RegionName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ServingText { get; set; } = string.Empty;

    public string IngredientsJson { get; set; } = "[]";

    public string InstructionsJson { get; set; } = "[]";

    public string NutritionJson { get; set; } = "{}";

    public string TagsJson { get; set; } = "[]";

    public string Tips { get; set; } = string.Empty;

    public string OriginalUrl { get; set; } = string.Empty;

    public string ImageReferenceUrl { get; set; } = string.Empty;

    public string RawPayload { get; set; } = string.Empty;

    public string ContentChecksum { get; set; } = string.Empty;

    public string LicenseCodeAtCollection { get; set; } = string.Empty;

    public string TextReusePolicyAtCollection { get; set; } = string.Empty;

    public string ImageReusePolicyAtCollection { get; set; } = string.Empty;

    public string AttributionText { get; set; } = string.Empty;

    public DateTime? SourceModifiedAtUtc { get; set; }

    public DateTime FirstCollectedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastCollectedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ContentExpiresAtUtc { get; set; }

    public bool IsRemovedAtSource { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class OfficialFoodRecipeCollectionRun
{
    public long Id { get; set; }

    public string RunKey { get; set; } = Guid.NewGuid().ToString("N");

    public string SourceKey { get; set; } = string.Empty;

    public string StatusCode { get; set; } = OfficialFoodRecipeCollectionStatuses.Running;

    public string QuerySummary { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public int FetchedCount { get; set; }

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int ExistingCount { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public ICollection<OfficialFoodRecipeVariant> NewRecipeVariants { get; set; } =
        new List<OfficialFoodRecipeVariant>();
}
