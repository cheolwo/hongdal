using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationWorldAreaSetLandscapeGraphTests
{
    [Fact]
    public void JSON권위와_Markdown참조가_같은다섯Graph정의를만든다()
    {
        var reader = Reader();

        Assert.True(reader.TryRead(out var catalog, out var errorCode), errorCode);
        Assert.Equal(PyeongchangAreaSetStableIds.AreaSet, catalog.AreaSet.AreaSetStableId);
        Assert.Equal(5, catalog.AreaSet.LandscapeGraphs.Length);
        Assert.Equal(4, catalog.AreaSet.GraphRelations.Length);
        Assert.Equal(
            [
                PyeongchangAreaSetStableIds.FarmGraph,
                PyeongchangAreaSetStableIds.FarmHubCorridorGraph,
                PyeongchangAreaSetStableIds.HubGraph,
                PyeongchangAreaSetStableIds.HubTownCorridorGraph,
                PyeongchangAreaSetStableIds.TownGraph,
            ],
            catalog.AreaSet.LandscapeGraphs.Select(item => item.LandscapeGraphStableId));
        Assert.Equal(64, catalog.AreaSet.DefinitionHashSha256.Length);
        Assert.Equal(64, catalog.AreaSet.DocumentHashSha256.Length);
        Assert.Equal(4, catalog.Graphs[PyeongchangAreaSetStableIds.FarmGraph].TileRefs.Length);
        Assert.Equal(2, catalog.Graphs[PyeongchangAreaSetStableIds.FarmHubCorridorGraph].AreaRefs.Length);
    }

    [Fact]
    public void Markdown참조가_JSON과다르면_정의를거부한다()
    {
        var source = Path.GetDirectoryName(DefinitionPath())!;
        var temporary = Path.Combine(Path.GetTempPath(), "areaset-invalid-" + Guid.NewGuid().ToString("N"));
        CopyDirectory(source, temporary);
        try
        {
            File.AppendAllText(Path.Combine(temporary, "authored", "area-set.md"),
                "\n@area area:sim:pyeongchang:not-declared\n");

            var reader = new FileSimulationWorldAreaSetDefinitionReader(
                Path.Combine(temporary, "area-set.json"));

            Assert.False(reader.TryRead(out _, out var errorCode));
            Assert.Equal("AreaMarkdownRefMismatch", errorCode);
        }
        finally
        {
            Directory.Delete(temporary, true);
        }
    }

    [Fact]
    public async Task GraphJob은_자료가있는타일만조립하고_나머지를명시적으로남긴다()
    {
        var store = new CapturingGraphStore();
        var job = Job(store);

        var graphs = await job.BuildAsync();

        Assert.Equal(5, graphs.Count);
        var farm = Assert.Single(graphs, item =>
            item.LandscapeGraphStableId == PyeongchangAreaSetStableIds.FarmGraph);
        Assert.Equal(SimulationWorldLandscapeCompositionCodes.PartialUnresolved, farm.StatusCode);
        Assert.Equal(5, farm.Placements.Length);
        Assert.Contains(farm.Unresolved, item =>
            item.ReasonCode == SimulationWorldLandscapeCompositionCodes.WaitingForSpatialArtifact);
        Assert.Equal(4, graphs.Count(item =>
            item.StatusCode == SimulationWorldLandscapeCompositionCodes.Declared));
        Assert.Equal(PyeongchangAreaSetStableIds.AreaSet, store.AreaSet!.AreaSetStableId);
        var statusDocument = SimulationWorldAreaSetStatusMarkdownRenderer.Render(
            store.AreaSet, graphs);
        Assert.Equal(statusDocument,
            SimulationWorldAreaSetStatusMarkdownRenderer.Render(store.AreaSet, graphs));
        Assert.Contains("대관령면 Farm [PartialUnresolved]", statusDocument);
        Assert.Contains(store.AreaSet.DefinitionHashSha256, statusDocument);
    }

    [Fact]
    public async Task 파생DB는_AreaSet과Graph를저장하고_기존타일응답을투영한다()
    {
        var options = new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase("areaset-graph-" + Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using var db = new SimulationWorld파생DbContext(options);
        var store = new SimulationWorldAreaSetGraphStore(db);

        var graphs = await Job(store).BuildAsync();
        var areaSet = await store.ReadAreaSetAsync(PyeongchangAreaSetStableIds.AreaSet);
        var graph = await store.ReadGraphAsync(PyeongchangAreaSetStableIds.FarmGraph);
        var tile = await store.ReadTileFacadeAsync("kr5186:l2:700:1145");

        Assert.NotNull(areaSet);
        Assert.Equal(5, areaSet!.LandscapeGraphs.Length);
        Assert.Equal(
            [
                PyeongchangAreaSetStableIds.FarmGraph,
                PyeongchangAreaSetStableIds.FarmHubCorridorGraph,
                PyeongchangAreaSetStableIds.HubGraph,
                PyeongchangAreaSetStableIds.HubTownCorridorGraph,
                PyeongchangAreaSetStableIds.TownGraph,
            ],
            areaSet.LandscapeGraphs.Select(item => item.LandscapeGraphStableId));
        Assert.NotNull(graph);
        Assert.Equal(4, graph!.TileRefs.Length);
        Assert.NotNull(tile);
        Assert.Equal(PyeongchangAreaSetStableIds.AreaSet, tile!.AreaSetStableId);
        Assert.Equal(5, tile.Placements.Length);
        Assert.StartsWith("legacy-tile-view:kr5186:l2:700:1145:", tile.GraphBuildStableId);
        Assert.Equal(5, await db.LandscapeGraphDefinitions.CountAsync());
        Assert.Equal(4, await db.LandscapeGraphRelations.CountAsync());
    }

    [Fact]
    public void 생성가능한두Graph의_Connector가호환되지않으면_부분미해결로남긴다()
    {
        var fromGraph = GraphWithStub("graph:from", "stub:from", "east", 350500d);
        var toGraph = GraphWithStub("graph:to", "stub:to", "east", 350500d);
        var areaSet = new SimulationWorldAreaSetDefinitionResponse
        {
            GraphRelations =
            [
                new SimulationWorldLandscapeGraphRelationResponse
                {
                    RelationStableId = "relation:test",
                    FromGraphStableId = fromGraph.LandscapeGraphStableId,
                    ToGraphStableId = toGraph.LandscapeGraphStableId,
                    RelationCode = SimulationWorldLandscapeCompositionCodes.GraphConnected,
                    ConnectorPair = new SimulationWorldLandscapeConnectorPairResponse
                    {
                        FromConnectorStableId = "stub:from",
                        ToConnectorStableId = "stub:to",
                        ConnectorTypeCode = "farm-road",
                        RouteSignature = "route:test",
                    },
                },
            ],
        };

        SimulationWorldLandscapeGraphRelationValidator.Apply(
            areaSet, [fromGraph, toGraph]);

        Assert.All([fromGraph, toGraph], graph =>
        {
            Assert.Equal(SimulationWorldLandscapeCompositionCodes.PartialUnresolved,
                graph.StatusCode);
            Assert.Contains(graph.Unresolved, item =>
                item.ReasonCode == SimulationWorldLandscapeCompositionCodes.GraphConnectorUnresolved);
            Assert.Equal(64, graph.GraphHashSha256.Length);
        });
    }

    private static SimulationWorldAreaSetLandscapeGraphJobShell Job(
        ISimulationWorldAreaSetGraphStore store) => new(
        Reader(),
        new SimulationWorldLandscapeGrammarManifestReader(GrammarPath()),
        new PyeongchangFirstLandscapeSkeletonSource(new CentralTileArtifactReader()),
        new SimulationWorldLandscapeGraphAssembler(),
        store);

    private static FileSimulationWorldAreaSetDefinitionReader Reader() => new(DefinitionPath());

    private static string DefinitionPath() => FindRepoFile("eng", "world-seedbeds", "area-sets",
        "pyeongchang-farm-hub-town.v1", "area-set.json");

    private static string GrammarPath() => FindRepoFile("eng", "world-seedbeds", "manifests",
        "pyeongchang-landscape-grammar.v1.json");

    private static string FindRepoFile(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(new[] { current.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new FileNotFoundException(string.Join('/', parts));
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(directory.Replace(source, target, StringComparison.Ordinal));
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(source, target, StringComparison.Ordinal));
    }

    private static SimulationWorldLandscapeGraphResponse GraphWithStub(
        string graphId,
        string stubId,
        string direction,
        double easting) => new()
    {
        LandscapeGraphStableId = graphId,
        StatusCode = SimulationWorldLandscapeCompositionCodes.Available,
        ExternalConnectorStubs =
        [
            new SimulationWorldLandscapeExternalConnectorResponse
            {
                StubStableId = stubId,
                PlacementStableId = "placement:" + graphId,
                NeighborTileKey = "kr5186:l2:701:1145",
                ConnectorTypeCode = "farm-road",
                RouteSignature = "route:test",
                DirectionCode = direction,
                EvidenceKindCode = SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario,
                WorldEastingMeters = easting,
                WorldNorthingMeters = 572750d,
                WidthMeters = 4d,
            },
        ],
    };

    private sealed class CentralTileArtifactReader : ISimulationWorldTileArtifactReader
    {
        public bool TryRead(
            string tileKey, string layerCode, out SimulationWorldTileArtifactSnapshot value)
        {
            value = new SimulationWorldTileArtifactSnapshot { TileKey = tileKey, LayerCode = layerCode };
            return tileKey == "kr5186:l2:700:1145";
        }
    }

    private sealed class CapturingGraphStore : ISimulationWorldAreaSetGraphStore
    {
        public SimulationWorldAreaSetDefinitionResponse? AreaSet { get; private set; }
        public IReadOnlyList<SimulationWorldLandscapeGraphResponse> Graphs { get; private set; } =
            Array.Empty<SimulationWorldLandscapeGraphResponse>();

        public Task ReplaceAreaSetBuildAsync(
            SimulationWorldAreaSetDefinitionResponse areaSet,
            IReadOnlyList<SimulationWorldLandscapeGraphResponse> graphs,
            CancellationToken cancellationToken = default)
        {
            AreaSet = areaSet;
            Graphs = graphs;
            return Task.CompletedTask;
        }

        public Task<SimulationWorldAreaSetDefinitionResponse?> ReadAreaSetAsync(
            string areaSetStableId, CancellationToken cancellationToken = default) =>
            Task.FromResult(AreaSet);

        public Task<SimulationWorldLandscapeGraphResponse?> ReadGraphAsync(
            string landscapeGraphStableId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Graphs.FirstOrDefault(item =>
                item.LandscapeGraphStableId == landscapeGraphStableId));

        public Task<SimulationWorldLandscapeCompositionTileResponse?> ReadTileFacadeAsync(
            string tileKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<SimulationWorldLandscapeCompositionTileResponse?>(null);
    }
}
