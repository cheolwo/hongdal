using System.Text.Json;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationWorldInteractionSpatialSeedbedTests
{
    [Fact]
    public void 일곱_공간모판은_21개_E3_WI와_경관구성후보를_결정적으로검증한다()
    {
        var first = SimulationWorldInteractionSpatialSeedbedTestFixture.Compile();
        var second = SimulationWorldInteractionSpatialSeedbedTestFixture.Compile();

        Assert.Equal("simulation-world-interaction-spatial-seedbeds.r1", first.Revision);
        Assert.Equal("simulation-world-interactions.r8",
            first.WorldInteractionCatalogRevision);
        Assert.Equal("pyeongchang-landscape-grammar.v1",
            first.LandscapeGrammarRevision);
        Assert.Equal(7, first.Definitions.Length);
        Assert.Equal(21, first.Definitions.SelectMany(value => value.IncludedWiIds)
            .Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(first.CatalogHashSha256, second.CatalogHashSha256);
        Assert.All(first.Definitions, definition =>
        {
            Assert.Equal(64, definition.DefinitionHashSha256.Length);
            Assert.Equal(64, definition.SourceFileHashSha256.Length);
            Assert.Equal(64, definition.AuthoredDocumentHashSha256.Length);
            Assert.Equal(SimulationWorld상호작용공간모판Codes.ApprovedForSimulation,
                definition.ReviewStatusCode);
            Assert.True(definition.PresentationOnly);
            Assert.False(definition.IsOperationalState);
            Assert.Equal(definition.DefinitionHashSha256,
                second.Definitions.Single(value => value.StableId == definition.StableId)
                    .DefinitionHashSha256);
        });

        var workYard = first.Definitions.Single(value =>
            value.StableId == "wi-spatial-seedbed:farm-work-yard.v1");
        Assert.Equal(2, workYard.InternalSpaces.Length);
        Assert.Contains(workYard.ExternalConnectorStubs, value =>
            value.FlowDirectionCode == SimulationWorld상호작용공간모판Codes.Input);
        Assert.Contains(workYard.ExternalConnectorStubs, value =>
            value.FlowDirectionCode == SimulationWorld상호작용공간모판Codes.Output);
    }

    [Fact]
    public void 공간모판_Scenario어댑터는_지역좌표없이_기존공간계약을생성한다()
    {
        var world = SimulationWorldInteractionSpatialSeedbedTestFixture.CreateSpatialWorld();

        Assert.Equal(14, world.Definitions.Length);
        Assert.Equal(world.Definitions.Length, world.Definitions
            .Select(value => value.SpatialStableId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(world.Definitions, definition =>
        {
            Assert.Equal(Simulation공간근거종류Codes.Scenario,
                definition.EvidenceKindCode);
            Assert.Equal(string.Empty, definition.LandscapeGraphStableId);
            Assert.Equal(string.Empty, definition.LandscapeNodeStableId);
            Assert.Contains("limitation:scenario-spatial-seedbed-not-landscape-graph",
                definition.SourceStableIds);
            Assert.Contains(definition.SourceStableIds,
                value => value.StartsWith("wi-spatial-seedbed:",
                    StringComparison.Ordinal));
            Assert.Equal(64, definition.DefinitionHashSha256.Length);
        });
        var storage = world.Definitions.Single(value => value.SpatialStableId ==
            SimulationWorldInteractionSpatialSeedbedTestFixture.HubStorage);
        Assert.Equal(10_000m, storage.BaseCapacities.Single(value =>
            value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity);
    }

    [Fact]
    public void 공간모판의_금지된_지역필드는_승인을차단한다()
    {
        using var fixture = MutableSeedbedFixture.Create();
        fixture.ReplaceInDefinition("farm-production.v1.json",
            "\"summary\":", "\"areaSetStableId\": \"forbidden\",\n  \"summary\":");

        var error = Assert.Throws<InvalidOperationException>(() => fixture.Compile());
        Assert.Equal("WiSpatialSeedbedForbiddenProperty:areaSetStableId", error.Message);
    }

    [Fact]
    public void 공간모판의_WI필수능력누락은_승인을차단한다()
    {
        using var fixture = MutableSeedbedFixture.Create();
        fixture.ReplaceInDefinition("farm-work-yard.v1.json",
            "\"Spatial.CollectionWorkArea\"", "\"Spatial.RepairWorkArea\"");

        var error = Assert.Throws<InvalidOperationException>(() => fixture.Compile());
        Assert.Equal("WiSpatialSeedbedCapabilityMissing:WI-FARM-05:Spatial.CollectionWorkArea",
            error.Message);
    }

    [Fact]
    public void 공간모판사이_외부연결구_유형불일치는_승인을차단한다()
    {
        using var fixture = MutableSeedbedFixture.Create();
        fixture.ReplaceInDefinition("farm-hub-corridor.v1.json",
            "\"connectorTypeCode\": \"farm-road\"",
            "\"connectorTypeCode\": \"mismatched-road\"");

        var error = Assert.Throws<InvalidOperationException>(() => fixture.Compile());
        Assert.Equal(
            "WiSpatialSeedbedExternalConnectorTypeMismatch:WI-LOG-02:WI-LOG-03",
            error.Message);
    }

    private sealed class MutableSeedbedFixture : IDisposable
    {
        private readonly string root;

        private MutableSeedbedFixture(string root) => this.root = root;

        public static MutableSeedbedFixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "ssalddel-wi-seedbed-" +
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            CopyDirectory(SimulationWorldInteractionSpatialSeedbedTestFixture.SeedbedRoot, root);
            return new MutableSeedbedFixture(root);
        }

        public void ReplaceInDefinition(string fileName, string oldValue, string newValue)
        {
            var path = Path.Combine(root, "definitions", fileName);
            var text = File.ReadAllText(path);
            Assert.Contains(oldValue, text, StringComparison.Ordinal);
            File.WriteAllText(path, text.Replace(oldValue, newValue,
                StringComparison.Ordinal));
        }

        public SimulationWorld상호작용공간모판Catalog Compile() =>
            new SimulationWorld상호작용공간모판Compiler(
                Path.Combine(root, "catalog.json"),
                SimulationWorldInteractionSpatialSeedbedTestFixture.WorldInteractionCatalog,
                SimulationWorldInteractionSpatialSeedbedTestFixture.LandscapeGrammar).Compile();

        public void Dispose() => Directory.Delete(root, recursive: true);

        private static void CopyDirectory(string source, string destination)
        {
            foreach (var directory in Directory.GetDirectories(source, "*",
                         SearchOption.AllDirectories))
                Directory.CreateDirectory(directory.Replace(source, destination,
                    StringComparison.Ordinal));
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(file, file.Replace(source, destination, StringComparison.Ordinal));
        }
    }
}

internal static class SimulationWorldInteractionSpatialSeedbedTestFixture
{
    internal const string ProductionPlot = "spatial:seedbed:farm-production:production-plot";
    internal const string CollectionArea = "spatial:seedbed:farm-work-yard:collection";
    internal const string PackingArea = "spatial:seedbed:farm-work-yard:packing";
    internal const string LoadingArea = "spatial:seedbed:farm-loading-gate:loading";
    internal const string FarmGate = "spatial:seedbed:farm-loading-gate:gate";
    internal const string FarmHubCorridor = "spatial:seedbed:farm-hub-corridor:transit";
    internal const string HubUnloading = "spatial:seedbed:hub-receiving:unloading";
    internal const string HubInspection = "spatial:seedbed:hub-receiving:inspection";
    internal const string HubStorage = "spatial:seedbed:hub-receiving:storage";

    internal static readonly string SeedbedRoot = Resolve(
        "eng/world-seedbeds/wi-spatial-seedbeds");
    internal static readonly string WorldInteractionCatalog = Resolve(
        "eng/execution-ledgers/world-interactions.json");
    internal static readonly string LandscapeGrammar = Resolve(
        "eng/world-seedbeds/manifests/pyeongchang-landscape-grammar.v1.json");

    internal static SimulationWorld상호작용공간모판Catalog Compile() =>
        new SimulationWorld상호작용공간모판Compiler(
            Path.Combine(SeedbedRoot, "catalog.json"),
            WorldInteractionCatalog,
            LandscapeGrammar).Compile();

    internal static Simulation공간세계InitialStateRequest CreateSpatialWorld()
    {
        var farmFacility = "facility:wi-farm:daegwallyeong";
        var hubFacility = "facility:sim:pyeongchang:jinbu-hub";
        var profile = new SimulationWorld상호작용공간모판ScenarioProfile
        {
            Revision = "pyeongchang-farm-hub-seedbed-scenario.r1",
            AreaSetStableId = "area-set:scenario:pyeongchang:farm-hub-seedbeds.v1",
            SourceStableIds = new[] { "scenario:pyeongchang-farm-hub-seedbeds.v1" },
            SpaceBindings = new[]
            {
                Binding("wi-spatial-seedbed:farm-production.v1", "production-plot",
                    ProductionPlot, farmFacility, "area:pyeongchang:daegwallyeong-farm"),
                Binding("wi-spatial-seedbed:farm-work-yard.v1", "collection-area",
                    CollectionArea, farmFacility, "area:pyeongchang:daegwallyeong-farm"),
                Binding("wi-spatial-seedbed:farm-work-yard.v1", "packing-area",
                    PackingArea, farmFacility, "area:pyeongchang:daegwallyeong-farm"),
                Binding("wi-spatial-seedbed:farm-loading-gate.v1", "loading-area",
                    LoadingArea, farmFacility, "area:pyeongchang:daegwallyeong-farm"),
                Binding("wi-spatial-seedbed:farm-loading-gate.v1", "farm-gate",
                    FarmGate, farmFacility, "area:pyeongchang:daegwallyeong-farm"),
                Binding("wi-spatial-seedbed:farm-hub-corridor.v1",
                    "cargo-transit-corridor", FarmHubCorridor,
                    "facility:scenario:pyeongchang:farm-hub-corridor",
                    "area:pyeongchang:farm-hub-corridor"),
                Binding("wi-spatial-seedbed:hub-receiving-storage.v1", "unloading-area",
                    HubUnloading, hubFacility, "area:sim:pyeongchang:jinbu-hub"),
                Binding("wi-spatial-seedbed:hub-receiving-storage.v1", "inspection-area",
                    HubInspection, hubFacility, "area:sim:pyeongchang:jinbu-hub"),
                Binding("wi-spatial-seedbed:hub-receiving-storage.v1", "storage-area",
                    HubStorage, hubFacility, "area:sim:pyeongchang:jinbu-hub"),
                Binding("wi-spatial-seedbed:nature-survival-home.v1", "safe-clearing",
                    "spatial:seedbed:nature-survival-home:safe-clearing",
                    "facility:scenario:pyeongchang:nature-home",
                    "area:pyeongchang:nature-home"),
                Binding("wi-spatial-seedbed:nature-survival-home.v1", "cabin-site",
                    "spatial:seedbed:nature-survival-home:cabin-site",
                    "facility:scenario:pyeongchang:nature-home",
                    "area:pyeongchang:nature-home"),
                Binding("wi-spatial-seedbed:nature-survival-home.v1", "cabin-threshold",
                    "spatial:seedbed:nature-survival-home:cabin-threshold",
                    "facility:scenario:pyeongchang:nature-home",
                    "area:pyeongchang:nature-home"),
                Binding("wi-spatial-seedbed:nature-survival-encounter.v1", "harvest-grove",
                    "spatial:seedbed:nature-survival-encounter:harvest-grove",
                    "facility:scenario:pyeongchang:nature-encounter",
                    "area:pyeongchang:nature-home"),
                Binding("wi-spatial-seedbed:nature-survival-encounter.v1", "encounter-edge",
                    "spatial:seedbed:nature-survival-encounter:encounter-edge",
                    "facility:scenario:pyeongchang:nature-encounter",
                    "area:pyeongchang:nature-home"),
            },
        };
        return SimulationWorld상호작용공간모판ScenarioBuilder.Build(Compile(), profile);
    }

    private static SimulationWorld상호작용공간모판ScenarioSpaceBinding Binding(
        string seedbedStableId,
        string spaceCode,
        string spatialStableId,
        string facilityStableId,
        string areaStableId) => new()
        {
            SeedbedStableId = seedbedStableId,
            InternalSpaceCode = spaceCode,
            SpatialStableId = spatialStableId,
            FacilityStableId = facilityStableId,
            AreaStableId = areaStableId,
        };

    private static string Resolve(string relativePath)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException(relativePath);
    }
}
