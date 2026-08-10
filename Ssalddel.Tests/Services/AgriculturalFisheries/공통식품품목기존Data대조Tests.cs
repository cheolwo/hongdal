using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class 공통식품품목기존Data대조Tests
{
    [Fact]
    public async Task 기존Kamis를_canonical연결_후보_미연결로분리한다()
    {
        await using var db = CreateDb();
        db.KamisPriceObservations.AddRange(
            Kamis(1, "100", "식량작물", "152", "감자"),
            Kamis(2, "400", "과일류", "411", "사과"),
            Kamis(3, "999", "기타", "999", "미분류"));
        db.UsdaAmsYearCommodityCatalog.AddRange(
            Ams("Potatoes"),
            Ams("Apples"));
        await db.SaveChangesAsync();
        var sut = new 공통식품품목기존Data대조UseCase(
            db,
            new FoodPriceCrosswalkCatalog());

        var result = await sut.PreviewAsync(2026);

        Assert.Equal(3, result.ObservedKamisItemCount);
        Assert.Equal(64, result.PreviewHash.Length);
        var potato = Assert.Single(result.Items, item => item.KamisItemCode == "152");
        Assert.Equal(공통식품품목대조StatusCodes.CanonicalLinked, potato.StatusCode);
        Assert.Equal("product:potato", potato.CanonicalProductStableId);
        Assert.Contains("0701", potato.HsCandidates);
        Assert.Contains("Potatoes", potato.UsdaAmsCommodityCandidates);
        Assert.Equal(공통식품품목관계StatusCodes.Unlinked, potato.NongsaroRelationStatusCode);

        var apple = Assert.Single(result.Items, item => item.KamisItemCode == "411");
        Assert.Equal(공통식품품목대조StatusCodes.CandidateOnly, apple.StatusCode);
        Assert.Null(apple.CanonicalProductStableId);
        Assert.Contains("080810", apple.HsCandidates);
        Assert.Contains("Apples", apple.UsdaAmsCommodityCandidates);

        var unknown = Assert.Single(result.Items, item => item.KamisItemCode == "999");
        Assert.Equal(공통식품품목대조StatusCodes.Unmapped, unknown.StatusCode);
        Assert.Empty(unknown.HsCandidates);
        Assert.Empty(unknown.UsdaAmsCommodityCandidates);
    }

    [Fact]
    public async Task PreviewHash를확인한59품목승격은_Kamis만확정하고_재실행시멱등이다()
    {
        await using var db = CreateDb();
        db.KamisPriceObservations.Add(Kamis(1, "400", "과일류", "411", "사과"));
        db.UsdaAmsYearCommodityCatalog.Add(Ams("Apples"));
        await db.SaveChangesAsync();
        var sut = new 공통식품품목기존Data대조UseCase(
            db,
            new FoodPriceCrosswalkCatalog());
        var preview = await sut.PreviewAsync(2026);

        var first = await sut.PromoteCandidatesAsync(
            2026,
            preview.PreviewHash,
            "test:catalog-reviewer");
        var secondPreview = await sut.PreviewAsync(2026);
        var second = await sut.PromoteCandidatesAsync(
            2026,
            secondPreview.PreviewHash,
            "test:catalog-reviewer");

        Assert.Equal(1, first.PromotedProductCount);
        Assert.Equal(4, first.CreatedRelationCount);
        Assert.Equal("product:food:400:411", Assert.Single(first.PromotedProductStableIds));
        Assert.Equal(0, second.PromotedProductCount);
        var identity = await db.CommonFoodProductIdentities
            .Include(item => item.CodeRelations)
            .ThenInclude(item => item.ReviewHistory)
            .SingleAsync(item => item.CanonicalProductStableId == "product:food:400:411");
        Assert.Equal(4, identity.CodeRelations.Count);
        Assert.Single(identity.CodeRelations, item =>
            item.RelationStatusCode == 공통식품품목관계StatusCodes.Confirmed);
        Assert.All(identity.CodeRelations.Where(item =>
            item.CodeScheme != 공통식품품목CodeSchemes.KamisItem), item =>
            Assert.NotEqual(공통식품품목관계StatusCodes.Confirmed, item.RelationStatusCode));
        Assert.All(identity.CodeRelations, item => Assert.Single(item.ReviewHistory));
        var publicProjection = await new 공통식품품목Identity조회UseCase(db)
            .단건조회Async("product:food:400:411");
        Assert.NotNull(publicProjection);
        Assert.NotEmpty(publicProjection.Limitations);
    }

    [Fact]
    public async Task Preview가변경되면_승격을거부한다()
    {
        await using var db = CreateDb();
        db.KamisPriceObservations.Add(Kamis(1, "400", "과일류", "411", "사과"));
        await db.SaveChangesAsync();
        var sut = new 공통식품품목기존Data대조UseCase(
            db,
            new FoodPriceCrosswalkCatalog());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.PromoteCandidatesAsync(2026, new string('0', 64), "test:reviewer"));

        Assert.Equal("CommonFoodProductPreviewChanged", error.Message);
        Assert.DoesNotContain(db.CommonFoodProductIdentities,
            item => item.CanonicalProductStableId == "product:food:400:411");
    }

    [Fact]
    public async Task 같은KamisCode에_서로다른이름이있으면_canonical자동연결을차단한다()
    {
        await using var db = CreateDb();
        db.KamisPriceObservations.AddRange(
            Kamis(1, "100", "식량작물", "152", "감자"),
            Kamis(2, "100", "식량작물", "152", "감자 다른 이름"));
        await db.SaveChangesAsync();
        var sut = new 공통식품품목기존Data대조UseCase(
            db,
            new FoodPriceCrosswalkCatalog());

        var result = await sut.PreviewAsync(2026);

        Assert.Equal(2, result.SourceConflictCount);
        Assert.All(result.Items, item =>
        {
            Assert.Equal(공통식품품목대조StatusCodes.SourceConflict, item.StatusCode);
            Assert.Null(item.CanonicalProductStableId);
        });
    }

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var db = new AgriculturalFisheriesDbContext(
            new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options);
        db.Database.EnsureCreated();
        return db;
    }

    private static KamisPriceObservation Kamis(
        long id,
        string categoryCode,
        string categoryName,
        string itemCode,
        string itemName)
        => new()
        {
            Id = id,
            RecordKey = $"kamis-{id}",
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            ItemCode = itemCode,
            ItemName = itemName,
            SurveyDate = new DateOnly(2026, 7, 30)
        };

    private static UsdaAms연도상품Catalog Ams(string commodity)
        => new()
        {
            Year = 2026,
            Commodity = commodity,
            FirstObservedDate = new DateOnly(2026, 1, 1),
            LastObservedDate = new DateOnly(2026, 7, 30)
        };
}
