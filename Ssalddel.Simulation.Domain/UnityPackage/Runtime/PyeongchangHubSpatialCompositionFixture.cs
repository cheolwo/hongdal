using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class PyeongchangHubSpatialCompositionCodes
    {
        public const string AreaCode = "Hub";
        public const string AreaSetStableId =
            "area-set:sim:pyeongchang:logistics-hub.v1";
        public const string ReceivingStorageH1 =
            "h1-stock:hub-receiving-storage";
        public const string OutboundStagingH1 =
            "h1-stock:hub-outbound-staging";
        public const string InternalWarehouseH2 =
            "h2-candidate:hub-internal-warehouse";
        public const string OutboundVehicleH2 =
            "h2-candidate:hub-outbound-vehicle";
        public const string JinbuHubH3 = "h3-candidate:jinbu-hub";
    }

    public static class PyeongchangHubSpatialCompositionFixture
    {
        public const string PlacementPlanSchemaVersion =
            "interior-placement-plan.v2";
        public const string PlacementPlanHashSha256 =
            "5dc48307b670b660dc8a846890b8c1fecf6faf4f316622f56f8e04535cd7a4b2";

        public static SpatialCompositionRuleCatalog CreateRuleCatalog()
        {
            var catalog = new SpatialCompositionRuleCatalog
            {
                Rules = new[]
                {
                    new SpatialCompositionRule
                    {
                        RuleStableId =
                            "spatial-composition-rule:hub-internal-warehouse.v1",
                        TargetLevelCode = SimulationSpatialCompositionCodes.H2,
                        TargetDefinitionStableId =
                            PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2,
                        RequiredChildDefinitionStableIds = new[]
                        {
                            PyeongchangHubSpatialCompositionCodes.ReceivingStorageH1,
                            PyeongchangHubSpatialCompositionCodes.OutboundStagingH1,
                        },
                        RequiredCapabilityCodes = new[]
                        {
                            "Spatial.CargoAccessible",
                            "Spatial.InspectionWorkArea",
                            "Spatial.OutboundStagingArea",
                            "Spatial.PickingWorkArea",
                            "Spatial.Storage",
                            "Spatial.WorkerAccessible",
                        },
                        MinimumStorageCapacityKgm = 300m,
                        MinimumWorkAreaSlots = 1,
                        Relations = new[]
                        {
                            new SpatialCompositionRelationRule
                            {
                                RelationStableId =
                                    "relation:hub-internal-warehouse:receiving-to-outbound",
                                FromChildDefinitionStableId =
                                    PyeongchangHubSpatialCompositionCodes.ReceivingStorageH1,
                                FromConnectorRoleCode = "Output",
                                ToChildDefinitionStableId =
                                    PyeongchangHubSpatialCompositionCodes.OutboundStagingH1,
                                ToConnectorRoleCode = "Input",
                                MovementKindCode = "CargoLogistics",
                            },
                        },
                    },
                    new SpatialCompositionRule
                    {
                        RuleStableId =
                            "spatial-composition-rule:jinbu-hub-landscape.r3",
                        TargetLevelCode = SimulationSpatialCompositionCodes.H3,
                        TargetDefinitionStableId =
                            PyeongchangHubSpatialCompositionCodes.JinbuHubH3,
                        RequiredChildDefinitionStableIds = new[]
                        {
                            PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2,
                            PyeongchangHubSpatialCompositionCodes.OutboundVehicleH2,
                        },
                        RequiredPlayableLoopStableIds = new[]
                        {
                            "playable-loop:hub-inbound-putaway.v1",
                            "playable-loop:hub-outbound-ready-return.v1",
                        },
                    },
                    new SpatialCompositionRule
                    {
                        RuleStableId =
                            "spatial-composition-rule:pyeongchang-hub-area-readiness.v1",
                        TargetLevelCode = SimulationSpatialCompositionCodes.H4,
                        TargetDefinitionStableId =
                            PyeongchangHubSpatialCompositionCodes.AreaSetStableId,
                        AuthorityCode =
                            SimulationSpatialCompositionCodes.ReadinessOnly,
                        RequiredChildDefinitionStableIds = new[]
                        {
                            PyeongchangHubSpatialCompositionCodes.JinbuHubH3,
                        },
                        RequiresPlacementValidation = false,
                    },
                },
            };
            catalog.CatalogHashSha256 =
                SimulationSpatialCompositionEngine.ComputeRuleCatalogHash(catalog);
            return catalog;
        }

        public static SpatialCompositionChildEvidence[] CreateH1Evidence(
            bool receivingOperational = true,
            bool outboundOperational = true,
            bool placementValidated = true)
            => new[]
            {
                new SpatialCompositionChildEvidence
                {
                    SpatialInstanceStableId =
                        "spatial-instance:h1:hub-receiving-storage:canonical",
                    DefinitionStableId =
                        PyeongchangHubSpatialCompositionCodes.ReceivingStorageH1,
                    LevelCode = SimulationSpatialCompositionCodes.H1,
                    Operational = receivingOperational,
                    PlacementValidated = placementValidated,
                    PlacementPlanSchemaVersion = PlacementPlanSchemaVersion,
                    PlacementPlanHashSha256 = PlacementPlanHashSha256,
                    CapabilityCodes = new[]
                    {
                        "Spatial.CargoAccessible",
                        "Spatial.InspectionWorkArea",
                        "Spatial.LoadingWorkArea",
                        "Spatial.Storage",
                        "Spatial.UnloadingWorkArea",
                        "Spatial.WorkerAccessible",
                    },
                    ConnectorRoleCodes = new[] { "Input", "Output" },
                    StorageCapacityKgm = 300m,
                    SourceStableIds = new[] { "wi:WI-001", "wi:WI-002" },
                },
                new SpatialCompositionChildEvidence
                {
                    SpatialInstanceStableId =
                        "spatial-instance:h1:hub-outbound-staging:canonical",
                    DefinitionStableId =
                        PyeongchangHubSpatialCompositionCodes.OutboundStagingH1,
                    LevelCode = SimulationSpatialCompositionCodes.H1,
                    Operational = outboundOperational,
                    PlacementValidated = placementValidated,
                    PlacementPlanSchemaVersion = PlacementPlanSchemaVersion,
                    PlacementPlanHashSha256 = PlacementPlanHashSha256,
                    CapabilityCodes = new[]
                    {
                        "Spatial.CargoAccessible",
                        "Spatial.OutboundStagingArea",
                        "Spatial.PickingWorkArea",
                        "Spatial.Storage",
                        "Spatial.WorkerAccessible",
                    },
                    ConnectorRoleCodes = new[] { "Input", "Output" },
                    WorkAreaSlots = 1,
                    SourceStableIds = new[]
                    {
                        "wi:WI-HUB-03", "wi:WI-HUB-04", "wi:WI-HUB-05",
                    },
                },
            };

        public static SpatialCompositionEvaluationRequest CreateRequest(
            int worldTick,
            long worldRevision,
            bool commitQualified,
            SimulationSpatialCompositionStateSnapshot? previous = null,
            SpatialCompositionChildEvidence[]? evidence = null)
            => new SpatialCompositionEvaluationRequest
            {
                AreaCode = PyeongchangHubSpatialCompositionCodes.AreaCode,
                AreaSetStableId =
                    PyeongchangHubSpatialCompositionCodes.AreaSetStableId,
                WorldTick = worldTick,
                WorldRevision = worldRevision,
                CommitQualifiedFormations = commitQualified,
                RuleCatalog = CreateRuleCatalog(),
                ChildEvidence = evidence ?? CreateH1Evidence(),
                ClosedPlayableLoopStableIds = new[]
                {
                    "playable-loop:hub-inbound-putaway.v1",
                    "playable-loop:hub-outbound-ready-return.v1",
                },
                PreviousState = previous,
            };
    }
}
