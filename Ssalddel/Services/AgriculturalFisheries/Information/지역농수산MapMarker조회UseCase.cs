using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I지역농수산MapMarker조회UseCase
{
    Task<RegionalAgriculturalMapMarkerListResponse> 조회Async(
        RegionalAgriculturalMapMarkerQuery query,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "한국·미국 농수산물 가격 관측을 검증된 행정구역 기준점으로 읽기 전용 투영",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "검토된 코드 교차표와 경계 기준점이 모두 있는 관측만 마커로 반환하고, 농장·개인·창고 위치를 추정하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Application,
    "가격 관측 집계와 지역 해석을 조율해 공개 마커 응답을 조립",
    ContractType = typeof(I지역농수산MapMarker조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "원산지, Shipping Point·port of entry, 시장 관측지를 별도 relation으로 유지하며 미해결 관측을 숨기지 않습니다.")]
public sealed class 지역농수산MapMarker조회UseCase(
    지역농수산Map가격관측Reader observationReader,
    지역농수산Map지역Resolver regionResolver) : I지역농수산MapMarker조회UseCase
{
    public async Task<RegionalAgriculturalMapMarkerListResponse> 조회Async(
        RegionalAgriculturalMapMarkerQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalized = 지역농수산MapMarkerQueryNormalizer.Normalize(query);
        var sourceAggregates = await observationReader.조회Async(
            normalized,
            cancellationToken);
        var projection = await regionResolver.투영Async(
            sourceAggregates,
            cancellationToken);
        var returnedMarkers = projection.Markers
            .Take(normalized.MaxItems)
            .ToArray();

        return new RegionalAgriculturalMapMarkerListResponse(
            normalized.CountryCode,
            normalized.RelationTypeCodes,
            normalized.ProductName,
            normalized.FromDate,
            normalized.ToDate,
            projection.Markers.Count,
            returnedMarkers.Length,
            projection.UnresolvedObservationCount,
            projection.MissingAnchorRegionCount,
            BuildNotices(normalized.CountryCode),
            returnedMarkers);
    }

    private static IReadOnlyList<string> BuildNotices(string countryCode)
    {
        var notices = new List<string>
        {
            "마커는 공식 행정구역의 검증된 대표 기준점이며 실제 농장·창고·개인의 위치가 아닙니다.",
            "코드 교차표나 좌표 근거가 없는 관측은 지도에 추정 표시하지 않고 미해결 건수로 반환합니다."
        };
        notices.Add(countryCode == RegionalAgriculturalMapCountryCodes.Korea
            ? "MAFRA SANCD는 원천 산지코드로 보존하며 관측일에 유효한 공식 행정구역 교차표가 검토된 경우에만 산지로 표시합니다."
            : "USDA AMS의 시장 위치는 가격 관측지이며, Shipping Point는 생산지 또는 port of entry일 수 있어 원산지로 표현하지 않습니다.");

        return notices;
    }
}
