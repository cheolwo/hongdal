using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation세계자산배치Codes
    {
        public const string MapSchemaVersion = "world-map-composition-plan.v1";
        public const string ChangeProjectionSchemaVersion =
            "spatial-change-projection.v1";
        public const string SpawnSchemaVersion = "environment-spawn-plan.v1";
        public const string AssetSchemaVersion =
            "world-asset-placement-plan.v1";
        public const string InteriorBodySchemaVersion =
            "interior-placement-plan-body.v1";
        public const string MapRuleRevision = "world-map-composition.r1";
        public const string ChangeProjectionRuleRevision =
            "spatial-change-projection.r1";
        public const string SpawnRuleRevision = "environment-spawn.r1";
        public const string AssetRuleRevision = "world-asset-placement.r1";
        public const string AssetRuleRevisionR2 = "world-asset-placement.r2";
        public const string ExteriorSchemaVersion =
            "exterior-asset-placement-plan.v2";
        public const string InteriorSchemaVersion =
            "interior-asset-placement-plan.v2";
        public const string CellAssemblySchemaVersion =
            "world-cell-assembly.v1";

        public const string PlayerDriven = "PlayerDriven";
        public const string WorldDerived = "WorldDerived";

        public const string SimulationEntity = "SimulationEntity";
        public const string DerivedWorldProp = "DerivedWorldProp";
        public const string AmbientPresentation = "AmbientPresentation";

        public const string Persistent = "Persistent";
        public const string DerivedPersistent = "DerivedPersistent";
        public const string Transient = "Transient";

        public const string MapAnchor = "MapAnchor";
        public const string Environment = "Environment";
        public const string Building = "Building";
        public const string ExteriorOverlay = "ExteriorOverlay";
        public const string InteriorBase = "InteriorBase";
        public const string InteriorOverlay = "InteriorOverlay";

        public const string NoiseTier = "NoiseTier";
        public const string SafetyTier = "SafetyTier";
        public const string ResourcePressureTier = "ResourcePressureTier";
        public const string RecoveryTier = "RecoveryTier";
        public const string ConstructionTier = "ConstructionTier";
        public const string StorageFillTier = "StorageFillTier";
        public const string SettlementActivityTier = "SettlementActivityTier";

        public const string Standing = "Standing";
        public const string Stump = "Stump";
        public const string BuildingState = "Building";
        public const string Operational = "Operational";

        public const string LhComposed = "LhComposed";
        public const string PlayerComposed = "PlayerComposed";
        public const string HybridEvolving = "HybridEvolving";
    }

    public sealed class Simulation지도구성Request
    {
        public string WorldSeed { get; set; } = string.Empty;
        public string GeneratorRevision { get; set; } =
            Simulation세계자산배치Codes.MapRuleRevision;
        public string CellStableId { get; set; } = string.Empty;
        public int CellX { get; set; }
        public int CellY { get; set; }
        public string WindowRoleCode { get; set; } = string.Empty;
        public string[] RequiredCapabilityCodes { get; set; }
            = Array.Empty<string>();
        public long WorldRevision { get; set; }
    }

    public sealed class Simulation지도H결속Snapshot
    {
        public string HLevelCode { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] WorldInteractionIds { get; set; }
            = Array.Empty<string>();
    }

    public sealed class Simulation지도연결구Snapshot
    {
        public string ConnectorStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string NeighborCellStableId { get; set; } = string.Empty;
        public string BoundaryHashSha256 { get; set; } = string.Empty;
        public bool Passable { get; set; }
    }

    public sealed class Simulation지도배치AnchorSnapshot
    {
        public string AnchorStableId { get; set; } = string.Empty;
        public string AnchorRoleCode { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string PreferredCompositionKey { get; set; } = string.Empty;
        public double LocalXMeters { get; set; }
        public double LocalZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public double MaximumSlopeDegrees { get; set; }
        public string[] AllowedAssetCategoryCodes { get; set; }
            = Array.Empty<string>();
        public bool FixedAnchor { get; set; }
    }

    public sealed class Simulation지도구성Plan
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.MapSchemaVersion;
        public string GeneratorRevision { get; set; } =
            Simulation세계자산배치Codes.MapRuleRevision;
        public string WorldSeed { get; set; } = string.Empty;
        public string CellStableId { get; set; } = string.Empty;
        public int CellX { get; set; }
        public int CellY { get; set; }
        public string WindowRoleCode { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SurfaceModeCode { get; set; } = "FlatCompatibility";
        public Simulation지도H결속Snapshot[] HBindings { get; set; }
            = Array.Empty<Simulation지도H결속Snapshot>();
        public Simulation지도연결구Snapshot[] Connectors { get; set; }
            = Array.Empty<Simulation지도연결구Snapshot>();
        public Simulation지도배치AnchorSnapshot[] Anchors { get; set; }
            = Array.Empty<Simulation지도배치AnchorSnapshot>();
        public string[] RequiredCapabilityCodes { get; set; }
            = Array.Empty<string>();
        public string MapPlanHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation공간변화FactSnapshot
    {
        public string ChangeStableId { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } =
            Simulation세계자산배치Codes.WorldDerived;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChangeCode { get; set; } = string.Empty;
        public int ChangeValue { get; set; }
        public int Quantity { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public double LocalXMeters { get; set; }
        public double LocalZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public string FormationModeCode { get; set; } =
            Simulation세계자산배치Codes.HybridEvolving;
        public long AppliedWorldRevision { get; set; }
    }

    public sealed class Simulation공간변화ProjectionSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.ChangeProjectionSchemaVersion;
        public string ProjectionRevision { get; set; } =
            Simulation세계자산배치Codes.ChangeProjectionRuleRevision;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public Simulation공간변화FactSnapshot[] Facts { get; set; }
            = Array.Empty<Simulation공간변화FactSnapshot>();
        public string ProjectionHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation환경발생ContextValueSnapshot
    {
        public string ContextCode { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public sealed class Simulation환경발생ContextSnapshot
    {
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CellStableId { get; set; } = string.Empty;
        public int SpawnEpoch { get; set; }
        public long SourceWorldRevision { get; set; }
        public string SourceChangeProjectionHashSha256 { get; set; }
            = string.Empty;
        public Simulation환경발생ContextValueSnapshot[] Values { get; set; }
            = Array.Empty<Simulation환경발생ContextValueSnapshot>();
    }

    public sealed class Simulation환경발생WeightModifier
    {
        public string ContextCode { get; set; } = string.Empty;
        public int MinimumContextValue { get; set; }
        public int WeightDeltaPerStep { get; set; }
    }

    public sealed class Simulation환경발생Candidate
    {
        public string CandidateStableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        public string AuthorityKindCode { get; set; } =
            Simulation세계자산배치Codes.AmbientPresentation;
        public string PersistenceKindCode { get; set; } =
            Simulation세계자산배치Codes.Transient;
        public int BaseWeight { get; set; }
        public int MaximumInstancesPerCell { get; set; } = 1;
        public double MinimumSpacingMeters { get; set; }
        public string[] AllowedHLevelCodes { get; set; }
            = Array.Empty<string>();
        public Simulation환경발생WeightModifier[] WeightModifiers { get; set; }
            = Array.Empty<Simulation환경발생WeightModifier>();
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class Simulation환경발생RuleCatalog
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.SpawnSchemaVersion;
        public string Revision { get; set; } =
            Simulation세계자산배치Codes.SpawnRuleRevision;
        public int MaximumWeight { get; set; } = 10000;
        public Simulation환경발생Candidate[] Candidates { get; set; }
            = Array.Empty<Simulation환경발생Candidate>();
        public string CatalogHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation환경발생Decision
    {
        public string DecisionStableId { get; set; } = string.Empty;
        public string CandidateStableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        public string AuthorityKindCode { get; set; } = string.Empty;
        public string PersistenceKindCode { get; set; } = string.Empty;
        public int SlotIndex { get; set; }
        public int EffectiveWeight { get; set; }
        public int DeterministicRoll { get; set; }
        public bool Selected { get; set; }
        public bool RequiresWorldTickCommit { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class Simulation환경발생DecisionPlan
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.SpawnSchemaVersion;
        public string RuleRevision { get; set; } =
            Simulation세계자산배치Codes.SpawnRuleRevision;
        public string RuleCatalogHashSha256 { get; set; } = string.Empty;
        public string WorldSeed { get; set; } = string.Empty;
        public string CellStableId { get; set; } = string.Empty;
        public int SpawnEpoch { get; set; }
        public long SourceWorldRevision { get; set; }
        public string SourceChangeProjectionHashSha256 { get; set; }
            = string.Empty;
        public Simulation환경발생Decision[] Decisions { get; set; }
            = Array.Empty<Simulation환경발생Decision>();
        public string DecisionPlanHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation세계자산배치Request
    {
        public Simulation지도구성Plan MapPlan { get; set; }
            = new Simulation지도구성Plan();
        public Simulation공간변화ProjectionSnapshot ChangeProjection
            { get; set; } = new Simulation공간변화ProjectionSnapshot();
        public Simulation환경발생RuleCatalog SpawnRuleCatalog { get; set; }
            = new Simulation환경발생RuleCatalog();
        public Simulation환경발생ContextSnapshot SpawnContext { get; set; }
            = new Simulation환경발생ContextSnapshot();
        public string AssetRuleRevision { get; set; } =
            Simulation세계자산배치Codes.AssetRuleRevision;
        public string[] CompatibilityCompositionKeys { get; set; }
            = Array.Empty<string>();
        public bool PreserveLegacyProceduralLayout { get; set; }
    }

    public sealed class Simulation세계자산PlacementSnapshot
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string ParentPlacementStableId { get; set; } = string.Empty;
        public string OwnerCellStableId { get; set; } = string.Empty;
        public string PlacementKindCode { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string AuthorityKindCode { get; set; } = string.Empty;
        public string PersistenceKindCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public double LocalXMeters { get; set; }
        public double LocalYMeters { get; set; }
        public double LocalZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public double UniformScale { get; set; } = 1d;
        public bool FixedAnchor { get; set; }
        public bool CollisionEligible { get; set; }
        public bool PresentationOnly { get; set; } = true;
        public string SourceSpawnDecisionStableId { get; set; } = string.Empty;
        public string[] SourceChangeStableIds { get; set; }
            = Array.Empty<string>();
    }

    public sealed class Simulation세계자산배치Plan
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.AssetSchemaVersion;
        public string RuleRevision { get; set; } =
            Simulation세계자산배치Codes.AssetRuleRevision;
        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string MapPlanHashSha256 { get; set; } = string.Empty;
        public string ChangeProjectionHashSha256 { get; set; }
            = string.Empty;
        public string SpawnDecisionPlanHashSha256 { get; set; }
            = string.Empty;
        public Simulation세계자산PlacementSnapshot[] Placements { get; set; }
            = Array.Empty<Simulation세계자산PlacementSnapshot>();
        public SimulationInteriorPlanHandleSnapshot[] InteriorPlanHandles
            { get; set; } = Array.Empty<SimulationInteriorPlanHandleSnapshot>();
        public SimulationInteriorPlacementPlanBodySnapshot[] InteriorPlanBodies
            { get; set; } = Array.Empty<SimulationInteriorPlacementPlanBodySnapshot>();
        public string AssetPlacementPlanHashSha256 { get; set; }
            = string.Empty;
    }

    public sealed class SimulationInteriorPlacementBodyItemSnapshot
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string ParentPlacementStableId { get; set; } = string.Empty;
        public string ZoneStableId { get; set; } = string.Empty;
        public string OwningH1StableId { get; set; } = string.Empty;
        public string PlacementLayerCode { get; set; } = string.Empty;
        public string PlacementRoleCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalY { get; set; }
        public double LocalZ { get; set; }
        public double LocalRotationDegrees { get; set; }
        public double UniformScale { get; set; } = 1d;
        public string ReferenceStableId { get; set; } = string.Empty;
        public string[] PresentationFlags { get; set; }
            = Array.Empty<string>();
    }

    public sealed class SimulationInteriorPlacementPlanBodySnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.InteriorBodySchemaVersion;
        public string BuildingPlacementStableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string SourceInteriorPlanSchemaVersion { get; set; }
            = string.Empty;
        public string InteriorPlacementPlanHashSha256 { get; set; }
            = string.Empty;
        public SimulationInteriorPlacementBodyItemSnapshot[] Placements
            { get; set; } = Array.Empty<SimulationInteriorPlacementBodyItemSnapshot>();
        public string BodyHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldAssetPlacementStateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.AssetSchemaVersion;
        public long SourceWorldRevision { get; set; }
        public Simulation지도구성Plan[] MapPlans { get; set; }
            = Array.Empty<Simulation지도구성Plan>();
        public Simulation공간변화ProjectionSnapshot[] ChangeProjections
            { get; set; } = Array.Empty<Simulation공간변화ProjectionSnapshot>();
        public Simulation환경발생DecisionPlan[] SpawnDecisionPlans
            { get; set; } = Array.Empty<Simulation환경발생DecisionPlan>();
        public Simulation세계자산배치Plan[] AssetPlacementPlans
            { get; set; } = Array.Empty<Simulation세계자산배치Plan>();
        public SimulationInteriorPlacementPlanBodySnapshot[] InteriorPlanBodies
            { get; set; } = Array.Empty<SimulationInteriorPlacementPlanBodySnapshot>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 기존 통합 배치 계획에서 건물 밖 배치만 분리한 파생 계획이다.
    /// 기존 저장 형식과 canonical hash를 바꾸지 않으며 Unity Prefab을 결정하지 않는다.
    /// </summary>
    public sealed class Simulation실외자산배치Plan
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.ExteriorSchemaVersion;
        public string RuleRevision { get; set; } =
            Simulation세계자산배치Codes.AssetRuleRevisionR2;
        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SourceLhBasePlanHashSha256 { get; set; } = string.Empty;
        public string SourceSurfaceStateHashSha256 { get; set; } = string.Empty;
        public string SourceCombinedPlanHashSha256 { get; set; } = string.Empty;
        public Simulation세계자산PlacementSnapshot[] Placements { get; set; }
            = Array.Empty<Simulation세계자산PlacementSnapshot>();
        public string ExteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 기존 통합 배치 계획에서 고정 실내 계획과 변화 Overlay만 분리한 파생 계획이다.
    /// 날씨와 지면 표현은 이 계획의 hash 입력이 아니다.
    /// </summary>
    public sealed class Simulation실내자산배치Plan
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.InteriorSchemaVersion;
        public string RuleRevision { get; set; } =
            Simulation세계자산배치Codes.AssetRuleRevisionR2;
        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SourceLhBasePlanHashSha256 { get; set; } = string.Empty;
        public string SourceCombinedPlanHashSha256 { get; set; } = string.Empty;
        public SimulationInteriorPlanHandleSnapshot[] InteriorPlanHandles
            { get; set; } = Array.Empty<SimulationInteriorPlanHandleSnapshot>();
        public SimulationInteriorPlacementPlanBodySnapshot[] InteriorPlanBodies
            { get; set; } = Array.Empty<SimulationInteriorPlacementPlanBodySnapshot>();
        public Simulation세계자산PlacementSnapshot[] OverlayPlacements { get; set; }
            = Array.Empty<Simulation세계자산PlacementSnapshot>();
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
    }

    /// <summary>
    /// LH 지면·셀 결과와 독립 실외·실내 계획을 같은 권위 revision으로 묶는
    /// 읽기 전용 인계 자료다. LH가 자산 배치 규칙을 소유한다는 뜻이 아니다.
    /// </summary>
    public sealed class SimulationWorldCellAssemblyResponse
    {
        public string SchemaVersion { get; set; } =
            Simulation세계자산배치Codes.CellAssemblySchemaVersion;
        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SourceLhBasePlanHashSha256 { get; set; } = string.Empty;
        public string SurfaceModeCode { get; set; } = string.Empty;
        public string SurfaceStateHashSha256 { get; set; } = string.Empty;
        public Simulation실외자산배치Plan ExteriorPlacement { get; set; }
            = new Simulation실외자산배치Plan();
        public Simulation실내자산배치Plan InteriorPlacement { get; set; }
            = new Simulation실내자산배치Plan();
        public string AssemblyHashSha256 { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string UnavailableReasonCode { get; set; } = string.Empty;
    }

    public sealed class Simulation실외환경표현Request
    {
        public Simulation실외자산배치Plan ExteriorPlacementPlan { get; set; }
            = new Simulation실외자산배치Plan();
        public SimulationAtmosphereStateSnapshot Atmosphere { get; set; }
            = new SimulationAtmosphereStateSnapshot();
        public string SurfaceModeCode { get; set; } = string.Empty;
        public string SurfaceStateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation실외PlacementPresentationSnapshot
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string VisualVariantCode { get; set; } = string.Empty;
        public string SurfaceAppearanceCode { get; set; } = string.Empty;
        public string WindResponseCode { get; set; } = string.Empty;
        public bool Visible { get; set; } = true;
    }

    /// <summary>
    /// Sky와 지면 상태가 실외 자산의 표현에만 미치는 결과다.
    /// 배치 Stable ID, 위치, Spawn과 Simulation 규칙은 변경하지 않는다.
    /// </summary>
    public sealed class Simulation실외환경표현Plan
    {
        public string CellStableId { get; set; } = string.Empty;
        public string SourceExteriorPlacementPlanHashSha256 { get; set; }
            = string.Empty;
        public string AtmosphereRuleRevision { get; set; } = string.Empty;
        public string WeatherCode { get; set; } = string.Empty;
        public string SurfaceModeCode { get; set; } = string.Empty;
        public string SurfaceStateHashSha256 { get; set; } = string.Empty;
        public Simulation실외PlacementPresentationSnapshot[] Placements { get; set; }
            = Array.Empty<Simulation실외PlacementPresentationSnapshot>();
        public string PresentationPlanHashSha256 { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "지면·셀·H·연결구를 자산 선택 없는 지도구성 계획으로 만든다.",
        StepKey = "contract.world-map-composition",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 16,
        Boundary = "건물·환경·실내 VisualKey와 Spawn 확률을 선택하지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "지도구성 입력·H 결속·연결구·Anchor의 공통 계약을 정의한다.",
        Boundary = "계약은 실제 셀 발현이나 Unity 배치를 증명하지 않는다.")]
    public interface ISimulation지도구성Engine
    {
        Simulation지도구성Plan Compose(Simulation지도구성Request request);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "결정적 환경 발생 후보와 선택 결과를 계산한다.",
        StepKey = "contract.environment-spawn-decision",
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 17,
        Boundary = "SimulationEntity 결과는 WorldTick Confirm 없이 권위 상태가 되지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "환경 발생 후보·가중치·결정 결과의 공통 계약을 정의한다.",
        Boundary = "발생 계약은 권위 개체 생성을 확정하지 않는다.")]
    public interface ISimulation환경발생DecisionEngine
    {
        Simulation환경발생DecisionPlan Decide(
            string worldSeed,
            Simulation환경발생RuleCatalog catalog,
            Simulation환경발생ContextSnapshot context);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "지도와 권위 변화에서 환경·건물·실내 자산 배치 계획을 만든다.",
        StepKey = "contract.world-asset-placement",
        DependsOnStepKeys = new[] {
            "contract.world-map-composition",
            "contract.environment-spawn-decision",
            "contract.interior-layout-plan"
        },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 19,
        Boundary = "LH 상세도와 Unity Prefab은 결정하지 않으며 권위 Spawn을 직접 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "지도·변화·환경 결정에서 자산 배치 계획을 만드는 계약을 정의한다.",
        Boundary = "계약은 실제 Scene 표현이나 플레이 입력 증거가 아니다.")]
    public interface ISimulation세계자산배치Engine
    {
        Simulation세계자산배치Plan Compose(
            Simulation세계자산배치Request request);
    }

    public interface ISimulation실외자산배치Engine
    {
        Simulation실외자산배치Plan ComposeExterior(
            Simulation세계자산배치Request request);
    }

    public interface ISimulation실내자산배치Engine
    {
        Simulation실내자산배치Plan ComposeInterior(
            Simulation세계자산배치Request request);
    }

    public interface ISimulation실외환경표현Engine
    {
        Simulation실외환경표현Plan ComposePresentation(
            Simulation실외환경표현Request request);
    }
}
