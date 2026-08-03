using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.PublicData;

public static class RegionalAgriculturalMapRoutes
{
    public const string RegionalMap = "/information/regional-agricultural-map";
    public const string KoreaMap = "/information/korea-agricultural-map";
    public const string MarkerApi = "api/v1/community/regional-map/markers";
    public const string OceanTileApi = "api/v1/community/regional-map/ocean-tiles";

    public static string ForCountry(string countryCode)
        => $"{RegionalMap}?country={Uri.EscapeDataString(
            RegionalAgriculturalMapCountryCodes.NormalizeOrDefault(countryCode))}";
}

public static class RegionalAgriculturalMapContentLayerKeys
{
    public const string AgriculturalLivingInformation = "agricultural-living-information";
    public const string MarineFishingAreas = "marine-fishing-areas";
    public const string HongikAcademyPhilosophyVideo = "hongik-academy-philosophy-video";

    public static IReadOnlyList<string> All { get; } =
        [AgriculturalLivingInformation, MarineFishingAreas, HongikAcademyPhilosophyVideo];

    public static string NormalizeOrDefault(string? contentLayerKey)
        => All.Contains(contentLayerKey?.Trim(), StringComparer.Ordinal)
            ? contentLayerKey!.Trim()
            : AgriculturalLivingInformation;
}

public static class RegionalAgriculturalMapCountryCodes
{
    public const string Korea = "KR";
    public const string UnitedStates = "US";

    public static IReadOnlyList<string> All { get; } = [Korea, UnitedStates];

    public static string NormalizeOrDefault(string? countryCode)
    {
        var normalized = countryCode?.Trim().ToUpperInvariant();
        return All.Contains(normalized, StringComparer.Ordinal)
            ? normalized!
            : Korea;
    }
}

public static class RegionalAgriculturalMapRegionTypeCodes
{
    public const string StateProvince = "StateProvince";
    public const string CountyMunicipality = "CountyMunicipality";
    public const string ShippingDistrict = "ShippingDistrict";
}

public static class RegionalAgriculturalMapCodeSchemeCodes
{
    public const string KoreaMoisAdministrative = "KR-MOIS-ADMIN";
    public const string KoreaMafraOrigin = "KR-MAFRA-ORIGIN";
    public const string UnitedStatesCensusGeoid = "US-CENSUS-GEOID";
    public const string UnitedStatesPostalState = "US-USPS-STATE";
    public const string UnitedStatesAmsShippingDistrict = "US-AMS-SHIPPING-DISTRICT";
}

public static class RegionalAgriculturalMapRelationTypeCodes
{
    public const string ConfirmedOrigin = "ConfirmedOrigin";
    public const string ShippingPointOrPortOfEntry = "ShippingPointOrPortOfEntry";
    public const string MarketObservation = "MarketObservation";

    public static IReadOnlyList<string> All { get; } =
        [ConfirmedOrigin, ShippingPointOrPortOfEntry, MarketObservation];
}

public static class RegionalAgriculturalMapConfidenceCodes
{
    public const string OfficialCodeCrosswalk = "OfficialCodeCrosswalk";
    public const string OfficialNameCrosswalk = "OfficialNameCrosswalk";
    public const string CuratedCrosswalk = "CuratedCrosswalk";
}

public sealed class RegionalAgriculturalMapMarkerQuery
{
    public string CountryCode { get; init; } = string.Empty;

    public string? RelationTypeCode { get; init; }

    public string? ProductName { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int MaxItems { get; init; } = 200;
}

public sealed record RegionalAgriculturalMapMarkerSourceDto(
    string DataSourceKey,
    string CodeScheme,
    string ExternalCode,
    string ExternalName,
    string CrosswalkConfidenceCode,
    DateTime CrosswalkVerifiedAtUtc,
    int ObservationCount,
    DateOnly EarliestObservedDate,
    DateOnly LatestObservedDate);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Contract,
    "한국·미국 농수산물 가격 관측과 검증된 행정구역 마커의 관계를 전달",
    FlowOrder = 10,
    Boundary = "원산지, Shipping Point·항구, 시장 관측지를 서로 교환하지 않으며 미해결 코드는 마커로 추정하지 않습니다.")]
public sealed record RegionalAgriculturalMapMarkerDto(
    string MarkerKey,
    string RegionKey,
    string CountryCode,
    string RegionTypeCode,
    string DisplayNameKo,
    string DisplayNameEn,
    string DisplayNameLocal,
    decimal Latitude,
    decimal Longitude,
    string AnchorSourceKey,
    string AnchorSourceVintage,
    string AnchorSourceUrl,
    DateTime AnchorVerifiedAtUtc,
    string RelationTypeCode,
    int ObservationCount,
    DateOnly EarliestObservedDate,
    DateOnly LatestObservedDate,
    IReadOnlyList<RegionalAgriculturalMapMarkerSourceDto> Sources);

public sealed record RegionalAgriculturalMapMarkerListResponse(
    string CountryCode,
    IReadOnlyList<string> RelationTypeCodes,
    string? ProductName,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int TotalMarkerCount,
    int ReturnedMarkerCount,
    int UnresolvedObservationCount,
    int MissingAnchorRegionCount,
    IReadOnlyList<string> Notices,
    IReadOnlyList<RegionalAgriculturalMapMarkerDto> Items);

public static class MarineFishingAreaGeometryBasisCodes
{
    public const string SchematicOceanCatalogLayout = "SchematicOceanCatalogLayout";
}

public sealed record MarineFishingAreaOceanTileDto(
    string TileKey,
    string SourceSeaName,
    string DisplayNameEn,
    int FishingAreaCount,
    decimal AnchorLeftPercent,
    decimal AnchorTopPercent,
    int AnimationDelayMilliseconds,
    IReadOnlyList<string> ExampleFishingAreas);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Contract,
    "해양수산부 어획구역 카탈로그를 바다별 개략 타일로 전달",
    FlowOrder = 12,
    Boundary = "원천 파일에는 좌표와 경계 도형이 없으므로 타일 위치는 탐색용 개략 배치이며 실제 조업 위치나 어획량을 뜻하지 않습니다.")]
public sealed record MarineFishingAreaOceanTileResponse(
    string SourceKey,
    string SourceName,
    string SourceUrl,
    string DatasetVersion,
    DateTime CollectedAtUtc,
    string ContentSha256,
    int SourceRowCount,
    int MappedFishingAreaCount,
    int ExcludedRowCount,
    string GeometryBasisCode,
    IReadOnlyList<string> Notices,
    IReadOnlyList<MarineFishingAreaOceanTileDto> Items);
