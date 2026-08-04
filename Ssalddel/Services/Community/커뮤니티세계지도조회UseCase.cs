using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Services.TraditionalMarkets;
using Ssalddel.Services.FoodCulture;
using Ssalddel.Services.Content;

namespace Ssalddel.Services.Community;

public interface I커뮤니티세계지도조회UseCase
{
    Task<커뮤니티세계지도SnapshotDto> 조회Async(
        string? datasetCode,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "낮의 생활·업무와 밤의 배움·성찰 공개 관측을 한 지도용 snapshot으로 구성",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공개 카탈로그와 공식 자료 연결만 읽으며 개인 위치·거래·추천 순위를 만들지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Application,
    "분야별 공개 관측을 안정 ID와 revision을 가진 지도 snapshot으로 조회",
    ContractType = typeof(I커뮤니티세계지도조회UseCase),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.None,
    Boundary = "조회 결과는 지도 탐색 근거이며 결제·주문·계약·배차를 실행하지 않습니다.")]
public sealed class 커뮤니티세계지도조회UseCase(
    I전통시장MapMarkerReader 전통시장MapMarkerReader,
    I해외제조업소MapMarkerReader 해외제조업소MapMarkerReader,
    I지역문화이미지MapMarkerReader? 지역문화이미지MapMarkerReader = null,
    I경기데이터드림가축사육MapMarkerReader? 경기데이터드림가축사육MapMarkerReader = null,
    I선택공공데이터MapMarkerReader? 선택공공데이터MapMarkerReader = null)
    : I커뮤니티세계지도조회UseCase
{
    private static readonly IReadOnlyDictionary<string, (double Latitude, double Longitude)> CountryCenters
        = new Dictionary<string, (double, double)>(StringComparer.Ordinal)
        {
            ["KR"] = (36.5, 127.8),
            ["US"] = (39.8283, -98.5795),
            ["CN"] = (35.8617, 104.1954),
            ["AU"] = (-25.2744, 133.7751),
            ["GB"] = (54.0, -2.0),
            ["FR"] = (46.2276, 2.2137),
            ["IN"] = (20.5937, 78.9629)
        };

    private static readonly IReadOnlyDictionary<string, (double Latitude, double Longitude)> RegionCenters
        = new Dictionary<string, (double, double)>(StringComparer.Ordinal)
        {
            ["us-maine"] = (45.2538, -69.4455),
            ["us-georgia"] = (32.1656, -82.9001),
            ["us-california"] = (36.7783, -119.4179),
            ["cn-shandong"] = (36.6683, 117.0204),
            ["cn-liaodong"] = (40.25, 122.15),
            ["cn-south-yangtze"] = (27.6, 113.9)
        };

    public async Task<커뮤니티세계지도SnapshotDto> 조회Async(
        string? datasetCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedDataset = NormalizeDataset(datasetCode);
        var observations = string.Equals(
                normalizedDataset,
                CommunityPageRoutes.WorldMapNightLearningDataset,
                StringComparison.Ordinal)
            ? BuildNightObservations()
            : await BuildDayObservationsAsync(cancellationToken);
        var layers = 커뮤니티세계지도LayerCatalog.ForDataset(normalizedDataset);

        return new 커뮤니티세계지도SnapshotDto(
            normalizedDataset,
            ComputeRevision(observations),
            DateTimeOffset.UtcNow,
            layers,
            observations);
    }

    private static string NormalizeDataset(string? datasetCode)
    {
        var normalized = datasetCode?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized)
            || string.Equals(normalized, CommunityPageRoutes.WorldMapDayWorkDataset, StringComparison.Ordinal))
        {
            return CommunityPageRoutes.WorldMapDayWorkDataset;
        }

        if (string.Equals(normalized, CommunityPageRoutes.WorldMapNightLearningDataset, StringComparison.Ordinal))
        {
            return CommunityPageRoutes.WorldMapNightLearningDataset;
        }

        throw new ArgumentException("지도 dataset은 day-work 또는 night-learning이어야 합니다.", nameof(datasetCode));
    }

    private async Task<IReadOnlyList<커뮤니티세계지도ObservationDto>> BuildDayObservationsAsync(
        CancellationToken cancellationToken)
    {
        var culture = 지역문화이미지MapMarkerReader is null
            ? BuildLegacyCultureObservations()
            : (await 지역문화이미지MapMarkerReader.공개Marker조회Async(cancellationToken))
                .Select(marker => new 커뮤니티세계지도ObservationDto(
                    $"culture:{marker.RegionKey}",
                    CommunityPageRoutes.WorldMapDayWorkDataset,
                    커뮤니티세계지도LayerCodes.RegionalCulture,
                    marker.CountryCode,
                    marker.CountryName,
                    marker.Latitude,
                    marker.Longitude,
                    $"{marker.RegionName} 생활문화",
                    $"{marker.CultureSummary} {지역문화행정구역대표점Catalog.LocationBoundary} 생성 이미지는 생활문화 이해를 위한 참고 자료이며 공식 기록이 아닙니다.",
                    지역문화행정구역대표점Catalog.SourceName,
                    marker.ImageUpdatedAtUtc,
                    커뮤니티세계지도EvidenceStatusCodes.Curated,
                    CommunityPageRoutes.BoardsFor(
                        boardName: "지역문화",
                        boardKey: "regional-culture",
                        search: marker.RegionName),
                    지역문화행정구역대표점Catalog.SourceUrl,
                    커뮤니티세계지도위치정밀도Codes.AdministrativeRegionRepresentative));

        var prices = new[]
        {
            Price("KR", "대한민국", "KAMIS 국내 유통단계 가격", "KAMIS · aT",
                커뮤니티세계지도Routes.KoreaPriceDetail),
            Price("US", "미국", "USDA 농산물 가격 관측", "USDA NASS",
                커뮤니티세계지도Routes.UnitedStatesPriceDetail),
            Price("CN", "중국", "중국 연결 가격 관측", "공식 연결 관측", "/information/produce-price-comparison"),
            Price("AU", "호주", "식품 물가지수 관측", "호주 ABS", "/information/public-data")
        };

        var wholesaleMarkets = 커뮤니티도매시장MapCatalog.All.Select(market =>
            new 커뮤니티세계지도ObservationDto(
                $"wholesale-market:{market.Key}",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.WholesaleMarket,
                market.CountryCode,
                market.CountryName,
                market.Latitude,
                market.Longitude,
                market.MarketName,
                market.LocationPrecisionCode == 커뮤니티도매시장위치정밀도Codes.도시중심점
                    ? $"{market.RegionName}의 USDA 터미널 도매시장 보고 위치입니다. 도시 중심 대표점이며 단일 시설·출입구·배송 주소가 아닙니다. {market.UpdateCycle} 갱신입니다."
                    : $"{market.RegionName}의 공영도매시장 대표 위치입니다. 지도 탐색용 대표점이며 출입구·배송 주소가 아닙니다. {market.UpdateCycle} 자료와 연결합니다.",
                market.SourceName,
                market.EvidenceAsOfUtc,
                커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                market.DetailHref,
                market.SourceHref,
                market.LocationPrecisionCode,
                market.MarketStageCode));

        var traditionalMarketHubs = (await 전통시장MapMarkerReader.공개Marker조회Async(cancellationToken))
            .Select(hub =>
            {
                var capabilities = new[]
                    {
                        hub.SupportsResidentPickup ? "주민 수령" : null,
                        hub.SupportsLastMileDelivery ? "근거리 배송" : null,
                        hub.SupportsRefrigeratedStorage ? "냉장 보관" : null,
                        hub.SupportsFrozenStorage ? "냉동 보관" : null
                    }
                    .Where(value => value is not null);
                var capabilityLabel = string.Join(" · ", capabilities.OfType<string>());
                return new 커뮤니티세계지도ObservationDto(
                    hub.HubReferenceKey,
                    CommunityPageRoutes.WorldMapDayWorkDataset,
                    커뮤니티세계지도LayerCodes.TraditionalMarketHub,
                    "KR",
                    "대한민국",
                    (double)hub.MapAnchor.Latitude,
                    (double)hub.MapAnchor.Longitude,
                    hub.MarketName,
                    $"{hub.Province} {hub.CityCounty}의 검증된 전통시장 공동 입고·수령 거점입니다. "
                    + $"생활권 {hub.ServiceRadiusKm:0.##}km · 일일 처리 {hub.DailyGroupPurchaseCapacity:N0}건"
                    + (string.IsNullOrWhiteSpace(capabilityLabel) ? string.Empty : $" · {capabilityLabel}"),
                    hub.MapAnchor.SourceName,
                    new DateTimeOffset(DateTime.SpecifyKind(hub.MapAnchor.VerifiedAtUtc, DateTimeKind.Utc)),
                    커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                    CommunityPageRoutes.BoardsFor(
                        boardName: "전통시장",
                        boardKey: "traditional-market",
                        search: hub.MarketName),
                    hub.MapAnchor.SourceHref,
                    hub.MapAnchor.LocationPrecisionCode,
                    null,
                    hub.Status,
                    hub.ServiceRadiusKm,
                    hub.DailyGroupPurchaseCapacity,
                    hub.CommunityScopeKey);
            });

        var overseasManufacturers = (await 해외제조업소MapMarkerReader
                .공개Marker조회Async(cancellationToken))
            .Select(marker => new 커뮤니티세계지도ObservationDto(
                $"overseas-manufacturer:{marker.StableRegionKey}",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.OverseasManufacturer,
                marker.CountryCode,
                marker.CountryName,
                marker.Latitude,
                marker.Longitude,
                $"{marker.RegionName} 해외제조업소 근거",
                $"식약처 해외제조업소 코드가 확인된 제조업소 {marker.OrganizationCount:N0}개와 재료 관계 근거 {marker.EvidenceCount:N0}건을 지역별로 집계했습니다. "
                + $"{marker.RegionBoundary} 제조업소 소재지는 원재료의 재배·어획 산지나 현재 공급 가능성을 뜻하지 않습니다.",
                marker.AnchorSourceName,
                marker.LastObservedAtUtc,
                커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                "/information/food-ingredients",
                marker.AnchorSourceUrl,
                커뮤니티세계지도위치정밀도Codes.AdministrativeRegionRepresentative,
                OrganizationCount: marker.OrganizationCount,
                EvidenceCount: marker.EvidenceCount));

        var gyeonggiLivestock = 경기데이터드림가축사육MapMarkerReader is null
            ? []
            : await 경기데이터드림가축사육MapMarkerReader
                .공개Marker조회Async(cancellationToken);
        var selectedPublicData = 선택공공데이터MapMarkerReader is null
            ? []
            : await 선택공공데이터MapMarkerReader
                .공개Marker조회Async(cancellationToken);
        var newsPublishers = 국가별뉴스출처MapCatalog.All
            .Select(국가별뉴스출처MapCatalog.ToObservation);

        return culture.Concat(prices)
            .Concat(wholesaleMarkets)
            .Concat(traditionalMarketHubs)
            .Concat(overseasManufacturers)
            .Concat(gyeonggiLivestock)
            .Concat(selectedPublicData)
            .Concat(newsPublishers)
            .OrderBy(item => item.StableId, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<커뮤니티세계지도ObservationDto> BuildLegacyCultureObservations()
        => RegionalCultureSpecialtyCatalog.All.Select(region =>
        {
            var center = RegionCenters[region.Key];
            return new 커뮤니티세계지도ObservationDto(
                $"culture:{region.Key}",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.RegionalCulture,
                region.CountryCode,
                region.CountryName,
                center.Latitude,
                center.Longitude,
                $"{region.RegionName} 문화·특산물",
                region.CultureSummary,
                "살뜰 지역문화 공개 카탈로그",
                null,
                커뮤니티세계지도EvidenceStatusCodes.Curated,
                RegionalCultureSpecialtyRoutes.DetailFor(region.Key));
        });

    private static 커뮤니티세계지도ObservationDto Price(
        string countryCode,
        string countryName,
        string title,
        string sourceName,
        string detailHref)
    {
        var center = CountryCenters[countryCode];
        return new(
            $"price:{countryCode.ToLowerInvariant()}",
            CommunityPageRoutes.WorldMapDayWorkDataset,
            커뮤니티세계지도LayerCodes.PublicPrice,
            countryCode,
            countryName,
            center.Latitude,
            center.Longitude,
            title,
            "가격은 구매가가 아닌 관측 정보입니다. 기준일·원 거래단위·통화·시장 단계를 상세 화면에서 확인합니다.",
            sourceName,
            null,
            커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
            detailHref);
    }

    private static IReadOnlyList<커뮤니티세계지도ObservationDto> BuildNightObservations()
        => YouTube지식성찰채널Catalog.항목
            .Select((channel, index) =>
            {
                var layerCode = channel.주제코드목록.Contains(
                    YouTube지식성찰주제코드.종교교육,
                    StringComparer.Ordinal)
                    ? 커뮤니티세계지도LayerCodes.ScriptureAndClassics
                    : 커뮤니티세계지도LayerCodes.LearningChannel;
                var center = CountryCenters[channel.국가코드];
                var offset = (index % 3 - 1) * 0.8;
                return new 커뮤니티세계지도ObservationDto(
                    $"learning:{channel.Key}",
                    CommunityPageRoutes.WorldMapNightLearningDataset,
                    layerCode,
                    channel.국가코드,
                    CountryName(channel.국가코드),
                    center.Latitude + offset,
                    center.Longitude + offset,
                    channel.표시이름,
                    $"{channel.관점표시} · {string.Join(" · ", channel.주제코드목록.Select(TopicName))}",
                    "YouTube 공개 채널",
                    new DateTimeOffset(DateTime.SpecifyKind(channel.자료확인일시Utc, DateTimeKind.Utc)),
                    커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                    channel.공식출처Url);
            })
            .OrderBy(item => item.StableId, StringComparer.Ordinal)
            .ToArray();

    private static string ComputeRevision(IReadOnlyList<커뮤니티세계지도ObservationDto> observations)
    {
        var canonicalRows = observations
            .OrderBy(item => item.StableId, StringComparer.Ordinal)
            .Select(item => string.Join("|",
                item.StableId,
                item.LayerCode,
                item.Title,
                item.Summary,
                item.SourceName,
                item.EvidenceAsOfUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
                item.DetailHref,
                item.SourceHref,
                item.LocationPrecisionCode,
                item.MarketStageCode,
                item.MarkerStatusCode,
                item.ServiceRadiusKm,
                item.DailyCapacity,
                item.CommunityScopeKey,
                item.SourceDatasetKey,
                item.SourceUpdatedAtUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
                item.CollectedAtUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
                item.UpdateCycle,
                item.FreshnessCode,
                item.BoundaryNotice,
                MetricRevision(item.Metrics)));
        var canonical = string.Join("\n", canonicalRows);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    private static string MetricRevision(IReadOnlyList<커뮤니티세계지도MetricDto>? metrics)
        => metrics is null
            ? string.Empty
            : string.Join(",", metrics.Select(metric =>
                $"{metric.Code}:{metric.Value}:{metric.Unit}"));

    private static string CountryName(string countryCode)
        => countryCode switch
        {
            "KR" => "대한민국",
            "US" => "미국",
            "GB" => "영국",
            "FR" => "프랑스",
            "IN" => "인도",
            _ => countryCode
        };

    private static string TopicName(string topicCode)
        => topicCode switch
        {
            YouTube지식성찰주제코드.자기계발 => "자기계발",
            YouTube지식성찰주제코드.철학 => "철학",
            YouTube지식성찰주제코드.심리 => "심리",
            YouTube지식성찰주제코드.윤리 => "윤리",
            YouTube지식성찰주제코드.마음챙김 => "마음챙김",
            YouTube지식성찰주제코드.종교교육 => "종교교육",
            YouTube지식성찰주제코드.아이디어 => "아이디어",
            _ => topicCode
        };
}
