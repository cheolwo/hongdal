using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Kamis중심UsdaAms가격비교QueryServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 29, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public void MappingCatalog는_동일품목과광의품목과후보없음을구분한다()
    {
        string[] available =
        [
            "Apples",
            "Lettuce",
            "Lettuce, Romaine",
            "Corn-Sweet"
        ];

        var apple = Kamis중심UsdaAms품목MappingCatalog.Resolve("411", available);
        var lettuce = Kamis중심UsdaAms품목MappingCatalog.Resolve("214", available);
        var rice = Kamis중심UsdaAms품목MappingCatalog.Resolve("111", available);

        Assert.Equal(
            Kamis중심UsdaAms매핑품질Codes.동일품목후보,
            apple.MatchQualityCode);
        Assert.Equal(["Apples"], apple.MatchedCommodities);
        Assert.Equal(
            Kamis중심UsdaAms매핑품질Codes.광의품목후보,
            lettuce.MatchQualityCode);
        Assert.Equal(
            ["Lettuce", "Lettuce, Romaine"],
            lettuce.MatchedCommodities);
        Assert.Equal(
            Kamis중심UsdaAms매핑상태Codes.후보없음,
            rice.MappingStatusCode);
    }

    [Fact]
    public async Task KAMIS품목코드순서로_국내가격과AMS시장단계가격을나란히반환한다()
    {
        await using var db = CreateDb();
        Seed(db);
        await db.SaveChangesAsync();
        var service = new Kamis중심UsdaAms가격비교QueryService(
            db,
            new FixedTimeProvider(Now),
            new FoodPriceCrosswalkCatalog());

        var result = await service.GetAsync(
            new Kamis중심UsdaAms가격비교Query
            {
                Year = 2026,
                PageSize = 100,
                KamisPointsPerItem = 20,
                AmsPointsPerStage = 5
            });

        Assert.Equal(Kamis중심UsdaAms가격비교상태Codes.완료, result.StatusCode);
        Assert.Equal(3, result.ObservedKamisItemCount);
        Assert.Equal(2, result.MappedKamisItemCount);
        Assert.Equal(1, result.UnmappedKamisItemCount);
        Assert.Equal(
            ["111", "152", "411"],
            result.Items.Select(item => item.KamisItemCode));

        var apple = Assert.Single(result.Items, item => item.KamisItemCode == "411");
        Assert.Equal(["Apples"], apple.MatchedAmsCommodities);
        Assert.False(apple.AllowsDirectPriceDifference);
        Assert.Equal("02", apple.KamisPricePoints[0].ProductClassCode);
        Assert.Contains(apple.KamisPricePoints, point => point.Unit == "1kg");
        Assert.Contains(
            apple.KamisPricePoints,
            point => point.SourcePackageLabel == "10개"
                     && point.ComparisonUnit == "1kg");
        Assert.Contains(apple.KamisPricePoints, point => point.PriceKrw == 5900m);
        Assert.DoesNotContain(apple.KamisPricePoints, point => point.PriceKrw == 9999m);
        Assert.Equal(
            [
                농수산시세시장단계Codes.산지출하,
                농수산시세시장단계Codes.도매터미널,
                농수산시세시장단계Codes.소매광고
            ],
            apple.AmsMarketStages.Select(stage => stage.MarketStageCode));

        var terminal = apple.AmsMarketStages.Single(stage =>
            stage.MarketStageCode == 농수산시세시장단계Codes.도매터미널);
        var terminalPoint = Assert.Single(terminal.PricePoints);
        Assert.Equal("USD", terminalPoint.CurrencyCode);
        Assert.Equal("package: cartons tray pack", terminalPoint.OriginalUnit);
        Assert.Equal(32m, terminalPoint.LowPrice);

        var retail = apple.AmsMarketStages.Single(stage =>
            stage.MarketStageCode == 농수산시세시장단계Codes.소매광고);
        Assert.Equal(1.99m, Assert.Single(retail.PricePoints).WeightedAveragePrice);

        Assert.Equal("kamis:400:411", apple.ProductCodeConnection.InternalProductKey);
        Assert.Contains(
            apple.ProductCodeConnection.HsClassificationCandidates,
            candidate => candidate.CodeScheme == "HS6"
                         && candidate.Code == "080810");
        Assert.Contains(
            apple.ProductCodeConnection.NationalTariffReviews,
            review => review.CodeScheme == "HTSUS10"
                      && review.RelationStatusCode
                         == Kamis중심상품코드연결상태Codes.전문가검토필요
                      && review.Code is null);

        var koreaWholesale = apple.DistributionStagePriceBands.Single(band =>
            band.CountryCode == "KR"
            && band.ComparisonStageCode == 농수산유통비교단계Codes.도매
            && band.OriginalUnit == "1kg");
        Assert.Equal(4300m, koreaWholesale.LowObservedPrice);
        Assert.Equal(40000m, koreaWholesale.HighObservedPrice);
        Assert.Equal("1kg", koreaWholesale.OriginalUnit);
        Assert.Contains("10개", koreaWholesale.SourcePackageLabels);
        Assert.Equal(
            KamisPriceUnitProvenanceParser.SourceKilogramConversionCode,
            koreaWholesale.PriceNormalizationCode);
        var usTerminal = apple.DistributionStagePriceBands.Single(band =>
            band.CountryCode == "US"
            && band.SourceMarketStageCode == 농수산시세시장단계Codes.도매터미널);
        Assert.Equal(32m, usTerminal.LowObservedPrice);
        Assert.Equal(36m, usTerminal.HighObservedPrice);
        Assert.False(usTerminal.AllowsDirectComparison);
    }

    [Fact]
    public async Task 매핑품목필터와잘못된빈도조건을구분한다()
    {
        await using var db = CreateDb();
        Seed(db);
        await db.SaveChangesAsync();
        var service = new Kamis중심UsdaAms가격비교QueryService(
            db,
            new FixedTimeProvider(Now),
            new FoodPriceCrosswalkCatalog());

        var mapped = await service.GetAsync(
            new Kamis중심UsdaAms가격비교Query
            {
                Year = 2026,
                OnlyMapped = true,
                PageSize = 100
            });

        Assert.Equal(["152", "411"], mapped.Items.Select(item => item.KamisItemCode));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAsync(new Kamis중심UsdaAms가격비교Query
            {
                Year = 2026,
                FrequencyCode = "Weekly"
            }));
    }

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static void Seed(AgriculturalFisheriesDbContext db)
    {
        var kamisRun = new KamisPriceCollectionRun
        {
            RequestedDate = new DateOnly(2026, 7, 28),
            StatusCode = KamisArchiveStatusCodes.Completed
        };
        db.KamisCollectionRuns.Add(kamisRun);
        db.KamisPriceObservations.AddRange(
            Kamis(kamisRun, "100", "식량작물", "111", "쌀", "02", 3100m),
            Kamis(kamisRun, "100", "식량작물", "152", "감자", "02", 2600m),
            Kamis(kamisRun, "400", "과일류", "411", "사과", "01", 5900m),
            Kamis(kamisRun, "400", "과일류", "411", "사과", "02", 4300m),
            Kamis(
                kamisRun,
                "400",
                "과일류",
                "411",
                "사과",
                "02",
                40000m,
                sourcePackageLabel: "10개",
                kindCode: "01"),
            Kamis(
                kamisRun,
                "400",
                "과일류",
                "411",
                "사과",
                "01",
                9999m,
                new DateOnly(2026, 1, 2)));

        var amsRun = new UsdaAms시장가격수집Run
        {
            DateFrom = new DateOnly(2026, 1, 1),
            DateTo = new DateOnly(2026, 7, 29),
            StatusCode = UsdaAms시장가격Archive상태Codes.완료
        };
        db.UsdaAmsMarketPriceCollectionRuns.Add(amsRun);
        db.UsdaAmsMarketPriceObservations.AddRange(
            Ams(
                amsRun,
                "shipping-apple",
                농수산시세정보원Keys.UsdaAms산지출하가격,
                농수산시세시장단계Codes.산지출하,
                "Apples",
                "package: cartons",
                low: 27m,
                high: 30m),
            Ams(
                amsRun,
                "terminal-apple",
                농수산시세정보원Keys.UsdaAms도매터미널가격,
                농수산시세시장단계Codes.도매터미널,
                "Apples",
                "package: cartons tray pack",
                low: 32m,
                high: 36m),
            Ams(
                amsRun,
                "retail-apple",
                농수산시세정보원Keys.UsdaAms소매광고가격,
                농수산시세시장단계Codes.소매광고,
                "Apples",
                "item size: per lb",
                weightedAverage: 1.99m),
            Ams(
                amsRun,
                "terminal-potato",
                농수산시세정보원Keys.UsdaAms도매터미널가격,
                농수산시세시장단계Codes.도매터미널,
                "Potatoes",
                "package: 50 lb cartons",
                low: 20m,
                high: 24m));
    }

    private static KamisPriceObservation Kamis(
        KamisPriceCollectionRun run,
        string categoryCode,
        string categoryName,
        string itemCode,
        string itemName,
        string productClassCode,
        decimal price,
        DateOnly? surveyDate = null,
        string unit = "1kg",
        string sourcePackageLabel = "1kg",
        string kindCode = "00")
        => new()
        {
            FirstCollectionRun = run,
            RecordKey = $"{itemCode}-{productClassCode}-{kindCode}-{unit}-{surveyDate:yyyyMMdd}",
            ProductClassCode = productClassCode,
            ProductClassName = productClassCode == "02" ? "도매" : "소매",
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            SurveyDate = surveyDate ?? new DateOnly(2026, 7, 28),
            RequestedDate = new DateOnly(2026, 7, 28),
            FrequencyCode = "Daily",
            ItemCode = itemCode,
            ItemName = itemName,
            KindCode = kindCode,
            KindName = "대표",
            RankCode = "04",
            RankName = "상품",
            Unit = unit,
            SourcePackageLabel = sourcePackageLabel,
            ComparisonUnit = unit,
            PriceNormalizationCode =
                KamisPriceUnitProvenanceParser.SourceKilogramConversionCode,
            PriceNormalizationBasis =
                KamisPriceUnitProvenanceParser.SourceKilogramConversionBasis,
            PriceKrw = price,
            PriceRaw = price.ToString(),
            RawJson = "{}"
        };

    private static UsdaAms시장가격관측 Ams(
        UsdaAms시장가격수집Run run,
        string recordKey,
        string sourceKey,
        string marketStageCode,
        string commodity,
        string originalUnit,
        decimal? low = null,
        decimal? high = null,
        decimal? weightedAverage = null)
        => new()
        {
            FirstCollectionRun = run,
            RecordKey = recordKey,
            SourceKey = sourceKey,
            MarketStageCode = marketStageCode,
            ReportBeginDate = new DateOnly(2026, 7, 28),
            ReportEndDate = new DateOnly(2026, 7, 28),
            MarketType = marketStageCode,
            Commodity = commodity,
            Variety = "Representative",
            Grade = "U.S. One",
            MarketLocationName = "Test Market",
            MarketLocationState = "WA",
            LowPrice = low,
            HighPrice = high,
            WeightedAveragePrice = weightedAverage,
            OriginalUnit = originalUnit,
            CurrencyCode = "USD",
            RawJson = "{}"
        };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
