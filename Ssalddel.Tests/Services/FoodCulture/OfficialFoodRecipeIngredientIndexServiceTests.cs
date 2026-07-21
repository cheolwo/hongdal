using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodRecipeIngredientIndexServiceTests
{
    [Fact]
    public void 식약처_묶음과_수량을_재료행으로_분리한다()
    {
        var parser = new OfficialFoodRecipeIngredientParser();

        var result = parser.Parse(
            "ko",
            [
                "●주재료 : 연두부 75g(3/4모), 달걀 30g(1/2개)",
                "●양념장 :",
                "저염간장 3g(2/3작은술), 다진 마늘 2g(1/2쪽), 참깨 약간"
            ]);

        Assert.Equal(5, result.Count);
        var tofu = Assert.Single(result, item => item.CanonicalName == "연두부");
        Assert.Equal("주재료", tofu.GroupName);
        Assert.Equal(75m, tofu.QuantityValue);
        Assert.Equal("g", tofu.UnitCode);
        Assert.Equal("3/4모", tofu.HouseholdMeasureText);
        Assert.Equal(OfficialFoodIngredientCategoryCodes.LegumeAndSoy, tofu.CategoryCode);

        var garlic = Assert.Single(result, item => item.CanonicalName == "다진 마늘");
        Assert.Equal("양념장", garlic.GroupName);
        Assert.Equal(OfficialFoodIngredientCategoryCodes.Vegetable, garlic.CategoryCode);

        var sesame = Assert.Single(result, item => item.CanonicalName == "참깨");
        Assert.Equal("약간", sesame.QuantityText);
        Assert.Equal("approx", sesame.UnitCode);
        Assert.False(sesame.RequiresReview);
    }

    [Fact]
    public void 괄호에서_끊긴_줄과_무수량_재료도_원문근거와_함께_보존한다()
    {
        var parser = new OfficialFoodRecipeIngredientParser();

        var result = parser.Parse(
            "ko",
            [
                "●육수 : 표고버섯 기둥, 참기름 2g(1/3작은술",
                "), 물 300ml(1½컵)"
            ]);

        Assert.Equal(3, result.Count);
        var mushroom = Assert.Single(result, item => item.CanonicalName == "표고버섯 기둥");
        Assert.True(mushroom.RequiresReview);
        Assert.Equal(OfficialFoodIngredientCategoryCodes.Mushroom, mushroom.CategoryCode);

        var water = Assert.Single(result, item => item.CanonicalName == "물");
        Assert.Equal(300m, water.QuantityValue);
        Assert.Equal("ml", water.UnitCode);
        Assert.Equal("1½컵", water.HouseholdMeasureText);
    }

    [Fact]
    public void 미분류_재료는_기타로_숨기지않고_검토대기로_남긴다()
    {
        var parser = new OfficialFoodRecipeIngredientParser();

        var item = Assert.Single(parser.Parse("ko", ["향토비법재료 1g"]));

        Assert.Equal(OfficialFoodIngredientCategoryCodes.Other, item.CategoryCode);
        Assert.Equal(
            OfficialFoodIngredientClassificationStates.PendingReview,
            item.ClassificationState);
        Assert.True(item.RequiresReview);
    }

    [Fact]
    public void 괄호안_주수량과_앞쪽_재료묶음도_분리한다()
    {
        var parser = new OfficialFoodRecipeIngredientParser();

        var result = parser.Parse(
            "ko",
            ["(반죽재료) 강력분(100g), 후춧가루(0.02g), 올리브오일(10g)"]);

        Assert.Equal(3, result.Count);
        var flour = Assert.Single(result, item => item.CanonicalName == "강력분");
        Assert.Equal("반죽재료", flour.GroupName);
        Assert.Equal(100m, flour.QuantityValue);
        Assert.Equal(OfficialFoodIngredientCategoryCodes.GrainAndStarch, flour.CategoryCode);
        Assert.Equal(
            OfficialFoodIngredientCategoryCodes.SeasoningAndSpice,
            result.Single(item => item.CanonicalName == "후춧가루").CategoryCode);
        Assert.Equal(
            OfficialFoodIngredientCategoryCodes.OilAndFat,
            result.Single(item => item.CanonicalName == "올리브오일").CategoryCode);
    }

    [Fact]
    public void 대괄호묶음과_붙여쓴수량은_분리하고_이름없는수량은_재료로만들지않는다()
    {
        var parser = new OfficialFoodRecipeIngredientParser();

        var result = parser.Parse(
            "ko",
            ["[주재료] 갈치(70g(1토막)), 기름5g, 넛맥약간, 10g"]);

        Assert.Equal(3, result.Count);
        var hairtail = Assert.Single(result, item => item.CanonicalName == "갈치");
        Assert.Equal("주재료", hairtail.GroupName);
        Assert.Equal(70m, hairtail.QuantityValue);
        Assert.Equal("1토막", hairtail.HouseholdMeasureText);
        Assert.Equal(5m, result.Single(item => item.CanonicalName == "기름").QuantityValue);
        Assert.Equal("약간", result.Single(item => item.CanonicalName == "넛맥").QuantityText);
    }

    [Fact]
    public void 한글자_재료규칙은_복합어를_잘못분류하지않는다()
    {
        var parser = new OfficialFoodRecipeIngredientParser();

        var result = parser.Parse("ko", ["물 100ml, 물엿 10g, 마 20g, 파스타 30g"]);

        Assert.Equal(
            OfficialFoodIngredientCategoryCodes.WaterAndStock,
            result.Single(item => item.CanonicalName == "물").CategoryCode);
        Assert.Equal(
            OfficialFoodIngredientCategoryCodes.SauceAndFermented,
            result.Single(item => item.CanonicalName == "물엿").CategoryCode);
        Assert.Equal(
            OfficialFoodIngredientCategoryCodes.Vegetable,
            result.Single(item => item.CanonicalName == "마").CategoryCode);
        Assert.Equal(
            OfficialFoodIngredientCategoryCodes.GrainAndStarch,
            result.Single(item => item.CanonicalName == "파스타").CategoryCode);
    }

    [Fact]
    public async Task 기존_레시피를_백필하면_공유마스터와_사용행을_멱등하게_만든다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var source = await db.OfficialFoodRecipeSources.SingleAsync(
            item => item.SourceKey == OfficialFoodRecipeSourceKeys.MfdsCookRecipe);
        var run = new OfficialFoodRecipeCollectionRun
        {
            SourceKey = source.SourceKey,
            StatusCode = OfficialFoodRecipeCollectionStatuses.Completed
        };
        var dish = new OfficialFoodDish
        {
            DishKey = "dish-1",
            CountryCode = "KR",
            Name = "두부 요리",
            OriginalName = "두부 요리"
        };
        db.AddRange(run, dish);
        db.OfficialFoodRecipeVariants.AddRange(
            Variant(source, run, dish, "recipe-1", ["두부 20g", "양파 10g"]),
            Variant(source, run, dish, "recipe-2", ["두부 30g", "참기름 약간"]));
        await db.SaveChangesAsync();
        var timeProvider = new OfficialFoodRecipeArchiveServiceTests.MutableTimeProvider(
            new DateTimeOffset(2026, 7, 21, 9, 0, 0, TimeSpan.Zero));
        var priceService = new OfficialFoodIngredientPublicPriceService(
            db,
            new OfficialFoodIngredientPriceMatchCatalog(new FoodPriceCrosswalkCatalog()),
            timeProvider);
        var service = new OfficialFoodRecipeIngredientIndexService(
            db,
            new OfficialFoodRecipeIngredientParser(),
            priceService,
            timeProvider);

        var first = await service.RebuildAsync(new OfficialFoodIngredientIndexRequest(
            source.SourceKey,
            10));
        var second = await service.RebuildAsync(new OfficialFoodIngredientIndexRequest(
            source.SourceKey,
            10));
        var forced = await service.RebuildAsync(new OfficialFoodIngredientIndexRequest(
            source.SourceKey,
            10,
            Force: true));
        var ingredients = await service.SearchIngredientsAsync(new OfficialFoodIngredientQuery());

        Assert.Equal(2, first.ProcessedRecipeVariantCount);
        Assert.Equal(4, first.RecipeIngredientCount);
        Assert.Equal(3, first.CatalogIngredientCount);
        Assert.Equal(0, second.ProcessedRecipeVariantCount);
        Assert.Equal(2, forced.ProcessedRecipeVariantCount);
        Assert.Equal(3, await db.OfficialFoodIngredients.CountAsync());
        Assert.Equal(4, await db.OfficialFoodRecipeIngredients.CountAsync());
        var tofu = ingredients.Single(item => item.CanonicalName == "두부");
        Assert.Equal(2, tofu.RecipeVariantCount);
        Assert.Equal(2, tofu.RelatedRecipes?.Count);
        Assert.Single(tofu.RelatedRecipes!.Select(item => item.DishKey).Distinct());
        Assert.Equal(18, (await service.GetCategoriesAsync()).Count);
    }

    [Fact]
    public async Task 재료조회는_실제사용관계에서_서로다른_대표음식_세개를_우선한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var source = await db.OfficialFoodRecipeSources.SingleAsync(
            item => item.SourceKey == OfficialFoodRecipeSourceKeys.MfdsCookRecipe);
        var run = new OfficialFoodRecipeCollectionRun
        {
            SourceKey = source.SourceKey,
            StatusCode = OfficialFoodRecipeCollectionStatuses.Completed
        };
        var dishes = Enumerable.Range(1, 4)
            .Select(index => new OfficialFoodDish
            {
                DishKey = $"dish-{index}",
                CountryCode = "KR",
                Name = $"두부 요리 {index}",
                OriginalName = $"두부 요리 {index}",
                RepresentationState = index == 1
                    ? OfficialFoodRecipeRepresentationStates.Representative
                    : OfficialFoodRecipeRepresentationStates.Candidate,
                ReviewState = index == 1
                    ? OfficialFoodRecipeReviewStates.Approved
                    : OfficialFoodRecipeReviewStates.PendingReview
            })
            .ToArray();
        db.Add(run);
        db.AddRange(dishes);
        db.OfficialFoodRecipeVariants.AddRange(dishes.Select((dish, index) =>
            Variant(source, run, dish, $"recipe-{index + 1}", ["두부 20g"])));
        await db.SaveChangesAsync();
        var timeProvider = new OfficialFoodRecipeArchiveServiceTests.MutableTimeProvider(
            new DateTimeOffset(2026, 7, 21, 9, 0, 0, TimeSpan.Zero));
        var priceService = new OfficialFoodIngredientPublicPriceService(
            db,
            new OfficialFoodIngredientPriceMatchCatalog(new FoodPriceCrosswalkCatalog()),
            timeProvider);
        var service = new OfficialFoodRecipeIngredientIndexService(
            db,
            new OfficialFoodRecipeIngredientParser(),
            priceService,
            timeProvider);

        await service.RebuildAsync(new OfficialFoodIngredientIndexRequest(source.SourceKey, 10));
        var tofu = Assert.Single(await service.SearchIngredientsAsync(
            new OfficialFoodIngredientQuery { SearchText = "두부" }));
        var relatedRecipes = tofu.RelatedRecipes
            ?? throw new Xunit.Sdk.XunitException("대표 레시피 관계가 반환되지 않았습니다.");

        Assert.Equal(4, tofu.RecipeVariantCount);
        Assert.Equal(3, relatedRecipes.Count);
        Assert.Equal(3, relatedRecipes.Select(item => item.DishKey).Distinct().Count());
        Assert.Equal("dish-1", relatedRecipes[0].DishKey);
        Assert.All(relatedRecipes, item => Assert.Equal(source.SourceKey, item.SourceKey));
    }

    private static OfficialFoodRecipeVariant Variant(
        OfficialFoodRecipeSource source,
        OfficialFoodRecipeCollectionRun run,
        OfficialFoodDish dish,
        string externalId,
        IReadOnlyList<string> ingredients)
        => new()
        {
            Source = source,
            Dish = dish,
            FirstCollectionRun = run,
            RecordKey = externalId,
            ExternalId = externalId,
            Title = dish.Name,
            IngredientsJson = JsonSerializer.Serialize(ingredients)
        };
}
