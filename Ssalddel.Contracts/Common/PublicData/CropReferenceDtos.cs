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
