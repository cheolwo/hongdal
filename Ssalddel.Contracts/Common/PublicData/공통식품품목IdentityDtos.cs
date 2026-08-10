namespace Ssalddel.Contracts.Common.PublicData;

public static class 공통식품품목IdentityRoutes
{
    public const string Api = "api/v1/agriculture/products/common-identities";

    public const string ReconciliationPreview = "reconciliation-preview";
}

public static class 공통식품품목대조StatusCodes
{
    public const string CanonicalLinked = "CanonicalLinked";
    public const string CandidateOnly = "CandidateOnly";
    public const string Unmapped = "Unmapped";
    public const string SourceConflict = "SourceConflict";
}

public static class 공통식품품목관계StatusCodes
{
    public const string Confirmed = "Confirmed";
    public const string Candidate = "Candidate";
    public const string Unlinked = "Unlinked";
}

public static class 공통식품품목CodeSchemes
{
    public const string KamisItem = "KAMIS_ITEM";
    public const string Hs4 = "HS4";
    public const string UsdaAmsCommodity = "USDA_AMS_COMMODITY";
    public const string NongsaroKindOfCommodity = "NONGSARO_KIND_OF_COMMODITY";
}

public sealed record 공통식품품목Code관계Response(
    string SourceKey,
    string CodeScheme,
    string? Code,
    string? ParentCode,
    string Label,
    string RelationStatusCode,
    string MatchQualityCode,
    string EvidenceNote,
    int Revision,
    IReadOnlyList<공통식품품목Code관계검토이력Response> ReviewHistory);

public sealed record 공통식품품목Code관계검토이력Response(
    int Revision,
    string RelationStatusCode,
    string? ExternalCode,
    string ReviewActionCode,
    string ReviewReason,
    DateTime ReviewedAtUtc);

public sealed record 공통식품품목IdentityResponse(
    string CanonicalProductStableId,
    string DisplayName,
    string Revision,
    IReadOnlyList<공통식품품목Code관계Response> CodeRelations,
    IReadOnlyList<string> Limitations);

public sealed record 공통식품품목IdentityListResponse(
    string Revision,
    IReadOnlyList<공통식품품목IdentityResponse> Items);

public sealed record 공통식품품목기존Data대조항목Response(
    string StatusCode,
    string? CanonicalProductStableId,
    string KamisCategoryCode,
    string KamisCategoryName,
    string KamisItemCode,
    string KamisItemName,
    DateOnly LatestKamisSurveyDate,
    IReadOnlyList<string> HsCandidates,
    IReadOnlyList<string> UsdaAmsCommodityCandidates,
    string NongsaroRelationStatusCode,
    string ReviewNote);

public sealed record 공통식품품목기존Data대조Response(
    int Year,
    string PreviewHash,
    int ObservedKamisItemCount,
    int CanonicalLinkedCount,
    int CandidateOnlyCount,
    int UnmappedCount,
    int SourceConflictCount,
    IReadOnlyList<공통식품품목기존Data대조항목Response> Items,
    IReadOnlyList<string> Boundaries);

public sealed record 공통식품품목기존Data승격Response(
    int Year,
    string PreviewHash,
    int PromotedProductCount,
    int CreatedRelationCount,
    int AlreadyPromotedCount,
    IReadOnlyList<string> PromotedProductStableIds,
    IReadOnlyList<string> Boundaries);
