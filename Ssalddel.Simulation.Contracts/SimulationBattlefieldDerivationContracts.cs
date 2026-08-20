using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationBattlefieldDerivationCodes
    {
        public const string LegacyContextSchemaVersion = "simulation-battle-world-context.v1";
        public const string ContextSchemaVersion = "simulation-battle-world-context.v2";
        public const string ContextDerivationRuleVersion = "battle-context.static-runtime-projection.r2";
        public const string PlanSchemaVersion = "simulation-battlefield-plan.v1";
        public const string GeneratorRevision = "battlefield-generator.constraint-pcg32.r1";
        public const string AnchorPolicyRevision = "battlefield-anchor-policy.nature-farm.r1";
        public const string FarmPerimeter500 = "FarmPerimeter500";
        public const string NatureField500 = "NatureField500";
        public const string ScenarioAuthored = "ScenarioAuthored";
        public const string BattleLocalMeters = "BattleLocalMeters";

        public const string Required = "Required";
        public const string Preferred = "Preferred";
        public const string ContextOnly = "ContextOnly";

        public const string Physical = "Physical";
        public const string Area = "Area";
        public const string Route = "Route";
        public const string Boundary = "Boundary";
        public const string Gate = "Gate";
        public const string Objective = "Objective";
        public const string ContextBoundaryPortal = "ContextBoundaryPortal";

        public const string Connects = "Connects";
        public const string Adjacent = "Adjacent";
        public const string Faces = "Faces";
        public const string Contains = "Contains";
        public const string Separates = "Separates";
        public const string LeadsTo = "LeadsTo";
        public const string ApproachesFrom = "ApproachesFrom";

        public const string AlliedDeployment = "AlliedDeployment";
        public const string HostileDeployment = "HostileDeployment";
        public const string ReinforcementGate = "ReinforcementGate";
        public const string RetreatGate = "RetreatGate";

        public const string CombatSimulationRevision = "battle-combat.fixed100ms.r2";
        public const string Move = "Move";
        public const string Attack = "Attack";
        public const string Hold = "Hold";
        public const string Retreat = "Retreat";
        public const string SetFormation = "SetFormation";

        public const string ExclusiveParticipation = "ExclusiveParticipation";
        public const string ConflictAwareWorldTarget = "ConflictAwareWorldTarget";
        public const string CommittedToBattle = "CommittedToBattle";
        public const string Reserved = "Reserved";
        public const string Released = "Released";

        public const string FacilityCombatDamage = "FacilityCombatDamage";
        public const string GateCombatDamage = "GateCombatDamage";
        public const string RouteObstructed = "RouteObstructed";
        public const string ActorCombatCasualty = "ActorCombatCasualty";
        public const string SupplyConsumed = "SupplyConsumed";
        public const string RetreatThroughConnector = "RetreatThroughConnector";
        public const string ObjectiveLost = "ObjectiveLost";
        public const string ObjectiveSecured = "ObjectiveSecured";
        public const string Light = "Light";
        public const string Moderate = "Moderate";
        public const string Severe = "Severe";
        public const string Destroyed = "Destroyed";
        public const string MaxSeverity = "MaxSeverity";
        public const string SumCapped = "SumCapped";
        public const string Pending = "Pending";
        public const string Applied = "Applied";
    }

    public sealed class SimulationBattleSpatialPoseSnapshot
    {
        public string CoordinateSpaceCode { get; set; } = SimulationWorldLayoutCodes.ScenarioLocalMeters;
        public double XMeters { get; set; }
        public double ZMeters { get; set; }
        public double RotationDegrees { get; set; }
    }

    public sealed class SimulationBattleSpatialOriginSnapshot
    {
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public long CapturedWorldRevision { get; set; }
        public string AreaSetInstanceStableId { get; set; } = string.Empty;
        public string H3Ref { get; set; } = string.Empty;
        public string H2Ref { get; set; } = string.Empty;
        public string H1Ref { get; set; } = string.Empty;
        public string ApproachConnectorStableId { get; set; } = string.Empty;
        public SimulationBattleSpatialPoseSnapshot EncounterPose { get; set; } = new();
        public SimulationBattleSpatialPoseSnapshot AttackerPose { get; set; } = new();
        public SimulationBattleSpatialPoseSnapshot DefenderPose { get; set; } = new();
        public string GroundingEvidenceHashSha256 { get; set; } = string.Empty;
        public string QuantizedBattleOriginHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleWorldContextItemSnapshot
    {
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceKindCode { get; set; } = string.Empty;
        public string SemanticCode { get; set; } = string.Empty;
        public string ParentStableId { get; set; } = string.Empty;
        public string SourceHashSha256 { get; set; } = string.Empty;
        public SimulationBattleSpatialPoseSnapshot Pose { get; set; } = new();
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public string[] RelationStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationBattleContextBoundaryPortalSnapshot
    {
        public string PortalStableId { get; set; } = string.Empty;
        public string SourceRouteStableId { get; set; } = string.Empty;
        public int CrossingOrdinal { get; set; }
        public SimulationBattleSpatialPoseSnapshot Pose { get; set; } = new();
        public double SourceDirectionDegrees { get; set; }
        public string[] TravelTypeCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationBattlefieldAnchorSnapshot
    {
        public string BattlefieldAnchorStableId { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string WorldEffectTargetStableId { get; set; } = string.Empty;
        public string SemanticCode { get; set; } = string.Empty;
        public string[] AnchorTypeCodes { get; set; } = Array.Empty<string>();
        public string PreservationPolicyCode { get; set; } = string.Empty;
        public string AggregationPolicyCode { get; set; } = SimulationBattlefieldDerivationCodes.MaxSeverity;
        public SimulationBattleSpatialPoseSnapshot SourcePose { get; set; } = new();
        public double SourceWidthMeters { get; set; }
        public double SourceDepthMeters { get; set; }
        public string[] ApprovedSizeVariantCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationBattlefieldRouteConstraintSnapshot
    {
        public string RouteConstraintStableId { get; set; } = string.Empty;
        public string SourceRouteStableId { get; set; } = string.Empty;
        public string FromAnchorStableId { get; set; } = string.Empty;
        public string ToAnchorStableId { get; set; } = string.Empty;
        public string[] OrderedSemanticStableIds { get; set; } = Array.Empty<string>();
        public string[] TravelTypeCodes { get; set; } = Array.Empty<string>();
        public double MinimumWidthMeters { get; set; }
        public bool ContinuityRequired { get; set; }
        public string RouteSignature { get; set; } = string.Empty;
    }

    public sealed class SimulationBattlefieldRelationConstraintSnapshot
    {
        public string RelationConstraintStableId { get; set; } = string.Empty;
        public string FromAnchorStableId { get; set; } = string.Empty;
        public string ToAnchorStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public sealed class SimulationBattleWorldContextSnapshot
    {
        public string SchemaVersion { get; set; } = SimulationBattlefieldDerivationCodes.ContextSchemaVersion;
        public string ContextStableId { get; set; } = string.Empty;
        public int ContextRevision { get; set; } = 1;
        public long SourceWorldRevision { get; set; }
        public string ContextDerivationRuleVersion { get; set; }
            = SimulationBattlefieldDerivationCodes.ContextDerivationRuleVersion;
        public string StaticSpatialContextHashSha256 { get; set; } = string.Empty;
        public string EncounterScopeHashSha256 { get; set; } = string.Empty;
        public string AttackerContextHashSha256 { get; set; } = string.Empty;
        public string DefenderContextHashSha256 { get; set; } = string.Empty;
        public string BattleRelevantOverlayHashSha256 { get; set; } = string.Empty;
        public SimulationBattleRelevantRuntimeProjectionSnapshot BattleRelevantRuntime { get; set; }
            = new SimulationBattleRelevantRuntimeProjectionSnapshot();
        public double CenterXMeters { get; set; }
        public double CenterZMeters { get; set; }
        public double ContextWidthMeters { get; set; } = 1000d;
        public double ContextDepthMeters { get; set; } = 1000d;
        public SimulationBattleWorldContextItemSnapshot[] Items { get; set; } = Array.Empty<SimulationBattleWorldContextItemSnapshot>();
        public SimulationBattleContextBoundaryPortalSnapshot[] BoundaryPortals { get; set; } = Array.Empty<SimulationBattleContextBoundaryPortalSnapshot>();
        public SimulationBattlefieldAnchorSnapshot[] Anchors { get; set; } = Array.Empty<SimulationBattlefieldAnchorSnapshot>();
        public SimulationBattlefieldRouteConstraintSnapshot[] RouteConstraints { get; set; } = Array.Empty<SimulationBattlefieldRouteConstraintSnapshot>();
        public SimulationBattlefieldRelationConstraintSnapshot[] RelationConstraints { get; set; } = Array.Empty<SimulationBattlefieldRelationConstraintSnapshot>();
        public string AnchorPolicyRevision { get; set; } = SimulationBattlefieldDerivationCodes.AnchorPolicyRevision;
        public string ContextHashSha256 { get; set; } = string.Empty;
        public string AnchorSetHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationBattlefieldTerrainCellSnapshot
    {
        public int CellX { get; set; }
        public int CellZ { get; set; }
        public int HeightCentimeters { get; set; }
        public int MovementCostPermille { get; set; } = 1000;
        public string TerrainCode { get; set; } = string.Empty;
        public bool Walkable { get; set; } = true;
    }

    public sealed class SimulationBattlefieldAnchorPlacementSnapshot
    {
        public string BattlefieldAnchorStableId { get; set; } = string.Empty;
        public SimulationBattleSpatialPoseSnapshot BattlePose { get; set; } = new()
        {
            CoordinateSpaceCode = SimulationBattlefieldDerivationCodes.BattleLocalMeters,
        };
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public string SizeVariantCode { get; set; } = SimulationWorldLayoutCodes.Reference;
    }

    public sealed class SimulationBattlefieldZoneSnapshot
    {
        public string ZoneStableId { get; set; } = string.Empty;
        public string ZoneKindCode { get; set; } = string.Empty;
        public SimulationBattleSpatialPoseSnapshot CenterPose { get; set; } = new()
        {
            CoordinateSpaceCode = SimulationBattlefieldDerivationCodes.BattleLocalMeters,
        };
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public string SourceAnchorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattlefieldPlanSnapshot
    {
        public string SchemaVersion { get; set; } = SimulationBattlefieldDerivationCodes.PlanSchemaVersion;
        public string BattlefieldPlanStableId { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public string GeneratorRevision { get; set; } = SimulationBattlefieldDerivationCodes.GeneratorRevision;
        public string CoordinateSpaceCode { get; set; } = SimulationBattlefieldDerivationCodes.BattleLocalMeters;
        public double WidthMeters { get; set; } = 500d;
        public double DepthMeters { get; set; } = 500d;
        public double GridCellSizeMeters { get; set; } = 4d;
        public string BattlefieldDerivationInputHashSha256 { get; set; } = string.Empty;
        public string BattlefieldSeedHashSha256 { get; set; } = string.Empty;
        public ulong BattlefieldSeed { get; set; }
        public SimulationBattlefieldAnchorPlacementSnapshot[] AnchorPlacements { get; set; } = Array.Empty<SimulationBattlefieldAnchorPlacementSnapshot>();
        public SimulationBattlefieldRouteConstraintSnapshot[] Routes { get; set; } = Array.Empty<SimulationBattlefieldRouteConstraintSnapshot>();
        public SimulationBattlefieldZoneSnapshot[] Zones { get; set; } = Array.Empty<SimulationBattlefieldZoneSnapshot>();
        public SimulationBattlefieldTerrainCellSnapshot[] TerrainCells { get; set; } = Array.Empty<SimulationBattlefieldTerrainCellSnapshot>();
        public string[] ValidationCodes { get; set; } = Array.Empty<string>();
        public string BattlefieldPlanHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationBattlefieldDerivationSnapshot
    {
        public SimulationBattleSpatialOriginSnapshot SpatialOrigin { get; set; } = new();
        public SimulationBattleWorldContextSnapshot WorldContext { get; set; } = new();
        public SimulationBattlefieldPlanSnapshot BattlefieldPlan { get; set; } = new();
        public string TacticalTerrainInputHashSha256 { get; set; } = string.Empty;
        public string BattlefieldDerivationInputHashSha256 { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationBattleCardModifierSnapshot
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardDefinitionStableId { get; set; } = string.Empty;
        public long SourceCardRevision { get; set; }
        public string ApplicableControlModeCode { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ModifierCode { get; set; } = string.Empty;
        public int BasisPoints { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleUnitSnapshot
    {
        public string UnitStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public string ThreatTypeCode { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int CombatStrength { get; set; }
        public int HealthPermille { get; set; } = 1000;
        public int StaminaPermille { get; set; } = 1000;
        public int MoralePermille { get; set; } = 1000;
        public string[] RoleCodes { get; set; } = Array.Empty<string>();
        public string[] EquipmentCodes { get; set; } = Array.Empty<string>();
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public string FormationCode { get; set; } = string.Empty;
        public SimulationBattleSpatialPoseSnapshot InitialPose { get; set; } = new()
        {
            CoordinateSpaceCode = SimulationBattlefieldDerivationCodes.BattleLocalMeters,
        };
    }

    public sealed class SimulationBattleUnitRosterSnapshot
    {
        public string CombatSimulationRevision { get; set; } = SimulationBattlefieldDerivationCodes.CombatSimulationRevision;
        public SimulationBattleUnitSnapshot[] Units { get; set; } = Array.Empty<SimulationBattleUnitSnapshot>();
        public SimulationBattleCardModifierSnapshot[] CardModifiers { get; set; } = Array.Empty<SimulationBattleCardModifierSnapshot>();
        public string BattleUnitRosterHashSha256 { get; set; } = string.Empty;
        public string CardModifierHashSha256 { get; set; } = string.Empty;
        public string CombatSeedHashSha256 { get; set; } = string.Empty;
        public ulong CombatSeed { get; set; }
    }

    public sealed class SimulationBattleParticipationReservationSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string BattleStableId { get; set; } = string.Empty;
        public int ReservedWorldTick { get; set; }
        public int? EnteredBattleTick { get; set; }
        public int? ReleasedWorldTick { get; set; }
        public string StateCode { get; set; } = SimulationBattlefieldDerivationCodes.CommittedToBattle;
    }

    public sealed class SimulationBattleWorldTargetReservationSnapshot
    {
        public string WorldEffectTargetStableId { get; set; } = string.Empty;
        public string BattleStableId { get; set; } = string.Empty;
        public string ReservationKindCode { get; set; } = SimulationBattlefieldDerivationCodes.ConflictAwareWorldTarget;
        public string[] ConflictCapabilityCodes { get; set; } = Array.Empty<string>();
        public int ReservedWorldTick { get; set; }
        public int? ReleasedWorldTick { get; set; }
        public string StateCode { get; set; } = SimulationBattlefieldDerivationCodes.Reserved;
    }

    public sealed class SimulationBattleSemanticEffectSnapshot
    {
        public string SemanticEffectStableId { get; set; } = string.Empty;
        public string BattleStableId { get; set; } = string.Empty;
        public string[] BattlefieldAnchorStableIds { get; set; } = Array.Empty<string>();
        public string WorldEffectTargetStableId { get; set; } = string.Empty;
        public string SemanticEffectCode { get; set; } = string.Empty;
        public string SeverityCode { get; set; } = string.Empty;
        public int TacticalEvidencePermille { get; set; }
        public string AggregationPolicyCode { get; set; } = SimulationBattlefieldDerivationCodes.MaxSeverity;
        public string RuleRevision { get; set; } = string.Empty;
        public string ReconciliationStateCode { get; set; } = SimulationBattlefieldDerivationCodes.Pending;
        public string WorldEffectApplicationKey { get; set; } = string.Empty;
        public int? AppliedWorldTick { get; set; }
        public long? AppliedWorldRevision { get; set; }
    }

    public sealed class SimulationBattleTacticalCommandConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string UnitStableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string TargetUnitStableId { get; set; } = string.Empty;
        public int TargetXCentimeters { get; set; }
        public int TargetZCentimeters { get; set; }
        public string FormationCode { get; set; } = string.Empty;
    }
}
