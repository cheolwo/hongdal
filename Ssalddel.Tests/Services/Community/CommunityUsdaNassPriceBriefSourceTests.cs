using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityUsdaNassPriceBriefSourceTests
{
    [Fact]
    public async Task BuildAsync_UsesOnlyLatestConsolidatedProducerPriceSeries()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var run = new UsdaNassPriceCollectionRun
        {
            StatusCode = UsdaNassArchiveStatusCodes.Completed,
            YearFrom = 2025
        };
        db.CollectionRuns.Add(run);
        db.PriceObservations.AddRange(
            Observation(run, "rice-total", "RICE", "ALL CLASSES", "$ / CWT", 22.50m),
            Observation(run, "rice-specific", "RICE", "LONG GRAIN", "$ / CWT", 99m),
            Observation(
                run,
                "corn-retail",
                "CORN",
                "ALL CLASSES",
                "$ / BU",
                7m,
                "RETAIL PRICE"));
        await db.SaveChangesAsync();
        var source = new CommunityUsdaNassPriceBriefSource(
            db,
            Options.Create(new CommunityEditorialBatchOptions
            {
                UsdaNassPriceBriefMaxItems = 5
            }));

        var draft = await source.BuildAsync(
            new DateOnly(2026, 7, 21),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));

        Assert.NotNull(draft);
        Assert.Equal(CommunityAutomatedPostSourceKeys.UsdaNassPriceBrief, draft.SourceKey);
        Assert.Equal("202606", draft.PeriodKey);
        Assert.Equal(CommunityBoardCatalog.PeriodicDataUsda.DisplayName, draft.Category);
        Assert.Equal(CultureTransportContentCatalog.PriceEvidenceWorkflowTag, draft.WorkflowTag);
        Assert.StartsWith("[문화교통·가격]", draft.Title, StringComparison.Ordinal);
        Assert.Contains("USD 22.50/CWT", draft.Body);
        Assert.DoesNotContain("99.00", draft.Body);
        Assert.DoesNotContain("CORN", draft.Body);
        Assert.Contains("미국 소매가격이 아닙니다", draft.Body);
    }

    [Fact]
    public void BuildBody_PreservesNonDollarOriginalUnitAndComparisonBoundary()
    {
        var body = CommunityUsdaNassPriceBriefSource.BuildBody(
            new DateOnly(2026, 6, 1),
            [
                new UsdaNassPriceBriefItem(
                    "ANIMALS & PRODUCTS",
                    "POULTRY PRODUCTS",
                    "EGGS",
                    "CENTS / DOZEN",
                    132m)
            ]);

        Assert.Contains("132.00 CENTS / DOZEN", body);
        Assert.Contains("생산자 수취가격", body);
        Assert.Contains("not endorsed or certified by NASS", body);
        Assert.Contains("한국 유통가격", body);
    }

    private static UsdaNassPriceObservation Observation(
        UsdaNassPriceCollectionRun run,
        string key,
        string commodity,
        string classDescription,
        string unit,
        decimal value,
        string statisticCategory = "PRICE RECEIVED")
        => new()
        {
            FirstCollectionRun = run,
            RecordKey = key,
            SourceDesc = "SURVEY",
            SectorDesc = "CROPS",
            GroupDesc = "FIELD CROPS",
            CommodityDesc = commodity,
            ClassDesc = classDescription,
            UtilPracticeDesc = "ALL UTILIZATION PRACTICES",
            ProductionPracticeDesc = "ALL PRODUCTION PRACTICES",
            StatisticCategoryDesc = statisticCategory,
            UnitDesc = unit,
            DomainDesc = "TOTAL",
            AggregationLevelDesc = "NATIONAL",
            CountryCode = "9000",
            CountryName = "UNITED STATES",
            Year = 2026,
            FrequencyDesc = "MONTHLY",
            BeginCode = "06",
            EndCode = "06",
            ReferencePeriodDesc = "JUN",
            ValueRaw = value.ToString(),
            NumericValue = value,
            SourceUrl = "https://quickstats.nass.usda.gov/api",
            LastSeenAtUtc = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)
        };
}
