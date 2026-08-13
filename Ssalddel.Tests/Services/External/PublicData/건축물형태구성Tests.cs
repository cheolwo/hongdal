using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 건축물형태구성Tests
{
    [Fact]
    public void 공식층수와공식비율은_추정값보다우선한다()
    {
        var building = Building("commercial-1", "제2종근린생활시설");
        building.AboveGroundFloorCount = 4;
        building.HeightMeters = 18m;
        building.SiteAreaSquareMeters = 400m;
        building.BuildingAreaSquareMeters = 100m;
        building.TotalFloorAreaSquareMeters = 600m;
        building.OfficialBuildingCoveragePercent = 24.5m;
        building.OfficialFloorAreaRatioPercent = 145.25m;

        var result = 건축물형태분석Engine.분석(
            building,
            건축물용도CategoryCodes.Commercial);

        Assert.Equal(4, result.관측지상층수);
        Assert.Null(result.추정지상층수);
        Assert.Equal(4, result.표현지상층수);
        Assert.Equal(24.5m, result.공식건폐율Percent);
        Assert.Equal(145.25m, result.공식용적률Percent);
        Assert.Equal(25m, result.단순건폐비율Percent);
        Assert.Equal(150m, result.단순연면적대지비율Percent);
        Assert.Equal(건축물형태근거종류Codes.관측값우선, result.근거종류Code);
    }

    [Fact]
    public void 층수가없으면_용도별층고와높이로만추정한다()
    {
        var building = Building("warehouse-1", "창고시설");
        building.HeightMeters = 10m;
        building.SiteAreaSquareMeters = 1_000m;
        building.BuildingAreaSquareMeters = 600m;

        var result = 건축물형태분석Engine.분석(
            building,
            건축물용도CategoryCodes.LogisticsStorage);
        var visual = 건축물형태분석Engine.시각구성(
            result,
            건축물용도CategoryCodes.LogisticsStorage);

        Assert.Null(result.관측지상층수);
        Assert.Equal(2, result.추정지상층수);
        Assert.Equal(5m, result.추정층고Meters);
        Assert.Equal("hub-warehouse", visual.시각FamilyCode);
        Assert.Equal("high", visual.대지점유등급Code);
        Assert.Equal("compact", visual.주변여백등급Code);
    }

    [Fact]
    public void 시각계획은_BaseMiddleRoof조합용반복수를계산한다()
    {
        var building = Building("housing-1", "공동주택");
        building.AboveGroundFloorCount = 7;
        building.SiteAreaSquareMeters = 500m;
        building.BuildingAreaSquareMeters = 150m;
        var result = 건축물형태분석Engine.분석(
            building,
            건축물용도CategoryCodes.Residential);
        var visual = 건축물형태분석Engine.시각구성(
            result,
            건축물용도CategoryCodes.Residential);

        Assert.Equal("city-midrise", visual.시각FamilyCode);
        Assert.Equal(7, visual.기준층수);
        Assert.Equal(5, visual.중간층반복수);
        Assert.Equal("region-and-task", visual.LOD등급Code);
    }

    [Fact]
    public async Task 형태Profile과시각계획은_같은원본과규칙에서중복되지않는다()
    {
        var options = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase($"building-massing-{Guid.NewGuid():N}")
            .Options;
        await using var db = new PublicDataIngestionDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var building = Building("warehouse-1", "창고시설");
        building.AboveGroundFloorCount = 2;
        building.SiteAreaSquareMeters = 1_000m;
        building.BuildingAreaSquareMeters = 600m;
        building.TotalFloorAreaSquareMeters = 900m;
        db.BuildingRegisterTitles.Add(building);
        await db.SaveChangesAsync();
        var categoryService = new 건축물주용도분류원장Service(db);
        await categoryService.ClassifyAndAggregateAsync("building-source-v1", "2026-03-01");
        var service = new 건축물형태구성원장Service(db);

        var first = await service.형태와시각계획생성Async("building-source-v1");
        var profileHash = (await db.건축물형태Profiles.SingleAsync()).ProfileHashSha256;
        var planHash = (await db.건축물시각구성계획들.SingleAsync()).계획HashSha256;
        var second = await service.형태와시각계획생성Async("building-source-v1");

        Assert.Equal(new 건축물형태구성원장Result(1, 1, 0, 1), first);
        Assert.Equal(new 건축물형태구성원장Result(1, 0, 1, 0), second);
        Assert.Equal(profileHash, (await db.건축물형태Profiles.SingleAsync()).ProfileHashSha256);
        Assert.Equal(planHash, (await db.건축물시각구성계획들.SingleAsync()).계획HashSha256);
        Assert.True((await db.건축물시각구성계획들.SingleAsync()).표현전용);
    }

    private static 건축물대장표제부Record Building(
        string registerManagementPk,
        string mainPurposeName) => new()
        {
            Id = Guid.NewGuid(),
            RegisterManagementPk = registerManagementPk,
            RegisterKindCode = "title",
            SigunguCode = "51760",
            LegalDongCode = "5176036021",
            MainPurposeName = mainPurposeName,
            SourceRevision = "building-source-v1",
            EvidenceSnapshotId = 1,
            ObservedAtUtc = DateTimeOffset.Parse("2026-03-01T00:00:00Z"),
        };
}
