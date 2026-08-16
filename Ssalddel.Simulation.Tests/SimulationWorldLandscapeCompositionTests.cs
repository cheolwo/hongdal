using Microsoft.EntityFrameworkCore;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldLandscapeCompositionTests
{
    [Fact]
    public void 안전Manifest는_52개모판군과_156개변형만_공개한다()
    {
        var path = FindRepoFile(
            "eng", "world-seedbeds", "manifests",
            "pyeongchang-landscape-grammar.v1.json");
        var json = File.ReadAllText(path);
        var reader = new SimulationWorldLandscapeGrammarManifestReader(path);

        Assert.True(reader.TryRead(out var catalog, out var errorCode), errorCode);
        Assert.Equal(156, catalog.Entries.Count);
        Assert.Equal(52, catalog.Entries.GroupBy(item =>
            (item.FamilyCode, item.SetName)).Count());
        Assert.DoesNotContain("Assets/", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".prefab", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"guid\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void 안전Manifest내용이바뀌고Hash가그대로면_대장을거부한다()
    {
        var sourcePath = FindRepoFile(
            "eng", "world-seedbeds", "manifests",
            "pyeongchang-landscape-grammar.v1.json");
        var altered = File.ReadAllText(sourcePath).Replace(
            "\"triangleCount\": 1330", "\"triangleCount\": 1331",
            StringComparison.Ordinal);
        Assert.NotEqual(File.ReadAllText(sourcePath), altered);
        var tempPath = Path.Combine(Path.GetTempPath(),
            "landscape-grammar-tampered-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(tempPath, altered);
            var reader = new SimulationWorldLandscapeGrammarManifestReader(tempPath);

            Assert.False(reader.TryRead(out _, out var errorCode));
            Assert.Equal(SimulationWorldLandscapeCompositionCodes.CatalogMismatch, errorCode);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void 같은공간좌표와문법은_같은GraphHash와배치를_만든다()
    {
        var catalog = ReadCatalog();
        var source = new PyeongchangFirstLandscapeSkeletonSource(
            new AllLayersAvailableReader());
        Assert.True(source.TryCreate("kr5186:l2:700:1145", out var skeleton, out _));
        var assembler = new SimulationWorldLandscapeGraphAssembler();

        var first = assembler.Assemble(skeleton, catalog);
        var second = assembler.Assemble(skeleton, catalog);

        Assert.Equal(first.GraphHashSha256, second.GraphHashSha256);
        Assert.Equal(first.Placements.Select(item => item.CompositionKey),
            second.Placements.Select(item => item.CompositionKey));
        Assert.All(first.Placements, item => Assert.True(item.PresentationOnly));
        Assert.Contains(first.ExternalConnectorStubs,
            item => item.NeighborTileKey == "kr5186:l2:701:1145");
        Assert.Contains(first.Edges, item =>
            item.EvidenceKindCode == SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario);
    }

    [Fact]
    public void 반복가능한면형모판도_같은변형을_세번연속선택하지않는다()
    {
        var catalog = ReadCatalog();
        var nodes = Enumerable.Range(0, 12).Select(index =>
            new SimulationWorldLandscapeSkeletonNode
            {
                NodeStableId = "field:" + index,
                NodeKindCode = SimulationWorldLandscapeCompositionCodes.Area,
                SemanticCode = "field",
                PreferredFamilyCode = "farm",
                PreferredSetName = "감자밭 두렁",
                EvidenceKindCode = SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario,
                CenterEastingMeters = 350050 + index * 24,
                CenterNorthingMeters = 572550,
                WidthMeters = 24,
                DepthMeters = 24,
            }).ToArray();
        var result = new SimulationWorldLandscapeGraphAssembler().Assemble(
            new SimulationWorldLandscapeSkeleton
            {
                TileKey = "kr5186:l2:700:1145",
                AreaSetStableId = "test-area-set",
                Nodes = nodes,
            }, catalog);
        var variants = result.Placements.Select(item =>
            item.CompositionKey.Split(':').Last()).ToArray();

        Assert.DoesNotContain(Enumerable.Range(0, variants.Length - 2), index =>
            variants[index] == variants[index + 1]
            && variants[index] == variants[index + 2]);
    }

    [Fact]
    public async Task 필수공간Layer가없으면_네타일모두자료대기로_저장한다()
    {
        var store = new CapturingStore();
        var shell = new SimulationWorldLandscapeCompositionJobShell(
            new SimulationWorldLandscapeGrammarManifestReader(FindRepoFile(
                "eng", "world-seedbeds", "manifests",
                "pyeongchang-landscape-grammar.v1.json")),
            new PyeongchangFirstLandscapeSkeletonSource(
                new DisabledSimulationWorldTileArtifactReader()),
            store,
            new SimulationWorldLandscapeGraphAssembler());

        var results = await shell.BuildFirstVerticalSliceAsync();

        Assert.Equal(4, results.Count);
        Assert.All(results, item => Assert.Equal(
            SimulationWorldLandscapeCompositionCodes.WaitingForSpatialArtifact,
            item.StatusCode));
        Assert.Equal(results, store.LastSaved);
    }

    [Fact]
    public async Task 경관Graph파생DB는_NodeEdge배치미해결을_같은조회로복원한다()
    {
        var options = new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase("landscape-graph-" + Guid.NewGuid().ToString("N"))
            .Options;
        await using var db = new SimulationWorld파생DbContext(options);
        var source = new PyeongchangFirstLandscapeSkeletonSource(
            new AllLayersAvailableReader());
        Assert.True(source.TryCreate("kr5186:l2:701:1144", out var skeleton, out _));
        var expected = new SimulationWorldLandscapeGraphAssembler()
            .Assemble(skeleton, ReadCatalog());
        var store = new SimulationWorldLandscapeCompositionStore(db);

        await store.ReplaceBuildAsync(new[] { expected });
        var actual = await store.ReadLatestAsync(expected.TileKey);

        Assert.NotNull(actual);
        Assert.Equal(expected.GraphHashSha256, actual!.GraphHashSha256);
        Assert.Equal(expected.Nodes.Length, actual.Nodes.Length);
        Assert.Equal(expected.Edges.Length, actual.Edges.Length);
        Assert.Equal(expected.Placements.Length, actual.Placements.Length);
        Assert.Equal(expected.ExternalConnectorStubs.Length,
            actual.ExternalConnectorStubs.Length);
    }

    private static SimulationWorldLandscapeGrammarCatalog ReadCatalog()
    {
        var reader = new SimulationWorldLandscapeGrammarManifestReader(FindRepoFile(
            "eng", "world-seedbeds", "manifests",
            "pyeongchang-landscape-grammar.v1.json"));
        Assert.True(reader.TryRead(out var catalog, out var errorCode), errorCode);
        return catalog;
    }

    private static string FindRepoFile(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(new[] { current.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new FileNotFoundException(string.Join(Path.DirectorySeparatorChar, parts));
    }

    private sealed class AllLayersAvailableReader : ISimulationWorldTileArtifactReader
    {
        public bool TryRead(string tileKey, string layerCode,
            out SimulationWorldTileArtifactSnapshot value)
        {
            value = new SimulationWorldTileArtifactSnapshot
            {
                TileKey = tileKey,
                LayerCode = layerCode,
                ArtifactHashSha256 = new string('a', 64),
            };
            return true;
        }
    }

    private sealed class CapturingStore : ISimulationWorldLandscapeCompositionStore
    {
        public IReadOnlyList<SimulationWorldLandscapeCompositionTileResponse> LastSaved { get; private set; }
            = Array.Empty<SimulationWorldLandscapeCompositionTileResponse>();

        public Task ReplaceBuildAsync(
            IReadOnlyList<SimulationWorldLandscapeCompositionTileResponse> tiles,
            CancellationToken cancellationToken = default)
        {
            LastSaved = tiles;
            return Task.CompletedTask;
        }
    }
}
