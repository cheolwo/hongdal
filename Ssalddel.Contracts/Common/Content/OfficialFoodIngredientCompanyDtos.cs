namespace Ssalddel.Contracts.Common.Content;

public static class OfficialFoodIngredientCompanyRelationCodes
{
    public const string DomesticManufacturer = "DomesticManufacturer";

    public const string DomesticImporter = "DomesticImporter";

    public const string ForeignManufacturer = "ForeignManufacturer";
}

public static class OfficialFoodIngredientCompanyEvidenceCodes
{
    public const string DomesticProductIngredientReport = "DomesticProductIngredientReport";

    public const string ImportedProductIngredientLabel = "ImportedProductIngredientLabel";
}

public static class OfficialFoodIngredientCompanyVerificationStatusCodes
{
    public const string OfficialProductReport = "OfficialProductReport";

    public const string ImportedLabelEvidenceOnly = "ImportedLabelEvidenceOnly";

    public const string OverseasFacilityMatched = "OverseasFacilityMatched";
}

public static class OfficialFoodIngredientCompanySourceStatusCodes
{
    public const string Available = "Available";

    public const string NotConfigured = "NotConfigured";

    public const string Failed = "Failed";

    public const string SupportingSource = "SupportingSource";
}

public static class OfficialFoodIngredientCompanyResearchStatusCodes
{
    public const string Available = "Available";

    public const string Partial = "Partial";

    public const string NotConfigured = "NotConfigured";

    public const string NoResults = "NoResults";

    public const string Failed = "Failed";
}

public static class OfficialFoodIngredientCompanyResearchTriggerCodes
{
    public const string Manual = "Manual";

    public const string Batch = "Batch";

    public const string Scheduled = "Scheduled";
}

public static class OfficialFoodIngredientCompanyResearchRunStatusCodes
{
    public const string Running = "Running";

    public const string Completed = "Completed";

    public const string Partial = "Partial";

    public const string Failed = "Failed";
}

public sealed class OfficialFoodIngredientCompanyQuery
{
    public string IngredientKey { get; init; } = string.Empty;

    public string IngredientName { get; init; } = string.Empty;

    public int Take { get; init; } = 12;
}

public sealed record OfficialFoodIngredientCompanySourceDto(
    string SourceKey,
    string Provider,
    string DisplayName,
    string CountryScope,
    string OfficialUrl,
    string StatusCode,
    string StatusMessage,
    bool ProvidesDirectIngredientEvidence,
    bool CanVerifyCurrentOrganizationStatus,
    bool RequiresLiveRecheck);

public sealed record OfficialFoodIngredientCompanyCandidateDto(
    string CandidateKey,
    string OrganizationName,
    string CountryCode,
    string CountryName,
    string RelationCode,
    string EvidenceCode,
    string EvidenceSummary,
    string RelatedProductName,
    string ProductCategory,
    string OfficialIdentifier,
    string VerificationStatusCode,
    bool RequiresAttention,
    string AttentionReason,
    string SourceKey,
    string SourceName,
    string SourceUrl,
    DateTimeOffset ObservedAtUtc,
    bool RequiresLiveRecheck,
    bool CanAutoSelect,
    bool CanAutoContact)
{
    public string RawIngredientText { get; init; } = string.Empty;

    public string EvidenceDate { get; init; } = string.Empty;

    public string EvidenceLastChangedDate { get; init; } = string.Empty;

    public string EvidenceSequence { get; init; } = string.Empty;

    public string EvidenceRecordIdentifier { get; init; } = string.Empty;
}

public sealed record OfficialFoodIngredientCompanyResearchResponse(
    string StatusCode,
    string IngredientKey,
    string IngredientName,
    DateTimeOffset ResearchedAtUtc,
    IReadOnlyList<OfficialFoodIngredientCompanySourceDto> Sources,
    IReadOnlyList<OfficialFoodIngredientCompanyCandidateDto> Candidates,
    IReadOnlyList<string> Notices)
{
    public bool Archived { get; init; }

    public string ArchiveRunKey { get; init; } = string.Empty;

    public int ArchivedOrganizationCount { get; init; }

    public int ArchivedEvidenceCount { get; init; }
}

public sealed record OfficialFoodIngredientCompanyCollectionRequest(
    int MaxIngredients = 500,
    int CandidatesPerIngredient = 100,
    bool Force = false,
    int RefreshAfterDays = 30,
    int RequestDelayMilliseconds = 250);

public sealed record OfficialFoodIngredientCompanyCollectionResponse(
    string RunKey,
    string TriggerCode,
    string StatusCode,
    int RequestedIngredientCount,
    int ProcessedIngredientCount,
    int SkippedIngredientCount,
    int AvailableIngredientCount,
    int PartialIngredientCount,
    int NoResultIngredientCount,
    int NotConfiguredIngredientCount,
    int FailedIngredientCount,
    int ObservedEvidenceCount,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc);

public sealed record OfficialFoodIngredientCompanyArchivedEvidenceDto(
    string CandidateKey,
    string EvidenceCode,
    string EvidenceSummary,
    string RelatedProductName,
    string ProductCategory,
    string OfficialIdentifier,
    string EvidenceRecordIdentifier,
    string VerificationStatusCode,
    string RawIngredientText,
    string EvidenceDate,
    string EvidenceLastChangedDate,
    string EvidenceSequence,
    string SourceKey,
    string SourceName,
    string SourceUrl,
    bool RequiresAttention,
    string AttentionReason,
    DateTimeOffset FirstObservedAtUtc,
    DateTimeOffset LastObservedAtUtc,
    int ObservationCount,
    bool IsCurrent,
    bool RequiresLiveRecheck,
    bool CanAutoSelect,
    bool CanAutoContact);

public sealed record OfficialFoodIngredientCompanyArchivedOrganizationDto(
    string OrganizationKey,
    string OrganizationName,
    string CountryCode,
    string CountryName,
    string RelationCode,
    string VerificationStatusCode,
    bool RequiresAttention,
    string AttentionReason,
    DateTimeOffset FirstObservedAtUtc,
    DateTimeOffset LastObservedAtUtc,
    int EvidenceCount,
    IReadOnlyList<OfficialFoodIngredientCompanyArchivedEvidenceDto> Evidence);

public sealed record OfficialFoodIngredientCompanyArchiveResponse(
    string StatusCode,
    string IngredientKey,
    string IngredientName,
    string LanguageCode,
    string CategoryCode,
    string ResearchQueryTerm,
    string LastRunKey,
    DateTimeOffset LastResearchedAtUtc,
    int OrganizationCount,
    int EvidenceCount,
    int DomesticManufacturerCount,
    int DomesticImporterCount,
    int ForeignManufacturerCount,
    IReadOnlyList<OfficialFoodIngredientCompanySourceDto> Sources,
    IReadOnlyList<OfficialFoodIngredientCompanyArchivedOrganizationDto> Organizations,
    IReadOnlyList<string> Notices);

public sealed record OfficialFoodIngredientCompanyCoverageResponse(
    int CatalogIngredientCount,
    int ResearchedIngredientCount,
    int UnresearchedIngredientCount,
    int StaleIngredientCount,
    int AvailableIngredientCount,
    int PartialIngredientCount,
    int NoResultIngredientCount,
    int NotConfiguredIngredientCount,
    int FailedIngredientCount,
    int CurrentOrganizationCount,
    int CurrentEvidenceCount,
    int DomesticManufacturerCount,
    int DomesticImporterCount,
    int ForeignManufacturerCount,
    DateTimeOffset? LastCompletedAtUtc);
