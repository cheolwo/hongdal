using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationWorldLandscapeGrammarCatalogReader
    {
        bool TryRead(out SimulationWorldLandscapeGrammarCatalog catalog, out string errorCode);
    }

    public interface ISimulationWorldLandscapeSkeletonSource
    {
        bool TryCreate(string tileKey, out SimulationWorldLandscapeSkeleton skeleton,
            out string missingLayerCode);
    }

    public interface ISimulationWorldLandscapeCompositionStore
    {
        Task ReplaceBuildAsync(
            IReadOnlyList<SimulationWorldLandscapeCompositionTileResponse> tiles,
            CancellationToken cancellationToken = default);
    }

    public interface ISimulationWorldLandscapeCompositionReader
    {
        Task<SimulationWorldLandscapeCompositionTileResponse?> ReadLatestAsync(
            string tileKey,
            CancellationToken cancellationToken = default);
    }

    public sealed class DisabledSimulationWorldLandscapeGrammarCatalogReader
        : ISimulationWorldLandscapeGrammarCatalogReader
    {
        public bool TryRead(out SimulationWorldLandscapeGrammarCatalog catalog,
            out string errorCode)
        {
            catalog = new SimulationWorldLandscapeGrammarCatalog();
            errorCode = SimulationWorldLandscapeCompositionCodes.WaitingForGrammarManifest;
            return false;
        }
    }

    public sealed class DisabledSimulationWorldLandscapeCompositionStore
        : ISimulationWorldLandscapeCompositionStore,
          ISimulationWorldLandscapeCompositionReader
    {
        public Task ReplaceBuildAsync(
            IReadOnlyList<SimulationWorldLandscapeCompositionTileResponse> tiles,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<SimulationWorldLandscapeCompositionTileResponse?> ReadLatestAsync(
            string tileKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<SimulationWorldLandscapeCompositionTileResponse?>(null);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "공간 Layer 준비 상태를 확인하고 네 L2 타일의 경관 Graph를 조립·저장한다.",
        StepKey = "application.landscape-graph-job",
        DependsOnStepKeys = new[] { "domain.landscape-graph-assembler" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        WritesTo = SsalddelCodeDataScope.DerivedWorld,
        Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
        FlowOrder = 44,
        Boundary = "자료가 없는 타일은 꾸며내지 않고 대기 상태로 저장하며 Unity Prefab을 해석하지 않는다.")]
    public sealed class SimulationWorldLandscapeCompositionJobShell
    {
        public static readonly string[] FirstVerticalSliceTileKeys =
        {
            "kr5186:l2:700:1145",
            "kr5186:l2:701:1145",
            "kr5186:l2:700:1144",
            "kr5186:l2:701:1144",
        };

        private readonly ISimulationWorldLandscapeGrammarCatalogReader _catalogReader;
        private readonly ISimulationWorldLandscapeSkeletonSource _skeletonSource;
        private readonly ISimulationWorldLandscapeCompositionStore _store;
        private readonly SimulationWorldLandscapeGraphAssembler _assembler;

        public SimulationWorldLandscapeCompositionJobShell(
            ISimulationWorldLandscapeGrammarCatalogReader catalogReader,
            ISimulationWorldLandscapeSkeletonSource skeletonSource,
            ISimulationWorldLandscapeCompositionStore store,
            SimulationWorldLandscapeGraphAssembler assembler)
        {
            _catalogReader = catalogReader;
            _skeletonSource = skeletonSource;
            _store = store;
            _assembler = assembler;
        }

        public async Task<IReadOnlyList<SimulationWorldLandscapeCompositionTileResponse>>
            BuildFirstVerticalSliceAsync(CancellationToken cancellationToken = default)
        {
            if (!_catalogReader.TryRead(out var catalog, out var catalogErrorCode))
            {
                var waiting = FirstVerticalSliceTileKeys.Select(tileKey => Waiting(
                    tileKey, catalogErrorCode, "경관 문법 Manifest를 읽을 수 없습니다.", string.Empty)).ToArray();
                await _store.ReplaceBuildAsync(waiting, cancellationToken).ConfigureAwait(false);
                return waiting;
            }

            try
            {
                catalog.ValidateCanonicalCatalog();
            }
            catch (InvalidOperationException exception)
            {
                var mismatch = FirstVerticalSliceTileKeys.Select(tileKey => Waiting(
                    tileKey, SimulationWorldLandscapeCompositionCodes.CatalogMismatch,
                    exception.Message, catalog.CatalogHashSha256)).ToArray();
                await _store.ReplaceBuildAsync(mismatch, cancellationToken).ConfigureAwait(false);
                return mismatch;
            }

            var results = new List<SimulationWorldLandscapeCompositionTileResponse>();
            foreach (var tileKey in FirstVerticalSliceTileKeys)
            {
                if (!_skeletonSource.TryCreate(tileKey, out var skeleton, out var missingLayerCode))
                {
                    results.Add(Waiting(tileKey,
                        SimulationWorldLandscapeCompositionCodes.WaitingForSpatialArtifact,
                        "필수 공간 Layer가 준비되지 않았습니다: " + missingLayerCode,
                        catalog.CatalogHashSha256));
                    continue;
                }
                results.Add(_assembler.Assemble(skeleton, catalog));
            }

            await _store.ReplaceBuildAsync(results, cancellationToken).ConfigureAwait(false);
            return results;
        }

        private static SimulationWorldLandscapeCompositionTileResponse Waiting(
            string tileKey, string statusCode, string detail, string grammarHash) =>
            new SimulationWorldLandscapeCompositionTileResponse
            {
                TileKey = tileKey,
                AreaSetStableId = PyeongchangAreaSetStableIds.AreaSet,
                GraphBuildStableId = "legacy-landscape-tile-view:" + tileKey,
                GrammarRevision = SimulationWorldLandscapeCompositionCodes.GrammarRevision,
                GrammarHashSha256 = grammarHash,
                StatusCode = statusCode,
                Unresolved = new[]
                {
                    new SimulationWorldLandscapeUnresolvedResponse
                    {
                        UnresolvedStableId = "unresolved:" + tileKey + ":source",
                        ReasonCode = statusCode,
                        RequiredSemanticCode = "spatial-layer-and-landscape-grammar",
                        EvidenceKindCode = SimulationWorldLandscapeAssemblyEvidenceCodes.Derived,
                        Detail = detail,
                    },
                },
                PresentationOnly = true,
                IsOperationalState = false,
            };
    }

    public sealed class SimulationWorldLandscapeCompositionService
    {
        private readonly ISimulationWorldLandscapeCompositionReader _reader;

        public SimulationWorldLandscapeCompositionService(
            ISimulationWorldLandscapeCompositionReader reader) => _reader = reader;

        public Task<SimulationWorldLandscapeCompositionTileResponse?> ReadLatestAsync(
            string tileKey,
            CancellationToken cancellationToken = default) =>
            _reader.ReadLatestAsync(tileKey, cancellationToken);
    }

    /// <summary>
    /// 첫 vertical slice용 골격이다. 공간 Layer의 존재를 먼저 검증하고,
    /// 세부 유형 위치는 관측 사실이 아닌 통계배분·Scenario 근거로 명시한다.
    /// </summary>
    public sealed class PyeongchangFirstLandscapeSkeletonSource
        : ISimulationWorldLandscapeSkeletonSource
    {
        private readonly ISimulationWorldTileArtifactReader _artifactReader;

        public PyeongchangFirstLandscapeSkeletonSource(
            ISimulationWorldTileArtifactReader artifactReader) => _artifactReader = artifactReader;

        public bool TryCreate(string tileKey, out SimulationWorldLandscapeSkeleton skeleton,
            out string missingLayerCode)
        {
            foreach (var layerCode in new[]
            {
                SimulationWorldStreamCodes.ElevationLayer,
                SimulationWorldStreamCodes.LandCoverLayer,
                SimulationWorldStreamCodes.PlacementMaskLayer,
            })
            {
                if (!_artifactReader.TryRead(tileKey, layerCode, out _))
                {
                    skeleton = new SimulationWorldLandscapeSkeleton();
                    missingLayerCode = layerCode;
                    return false;
                }
            }

            if (!TryParseTileKey(tileKey, out var tileX, out var tileY))
            {
                skeleton = new SimulationWorldLandscapeSkeleton();
                missingLayerCode = "invalid-tile-key";
                return false;
            }

            var minEasting = tileX * 500d;
            var minNorthing = tileY * 500d;
            var centerEasting = minEasting + 250d;
            var centerNorthing = minNorthing + 250d;
            var suffix = tileX + "-" + tileY;
            var north = tileY >= 1145;
            var east = tileX >= 701;
            var nodes = new List<SimulationWorldLandscapeSkeletonNode>
            {
                Node("macro:" + suffix, string.Empty, SimulationWorldLandscapeCompositionCodes.Area,
                    "forest-region", "nature", north ? "침엽수림 군집" : "혼효림 군집",
                    SimulationWorldLandscapeAssemblyEvidenceCodes.StatisticallyAllocated,
                    centerEasting - 120d, centerNorthing + 110d, 160d, 140d),
                Node("farm:" + suffix, "macro:" + suffix, SimulationWorldLandscapeCompositionCodes.Area,
                    "agriculture-region", "farm", east ? "시설하우스 단동" : "감자밭 두렁",
                    SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario,
                    centerEasting + 80d, centerNorthing + 90d, 72d, 64d),
                Node("road:" + suffix, "macro:" + suffix, SimulationWorldLandscapeCompositionCodes.Linear,
                    "scenario-farm-road", "network", "농촌도로 직선",
                    SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario,
                    centerEasting, centerNorthing, 40d, 12d, 90d),
                Node("edge:" + suffix, "macro:" + suffix, SimulationWorldLandscapeCompositionCodes.Transition,
                    "nature-farm-edge", "transition", "Nature–Farm 전환",
                    SimulationWorldLandscapeAssemblyEvidenceCodes.StatisticallyAllocated,
                    centerEasting - 25d, centerNorthing + 95d, 30d, 30d),
                Node("detail:" + suffix, "macro:" + suffix, SimulationWorldLandscapeCompositionCodes.Detail,
                    "forest-edge-detail", "nature", "숲 가장자리",
                    SimulationWorldLandscapeAssemblyEvidenceCodes.Decorative,
                    centerEasting - 55d, centerNorthing + 55d, 20d, 18d),
            };
            if (east && !north)
                nodes.Add(Node("hub:" + suffix, "macro:" + suffix,
                    SimulationWorldLandscapeCompositionCodes.Landmark,
                    "scenario-logistics-hub", "city", "물류 Station 진입부",
                    SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario,
                    centerEasting + 130d, centerNorthing - 100d, 45d, 36d));

            var neighborTileKey = "kr5186:l2:" + (tileX + 1) + ":" + tileY;
            skeleton = new SimulationWorldLandscapeSkeleton
            {
                TileKey = tileKey,
                AreaSetStableId = PyeongchangAreaSetStableIds.AreaSet,
                Nodes = nodes,
                Edges = new[]
                {
                    Edge("contains-farm:" + suffix, "macro:" + suffix,
                        SimulationWorldLandscapeCompositionCodes.Contains, "farm:" + suffix,
                        string.Empty, SimulationWorldLandscapeAssemblyEvidenceCodes.StatisticallyAllocated),
                    Edge("connect-road-farm:" + suffix, "road:" + suffix,
                        SimulationWorldLandscapeCompositionCodes.Connects, "farm:" + suffix,
                        "farm-road", SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario),
                    Edge("transition-nature-farm:" + suffix, "edge:" + suffix,
                        SimulationWorldLandscapeCompositionCodes.TransitionsTo, "farm:" + suffix,
                        "field-continuation", SimulationWorldLandscapeAssemblyEvidenceCodes.StatisticallyAllocated),
                    Edge("external-road-east:" + suffix, "road:" + suffix,
                        SimulationWorldLandscapeCompositionCodes.Connects, "road:" + (tileX + 1) + "-" + tileY,
                        "farm-road", SimulationWorldLandscapeAssemblyEvidenceCodes.Scenario,
                        neighborTileKey, "east"),
                },
            };
            missingLayerCode = string.Empty;
            return true;
        }

        private static SimulationWorldLandscapeSkeletonNode Node(
            string id, string parent, string topology, string semantic, string family, string setName,
            string evidence, double easting, double northing, double width, double depth,
            double rotation = 0d) => new SimulationWorldLandscapeSkeletonNode
        {
            NodeStableId = id,
            ParentNodeStableId = parent,
            NodeKindCode = topology,
            SemanticCode = semantic,
            PreferredFamilyCode = family,
            PreferredSetName = setName,
            EvidenceKindCode = evidence,
            CenterEastingMeters = easting,
            CenterNorthingMeters = northing,
            PhysicalElevationMeters = 0d,
            WidthMeters = width,
            DepthMeters = depth,
            RotationDegrees = rotation,
        };

        private static SimulationWorldLandscapeSkeletonEdge Edge(
            string id, string from, string relation, string to, string connector, string evidence,
            string? neighborTileKey = null, string direction = "") =>
            new SimulationWorldLandscapeSkeletonEdge
            {
                EdgeStableId = id,
                FromNodeStableId = from,
                RelationCode = relation,
                ToNodeStableId = to,
                ConnectorTypeCode = connector,
                EvidenceKindCode = evidence,
                NeighborTileKey = neighborTileKey,
                DirectionCode = direction,
            };

        private static bool TryParseTileKey(string tileKey, out int x, out int y)
        {
            x = 0;
            y = 0;
            var parts = (tileKey ?? string.Empty).Split(':');
            return parts.Length == 4
                && string.Equals(parts[0], "kr5186", StringComparison.Ordinal)
                && string.Equals(parts[1], "l2", StringComparison.Ordinal)
                && int.TryParse(parts[2], out x)
                && int.TryParse(parts[3], out y);
        }
    }
}
