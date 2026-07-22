namespace Ssalddel.Contracts.Common.Content;

public static class OfficialFoodIngredientHsMappingStates
{
    public const string Candidate = "Candidate";
    public const string Confirmed = "Confirmed";
    public const string Rejected = "Rejected";
    public const string Superseded = "Superseded";
}

public static class OfficialFoodIngredientHsMatchQualityCodes
{
    public const string ExactCatalogNameCandidate = "ExactCatalogNameCandidate";
    public const string CuratedHsFamilyCandidate = "CuratedHsFamilyCandidate";
    public const string CatalogTextCandidate = "CatalogTextCandidate";
}

public static class OfficialFoodIngredientHsJurisdictionUseCodes
{
    public const string InternationalHsReference = "InternationalHsReference";
    public const string KoreaExportDeclaration = "KoreaExportDeclaration";
    public const string UnitedStatesImportEntry = "UnitedStatesImportEntry";
    public const string NationalReference = "NationalReference";
}

public sealed class OfficialFoodIngredientHsQuery
{
    public string? IngredientKey { get; init; }

    public string? IngredientName { get; init; }

    public string? CountryCode { get; init; }

    public bool Refresh { get; init; }
}

public sealed record OfficialFoodIngredientHsCandidateDto(
    long MappingId,
    long HsCodeEntryId,
    string CountryCode,
    string JurisdictionUseCode,
    string StandardCode,
    string CatalogRevision,
    int CodeDigits,
    string HsCode,
    string NormalizedHsCode,
    int HsCodeLevel,
    string KoreanName,
    string EnglishName,
    string Description,
    string MatchMethod,
    string MatchQualityCode,
    decimal MatchConfidence,
    string MappingState,
    string MatchBasis,
    string ReviewReason,
    IReadOnlyList<string> RequiredProductDetails,
    string SourceName,
    string SourceUrl,
    DateTime CatalogEffectiveFrom,
    DateTime? CatalogEffectiveTo,
    DateTime CatalogImportedAtUtc,
    DateTime LastCheckedAtUtc,
    bool RequiresProfessionalReview,
    bool IsDeclarationReady);

public sealed record OfficialFoodIngredientHsMappingResponse(
    string IngredientKey,
    string IngredientName,
    string? CountryCode,
    bool HasActiveCatalog,
    DateTime GeneratedAtUtc,
    IReadOnlyList<OfficialFoodIngredientHsCandidateDto> Candidates,
    IReadOnlyList<string> Notices);

public sealed record OfficialFoodIngredientHsIndexRequest(
    int MaxItems = 5000,
    bool Force = false,
    IReadOnlyList<string>? CountryCodes = null);

public sealed record OfficialFoodIngredientHsIndexResponse(
    int ProcessedIngredientCount,
    int MappedIngredientCount,
    int CandidateCount,
    int UnmappedIngredientCount,
    int ActiveCatalogVersionCount,
    int ActiveCatalogEntryCount,
    IReadOnlyDictionary<string, int> CountryCandidateCounts,
    DateTime CompletedAtUtc);
