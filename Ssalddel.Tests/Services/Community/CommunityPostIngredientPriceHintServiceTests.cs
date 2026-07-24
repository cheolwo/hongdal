using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.Community;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityPostIngredientPriceHintServiceTests
{
    [Fact]
    public async Task 본문의_사과_배_복숭아를_보관된_Kamis가격과연결한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var run = new KamisPriceCollectionRun
        {
            StatusCode = KamisArchiveStatusCodes.Completed,
            RequestedDate = new DateOnly(2026, 7, 22)
        };
        db.KamisPriceObservations.AddRange(
            Observation(run, "apple", "411", "사과", 31_000m),
            Observation(run, "pear", "412", "배", 37_000m),
            Observation(run, "peach", "413", "복숭아", 28_000m));
        await db.SaveChangesAsync();
        var service = new CommunityPostIngredientPriceHintService(
            db,
            new FoodPriceCrosswalkCatalog(),
            new TestTimeProvider());

        var result = await service.GetHintsAsync(
            new CommunityPostIngredientPriceHintRequest(
                "사과 배 복숭아를 함께 사서 나누고 싶습니다."));

        Assert.Equal(["사과", "배", "복숭아"], result.Hints.Select(hint => hint.IngredientName));
        Assert.All(result.Hints, hint => Assert.True(hint.HasPrice));
        Assert.Equal(31_000m, result.Hints[0].AveragePrice);
        Assert.True(result.Hints[1].RequiresConfirmation);
        Assert.Contains("실시간 판매가", result.Notice, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 배추와_배가고프다는_과일배로오인하지않는다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var service = new CommunityPostIngredientPriceHintService(
            db,
            new FoodPriceCrosswalkCatalog(),
            new TestTimeProvider());

        var result = await service.GetHintsAsync(
            new CommunityPostIngredientPriceHintRequest("배추김치를 먹었더니 배가 고프다."));

        Assert.Empty(result.Hints);
    }

    [Fact]
    public async Task 본문길이제한을넘으면_조회하지않는다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        var service = new CommunityPostIngredientPriceHintService(
            db,
            new FoodPriceCrosswalkCatalog(),
            new TestTimeProvider());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetHintsAsync(
                new CommunityPostIngredientPriceHintRequest(new string('사', 4_001))));
    }

    private static KamisPriceObservation Observation(
        KamisPriceCollectionRun run,
        string key,
        string itemCode,
        string itemName,
        decimal price)
        => new()
        {
            FirstCollectionRun = run,
            RecordKey = key,
            ProductClassCode = "01",
            ProductClassName = "소매",
            CategoryCode = "400",
            CategoryName = "과일류",
            CountryCode = "ALL",
            CountryName = "전국",
            RequestedDate = new DateOnly(2026, 7, 22),
            SurveyDate = new DateOnly(2026, 7, 22),
            FrequencyCode = "Daily",
            ItemName = itemName,
            ItemCode = itemCode,
            KindName = itemName,
            KindCode = "00",
            RankName = "상품",
            RankCode = "04",
            Unit = "10개",
            PriceRaw = price.ToString(),
            PriceKrw = price,
            SourceUrl = "https://www.kamis.or.kr/service/price/xml.do",
            LastSeenAtUtc = new DateTime(2026, 7, 22, 23, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 7, 22, 23, 0, 0, DateTimeKind.Utc)
        };

    private sealed class TestTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(2026, 7, 23, 0, 0, 0, TimeSpan.Zero);
    }
}
