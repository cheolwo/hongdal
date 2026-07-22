namespace Ssalddel.Domain.FoodCulture;

public sealed class OfficialFoodIngredientCompanyResearchRun
{
    public long Id { get; set; }

    public string RunKey { get; set; } = string.Empty;

    public string TriggerCode { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public int RequestedIngredientCount { get; set; }

    public int ProcessedIngredientCount { get; set; }

    public int SkippedIngredientCount { get; set; }

    public int AvailableIngredientCount { get; set; }

    public int PartialIngredientCount { get; set; }

    public int NoResultIngredientCount { get; set; }

    public int NotConfiguredIngredientCount { get; set; }

    public int FailedIngredientCount { get; set; }

    public int ObservedEvidenceCount { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;
}

public sealed class OfficialFoodIngredientCompanyProfile
{
    public long IngredientId { get; set; }

    public OfficialFoodIngredient? Ingredient { get; set; }

    public long LastResearchRunId { get; set; }

    public OfficialFoodIngredientCompanyResearchRun? LastResearchRun { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string ResearchQueryTerm { get; set; } = string.Empty;

    public DateTime LastResearchedAtUtc { get; set; } = DateTime.UtcNow;

    public int OrganizationCount { get; set; }

    public int EvidenceCount { get; set; }

    public int DomesticManufacturerCount { get; set; }

    public int DomesticImporterCount { get; set; }

    public int ForeignManufacturerCount { get; set; }

    public int AvailableSourceCount { get; set; }

    public int FailedSourceCount { get; set; }

    public int NotConfiguredSourceCount { get; set; }

    public int ConsecutiveFailureCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class OfficialFoodIngredientCompanyEvidence
{
    public long Id { get; set; }

    public long IngredientId { get; set; }

    public OfficialFoodIngredient? Ingredient { get; set; }

    public long LastResearchRunId { get; set; }

    public OfficialFoodIngredientCompanyResearchRun? LastResearchRun { get; set; }

    public string CandidateKey { get; set; } = string.Empty;

    public string OrganizationKey { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string NormalizedOrganizationName { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public string RelationCode { get; set; } = string.Empty;

    public string EvidenceCode { get; set; } = string.Empty;

    public string EvidenceSummary { get; set; } = string.Empty;

    public string RelatedProductName { get; set; } = string.Empty;

    public string ProductCategory { get; set; } = string.Empty;

    public string OfficialIdentifier { get; set; } = string.Empty;

    public string EvidenceRecordIdentifier { get; set; } = string.Empty;

    public string VerificationStatusCode { get; set; } = string.Empty;

    public string RawIngredientText { get; set; } = string.Empty;

    public string EvidenceDate { get; set; } = string.Empty;

    public string EvidenceLastChangedDate { get; set; } = string.Empty;

    public string EvidenceSequence { get; set; } = string.Empty;

    public bool RequiresAttention { get; set; }

    public string AttentionReason { get; set; } = string.Empty;

    public string SourceKey { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public string ResearchQueryTerm { get; set; } = string.Empty;

    public DateTime FirstObservedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastObservedAtUtc { get; set; } = DateTime.UtcNow;

    public int ObservationCount { get; set; } = 1;

    public bool IsCurrent { get; set; } = true;

    public bool RequiresLiveRecheck { get; set; } = true;

    public bool CanAutoSelect { get; set; }

    public bool CanAutoContact { get; set; }
}

public sealed class OfficialFoodIngredientCompanySourceObservation
{
    public long Id { get; set; }

    public long ResearchRunId { get; set; }

    public OfficialFoodIngredientCompanyResearchRun? ResearchRun { get; set; }

    public long IngredientId { get; set; }

    public OfficialFoodIngredient? Ingredient { get; set; }

    public string SourceKey { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string CountryScope { get; set; } = string.Empty;

    public string OfficialUrl { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public string StatusMessage { get; set; } = string.Empty;

    public bool ProvidesDirectIngredientEvidence { get; set; }

    public bool CanVerifyCurrentOrganizationStatus { get; set; }

    public bool RequiresLiveRecheck { get; set; } = true;

    public DateTime ObservedAtUtc { get; set; } = DateTime.UtcNow;
}
