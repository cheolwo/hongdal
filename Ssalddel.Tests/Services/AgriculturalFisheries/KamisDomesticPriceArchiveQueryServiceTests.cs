using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class KamisDomesticPriceArchiveQueryServiceTests
{
    [Fact]
    public async Task 수산물은_저장된Kamis최신관측을_도소매별1kg가격으로조회한다()
    {
        await using var db = CreateDb();
        db.KamisPriceObservations.AddRange(
            Observation(1, "01", "05", new DateOnly(2026, 8, 1), 12_000m),
            Observation(2, "01", "05", new DateOnly(2026, 8, 2), 13_000m),
            Observation(3, "01", "06", new DateOnly(2026, 8, 2), 15_000m),
            Observation(4, "02", "01", new DateOnly(2026, 8, 2), 10_000m),
            Observation(5, "02", "02", new DateOnly(2026, 8, 2), 9_000m, "국산 냉동"),
            Observation(6, "02", "04", new DateOnly(2026, 8, 2), 5_000m, "냉동 수입"),
            Observation(7, "01", "10", new DateOnly(2026, 8, 2), 6_000m, "수입 고등어"),
            Observation(8, "01", "05", new DateOnly(2026, 8, 2), 99_000m, itemCode: "619"));
        await db.SaveChangesAsync();

        var service = new KamisDomesticPriceArchiveQueryService(db);
        var result = await service.LookupAsync(new AtDomesticFoodPriceRequest
        {
            CategoryCode = "600",
            ItemCode = "611",
            StartDate = "20260801",
            EndDate = "20260802",
            VarietyCodes = ["05", "06"],
            WholesaleVarietyCodes = ["01", "02"],
            RetailVarietyCodes = ["05"],
            ExcludedNameTokens = ["수입"]
        });

        Assert.True(result.Success);
        Assert.Equal("고등어", result.ItemName);
        Assert.Equal(13_000m, result.Retail?.AverageKrwPerKg);
        Assert.Equal(1, result.Retail?.SampleCount);
        Assert.Equal(9_500m, result.Wholesale?.AverageKrwPerKg);
        Assert.Equal(2, result.Wholesale?.SampleCount);
        Assert.Equal("20260802", result.Retail?.LatestSurveyDate);
        Assert.Contains("저장 원장", result.DataSource, StringComparison.Ordinal);
    }

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static KamisPriceObservation Observation(
        long id,
        string productClassCode,
        string kindCode,
        DateOnly surveyDate,
        decimal priceKrw,
        string kindName = "국산 냉장",
        string itemCode = "611")
        => new()
        {
            Id = id,
            RecordKey = $"record-{id}",
            ProductClassCode = productClassCode,
            ProductClassName = productClassCode == "01" ? "소매" : "도매",
            CategoryCode = "600",
            CategoryName = "수산물",
            CountryCode = "ALL",
            CountryName = "전국",
            RequestedDate = surveyDate,
            SurveyDate = surveyDate,
            FrequencyCode = "Daily",
            ItemName = "고등어",
            ItemCode = itemCode,
            KindName = kindName,
            KindCode = kindCode,
            RankName = "중품",
            RankCode = "05",
            Unit = "1kg",
            ComparisonUnit = "1kg",
            PriceKrw = priceKrw,
            SourceUrl = "https://www.kamis.or.kr/service/price/xml.do"
        };
}
