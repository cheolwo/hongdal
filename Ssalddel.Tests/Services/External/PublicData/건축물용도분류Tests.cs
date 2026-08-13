using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 건축물용도분류Tests
{
    [Theory]
    [InlineData("단독주택", 건축물용도CategoryCodes.Residential)]
    [InlineData("제2종근린생활시설", 건축물용도CategoryCodes.Commercial)]
    [InlineData("창고시설", 건축물용도CategoryCodes.LogisticsStorage)]
    [InlineData("동물 및 식물 관련 시설", 건축물용도CategoryCodes.Agriculture)]
    [InlineData("교육연구시설", 건축물용도CategoryCodes.EducationResearch)]
    public void 공식주용도명은_버전이있는Category로분류된다(
        string officialMainPurposeName,
        string expectedCategory)
    {
        Assert.Equal(
            expectedCategory,
            건축물주용도분류Engine.Classify(officialMainPurposeName));
        Assert.Equal("kr-building-main-purpose-v1", 건축물주용도분류Engine.RuleRevision);
    }

    [Fact]
    public void 원문이없으면_그럴듯한용도를꾸며내지않고미분류로남긴다()
    {
        Assert.Equal(
            건축물용도CategoryCodes.Unresolved,
            건축물주용도분류Engine.Classify(null));
    }

    [Fact]
    public async Task Db에는_분류대장이시드되고_건물행은임의로생성되지않는다()
    {
        var options = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase($"building-categories-{Guid.NewGuid():N}")
            .Options;
        await using var db = new PublicDataIngestionDbContext(options);
        await db.Database.EnsureCreatedAsync();

        Assert.Equal(15, await db.BuildingCategoryDefinitions.CountAsync());
        Assert.Contains(
            await db.BuildingCategoryDefinitions.ToListAsync(),
            item => item.CategoryCode == 건축물용도CategoryCodes.LogisticsStorage
                && item.WorldRoleCode == "hub"
                && item.PresentationEligible);
        Assert.Empty(await db.BuildingRegisterTitles.ToListAsync());
        Assert.Empty(await db.AdministrativeBuildingCategoryAggregates.ToListAsync());
    }

    [Fact]
    public async Task 공식주용도분류와_행정동집계는_같은규칙에서멱등이다()
    {
        var options = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase($"building-classification-{Guid.NewGuid():N}")
            .Options;
        await using var db = new PublicDataIngestionDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var warehouseId = Guid.NewGuid();
        var houseId = Guid.NewGuid();
        db.BuildingRegisterTitles.AddRange(
            Building(warehouseId, "warehouse-1", "창고시설", 100m, 200m),
            Building(houseId, "house-1", "단독주택", 80m, 120m));
        db.BuildingRegionAssignments.AddRange(
            Region(warehouseId),
            Region(houseId));
        await db.SaveChangesAsync();

        var service = new 건축물주용도분류원장Service(db);
        var first = await service.ClassifyAndAggregateAsync("building-source-v1", "2026-03-01");
        var firstHashes = await db.AdministrativeBuildingCategoryAggregates
            .OrderBy(item => item.CategoryCode)
            .Select(item => item.AggregateHashSha256)
            .ToArrayAsync();
        var second = await service.ClassifyAndAggregateAsync("building-source-v1", "2026-03-01");
        var secondHashes = await db.AdministrativeBuildingCategoryAggregates
            .OrderBy(item => item.CategoryCode)
            .Select(item => item.AggregateHashSha256)
            .ToArrayAsync();

        Assert.Equal(new 건축물분류원장Result(2, 2, 0, 2), first);
        Assert.Equal(new 건축물분류원장Result(2, 0, 2, 2), second);
        Assert.Equal(2, await db.BuildingCategoryAssignments.CountAsync());
        Assert.Equal(firstHashes, secondHashes);
        var logistics = await db.AdministrativeBuildingCategoryAggregates.SingleAsync(
            item => item.CategoryCode == 건축물용도CategoryCodes.LogisticsStorage);
        Assert.Equal("region:kr:hjd:5176036000", logistics.AdministrativeRegionStableId);
        Assert.Equal(1, logistics.BuildingCount);
        Assert.Equal(100m, logistics.BuildingAreaSquareMeters);
        Assert.Equal(200m, logistics.TotalFloorAreaSquareMeters);
    }

    private static 건축물대장표제부Record Building(
        Guid id,
        string registerManagementPk,
        string mainPurposeName,
        decimal buildingArea,
        decimal totalFloorArea) => new()
        {
            Id = id,
            RegisterManagementPk = registerManagementPk,
            RegisterKindCode = "title",
            SigunguCode = "51760",
            LegalDongCode = "5176036021",
            MainPurposeName = mainPurposeName,
            BuildingAreaSquareMeters = buildingArea,
            TotalFloorAreaSquareMeters = totalFloorArea,
            SourceRevision = "building-source-v1",
            EvidenceSnapshotId = 1,
            ObservedAtUtc = DateTimeOffset.Parse("2026-03-01T00:00:00Z"),
        };

    private static 건축물행정구역Assignment Region(Guid buildingId) => new()
    {
        Id = Guid.NewGuid(),
        BuildingRecordId = buildingId,
        LegalRegionStableId = "region:kr:bjd:5176036021",
        AdministrativeRegionStableId = "region:kr:hjd:5176036000",
        AssignmentMethodCode = "OfficialJurisdictionCrosswalk",
        ConfidenceCode = "OfficialOneToOne",
        SourceVintage = "2026-03-01",
        RuleRevision = "kr-building-region-v1",
        ValidFromUtc = DateTimeOffset.Parse("2026-03-01T00:00:00Z"),
    };
}
