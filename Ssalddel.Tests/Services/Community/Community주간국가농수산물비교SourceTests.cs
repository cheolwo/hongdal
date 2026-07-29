using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Ssalddel.Services.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Community;

public sealed class Community주간국가농수산물비교SourceTests
{
    [Fact]
    public async Task 완료주를_한미중품목별Snapshot으로저장하고_원단위를보존한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        SeedPriceObservations(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var snapshot = await service.UpsertPreviousCompletedWeekAsync(
            new DateOnly(2026, 7, 27));

        Assert.NotNull(snapshot);
        Assert.Equal("2026-W30", snapshot.PeriodKey);
        Assert.Equal(new DateOnly(2026, 7, 20), snapshot.WeekStartDate);
        Assert.Equal(new DateOnly(2026, 7, 26), snapshot.WeekEndDate);
        Assert.Equal(2, snapshot.AvailableObservationCount);
        Assert.Equal(3, snapshot.Items.Count);

        var korea = Assert.Single(snapshot.Items, item => item.CountryCode == "KR");
        Assert.Equal(주간국가농수산물비교상태Codes.관측값있음, korea.StatusCode);
        Assert.Equal("KRW", korea.CurrencyCode);
        Assert.Equal("10kg", korea.Unit);
        Assert.Equal(32000m, korea.Price);

        var unitedStates = Assert.Single(snapshot.Items, item => item.CountryCode == "US");
        Assert.Equal(주간국가농수산물비교상태Codes.관측값있음, unitedStates.StatusCode);
        Assert.Equal("USD", unitedStates.CurrencyCode);
        Assert.Equal("$ / CWT", unitedStates.Unit);
        Assert.Equal(41.25m, unitedStates.Price);

        var china = Assert.Single(snapshot.Items, item => item.CountryCode == "CN");
        Assert.Equal(주간국가농수산물비교상태Codes.원천미등록, china.StatusCode);
        Assert.Null(china.Price);
        Assert.Contains("임의 생성", china.ComparisonNote);
    }

    [Fact]
    public async Task 같은주재실행은_Snapshot을중복하지않고_커뮤니티글도같은기간Key를사용한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        SeedPriceObservations(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var source = new Community주간국가농수산물비교Source(service);

        var first = await source.BuildAsync(
            new DateOnly(2026, 7, 27),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));
        var second = await source.BuildAsync(
            new DateOnly(2026, 7, 29),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal("2026-W30", first.PeriodKey);
        Assert.Equal(first.PeriodKey, second.PeriodKey);
        Assert.Equal(CommunityBoardCatalog.InformationPrices.DisplayName, first.Category);
        Assert.Single(await db.WeeklyCountryProductComparisonSnapshots.ToListAsync());
        Assert.Equal(3, await db.WeeklyCountryProductComparisonItems.CountAsync());
        Assert.Contains("KRW 32,000/10kg", first.Body);
        Assert.Contains("USD 41.25/CWT", first.Body);
        Assert.Contains("중국: 자료 없음", first.Body);
        Assert.Contains("가격차나 순위를 계산하지 않고", first.Body);
    }

    [Fact]
    public async Task 검증된관측값이하나도없으면_Snapshot과빈글을만들지않는다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        var source = new Community주간국가농수산물비교Source(CreateService(db));

        var draft = await source.BuildAsync(
            new DateOnly(2026, 7, 27),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));

        Assert.Null(draft);
        Assert.Empty(await db.WeeklyCountryProductComparisonSnapshots.ToListAsync());
    }

    private static AgriculturalFisheriesDbContext CreateContext(SqliteConnection connection)
        => new(new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options);

    private static 주간국가농수산물비교SnapshotService CreateService(
        AgriculturalFisheriesDbContext db)
        => new(
            db,
            Options.Create(new CommunityEditorialBatchOptions
            {
                WeeklyCountryProductComparisonMaxProducts = 1,
                WeeklyCountryProductComparisonMaxObservationAgeDays = 62
            }),
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 27, 0, 0, 0, TimeSpan.Zero)));

    private static void SeedPriceObservations(AgriculturalFisheriesDbContext db)
    {
        var kamisRun = new KamisPriceCollectionRun
        {
            StatusCode = KamisArchiveStatusCodes.Completed,
            RequestedDate = new DateOnly(2026, 7, 25)
        };
        db.KamisCollectionRuns.Add(kamisRun);
        db.KamisPriceObservations.Add(new KamisPriceObservation
        {
            FirstCollectionRun = kamisRun,
            RecordKey = "kamis-apple-20260725",
            ProductClassName = "소매",
            CategoryName = "과일류",
            ItemName = "사과",
            KindName = "후지",
            RankName = "상품",
            Unit = "10kg",
            RequestedDate = new DateOnly(2026, 7, 25),
            SurveyDate = new DateOnly(2026, 7, 25),
            FrequencyCode = "Daily",
            PriceRaw = "32000",
            PriceKrw = 32000m,
            SourceUrl = "https://www.kamis.or.kr"
        });

        var usdaRun = new UsdaNassPriceCollectionRun
        {
            StatusCode = UsdaNassArchiveStatusCodes.Completed,
            YearFrom = 2025
        };
        db.CollectionRuns.Add(usdaRun);
        db.PriceObservations.Add(new UsdaNassPriceObservation
        {
            FirstCollectionRun = usdaRun,
            RecordKey = "usda-apples-202606",
            SourceDesc = "SURVEY",
            SectorDesc = "CROPS",
            GroupDesc = "FRUIT & TREE NUTS",
            CommodityDesc = "APPLES",
            ClassDesc = "ALL CLASSES",
            UtilPracticeDesc = "ALL UTILIZATION PRACTICES",
            ProductionPracticeDesc = "ALL PRODUCTION PRACTICES",
            StatisticCategoryDesc = "PRICE RECEIVED",
            UnitDesc = "$ / CWT",
            DomainDesc = "TOTAL",
            AggregationLevelDesc = "NATIONAL",
            Year = 2026,
            FrequencyDesc = "MONTHLY",
            BeginCode = "06",
            EndCode = "06",
            ReferencePeriodDesc = "JUN",
            NumericValue = 41.25m,
            SourceUrl = "https://quickstats.nass.usda.gov/api"
        });
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
