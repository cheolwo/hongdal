using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Services.Community;
using Ssalddel.Services.TraditionalMarkets;
using Ssalddel.Services.FoodCulture;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도조회UseCaseTests
{
    private readonly 커뮤니티세계지도조회UseCase _useCase = new(
        new Stub전통시장MapMarkerReader(),
        new Stub해외제조업소MapMarkerReader());

    [Fact]
    public async Task 낮Snapshot은_문화와가격을_서로다른Layer로제공한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        Assert.Equal(CommunityPageRoutes.WorldMapDayWorkDataset, snapshot.DatasetCode);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.RegionalCulture);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.PublicPrice);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.WholesaleMarket);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.TraditionalMarketHub);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.OverseasManufacturer);
        Assert.Contains(snapshot.Observations, item => item.StableId == "culture:us-maine");
        Assert.Contains(snapshot.Observations, item => item.StableId == "price:kr");
        Assert.Contains(snapshot.Observations, item => item.StableId == "overseas-manufacturer:us-california");
        Assert.All(snapshot.Observations, item => Assert.False(string.IsNullOrWhiteSpace(item.SourceName)));
    }

    [Fact]
    public async Task 낮Snapshot은_서울을제외한_97개지역이미지를_독립문화Marker로제공한다()
    {
        var regionMarkers = 지역문화행정구역대표점Catalog.All
            .Select((anchor, index) => new 지역문화이미지MapMarker(
                anchor.RegionKey,
                anchor.RegionKey.StartsWith("kr-", StringComparison.Ordinal) ? "KR"
                    : anchor.RegionKey.StartsWith("us-", StringComparison.Ordinal) ? "US" : "CN",
                "국가",
                $"SUB-{index:000}",
                anchor.RegionKey,
                anchor.Latitude,
                anchor.Longitude,
                "지역 생활문화 조사 요약",
                new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero)))
            .ToArray();
        var useCase = new 커뮤니티세계지도조회UseCase(
            new Stub전통시장MapMarkerReader(),
            new Stub해외제조업소MapMarkerReader(),
            new Stub지역문화이미지MapMarkerReader(regionMarkers));

        var snapshot = await useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);
        var culture = snapshot.Observations
            .Where(item => item.LayerCode == 커뮤니티세계지도LayerCodes.RegionalCulture)
            .ToArray();

        Assert.Equal(97, 지역문화행정구역대표점Catalog.All.Count);
        Assert.Equal(97, culture.Length);
        Assert.Equal(16, culture.Count(item => item.CountryCode == "KR"));
        Assert.Equal(50, culture.Count(item => item.CountryCode == "US"));
        Assert.Equal(31, culture.Count(item => item.CountryCode == "CN"));
        Assert.DoesNotContain(culture, item => item.StableId == "culture:kr-seoul");
        Assert.Equal(culture.Length, culture.Select(item => item.StableId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(culture, item =>
        {
            Assert.Equal(
                커뮤니티세계지도위치정밀도Codes.AdministrativeRegionRepresentative,
                item.LocationPrecisionCode);
            Assert.Contains("배송 주소가 아닙니다", item.Summary, StringComparison.Ordinal);
            Assert.StartsWith("https://", item.SourceHref, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task 낮Snapshot은_검증된전통시장거점을_공동구매원장근거로제공한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        var hub = Assert.Single(snapshot.Observations, item =>
            item.LayerCode == 커뮤니티세계지도LayerCodes.TraditionalMarketHub);
        Assert.Equal("traditional-market-hub:a001", hub.StableId);
        Assert.Equal("광장시장", hub.Title);
        Assert.Equal("KR", hub.CountryCode);
        Assert.Equal(3.5m, hub.ServiceRadiusKm);
        Assert.Equal(120, hub.DailyCapacity);
        Assert.Equal(TraditionalMarketLogisticsHubStatuses.Pilot, hub.MarkerStatusCode);
        Assert.Equal("traditional-market:a001", hub.CommunityScopeKey);
        Assert.Equal(TraditionalMarketMapLocationPrecisionCodes.MarketSiteRepresentative, hub.LocationPrecisionCode);
        Assert.StartsWith("https://", hub.SourceHref, StringComparison.Ordinal);
        Assert.Contains("공동 입고·수령", hub.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 낮Snapshot은_한국과미국개별도매시장을_좌표정밀도와공식출처로제공한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        var markets = snapshot.Observations
            .Where(item => item.LayerCode == 커뮤니티세계지도LayerCodes.WholesaleMarket)
            .ToArray();

        Assert.Equal(18, markets.Length);
        Assert.Equal(6, markets.Count(item => item.CountryCode == "KR"));
        Assert.Equal(12, markets.Count(item => item.CountryCode == "US"));
        Assert.Equal(markets.Length, markets.Select(item => item.StableId).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(markets, item => item.Title == "서울 가락동 농수산물도매시장");
        Assert.Contains(markets, item => item.Title == "New York Terminal Market");
        Assert.All(markets, item =>
        {
            Assert.InRange(item.Latitude, -90, 90);
            Assert.InRange(item.Longitude, -180, 180);
            Assert.NotNull(item.EvidenceAsOfUtc);
            Assert.StartsWith("https://", item.SourceHref, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(item.LocationPrecisionCode));
            Assert.False(string.IsNullOrWhiteSpace(item.MarketStageCode));
            Assert.Contains("배송 주소가 아닙니다", item.Summary, StringComparison.Ordinal);
        });
        Assert.All(markets.Where(item => item.CountryCode == "KR"), item =>
            Assert.Equal(커뮤니티도매시장위치정밀도Codes.시장대표점, item.LocationPrecisionCode));
        Assert.All(markets.Where(item => item.CountryCode == "US"), item =>
            Assert.Equal(커뮤니티도매시장위치정밀도Codes.도시중심점, item.LocationPrecisionCode));
    }

    [Fact]
    public async Task 낮Snapshot은_해외제조업소를_개별주소가아닌행정권역집계Marker로제공한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        var marker = Assert.Single(snapshot.Observations, item =>
            item.LayerCode == 커뮤니티세계지도LayerCodes.OverseasManufacturer);
        Assert.Equal("overseas-manufacturer:us-california", marker.StableId);
        Assert.Equal(12, marker.OrganizationCount);
        Assert.Equal(31, marker.EvidenceCount);
        Assert.Equal(
            커뮤니티세계지도위치정밀도Codes.AdministrativeRegionRepresentative,
            marker.LocationPrecisionCode);
        Assert.Contains("개별 제조업소 주소가 아닙니다", marker.Summary, StringComparison.Ordinal);
        Assert.Contains("원재료의 재배·어획 산지", marker.Summary, StringComparison.Ordinal);
        Assert.StartsWith("https://", marker.SourceHref, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 밤Snapshot은_배움과경전공부Layer를_구분한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapNightLearningDataset);

        Assert.Contains(snapshot.Observations, item => item.LayerCode == 커뮤니티세계지도LayerCodes.LearningChannel);
        Assert.Contains(snapshot.Observations, item => item.LayerCode == 커뮤니티세계지도LayerCodes.ScriptureAndClassics);
        Assert.All(snapshot.Observations, item => Assert.StartsWith("https://", item.DetailHref, StringComparison.Ordinal));
    }

    [Fact]
    public async Task 같은공개자료는_생성시각이달라도_같은Revision을유지한다()
    {
        var first = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);
        var second = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        Assert.Equal(first.Revision, second.Revision);
        Assert.NotEmpty(first.Revision);
    }

    [Fact]
    public async Task 알수없는Dataset은_거부한다()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.조회Async("unknown"));

        Assert.Contains("day-work", exception.Message);
    }

    private sealed class Stub전통시장MapMarkerReader : I전통시장MapMarkerReader
    {
        public Task<IReadOnlyList<TraditionalMarketMapMarkerResponse>> 공개Marker조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TraditionalMarketMapMarkerResponse>>(
            [
                new()
                {
                    MarketCode = "a001",
                    MarketName = "광장시장",
                    CommunityScopeKey = "traditional-market:a001",
                    HubReferenceKey = "traditional-market-hub:a001",
                    Province = "서울특별시",
                    CityCounty = "종로구",
                    Status = TraditionalMarketLogisticsHubStatuses.Pilot,
                    ServiceRadiusKm = 3.5m,
                    DailyGroupPurchaseCapacity = 120,
                    SupportsResidentPickup = true,
                    SupportsRefrigeratedStorage = true,
                    MapAnchor = new()
                    {
                        Latitude = 37.5701m,
                        Longitude = 126.9996m,
                        LocationPrecisionCode = TraditionalMarketMapLocationPrecisionCodes.MarketSiteRepresentative,
                        SourceName = "운영자 검증 좌표",
                        SourceHref = "https://example.test/markets/a001",
                        VerifiedAtUtc = new DateTime(2026, 8, 2, 1, 2, 3, DateTimeKind.Utc)
                    }
                }
            ]);
    }

    private sealed class Stub해외제조업소MapMarkerReader : I해외제조업소MapMarkerReader
    {
        public Task<IReadOnlyList<해외제조업소MapMarker>> 공개Marker조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<해외제조업소MapMarker>>(
            [
                new(
                    "us-california",
                    "US",
                    "미국",
                    "캘리포니아주",
                    36.7783,
                    -119.4179,
                    12,
                    31,
                    new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero),
                    "식품의약품안전처 해외제조업소 정보",
                    "https://www.data.go.kr/data/15073967/openapi.do",
                    "행정권역 대표점이며 개별 제조업소 주소가 아닙니다.")
            ]);
    }

    private sealed class Stub지역문화이미지MapMarkerReader(
        IReadOnlyList<지역문화이미지MapMarker> markers) : I지역문화이미지MapMarkerReader
    {
        public Task<IReadOnlyList<지역문화이미지MapMarker>> 공개Marker조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(markers);
    }
}
