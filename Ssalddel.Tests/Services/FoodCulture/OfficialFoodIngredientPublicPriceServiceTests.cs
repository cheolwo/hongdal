using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodIngredientPublicPriceServiceTests
{
    [Fact]
    public void 명확한_원재료만_국가별_공공가격품목에_매핑한다()
    {
        var catalog = new OfficialFoodIngredientPriceMatchCatalog(
            new FoodPriceCrosswalkCatalog());

        var rice = catalog.Match(Ingredient(
            "쌀",
            OfficialFoodIngredientCategoryCodes.GrainAndStarch));
        var garlic = catalog.Match(Ingredient(
            "마늘",
            OfficialFoodIngredientCategoryCodes.Vegetable));
        var preparedGarlic = catalog.Match(Ingredient(
            "다진 마늘",
            OfficialFoodIngredientCategoryCodes.Vegetable));
        var sauce = catalog.Match(Ingredient(
            "저염간장",
            OfficialFoodIngredientCategoryCodes.SauceAndFermented));
        var unknown = catalog.Match(Ingredient(
            "향토비법재료",
            OfficialFoodIngredientCategoryCodes.Other,
            OfficialFoodIngredientClassificationStates.PendingReview,
            0.20m));

        Assert.Equal(2, rice.Count);
        Assert.Contains(rice, match => match.CountryCode == "KR" && match.ExternalItemCode == "111");
        Assert.Contains(rice, match => match.CountryCode == "US" && match.ExternalItemCode == "RICE");
        Assert.Equal(
            "01,03,04,05,06",
            garlic.Single(match => match.CountryCode == "KR").ExternalVariantCode);
        Assert.Empty(preparedGarlic);
        Assert.Empty(sauce);
        Assert.Empty(unknown);
    }

    [Fact]
    public async Task 보관된_최신_한국도소매와_미국생산자가격을_서로다른기준으로_표시한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var rice = Ingredient("쌀", OfficialFoodIngredientCategoryCodes.GrainAndStarch);
        var garlic = Ingredient("마늘", OfficialFoodIngredientCategoryCodes.Vegetable);
        var unknown = Ingredient(
            "향토비법재료",
            OfficialFoodIngredientCategoryCodes.Other,
            OfficialFoodIngredientClassificationStates.PendingReview,
            0.20m);
        db.OfficialFoodIngredients.AddRange(rice, garlic, unknown);
        var kamisRun = new KamisPriceCollectionRun
        {
            StatusCode = KamisArchiveStatusCodes.Completed,
            RequestedDate = new DateOnly(2026, 7, 20)
        };
        var usdaRun = new UsdaNassPriceCollectionRun
        {
            StatusCode = UsdaNassArchiveStatusCodes.Completed,
            YearFrom = 2025
        };
        db.AddRange(kamisRun, usdaRun);
        db.KamisPriceObservations.AddRange(
            KamisObservation(kamisRun, "retail-rice", "01", 5_000m),
            KamisObservation(kamisRun, "wholesale-rice", "02", 3_000m),
            KamisObservation(
                kamisRun,
                "retail-garlic-domestic",
                "01",
                10_000m,
                "258",
                "깐마늘(국산)",
                "01"),
            KamisObservation(
                kamisRun,
                "retail-garlic-excluded",
                "01",
                100m,
                "258",
                "깐마늘(국산)",
                "99"));
        db.PriceObservations.Add(new UsdaNassPriceObservation
        {
            FirstCollectionRun = usdaRun,
            RecordKey = "us-rice",
            SourceDesc = "SURVEY",
            SectorDesc = "CROPS",
            CommodityDesc = "RICE",
            ClassDesc = "ALL CLASSES",
            UtilPracticeDesc = "ALL UTILIZATION PRACTICES",
            ProductionPracticeDesc = "ALL PRODUCTION PRACTICES",
            StatisticCategoryDesc = "PRICE RECEIVED",
            UnitDesc = "$ / CWT",
            ShortDesc = "RICE - PRICE RECEIVED, MEASURED IN $ / CWT",
            DomainDesc = "TOTAL",
            AggregationLevelDesc = "NATIONAL",
            CountryCode = "9000",
            CountryName = "UNITED STATES",
            Year = 2026,
            FrequencyDesc = "MONTHLY",
            BeginCode = "06",
            EndCode = "06",
            ReferencePeriodDesc = "JUN",
            ValueRaw = "22.50",
            NumericValue = 22.50m,
            SourceUrl = "https://quickstats.nass.usda.gov/api",
            LastSeenAtUtc = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();

        var timeProvider = new TestTimeProvider(
            new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero));
        var service = new OfficialFoodIngredientPublicPriceService(
            db,
            new OfficialFoodIngredientPriceMatchCatalog(new FoodPriceCrosswalkCatalog()),
            timeProvider);

        var index = await service.RebuildMappingsAsync(
            new OfficialFoodIngredientPriceIndexRequest(100, Force: true));
        var secondIndex = await service.RebuildMappingsAsync(
            new OfficialFoodIngredientPriceIndexRequest(100));
        var prices = await service.GetLatestPricesAsync([rice.Id, garlic.Id, unknown.Id]);
        var ingredientIndexService = new OfficialFoodRecipeIngredientIndexService(
            db,
            new OfficialFoodRecipeIngredientParser(),
            service,
            timeProvider);
        var listedIngredients = await ingredientIndexService.SearchIngredientsAsync(
            new OfficialFoodIngredientQuery());

        Assert.Equal(3, index.MappingCount);
        Assert.Equal(3, secondIndex.MappingCount);
        Assert.Equal(3, await db.OfficialFoodIngredientPriceMappings.CountAsync());
        Assert.Equal(2, index.MappedIngredientCount);
        Assert.Equal(2, index.PricedIngredientCount);
        Assert.Equal(3, index.KoreanPriceCount);
        Assert.Equal(1, index.UnitedStatesPriceCount);
        Assert.Equal(3, prices[rice.Id].Count);
        Assert.Equal(
            5_000m,
            prices[rice.Id].Single(price =>
                price.MarketStageCode == OfficialFoodIngredientPriceMarketStages.Retail).AveragePrice);
        Assert.Equal(
            3_000m,
            prices[rice.Id].Single(price =>
                price.MarketStageCode == OfficialFoodIngredientPriceMarketStages.Wholesale).AveragePrice);
        var usPrice = prices[rice.Id].Single(price =>
            price.MarketStageCode == OfficialFoodIngredientPriceMarketStages.ProducerReceived);
        Assert.Equal(22.50m, usPrice.AveragePrice);
        Assert.Equal("USD", usPrice.CurrencyCode);
        Assert.Equal("$ / CWT", usPrice.Unit);
        Assert.False(usPrice.IsDirectlyComparableAcrossCountries);
        Assert.Equal(10_000m, Assert.Single(prices[garlic.Id]).AveragePrice);
        Assert.Empty(prices[unknown.Id]);
        Assert.Equal(
            3,
            listedIngredients.Single(item => item.CanonicalName == "쌀").PublicPrices?.Count);
        Assert.Empty(
            listedIngredients.Single(item => item.CanonicalName == "향토비법재료").PublicPrices ?? []);
    }

    private static OfficialFoodIngredient Ingredient(
        string name,
        string categoryCode,
        string classificationState = OfficialFoodIngredientClassificationStates.AutoClassified,
        decimal confidence = 0.95m)
        => new()
        {
            IngredientKey = $"key-{name}",
            LanguageCode = "ko",
            CanonicalName = name,
            NormalizedName = OfficialFoodRecipeIngredientParser.NormalizeName(name),
            CategoryCode = categoryCode,
            ClassificationMethod = "test",
            ClassificationConfidence = confidence,
            ClassificationState = classificationState
        };

    private static KamisPriceObservation KamisObservation(
        KamisPriceCollectionRun run,
        string key,
        string productClassCode,
        decimal price,
        string itemCode = "111",
        string itemName = "쌀",
        string kindCode = "00")
        => new()
        {
            FirstCollectionRun = run,
            RecordKey = key,
            ProductClassCode = productClassCode,
            ProductClassName = productClassCode == "01" ? "소매" : "도매",
            CategoryCode = itemCode == "111" ? "100" : "200",
            CategoryName = itemCode == "111" ? "식량작물" : "채소류",
            CountryCode = "ALL",
            CountryName = "전국",
            RequestedDate = new DateOnly(2026, 7, 20),
            SurveyDate = new DateOnly(2026, 7, 20),
            FrequencyCode = "Daily",
            ItemName = itemName,
            ItemCode = itemCode,
            KindName = itemName,
            KindCode = kindCode,
            RankName = "상품",
            RankCode = "04",
            Unit = "1kg",
            PriceRaw = price.ToString(),
            PriceKrw = price,
            SourceUrl = "https://www.kamis.or.kr/service/price/xml.do",
            LastSeenAtUtc = new DateTime(2026, 7, 20, 23, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 7, 20, 23, 0, 0, DateTimeKind.Utc)
        };

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
