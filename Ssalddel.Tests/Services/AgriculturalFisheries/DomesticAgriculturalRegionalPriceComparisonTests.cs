using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Controllers.Common;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class DomesticAgriculturalRegionalPriceComparisonTests
{
    [Fact]
    public async Task 최신정산일의_품목과지역선택지를_제공한다()
    {
        await using var db = CreateDb();
        db.DomesticAuctionPriceObservations.AddRange(
            Observation("사과", "홍로", "11", "서울", "110001", new(2024, 9, 1)),
            Observation("배", "신고", "41", "경기", "110001", new(2024, 9, 2)),
            Observation("사과", "아오리", "44", "충남", "250001", new(2024, 9, 2)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var response = await service.GetOptionsAsync(new 농산물지역가격비교선택지요청
        {
            ItemName = "사과"
        });

        Assert.True(response.Success);
        Assert.Equal(new DateOnly(2024, 9, 2), response.SettlementDate);
        Assert.Equal(["배", "사과"], response.ItemNames);
        Assert.Equal(["아오리"], response.VarietyNames);
        Assert.Equal("충남", Assert.Single(response.OriginRegions).Name);
        Assert.Equal("250001", Assert.Single(response.WholesaleMarkets).Code);
    }

    [Fact]
    public async Task 원산지별_거래중량가중_원kg가격을_계산한다()
    {
        await using var db = CreateDb();
        db.DomesticAuctionPriceObservations.AddRange(
            Observation(
                "사과", "홍로", "11", "서울", "110001", new(2024, 9, 2),
                unitWeight: 10m, quantity: 2m, auctionPrice: 30000m),
            Observation(
                "사과", "홍로", "11", "서울", "110001", new(2024, 9, 2),
                unitWeight: 5m, quantity: 2m, auctionPrice: 20000m),
            Observation(
                "사과", "홍로", "26", "부산", "260001", new(2024, 9, 2),
                unitWeight: 10m, quantity: 1m, auctionPrice: 50000m));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var response = await service.CompareAsync(new 농산물지역가격비교요청
        {
            ItemName = "사과",
            VarietyName = "홍로",
            StartDate = "2024-09-02",
            EndDate = "2024-09-02"
        });

        Assert.True(response.Success);
        Assert.Equal(3750m, response.OverallAveragePriceKrwPerKg);
        Assert.Equal(2, response.Regions.Count);
        var seoul = Assert.Single(response.Regions, item => item.RegionName == "서울");
        Assert.Equal(30m, seoul.TotalQuantityKg);
        Assert.Equal(3333.33m, seoul.AveragePriceKrwPerKg);
        Assert.Equal(3000m, seoul.MinimumPriceKrwPerKg);
        Assert.Equal(4000m, seoul.MaximumPriceKrwPerKg);
        Assert.Equal(88.9m, seoul.ComparisonIndex);
    }

    [Fact]
    public async Task 비교기간은_31일을_초과할수없다()
    {
        await using var db = CreateDb();
        var response = await CreateService(db).CompareAsync(new 농산물지역가격비교요청
        {
            ItemName = "사과",
            StartDate = "2024-01-01",
            EndDate = "2024-02-01"
        });

        Assert.False(response.Success);
        Assert.Equal(국내농산물경락가격조회상태Codes.잘못된요청, response.StatusCode);
        Assert.Contains("31일", response.ErrorMessage);
    }

    [Fact]
    public async Task 날짜를_생략하면_해당품목의_최신정산일만_비교한다()
    {
        await using var db = CreateDb();
        db.DomesticAuctionPriceObservations.AddRange(
            Observation("사과", "홍로", "11", "서울", "110001", new(2024, 9, 1)),
            Observation("사과", "홍로", "26", "부산", "260001", new(2024, 9, 2)));
        await db.SaveChangesAsync();

        var response = await CreateService(db).CompareAsync(new 농산물지역가격비교요청
        {
            ItemName = " 사과 "
        });

        Assert.True(response.Success);
        Assert.Equal(new DateOnly(2024, 9, 2), response.ResolvedStartDate);
        Assert.Equal(response.ResolvedStartDate, response.ResolvedEndDate);
        Assert.Equal("사과", response.Query.ItemName);
        Assert.Equal("부산", Assert.Single(response.Regions).RegionName);
    }

    [Fact]
    public async Task 도매시장별로_정규화된_원kg가격을_비교한다()
    {
        await using var db = CreateDb();
        db.DomesticAuctionPriceObservations.AddRange(
            Observation(
                "배", "신고", "41", "경기", "110001", new(2024, 9, 2),
                unitWeight: 10m, quantity: 2m, auctionPrice: 20000m),
            Observation(
                "배", "신고", "41", "경기", "260001", new(2024, 9, 2),
                unitWeight: 5m, quantity: 2m, auctionPrice: 15000m));
        await db.SaveChangesAsync();

        var response = await CreateService(db).CompareAsync(new 농산물지역가격비교요청
        {
            ItemName = "배",
            RegionBasisCode = " wholesalemarket "
        });

        Assert.True(response.Success);
        Assert.Equal(농산물지역가격비교기준Codes.도매시장, response.Query.RegionBasisCode);
        Assert.Collection(
            response.Regions,
            first =>
            {
                Assert.Equal("110001", first.RegionCode);
                Assert.Equal(2000m, first.AveragePriceKrwPerKg);
            },
            second =>
            {
                Assert.Equal("260001", second.RegionCode);
                Assert.Equal(3000m, second.AveragePriceKrwPerKg);
            });
    }

    [Fact]
    public async Task 품목이나_지역기준이_유효하지않으면_잘못된요청을_반환한다()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var missingItem = await service.CompareAsync(new 농산물지역가격비교요청
        {
            ItemName = null!
        });
        var invalidRegionBasis = await service.CompareAsync(new 농산물지역가격비교요청
        {
            ItemName = "사과",
            RegionBasisCode = "Province"
        });

        Assert.False(missingItem.Success);
        Assert.Equal(국내농산물경락가격조회상태Codes.잘못된요청, missingItem.StatusCode);
        Assert.False(invalidRegionBasis.Success);
        Assert.Equal(
            국내농산물경락가격조회상태Codes.잘못된요청,
            invalidRegionBasis.StatusCode);
    }

    [Fact]
    public void MySql공급자가_지역별집계를_GroupBy_SQL로_변환한다()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_query_test;User=test;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        using var db = new AgriculturalFisheriesDbContext(options);
        var baseQuery = db.DomesticAuctionPriceObservations
            .Where(item =>
                item.SourceKey
                    == 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement
                && item.ItemName == "사과"
                && item.AuctionPriceKrw.HasValue
                && item.UnitWeight.HasValue
                && item.UnitWeight > 0m);

        var originSql = 농산물지역가격비교QueryService
            .BuildOriginAggregateQuery(baseQuery)
            .ToQueryString();
        var marketSql = 농산물지역가격비교QueryService
            .BuildMarketAggregateQuery(baseQuery)
            .ToQueryString();

        Assert.Contains("GROUP BY", originSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OriginName", originSql, StringComparison.Ordinal);
        Assert.Contains("GROUP BY", marketSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WholesaleMarketCode", marketSql, StringComparison.Ordinal);
    }

    [Fact]
    public void 공개Controller가_선택지와_지역비교_API를_노출한다()
    {
        var controllerType = typeof(국내농산물경락가격Controller);

        Assert.NotNull(controllerType.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "api/v1/agricultural-fisheries/domestic-auction-prices",
            controllerType.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal(
            "archive/comparison-options",
            controllerType
                .GetMethod(nameof(국내농산물경락가격Controller.지역가격비교선택지조회))
                ?.GetCustomAttribute<HttpGetAttribute>()
                ?.Template);
        Assert.Equal(
            "archive/region-comparison",
            controllerType
                .GetMethod(nameof(국내농산물경락가격Controller.지역가격비교))
                ?.GetCustomAttribute<HttpGetAttribute>()
                ?.Template);
    }

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase($"regional-price-comparison-{Guid.NewGuid():N}")
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static 농산물지역가격비교QueryService CreateService(
        AgriculturalFisheriesDbContext db)
        => new(db, new StubSourceService());

    private static 국내농산물경락가격관측 Observation(
        string itemName,
        string varietyName,
        string originCode,
        string originName,
        string marketCode,
        DateOnly settlementDate,
        decimal unitWeight = 10m,
        decimal quantity = 1m,
        decimal auctionPrice = 30000m)
        => new()
        {
            FirstCollectionRunId = 1,
            RecordKey = Guid.NewGuid().ToString("N"),
            SourceKey = 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
            SettlementDate = settlementDate,
            WholesaleMarketCode = marketCode,
            CorporationCode = "corporation",
            ItemName = itemName,
            VarietyName = varietyName,
            UnitWeight = unitWeight,
            Quantity = quantity,
            AuctionPriceKrw = auctionPrice,
            OriginCode = originCode,
            OriginName = originName,
            TotalQuantity = unitWeight * quantity,
            TotalAmountKrw = auctionPrice * quantity,
            LastSeenAtUtc = new DateTime(2024, 9, 2, 0, 0, 0, DateTimeKind.Utc)
        };

    private sealed class StubSourceService : I국내농산물경락가격조회Service
    {
        public IReadOnlyList<국내농산물경락가격원천응답> GetSources()
            =>
            [
                new()
                {
                    Key = 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
                    Provider = "농림축산식품부",
                    DisplayName = "전국 공영도매시장 경매원천 정산가격",
                    IsConfigured = true
                }
            ];

        public Task<국내농산물경락가격조회응답> 조회Async(
            국내농산물경락가격조회요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
