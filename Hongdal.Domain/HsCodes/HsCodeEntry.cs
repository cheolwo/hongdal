namespace Hongdal.Domain.HsCodes;

public enum HsCodeLevel
{
    Chapter = 2,
    Heading = 4,
    Subheading = 6,
    National = 10
}

public enum HsCodeBusinessCategory
{
    Unknown = 0,
    Food = 10,
    GeneralCargo = 20,
    Mixed = 30
}

public enum HsCodeRiskTagType
{
    Food = 10,
    FoodQuarantine = 20,
    SupplementOrPreparedFoodReview = 30,
    Textile = 40,
    Chemical = 50,
    ElectricalCertification = 60,
    BatteryIncludedPossible = 70,
    Furniture = 80,
    BrokerReviewRecommended = 900
}

public enum HsCodeRiskTagSource
{
    SystemRule = 10,
    AdminOverride = 20,
    BrokerReview = 30
}

public sealed class HsCodeEntry
{
    public long Id { get; set; }

    public long CatalogVersionId { get; set; }

    public HsCodeCatalogVersion? CatalogVersion { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NormalizedCode { get; set; } = string.Empty;

    public string? ParentNormalizedCode { get; set; }

    public HsCodeLevel Level { get; set; }

    public string KoreanName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SearchKeywords { get; set; } = string.Empty;

    public HsCodeBusinessCategory BusinessCategory { get; set; } = HsCodeBusinessCategory.Unknown;

    public string BusinessCategoryReason { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<HsCodeEntryRiskTag> RiskTags { get; set; } = new List<HsCodeEntryRiskTag>();
}
