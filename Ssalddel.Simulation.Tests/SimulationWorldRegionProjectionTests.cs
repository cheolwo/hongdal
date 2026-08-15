using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldRegionProjectionTests
{
    private const string LegalRegion = "region:kr:legal:5176038000";
    private const string AdministrativeRegion = "region:kr:administrative:5176038000";

    [Fact]
    public async Task 평창군Pipeline은_법정동과행정동을먼저가공해_지역Projection으로저장한다()
    {
        await using var publicDb = CreatePublicDataDb();
        await using var derivedDb = CreateDerivedDb();
        await publicDb.Database.EnsureCreatedAsync();
        await derivedDb.Database.EnsureCreatedAsync();
        await SeedRegionDataAsync(publicDb);
        var pipeline = new 평창군공간파생Pipeline(
            publicDb,
            new SimulationWorld파생원장Store(derivedDb));

        var result = await pipeline.실행Async(CancellationToken.None);

        Assert.Equal(평창군공간파생Pipeline.완료, result.상태코드);
        Assert.Equal(1, await derivedDb.Nodes.CountAsync(item =>
            item.NodeKindCode == SimulationWorldRegionProjectionCodes.LegalRegion
            && item.SourceRecordStableId == LegalRegion));
        Assert.Equal(1, await derivedDb.Nodes.CountAsync(item =>
            item.NodeKindCode == SimulationWorldRegionProjectionCodes.AdministrativeRegion
            && item.SourceRecordStableId == AdministrativeRegion));
        Assert.Equal(1, await derivedDb.Nodes.CountAsync(item =>
            item.NodeKindCode == "AdministrativeRegionBuildingCategoryAggregate"
            && item.RepresentativeGroupCode == 건축물용도CategoryCodes.Agriculture
            && item.RepresentedRecordCount == 12));
        Assert.Equal(1, await derivedDb.Relations.CountAsync(item =>
            item.RelationCode == "LegalAdministrativeRegionCrosswalk"));
        Assert.Equal(1, await derivedDb.Relations.CountAsync(item =>
            item.RelationCode == "LocatedInLegalRegion"));
        Assert.Equal(1, await derivedDb.Relations.CountAsync(item =>
            item.RelationCode == "AggregatedInAdministrativeRegion"));
    }

    [Fact]
    public async Task 지역Reader는_최신파생실행의행정동건물집계를반환하고_경계부족을명시한다()
    {
        await using var publicDb = CreatePublicDataDb();
        await using var derivedDb = CreateDerivedDb();
        await publicDb.Database.EnsureCreatedAsync();
        await derivedDb.Database.EnsureCreatedAsync();
        await SeedRegionDataAsync(publicDb);
        var pipeline = new 평창군공간파생Pipeline(
            publicDb,
            new SimulationWorld파생원장Store(derivedDb));
        await pipeline.실행Async(CancellationToken.None);
        derivedDb.ChangeTracker.Clear();
        var reader = new SimulationWorld지역ProjectionReader(derivedDb);

        var result = await reader.조회Async(AdministrativeRegion, CancellationToken.None);

        Assert.True(result.파생Db사용가능);
        Assert.NotNull(result.Projection);
        Assert.Equal(SimulationWorldRegionProjectionCodes.AdministrativeRegion,
            result.Projection.RegionKindCode);
        Assert.Equal(SimulationWorldRegionProjectionCodes.WaitingForRegionGeometry,
            result.Projection.ProjectionStatusCode);
        Assert.Empty(result.Projection.TileKeys);
        Assert.Equal(LegalRegion, Assert.Single(result.Projection.RelatedRegionStableIds));
        var category = Assert.Single(result.Projection.BuildingCategories);
        Assert.Equal(건축물용도CategoryCodes.Agriculture, category.CategoryCode);
        Assert.Equal(12, category.BuildingCount);
        Assert.True(result.Projection.PresentationOnly);
        Assert.False(result.Projection.IsOperationalState);
    }

    private static async Task SeedRegionDataAsync(PublicDataIngestionDbContext db)
    {
        var buildingId = Guid.NewGuid();
        db.BuildingRegisterTitles.Add(new 건축물대장표제부Record
        {
            Id = buildingId,
            RegisterManagementPk = "region-projection-building-1",
            RegisterKindCode = "title",
            SigunguCode = "51760",
            LegalDongCode = "5176038000",
            BuildingName = "대관령 농업 건물",
            MainPurposeName = "동물 및 식물 관련 시설",
            BuildingAreaSquareMeters = 240m,
            SourceRevision = "building-2026-08",
            EvidenceSnapshotId = 1,
            ObservedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
        });
        db.BuildingRegionAssignments.Add(new 건축물행정구역Assignment
        {
            Id = Guid.NewGuid(),
            BuildingRecordId = buildingId,
            LegalRegionStableId = LegalRegion,
            AdministrativeRegionStableId = AdministrativeRegion,
            AssignmentMethodCode = "OfficialCrosswalk",
            ConfidenceCode = "DerivedHigh",
            SourceVintage = "2026-08",
            RuleRevision = "building-region-assignment.v1",
            ValidFromUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
        });
        db.BuildingCategoryAssignments.Add(new 건축물용도CategoryAssignment
        {
            Id = Guid.NewGuid(),
            BuildingRecordId = buildingId,
            CategoryCode = 건축물용도CategoryCodes.Agriculture,
            IsPrimary = true,
            AssignmentMethodCode = "OfficialPurposeCode",
            EvidenceKindCode = 건축물분류EvidenceKindCodes.Derived,
            RuleRevision = "building-category.v1",
            ClassifiedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
        });
        db.AdministrativeBuildingCategoryAggregates.Add(new 행정동건축물CategoryAggregate
        {
            Id = Guid.NewGuid(),
            AdministrativeRegionStableId = AdministrativeRegion,
            SourceVintage = "2026-08",
            CategoryCode = 건축물용도CategoryCodes.Agriculture,
            BuildingCount = 12,
            BuildingAreaSquareMeters = 3200m,
            TotalFloorAreaSquareMeters = 4100m,
            NamedBuildingCount = 8,
            GeometryLinkedCount = 0,
            UnresolvedBuildingCount = 0,
            EvidenceKindCode = 건축물분류EvidenceKindCodes.Derived,
            RuleRevision = "administrative-building-aggregate.v1",
            AggregateHashSha256 = new string('a', 64),
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-13T01:00:00Z"),
        });
        await db.SaveChangesAsync();
    }

    private static PublicDataIngestionDbContext CreatePublicDataDb()
        => new(new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);

    private static SimulationWorld파생DbContext CreateDerivedDb()
        => new(new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);
}
