namespace Ssalddel.Contracts.Common.PublicData;

public static class CropReferenceRoutes
{
    public const string CategoryApi = "api/v1/agriculture/crops/reference-categories";
}

public static class CropReferenceSourceTypeCodes
{
    public const string PublicReference = "PublicReference";
}

public sealed record CropReferenceCategoryItem(
    string StableId,
    string CategoryCode,
    string CategoryName);

public sealed record CropReferenceCategoryListResponse(
    string SourceTypeCode,
    string SourceKey,
    string SourceName,
    string SourceHref,
    DateTimeOffset RetrievedAt,
    string Boundary,
    IReadOnlyList<CropReferenceCategoryItem> Items);

public static class 작물생육요구검토StatusCodes
{
    public const string PendingHumanReview = "PendingHumanReview";
    public const string ApprovedForRuleDraft = "ApprovedForRuleDraft";
    public const string Rejected = "Rejected";
}

public static class 작물생육근거StatusCodes
{
    public const string LocatedNeedsReview = "LocatedNeedsReview";
    public const string NotLocated = "NotLocated";
}

public static class 작물생육근거TopicCodes
{
    public const string Soil = "Soil";
    public const string Water = "Water";
    public const string Temperature = "Temperature";
    public const string Sunlight = "Sunlight";
    public const string GrowthStage = "GrowthStage";
    public const string CultivationMethod = "CultivationMethod";
}

public sealed record 농사로작물생육SourceSnapshot(
    string SourceStableId,
    string ServiceName,
    string OperationName,
    string SourceRecordId,
    DateTimeOffset RetrievedAtUtc,
    string SourceHref);

public sealed record 농사로작물생육근거Topic(
    string TopicCode,
    string DisplayName,
    string EvidenceStatusCode,
    string SourceStableId,
    string ReviewNote);

public sealed record 농사로작물생육요구ProfileResponse(
    string StableId,
    int Revision,
    string CanonicalProductStableId,
    string CropDisplayName,
    string WorkScheduleGroupCode,
    string WorkScheduleGroupName,
    string WorkScheduleContentNo,
    string NongsaroProductRelationStatusCode,
    string ReviewStatusCode,
    bool CanPublishSimulationRule,
    DateTimeOffset RetrievedAtUtc,
    IReadOnlyList<농사로작물생육SourceSnapshot> Sources,
    IReadOnlyList<농사로작물생육근거Topic> EvidenceTopics,
    IReadOnlyList<string> Limitations);
