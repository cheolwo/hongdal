using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;
using Ssalddel.Simulation.Persistence;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class PyeongchangRepresentativeSelectionTests
{
    [Fact]
    public void 경관완결영역은_2곱하기2_L2타일과필요한상위타일만선택한다()
    {
        var tiles = PyeongchangSimulationWorldStableIds.대관령Farm경관완결L2타일키
            .Concat(PyeongchangSimulationWorldStableIds.대관령Farm경관완결상위타일키)
            .Append("kr5186:l2:702:1144")
            .Select(Tile)
            .ToArray();

        var selected = 평창군공간파생Pipeline.Select경관완결영역Tiles(tiles);

        Assert.Equal(6, selected.Length);
        Assert.DoesNotContain(selected, item => item.TileKey == "kr5186:l2:702:1144");
        Assert.Equal(
            PyeongchangSimulationWorldStableIds.대관령Farm경관완결L2타일키
                .OrderBy(item => item, StringComparer.Ordinal),
            selected.Where(item => item.Level == 2)
                .Select(item => item.TileKey)
                .OrderBy(item => item, StringComparer.Ordinal));
        Assert.Equal(new[] { 0, 1, 2, 2, 2, 2 }, selected.Select(item => item.Level));
    }

    [Fact]
    public void 대표선정은_건물종류마다정확히하나만선택한다()
    {
        var buildings = Buildings(620);
        var categories = buildings.ToDictionary(
            item => item.Id,
            item => "category-" + (Index(item.Id) % 5));

        var selected = 평창군공간파생Pipeline.SelectOneRepresentativePerBuildingCategory(
            buildings, categories);

        Assert.Equal(5, selected.Count);
        Assert.Equal(620, selected.Sum(item => item.RepresentedRecordCount));
        Assert.Equal(5, selected.Select(item => item.Building.Id).Distinct().Count());
        Assert.Equal(5, selected.Select(item => item.GroupCode).Distinct().Count());
        Assert.All(selected, item => Assert.Equal(1, item.Rank));
    }

    [Fact]
    public void 대표선정은_입력순서가달라도같은건물과대표분담을선택한다()
    {
        var buildings = Buildings(620);
        var categories = buildings.ToDictionary(item => item.Id, item => "category-" + (Index(item.Id) % 5));

        var first = 평창군공간파생Pipeline.SelectOneRepresentativePerBuildingCategory(buildings, categories);
        var second = 평창군공간파생Pipeline.SelectOneRepresentativePerBuildingCategory(
            buildings.Reverse().ToArray(), categories);

        Assert.Equal(
            first.Select(Value).OrderBy(item => item, StringComparer.Ordinal),
            second.Select(Value).OrderBy(item => item, StringComparer.Ordinal));
    }

    [Fact]
    public async Task 평창군Pipeline은_공유DB620건을보존하고_공간실행에는종류별대표만저장한다()
    {
        await using var publicDb = CreatePublicDataDb();
        await using var derivedDb = CreateDerivedDb();
        await publicDb.Database.EnsureCreatedAsync();
        await derivedDb.Database.EnsureCreatedAsync();
        var buildings = Buildings(620);
        publicDb.BuildingRegisterTitles.AddRange(buildings);
        var definitions = Enumerable.Range(0, 5).Select(index => new 건축물용도CategoryDefinition
        {
            CategoryCode = "category-" + index, DisplayNameKo = "시험 건물 종류 " + index,
            DescriptionKo = "종류별 하나 대표 선정 시험", WorldRoleCode = "Test",
            SortOrder = index, PresentationEligible = true,
        }).ToArray();
        publicDb.BuildingCategoryDefinitions.AddRange(definitions);
        publicDb.BuildingCategoryAssignments.AddRange(buildings.Select((building, index) => new 건축물용도CategoryAssignment
        {
            Id = Guid.NewGuid(), BuildingRecordId = building.Id, BuildingRecord = building,
            CategoryCode = definitions[index % definitions.Length].CategoryCode,
            Category = definitions[index % definitions.Length], IsPrimary = true,
            AssignmentMethodCode = "Fixture", EvidenceKindCode = "Derived",
            RuleRevision = "fixture-category.v1", ClassifiedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
        }));
        await publicDb.SaveChangesAsync();
        var pipeline = new 평창군공간파생Pipeline(
            publicDb, new SimulationWorld파생원장Store(derivedDb));

        var result = await pipeline.실행Async(CancellationToken.None);

        Assert.Equal(620, result.건축물수);
        Assert.Equal(5, result.대표건축물수);
        Assert.Equal(615, result.표현제외건축물수);
        Assert.Equal(620, await publicDb.BuildingRegisterTitles.CountAsync());
        Assert.Equal(5, await derivedDb.Nodes.CountAsync(item => item.NodeKindCode == "Building"));
        Assert.Equal(620, await derivedDb.Nodes.Where(item => item.NodeKindCode == "Building")
            .SumAsync(item => item.RepresentedRecordCount ?? 0));
        Assert.Equal(5, result.대표군수);
    }

    [Fact]
    public async Task 종류별대표는_고정Seed시험상태와공간규칙을결합해표현결과를저장한다()
    {
        await using var publicDb = CreatePublicDataDb();
        await using var derivedDb = CreateDerivedDb();
        await publicDb.Database.EnsureCreatedAsync();
        await derivedDb.Database.EnsureCreatedAsync();
        var buildings = Buildings(50);
        publicDb.BuildingRegisterTitles.AddRange(buildings);
        var definitions = Enumerable.Range(0, 5).Select(index => new 건축물용도CategoryDefinition
        {
            CategoryCode = "category-" + index, DisplayNameKo = "시험 종류 " + index,
            DescriptionKo = "시험", WorldRoleCode = "Test", SortOrder = index, PresentationEligible = true,
        }).ToArray();
        publicDb.BuildingCategoryDefinitions.AddRange(definitions);
        publicDb.BuildingCategoryAssignments.AddRange(buildings.Select((building, index) => new 건축물용도CategoryAssignment
        {
            Id = Guid.NewGuid(), BuildingRecordId = building.Id, BuildingRecord = building,
            CategoryCode = definitions[index % 5].CategoryCode, Category = definitions[index % 5], IsPrimary = true,
            AssignmentMethodCode = "Fixture", EvidenceKindCode = "Derived", RuleRevision = "fixture.v1",
            ClassifiedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
        }));
        await publicDb.SaveChangesAsync();
        var spatial = await new 평창군공간파생Pipeline(
            publicDb, new SimulationWorld파생원장Store(derivedDb)).실행Async(CancellationToken.None);
        var store = new SimulationWorld객체표현규칙Store(derivedDb);
        var shell = new SimulationWorld객체표현해석JobShell(
            new SimulationWorld공간실행Reader(derivedDb), store);
        var demo = new SimulationWorld건물종류DemoPipeline(
            new SimulationWorld공간실행Reader(derivedDb), store, shell);

        var first = await demo.실행Async(spatial.파생실행고유식별자, CancellationToken.None);
        derivedDb.ChangeTracker.Clear();
        var second = await demo.실행Async(spatial.파생실행고유식별자, CancellationToken.None);

        Assert.Equal(5, first.Presentations.Count);
        Assert.Equal(50, first.Presentations.Sum(item => item.RepresentedRecordCount));
        Assert.All(first.Presentations, item => Assert.Contains(
            item.FixtureSimulationStateCode, new[] { "Idle", "Operating", "Loading", "Maintenance" }));
        Assert.True(first.RuleCatalogInserted);
        Assert.True(first.InterpretationInserted);
        Assert.False(second.RuleCatalogInserted);
        Assert.False(second.InterpretationInserted);
        Assert.Equal(first.OutputHashSha256, second.OutputHashSha256);
        Assert.Equal(5, await derivedDb.SpatialRuleMetadata.CountAsync());
        Assert.Equal(5, await derivedDb.SimulationRuleMetadata.CountAsync());
        Assert.Equal(5, await derivedDb.ObjectRepresentationBindingRules.CountAsync());
        Assert.Equal(5, await derivedDb.ObjectRepresentationInterpretationResults.CountAsync());
    }

    private static 건축물대장표제부Record[] Buildings(int count) => Enumerable.Range(0, count)
        .Select(index => new 건축물대장표제부Record
        {
            Id = GuidFor(index), RegisterManagementPk = "building-" + index,
            RegisterKindCode = "title", SigunguCode = "51760",
            LegalDongCode = (index % 4) switch { 0 => "38000", 1 => "36000", 2 => "25000", _ => "99999" },
            BuildingName = index % 23 == 0 ? "대표 이름 건물 " + index : null,
            MainPurposeName = "시험 건물", BuildingAreaSquareMeters = index % 3 == 0 ? 100 + index : null,
            AboveGroundFloorCount = index % 4 == 0 ? 2 : null,
            SourceRevision = "building-test-v1", EvidenceSnapshotId = 1,
            ObservedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
        }).ToArray();

    private static Guid GuidFor(int index)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(index + 1).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private static int Index(Guid id) => BitConverter.ToInt32(id.ToByteArray(), 0) - 1;
    private static string Value(대표건축물선정항목 item) =>
        $"{item.Building.Id:N}|{item.GroupCode}|{item.RepresentedRecordCount}|{item.Rank}";

    private static SimulationWorldUnity타일Manifest Tile(string tileKey)
    {
        var segments = tileKey.Split(':');
        var level = int.Parse(segments[1][1..], System.Globalization.CultureInfo.InvariantCulture);
        return new SimulationWorldUnity타일Manifest
        {
            StableId = "unity-tile:" + tileKey,
            TileKey = tileKey,
            Level = level,
            SizeMeters = level switch { 0 => 8000m, 1 => 2000m, _ => 500m },
        };
    }

    private static PublicDataIngestionDbContext CreatePublicDataDb() => new(
        new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase("representative-public-" + Guid.NewGuid().ToString("N")).Options);

    private static SimulationWorld파생DbContext CreateDerivedDb() => new(
        new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase("representative-derived-" + Guid.NewGuid().ToString("N")).Options);
}
