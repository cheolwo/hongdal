namespace Ssalddel.Domain.FoodCulture;

public sealed class OfficialFoodIngredientHsMapping
{
    public long Id { get; set; }

    public long IngredientId { get; set; }

    public OfficialFoodIngredient? Ingredient { get; set; }

    public long HsCodeCatalogVersionId { get; set; }

    public long HsCodeEntryId { get; set; }

    public string CountryCode { get; set; } = string.Empty;

    public string JurisdictionUseCode { get; set; } = string.Empty;

    public string StandardCode { get; set; } = string.Empty;

    public string CatalogRevision { get; set; } = string.Empty;

    public int CodeDigits { get; set; }

    public DateTime CatalogEffectiveFrom { get; set; }

    public DateTime? CatalogEffectiveTo { get; set; }

    public DateTime CatalogImportedAtUtc { get; set; }

    public string HsCode { get; set; } = string.Empty;

    public string NormalizedHsCode { get; set; } = string.Empty;

    public int HsCodeLevel { get; set; }

    public string KoreanName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MatchMethod { get; set; } = string.Empty;

    public string MatchQualityCode { get; set; } = string.Empty;

    public decimal MatchConfidence { get; set; }

    public string MappingState { get; set; } = "Candidate";

    public string MatchBasis { get; set; } = string.Empty;

    public string ReviewReason { get; set; } = string.Empty;

    public string RequiredProductDetailsJson { get; set; } = "[]";

    public string SourceName { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public bool RequiresProfessionalReview { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastCheckedAtUtc { get; set; } = DateTime.UtcNow;
}
