using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationLhWorldCodes
    {
        public const string SchemaVersion = "lh-world.v1";
        public const string GeneratorVersion = "lh-generator.pyeongchang.v1";
        public const string WorldSeed = "pyeongchang-daegwallyeong-farm-2026";
        public const string ScenarioProcedural = "ScenarioProcedural";
        public const string AuthoritativeWorld = "AuthoritativeWorld";
        public const string ApprovedReference = "ApprovedReference";
        public const string IdeaInventory = "IdeaInventory";
        public const string ExploratoryInventory = "ExploratoryInventory";

        public const string TerrainVisual = "TerrainVisual";
        public const string Collision = "Collision";
        public const string Connector = "Connector";
        public const string H1Interaction = "H1Interaction";
        public const string NpcNavigation = "NpcNavigation";
        public const string SeasonPresentation = "SeasonPresentation";

        public const string Detail = "Detail";
        public const string Active = "Active";
        public const string Prefetch = "Prefetch";

        public const string North = "N";
        public const string NorthEast = "NE";
        public const string East = "E";
        public const string SouthEast = "SE";
        public const string South = "S";
        public const string SouthWest = "SW";
        public const string West = "W";
        public const string NorthWest = "NW";
        public const string None = "None";

        public const string Spring = "Spring";
        public const string Summer = "Summer";
        public const string Autumn = "Autumn";
        public const string Winter = "Winter";

        public const string DeltaDiscovered = "Discovered";
        public const string DeltaStateChanged = "StateChanged";
        public const string DeltaPlaced = "Placed";
        public const string DeltaRemoved = "Removed";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Contract,
        "L 해상도와 H 공간 계보를 결합하는 LH World 생성·스트리밍 Profile을 전달한다.",
        StepKey = "contract.lh-world-profile",
        DependsOnStepKeys = new[] { "contract.stream-recipe" },
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 11,
        Boundary = "Profile과 Cell Preview는 Simulation 공간 후보이며 H 권위나 운영 상태를 새로 확정하지 않는다.")]
    public sealed class SimulationLhWorldProfileResponse
    {
        public string SchemaVersion { get; set; } = SimulationLhWorldCodes.SchemaVersion;
        public string ProfileRevision { get; set; } = string.Empty;
        public string ProfileHashSha256 { get; set; } = string.Empty;
        public string WorldSeed { get; set; } = string.Empty;
        public string GeneratorVersion { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AreaSetRevision { get; set; } = string.Empty;
        public string AreaSetBoundaryHashSha256 { get; set; } = string.Empty;
        public string SpatialKnowledgeRevision { get; set; } = string.Empty;
        public string SpatialCompositionPlanStableId { get; set; } = string.Empty;
        public SimulationLhLevelResponse[] Levels { get; set; }
            = Array.Empty<SimulationLhLevelResponse>();
        public int DetailRadius { get; set; }
        public int ActiveRadius { get; set; }
        public int PrefetchRadius { get; set; }
        public int MaxConcurrentPreparations { get; set; }
        public double BoundaryPrefetchFraction { get; set; }
        public double MainThreadAssemblyBudgetMilliseconds { get; set; }
        public int CachedCellCapacity { get; set; }
        public int OriginShiftThresholdWorldUnits { get; set; }
        public SimulationLhGenerationLayerResponse[] GenerationLayers { get; set; }
            = Array.Empty<SimulationLhGenerationLayerResponse>();
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationLhLevelResponse
    {
        public string LevelCode { get; set; } = string.Empty;
        public int CellSizeMeters { get; set; }
        // 기존 소비자 호환용 이름이다. L과 H가 같은 계층이라는 뜻이 아니다.
        public string DefaultHLevelCode { get; set; } = string.Empty;
        public string PrimaryHQueryLevelCode { get; set; } = string.Empty;
    }

    public sealed class SimulationLhGenerationLayerResponse
    {
        public string LayerCode { get; set; } = string.Empty;
        public string[] DependsOnLayerCodes { get; set; } = Array.Empty<string>();
        public int MaximumPaddingMeters { get; set; }
        public string OwnershipRuleCode { get; set; } = string.Empty;
    }

    public sealed class SimulationLhCellPreviewRequest
    {
        public string RequestEpoch { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string FocusL3CellKey { get; set; } = string.Empty;
        public string MovementDirectionCode { get; set; } = SimulationLhWorldCodes.None;
        public string[] RequiredCapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] KnownCellPlanHashesSha256 { get; set; } = Array.Empty<string>();
        public long ExpectedWorldRevision { get; set; }
    }

    public sealed class SimulationLhCellPreviewResponse
    {
        public string RequestEpoch { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string ContentSourceCode { get; set; }
            = SimulationLhWorldCodes.ScenarioProcedural;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public SimulationLhSeasonSnapshot Season { get; set; } = new();
        public SimulationLhWorldProfileResponse Profile { get; set; } = new();
        public SimulationLhCellPlanResponse[] Cells { get; set; }
            = Array.Empty<SimulationLhCellPlanResponse>();
        public string[] OutsideCoverageCellKeys { get; set; } = Array.Empty<string>();
        public bool IsCandidateOnly { get; set; } = true;
        public bool DoesNotApplyResourceLedgers { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationLhSeasonSnapshot
    {
        public string SeasonCode { get; set; } = string.Empty;
        public int SeasonIndex { get; set; }
        public int SeasonDay { get; set; }
        public double SeasonProgress01 { get; set; }
        public string NextSeasonCode { get; set; } = string.Empty;
        public string SeasonRuleVersion { get; set; } = string.Empty;
        public int DayNumber { get; set; }
    }

    public sealed class SimulationLhCellPlanResponse
    {
        public string CellKey { get; set; } = string.Empty;
        public int CellX { get; set; }
        public int CellY { get; set; }
        public string L2ParentCellKey { get; set; } = string.Empty;
        public string WindowRoleCode { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ContentSourceCode { get; set; }
            = SimulationLhWorldCodes.ScenarioProcedural;
        public string BasePlanHashSha256 { get; set; } = string.Empty;
        public string PresentationHashSha256 { get; set; } = string.Empty;
        public SimulationLhHBindingResponse[] HBindings { get; set; }
            = Array.Empty<SimulationLhHBindingResponse>();
        public SimulationLhPlacementResponse[] Placements { get; set; }
            = Array.Empty<SimulationLhPlacementResponse>();
        public SimulationLhConnectorResponse[] Connectors { get; set; }
            = Array.Empty<SimulationLhConnectorResponse>();
        public string[] RequiredCapabilityCodes { get; set; } = Array.Empty<string>();
        public bool PlayerTraversalRequired { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationLhHBindingResponse
    {
        public string HLevelCode { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationLhPlacementResponse
    {
        public string GeneratedStableId { get; set; } = string.Empty;
        public string OwnerCellKey { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = SimulationLhWorldCodes.ScenarioProcedural;
        public double LocalXMeters { get; set; }
        public double LocalZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public double UniformScale { get; set; } = 1d;
        public bool FixedAnchor { get; set; }
        public bool CollisionEligible { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationLhConnectorResponse
    {
        public string ConnectorStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string NeighborCellKey { get; set; } = string.Empty;
        public string BoundaryHashSha256 { get; set; } = string.Empty;
        public bool Passable { get; set; }
    }

    public sealed class SimulationLhWorldStateSnapshot
    {
        public string WorldSeed { get; set; } = string.Empty;
        public string GeneratorVersion { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AreaSetRevision { get; set; } = string.Empty;
        public string AreaSetBoundaryHashSha256 { get; set; } = string.Empty;
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public string PlacementAuthorityCode { get; set; } = string.Empty;
        public string WorldGroundingStateCode { get; set; } = string.Empty;
        public string GroundingEvidenceHashSha256 { get; set; } = string.Empty;
        public string LastL3CellKey { get; set; } = string.Empty;
        public SimulationLhWorldDeltaSnapshot[] Deltas { get; set; }
            = Array.Empty<SimulationLhWorldDeltaSnapshot>();
    }

    public sealed class SimulationLhWorldDeltaSnapshot
    {
        public string GeneratedStableId { get; set; } = string.Empty;
        public string DeltaKindCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long AppliedWorldRevision { get; set; }
        public bool Tombstone { get; set; }
    }
}
