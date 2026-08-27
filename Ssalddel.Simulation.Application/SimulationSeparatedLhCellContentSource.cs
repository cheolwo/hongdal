using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// LH의 기존 셀 실행 계약을 유지하면서 지도구성과 자산배치를 서로 다른
    /// 엔진에서 계산한다. 이 어댑터는 기존 r1 결과와 hash를 보존한다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "분리된 지도구성·세계자산배치 결과를 기존 LH 셀 계약으로 변환한다.",
        StepKey = "adapter.lh-separated-cell-content",
        DependsOnStepKeys = new[] {
            "application.world-map-composition",
            "application.world-asset-placement"
        },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 30,
        Boundary = "LH는 배치 규칙을 소유하지 않고 이미 계산된 계획의 준비·활성·해제만 담당한다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "분리 엔진을 기존 LH 셀 실행 계약에 연결하고 r1 hash 호환을 유지한다.",
        Boundary = "호환 Adapter는 실제 공간 활성화나 화면 증거를 만들지 않는다.")]
    public sealed class SimulationSeparatedLhCellContentSource
        : ISimulationLhCellContentSource
    {
        private static readonly string[] CompatibilityCompositionKeys =
        {
            "farm:감자밭 두렁:A",
            "farm:감자밭 두렁:B",
            "farm:혼합 작물밭:A",
            "nature:초지·야생화:A",
            "nature:숲 가장자리:A",
            "nature:침엽수림 군집:A",
        };

        private readonly ISimulation지도구성Engine mapEngine;
        private readonly ISimulation세계자산배치Engine assetEngine;

        public SimulationSeparatedLhCellContentSource()
            : this(new SimulationScenario지도구성Engine(),
                new Simulation결정적세계자산배치Engine())
        {
        }

        public SimulationSeparatedLhCellContentSource(
            ISimulation지도구성Engine worldMapEngine,
            ISimulation세계자산배치Engine worldAssetPlacementEngine)
        {
            mapEngine = worldMapEngine
                ?? throw new ArgumentNullException(nameof(worldMapEngine));
            assetEngine = worldAssetPlacementEngine
                ?? throw new ArgumentNullException(
                    nameof(worldAssetPlacementEngine));
        }

        public string ContentSourceCode =>
            SimulationLhWorldCodes.ScenarioProcedural;

        public SimulationLhCellPlanResponse CreateCellPlan(
            SimulationLhWindowCell windowCell,
            SimulationLhCellContentContext context)
        {
            if (windowCell == null)
                throw new ArgumentNullException(nameof(windowCell));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var capabilities = context.RequiredCapabilityCodes.Length == 0
                ? DefaultCapabilities(windowCell.WindowRoleCode)
                : context.RequiredCapabilityCodes
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var map = mapEngine.Compose(new Simulation지도구성Request
            {
                WorldSeed = SimulationLhWorldCodes.WorldSeed,
                GeneratorRevision = Simulation세계자산배치Codes.MapRuleRevision,
                CellStableId = windowCell.CellKey,
                CellX = windowCell.CellX,
                CellY = windowCell.CellY,
                WindowRoleCode = windowCell.WindowRoleCode,
                RequiredCapabilityCodes = capabilities,
                WorldRevision = context.WorldRevision,
            });
            var changes = new Simulation공간변화ProjectionSnapshot
            {
                AreaSetStableId = PyeongchangAreaSetStableIds.AreaSet,
                CellStableId = windowCell.CellKey,
                SourceWorldRevision = context.WorldRevision,
            };
            changes.ProjectionHashSha256 = Simulation세계자산CanonicalHash
                .ComputeChangeProjectionHash(changes);
            var spawnCatalog = new Simulation환경발생RuleCatalog();
            spawnCatalog.CatalogHashSha256 = Simulation세계자산CanonicalHash
                .ComputeSpawnCatalogHash(spawnCatalog);
            var spawnContext = new Simulation환경발생ContextSnapshot
            {
                AreaSetStableId = changes.AreaSetStableId,
                CellStableId = windowCell.CellKey,
                SourceWorldRevision = context.WorldRevision,
                SourceChangeProjectionHashSha256 =
                    changes.ProjectionHashSha256,
            };
            var assets = assetEngine.Compose(
                new Simulation세계자산배치Request
                {
                    MapPlan = map,
                    ChangeProjection = changes,
                    SpawnRuleCatalog = spawnCatalog,
                    SpawnContext = spawnContext,
                    CompatibilityCompositionKeys =
                        CompatibilityCompositionKeys,
                    PreserveLegacyProceduralLayout = true,
                });

            var hBindings = map.HBindings.Select(value =>
                new SimulationLhHBindingResponse
                {
                    HLevelCode = value.HLevelCode,
                    SpatialStableId = value.SpatialStableId,
                    StateCode = value.StateCode,
                    WorldInteractionIds = value.WorldInteractionIds,
                }).ToArray();
            var placements = assets.Placements.Select(value =>
                new SimulationLhPlacementResponse
                {
                    GeneratedStableId = value.PlacementStableId,
                    OwnerCellKey = value.OwnerCellStableId,
                    LayerCode = value.LayerCode,
                    CompositionKey = value.CompositionKey,
                    H1StableId = value.H1StableId,
                    EvidenceKindCode = ContentSourceCode,
                    LocalXMeters = value.LocalXMeters,
                    LocalZMeters = value.LocalZMeters,
                    RotationDegrees = value.RotationDegrees,
                    UniformScale = value.UniformScale,
                    FixedAnchor = value.FixedAnchor,
                    CollisionEligible = value.CollisionEligible,
                    PresentationOnly = value.PresentationOnly,
                }).OrderBy(value => value.GeneratedStableId,
                    StringComparer.Ordinal).ToArray();
            var connectors = map.Connectors.Select(value =>
                new SimulationLhConnectorResponse
                {
                    ConnectorStableId = value.ConnectorStableId,
                    SideCode = value.SideCode,
                    NeighborCellKey = value.NeighborCellStableId,
                    BoundaryHashSha256 = value.BoundaryHashSha256,
                    Passable = value.Passable,
                }).ToArray();
            var baseHash = Simulation세계자산CanonicalHash.Hash(
                string.Join("|", new[]
                {
                    SimulationLhWorldCodes.WorldSeed,
                    SimulationLhWorldCodes.GeneratorVersion,
                    windowCell.CellKey,
                    string.Join(",", hBindings.Select(value =>
                        value.HLevelCode + ":" + value.SpatialStableId + ":"
                        + value.StateCode)),
                    string.Join(",", placements.Select(PlacementCanonical)),
                    string.Join(",", connectors.Select(value =>
                        value.ConnectorStableId + ":"
                        + value.BoundaryHashSha256)),
                }));
            return new SimulationLhCellPlanResponse
            {
                CellKey = windowCell.CellKey,
                CellX = windowCell.CellX,
                CellY = windowCell.CellY,
                L2ParentCellKey = $"kr5186:l2:{SimulationLhWorldGrid.FloorDiv(windowCell.CellX, 4)}:{SimulationLhWorldGrid.FloorDiv(windowCell.CellY, 4)}",
                WindowRoleCode = windowCell.WindowRoleCode,
                Priority = windowCell.Priority,
                ContentSourceCode = ContentSourceCode,
                BasePlanHashSha256 = baseHash,
                PresentationHashSha256 = Simulation세계자산CanonicalHash.Hash(
                    baseHash + "|" + context.Season.SeasonCode + "|"
                    + context.Season.SeasonRuleVersion),
                HBindings = hBindings,
                Placements = placements,
                Connectors = connectors,
                RequiredCapabilityCodes = capabilities,
                PlayerTraversalRequired = windowCell.WindowRoleCode !=
                    SimulationLhWorldCodes.Prefetch,
                PresentationOnly = true,
            };
        }

        private static string[] DefaultCapabilities(string role)
        {
            if (role == SimulationLhWorldCodes.Prefetch)
                return new[] { SimulationLhWorldCodes.TerrainVisual };
            if (role == SimulationLhWorldCodes.Active)
                return new[]
                {
                    SimulationLhWorldCodes.TerrainVisual,
                    SimulationLhWorldCodes.Collision,
                    SimulationLhWorldCodes.Connector,
                };
            return new[]
            {
                SimulationLhWorldCodes.TerrainVisual,
                SimulationLhWorldCodes.Collision,
                SimulationLhWorldCodes.Connector,
                SimulationLhWorldCodes.H1Interaction,
                SimulationLhWorldCodes.NpcNavigation,
                SimulationLhWorldCodes.SeasonPresentation,
            };
        }

        private static string PlacementCanonical(
            SimulationLhPlacementResponse value)
            => string.Join(":", new[]
            {
                value.GeneratedStableId,
                value.CompositionKey,
                value.H1StableId,
                value.LocalXMeters.ToString("R", CultureInfo.InvariantCulture),
                value.LocalZMeters.ToString("R", CultureInfo.InvariantCulture),
                value.RotationDegrees.ToString("R", CultureInfo.InvariantCulture),
                value.UniformScale.ToString("R", CultureInfo.InvariantCulture),
                value.FixedAnchor.ToString(),
            });
    }
}
