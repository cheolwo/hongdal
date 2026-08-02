using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Domain.TraditionalMarkets;
using Ssalddel.Infrastructure.Persistence.TraditionalMarkets;
using Ssalddel.Services.TraditionalMarkets;

namespace Ssalddel.Tests.Services.TraditionalMarkets;

public sealed class 전통시장MapMarkerReaderTests
{
    [Fact]
    public async Task 거점수정은_관리자가확인한좌표출처와검증시각을_별도로저장한다()
    {
        await using var db = CreateContext();
        AddMarket(db, "anchor", "좌표 검증 시장");
        await db.SaveChangesAsync();
        var service = new TraditionalMarketLogisticsHubService(db);

        var response = await service.UpsertAsync(
            "anchor",
            new TraditionalMarketLogisticsHubUpsertRequest
            {
                OperatorOrganizationName = "시장 상인회",
                ServiceRadiusKm = 2.5m,
                DailyGroupPurchaseCapacity = 80,
                SupportsBulkReceiving = true,
                SupportsSorting = true,
                SupportsResidentPickup = true,
                HasOperatorConsent = true,
                IsSiteVerified = true,
                MapAnchor = new()
                {
                    Latitude = 37.5701m,
                    Longitude = 126.9996m,
                    LocationPrecisionCode = TraditionalMarketMapLocationPrecisionCodes.MarketSiteRepresentative,
                    SourceName = "Google Maps 관리자 확인",
                    SourceHref = "https://maps.google.com/?q=37.5701,126.9996",
                    ConfirmCoordinateVerification = true
                }
            },
            "admin-1");

        Assert.NotNull(response.MapAnchor);
        Assert.Equal(37.5701m, response.MapAnchor.Latitude);
        Assert.Equal("Google Maps 관리자 확인", response.MapAnchor.SourceName);
        Assert.NotEqual(default, response.MapAnchor.VerifiedAtUtc);
        var stored = await db.LogisticsHubs.SingleAsync();
        Assert.Equal("admin-1", stored.MapLocationVerifiedByUserId);
    }

    [Fact]
    public async Task 거점수정은_출처없는좌표나_확인하지않은좌표를_거부한다()
    {
        await using var db = CreateContext();
        AddMarket(db, "invalid", "좌표 미확인 시장");
        await db.SaveChangesAsync();
        var service = new TraditionalMarketLogisticsHubService(db);
        var request = new TraditionalMarketLogisticsHubUpsertRequest
        {
            MapAnchor = new()
            {
                Latitude = 37.5701m,
                Longitude = 126.9996m,
                LocationPrecisionCode = TraditionalMarketMapLocationPrecisionCodes.MarketAddressGeocoded,
                SourceName = "출처",
                SourceHref = "https://example.test/market",
                ConfirmCoordinateVerification = false
            }
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpsertAsync("invalid", request, "admin-1"));

        Assert.Contains("일치함을 확인", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 공개Marker조회Async는_공개상태와동의_현장및좌표검증을_모두요구한다()
    {
        await using var db = CreateContext();
        var now = new DateTime(2026, 8, 2, 1, 2, 3, DateTimeKind.Utc);
        AddMarket(db, "ready", "표시 시장");
        AddHub(db, "ready", TraditionalMarketLogisticsHubStatuses.Pilot, now, hasMapAnchor: true);
        AddMarket(db, "no-map", "좌표 없는 시장");
        AddHub(db, "no-map", TraditionalMarketLogisticsHubStatuses.Active, now, hasMapAnchor: false);
        AddMarket(db, "review", "검토 시장");
        AddHub(db, "review", TraditionalMarketLogisticsHubStatuses.UnderReview, now, hasMapAnchor: true);
        AddMarket(db, "no-consent", "동의 없는 시장");
        AddHub(db, "no-consent", TraditionalMarketLogisticsHubStatuses.Pilot, now, hasMapAnchor: true, hasConsent: false);
        await db.SaveChangesAsync();

        var markers = await new 전통시장MapMarkerReader(db).공개Marker조회Async();

        var marker = Assert.Single(markers);
        Assert.Equal("ready", marker.MarketCode);
        Assert.Equal("표시 시장", marker.MarketName);
        Assert.Equal("traditional-market:ready", marker.CommunityScopeKey);
        Assert.Equal(37.5701m, marker.MapAnchor.Latitude);
        Assert.Equal(TraditionalMarketMapLocationPrecisionCodes.MarketSiteRepresentative, marker.MapAnchor.LocationPrecisionCode);
    }

    private static TraditionalMarketDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TraditionalMarketDbContext>()
            .UseInMemoryDatabase($"traditional-market-map-{Guid.NewGuid():N}")
            .Options;
        return new TraditionalMarketDbContext(options);
    }

    private static void AddMarket(TraditionalMarketDbContext db, string code, string name)
        => db.Markets.Add(new TraditionalMarket
        {
            MarketCode = code,
            Name = name,
            Province = "서울특별시",
            CityCounty = "종로구",
            SourceDatasetKey = "test",
            SourceHash = new string('a', 64),
            SourceReferenceDate = new DateOnly(2026, 8, 1),
            IsActive = true
        });

    private static void AddHub(
        TraditionalMarketDbContext db,
        string code,
        string status,
        DateTime now,
        bool hasMapAnchor,
        bool hasConsent = true)
        => db.LogisticsHubs.Add(new TraditionalMarketLogisticsHub
        {
            MarketCode = code,
            Status = status,
            HasOperatorConsent = hasConsent,
            SiteVerifiedAtUtc = now,
            ServiceRadiusKm = 3.5m,
            DailyGroupPurchaseCapacity = 120,
            SupportsResidentPickup = true,
            MapLatitude = hasMapAnchor ? 37.5701m : null,
            MapLongitude = hasMapAnchor ? 126.9996m : null,
            MapLocationPrecisionCode = hasMapAnchor
                ? TraditionalMarketMapLocationPrecisionCodes.MarketSiteRepresentative
                : string.Empty,
            MapLocationSourceName = hasMapAnchor ? "검증 출처" : string.Empty,
            MapLocationSourceHref = hasMapAnchor ? "https://example.test/markets/ready" : string.Empty,
            MapLocationVerifiedAtUtc = hasMapAnchor ? now : null
        });
}
