namespace Ssalddel.Contracts.Common.PublicData;

public static class 경기데이터드림ModuleKeys
{
    public const string 농산스마트팜식물건강 = "gg-crop-smartfarm-plant-health";
    public const string 농산생산인증유통 = "gg-agri-production-certification-distribution";
    public const string 축산농장사육 = "gg-livestock-farm-breeding";
    public const string 축산사료방역안전 = "gg-livestock-feed-health-biosecurity";
    public const string 축산가공물류인허가 = "gg-livestock-processing-logistics-license";
    public const string 수산양식안전 = "gg-fisheries-aquaculture-safety";
    public const string 수산위판역사 = "gg-fisheries-auction-historical";
    public const string 반려동물제외 = "gg-companion-animal-out-of-scope";
    public const string 산림별도 = "gg-forestry-adjacent";
    public const string 농촌기타Catalog = "gg-other-agri-rural-catalog";
}

public sealed record 경기데이터드림ModuleDefinition(
    string Key,
    string DisplayName,
    string ProductBoundary);

public sealed record 경기데이터드림CatalogItem
{
    public required string InfId { get; init; }
    public required int InfSeq { get; init; }
    public required int ApiInfSeq { get; init; }
    public required string DisplayName { get; init; }
    public string Description { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public required string ModuleKey { get; init; }
    public required string DetailUrl { get; init; }
    public DateOnly? RegisteredOn { get; init; }
    public DateOnly? UpdatedOn { get; init; }
}

public sealed record 경기데이터드림CatalogResponse
{
    public required DateTimeOffset ObservedAt { get; init; }
    public required IReadOnlyList<경기데이터드림ModuleDefinition> Modules { get; init; }
    public required IReadOnlyList<경기데이터드림CatalogItem> Items { get; init; }
}

public sealed record 경기데이터드림ApiRequest
{
    public required string DatasetPath { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 100;
    public IReadOnlyDictionary<string, string?> Parameters { get; init; }
        = new Dictionary<string, string?>();
}

public sealed record 경기데이터드림ApiResponse
{
    public bool Success { get; init; }
    public int HttpStatusCode { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public required DateTimeOffset ObservedAt { get; init; }
}

public sealed record 경기데이터드림가축사육지역집계
{
    public required string RegionCode { get; init; }
    public required string RegionName { get; init; }
    public required string BusinessStatus { get; init; }
    public required int BusinessCount { get; init; }
}

public sealed record 경기데이터드림가축사육집계Response
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public required DateTimeOffset ObservedAt { get; init; }
    public required IReadOnlyList<경기데이터드림가축사육지역집계> Items { get; init; }
}
