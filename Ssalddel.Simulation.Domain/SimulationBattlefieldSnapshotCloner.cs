using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static class SimulationBattlefieldSnapshotCloner
    {
        public static SimulationBattlefieldDerivationSnapshot Derivation(
            SimulationBattlefieldDerivationSnapshot? value)
            => value == null ? new SimulationBattlefieldDerivationSnapshot() : new()
            {
                SpatialOrigin = Origin(value.SpatialOrigin),
                WorldContext = Context(value.WorldContext),
                BattlefieldPlan = Plan(value.BattlefieldPlan),
                TacticalTerrainInputHashSha256 = value.TacticalTerrainInputHashSha256,
                BattlefieldDerivationInputHashSha256 =
                    value.BattlefieldDerivationInputHashSha256,
                CanConfirm = value.CanConfirm,
                BlockingReasonCodes = value.BlockingReasonCodes?.ToArray()
                    ?? Array.Empty<string>(),
            };

        public static SimulationBattleUnitRosterSnapshot Roster(
            SimulationBattleUnitRosterSnapshot? value)
            => value == null ? new SimulationBattleUnitRosterSnapshot() : new()
            {
                CombatSimulationRevision = value.CombatSimulationRevision,
                Units = value.Units?.Select(Unit).ToArray()
                    ?? Array.Empty<SimulationBattleUnitSnapshot>(),
                CardModifiers = value.CardModifiers?.Select(CardModifier).ToArray()
                    ?? Array.Empty<SimulationBattleCardModifierSnapshot>(),
                BattleUnitRosterHashSha256 = value.BattleUnitRosterHashSha256,
                CardModifierHashSha256 = value.CardModifierHashSha256,
                CombatSeedHashSha256 = value.CombatSeedHashSha256,
                CombatSeed = value.CombatSeed,
            };

        public static SimulationBattleParticipationReservationSnapshot Participation(
            SimulationBattleParticipationReservationSnapshot value) => new()
        {
            ActorStableId = value.ActorStableId,
            BattleStableId = value.BattleStableId,
            ReservedWorldTick = value.ReservedWorldTick,
            EnteredBattleTick = value.EnteredBattleTick,
            ReleasedWorldTick = value.ReleasedWorldTick,
            StateCode = value.StateCode,
        };

        public static SimulationBattleWorldTargetReservationSnapshot WorldTarget(
            SimulationBattleWorldTargetReservationSnapshot value) => new()
        {
            WorldEffectTargetStableId = value.WorldEffectTargetStableId,
            BattleStableId = value.BattleStableId,
            ReservationKindCode = value.ReservationKindCode,
            ConflictCapabilityCodes = value.ConflictCapabilityCodes?.ToArray()
                ?? Array.Empty<string>(),
            ReservedWorldTick = value.ReservedWorldTick,
            ReleasedWorldTick = value.ReleasedWorldTick,
            StateCode = value.StateCode,
        };

        public static SimulationBattleSemanticEffectSnapshot Effect(
            SimulationBattleSemanticEffectSnapshot value) => new()
        {
            SemanticEffectStableId = value.SemanticEffectStableId,
            BattleStableId = value.BattleStableId,
            BattlefieldAnchorStableIds = value.BattlefieldAnchorStableIds?.ToArray()
                ?? Array.Empty<string>(),
            WorldEffectTargetStableId = value.WorldEffectTargetStableId,
            SemanticEffectCode = value.SemanticEffectCode,
            SeverityCode = value.SeverityCode,
            TacticalEvidencePermille = value.TacticalEvidencePermille,
            AggregationPolicyCode = value.AggregationPolicyCode,
            RuleRevision = value.RuleRevision,
            ReconciliationStateCode = value.ReconciliationStateCode,
            WorldEffectApplicationKey = value.WorldEffectApplicationKey,
            AppliedWorldTick = value.AppliedWorldTick,
            AppliedWorldRevision = value.AppliedWorldRevision,
        };

        private static SimulationBattleSpatialOriginSnapshot Origin(
            SimulationBattleSpatialOriginSnapshot value) => new()
        {
            WorldLayoutStableId = value.WorldLayoutStableId,
            WorldLayoutRevision = value.WorldLayoutRevision,
            WorldLayoutHashSha256 = value.WorldLayoutHashSha256,
            CapturedWorldRevision = value.CapturedWorldRevision,
            AreaSetInstanceStableId = value.AreaSetInstanceStableId,
            H3Ref = value.H3Ref,
            H2Ref = value.H2Ref,
            H1Ref = value.H1Ref,
            ApproachConnectorStableId = value.ApproachConnectorStableId,
            EncounterPose = Pose(value.EncounterPose),
            AttackerPose = Pose(value.AttackerPose),
            DefenderPose = Pose(value.DefenderPose),
            GroundingEvidenceHashSha256 = value.GroundingEvidenceHashSha256,
            QuantizedBattleOriginHashSha256 = value.QuantizedBattleOriginHashSha256,
        };

        private static SimulationBattleWorldContextSnapshot Context(
            SimulationBattleWorldContextSnapshot value) => new()
        {
            SchemaVersion = value.SchemaVersion,
            ContextStableId = value.ContextStableId,
            ContextRevision = value.ContextRevision,
            SourceWorldRevision = value.SourceWorldRevision,
            ContextDerivationRuleVersion = value.ContextDerivationRuleVersion,
            StaticSpatialContextHashSha256 = value.StaticSpatialContextHashSha256,
            EncounterScopeHashSha256 = value.EncounterScopeHashSha256,
            AttackerContextHashSha256 = value.AttackerContextHashSha256,
            DefenderContextHashSha256 = value.DefenderContextHashSha256,
            BattleRelevantOverlayHashSha256 = value.BattleRelevantOverlayHashSha256,
            BattleRelevantRuntime = new SimulationBattleRelevantRuntimeProjectionSnapshot
            {
                EncounterScopeStableId = value.BattleRelevantRuntime.EncounterScopeStableId,
                Facilities = value.BattleRelevantRuntime.Facilities.Select(facility =>
                    new SimulationRuntimeFacilitySnapshot
                    {
                        FacilityStableId = facility.FacilityStableId,
                        FacilityDefinitionStableId = facility.FacilityDefinitionStableId,
                        FacilityDefinitionRevision = facility.FacilityDefinitionRevision,
                        FacilityDefinitionHashSha256 = facility.FacilityDefinitionHashSha256,
                        PlacementH1StableId = facility.PlacementH1StableId,
                        AccessConnectorStableIds = facility.AccessConnectorStableIds.ToArray(),
                        LifecycleCode = facility.LifecycleCode,
                        IntegrityCode = facility.IntegrityCode,
                        MaintenanceCode = facility.MaintenanceCode,
                        DefinedCapabilityCodes = facility.DefinedCapabilityCodes.ToArray(),
                        EffectiveCapabilities = facility.EffectiveCapabilities.Select(capability =>
                            new SimulationEffectiveFacilityCapabilitySnapshot
                            {
                                CapabilityCode = capability.CapabilityCode,
                                StateCode = capability.StateCode,
                                SourceRestrictionStableIds = capability.SourceRestrictionStableIds.ToArray(),
                            }).ToArray(),
                    }).ToArray(),
                Formations = value.BattleRelevantRuntime.Formations.Select(formation =>
                    new SimulationFormationSnapshot
                    {
                        FormationStableId = formation.FormationStableId,
                        StateCode = formation.StateCode,
                        MemberActorStableIds = formation.MemberActorStableIds.ToArray(),
                        GarrisonFacilityStableId = formation.GarrisonFacilityStableId,
                        StateCompletesAtTick = formation.StateCompletesAtTick,
                    }).ToArray(),
                BattleAvailableActorStableIds = value.BattleRelevantRuntime
                    .BattleAvailableActorStableIds.ToArray(),
                BattleRelevantOverlayHashSha256 = value.BattleRelevantRuntime
                    .BattleRelevantOverlayHashSha256,
            },
            CenterXMeters = value.CenterXMeters,
            CenterZMeters = value.CenterZMeters,
            ContextWidthMeters = value.ContextWidthMeters,
            ContextDepthMeters = value.ContextDepthMeters,
            Items = value.Items?.Select(Item).ToArray()
                ?? Array.Empty<SimulationBattleWorldContextItemSnapshot>(),
            BoundaryPortals = value.BoundaryPortals?.Select(Portal).ToArray()
                ?? Array.Empty<SimulationBattleContextBoundaryPortalSnapshot>(),
            Anchors = value.Anchors?.Select(Anchor).ToArray()
                ?? Array.Empty<SimulationBattlefieldAnchorSnapshot>(),
            RouteConstraints = value.RouteConstraints?.Select(Route).ToArray()
                ?? Array.Empty<SimulationBattlefieldRouteConstraintSnapshot>(),
            RelationConstraints = value.RelationConstraints?.Select(Relation).ToArray()
                ?? Array.Empty<SimulationBattlefieldRelationConstraintSnapshot>(),
            AnchorPolicyRevision = value.AnchorPolicyRevision,
            ContextHashSha256 = value.ContextHashSha256,
            AnchorSetHashSha256 = value.AnchorSetHashSha256,
            SimulationOnly = value.SimulationOnly,
            IsOperationalState = value.IsOperationalState,
        };

        private static SimulationBattlefieldPlanSnapshot Plan(
            SimulationBattlefieldPlanSnapshot value) => new()
        {
            SchemaVersion = value.SchemaVersion,
            BattlefieldPlanStableId = value.BattlefieldPlanStableId,
            ProfileCode = value.ProfileCode,
            ProfileRevision = value.ProfileRevision,
            GeneratorRevision = value.GeneratorRevision,
            CoordinateSpaceCode = value.CoordinateSpaceCode,
            WidthMeters = value.WidthMeters,
            DepthMeters = value.DepthMeters,
            GridCellSizeMeters = value.GridCellSizeMeters,
            BattlefieldDerivationInputHashSha256 =
                value.BattlefieldDerivationInputHashSha256,
            BattlefieldSeedHashSha256 = value.BattlefieldSeedHashSha256,
            BattlefieldSeed = value.BattlefieldSeed,
            AnchorPlacements = value.AnchorPlacements?.Select(Placement).ToArray()
                ?? Array.Empty<SimulationBattlefieldAnchorPlacementSnapshot>(),
            Routes = value.Routes?.Select(Route).ToArray()
                ?? Array.Empty<SimulationBattlefieldRouteConstraintSnapshot>(),
            Zones = value.Zones?.Select(Zone).ToArray()
                ?? Array.Empty<SimulationBattlefieldZoneSnapshot>(),
            TerrainCells = value.TerrainCells?.Select(TerrainCell).ToArray()
                ?? Array.Empty<SimulationBattlefieldTerrainCellSnapshot>(),
            ValidationCodes = value.ValidationCodes?.ToArray() ?? Array.Empty<string>(),
            BattlefieldPlanHashSha256 = value.BattlefieldPlanHashSha256,
            SimulationOnly = value.SimulationOnly,
            IsOperationalState = value.IsOperationalState,
        };

        private static SimulationBattleWorldContextItemSnapshot Item(
            SimulationBattleWorldContextItemSnapshot value) => new()
        {
            SourceStableId = value.SourceStableId,
            SourceKindCode = value.SourceKindCode,
            SemanticCode = value.SemanticCode,
            ParentStableId = value.ParentStableId,
            SourceHashSha256 = value.SourceHashSha256,
            Pose = Pose(value.Pose),
            WidthMeters = value.WidthMeters,
            DepthMeters = value.DepthMeters,
            RelationStableIds = value.RelationStableIds?.ToArray() ?? Array.Empty<string>(),
        };

        private static SimulationBattleContextBoundaryPortalSnapshot Portal(
            SimulationBattleContextBoundaryPortalSnapshot value) => new()
        {
            PortalStableId = value.PortalStableId,
            SourceRouteStableId = value.SourceRouteStableId,
            CrossingOrdinal = value.CrossingOrdinal,
            Pose = Pose(value.Pose),
            SourceDirectionDegrees = value.SourceDirectionDegrees,
            TravelTypeCodes = value.TravelTypeCodes?.ToArray() ?? Array.Empty<string>(),
        };

        private static SimulationBattlefieldAnchorSnapshot Anchor(
            SimulationBattlefieldAnchorSnapshot value) => new()
        {
            BattlefieldAnchorStableId = value.BattlefieldAnchorStableId,
            SourceStableId = value.SourceStableId,
            WorldEffectTargetStableId = value.WorldEffectTargetStableId,
            SemanticCode = value.SemanticCode,
            AnchorTypeCodes = value.AnchorTypeCodes?.ToArray() ?? Array.Empty<string>(),
            PreservationPolicyCode = value.PreservationPolicyCode,
            AggregationPolicyCode = value.AggregationPolicyCode,
            SourcePose = Pose(value.SourcePose),
            SourceWidthMeters = value.SourceWidthMeters,
            SourceDepthMeters = value.SourceDepthMeters,
            ApprovedSizeVariantCodes = value.ApprovedSizeVariantCodes?.ToArray()
                ?? Array.Empty<string>(),
        };

        private static SimulationBattlefieldRouteConstraintSnapshot Route(
            SimulationBattlefieldRouteConstraintSnapshot value) => new()
        {
            RouteConstraintStableId = value.RouteConstraintStableId,
            SourceRouteStableId = value.SourceRouteStableId,
            FromAnchorStableId = value.FromAnchorStableId,
            ToAnchorStableId = value.ToAnchorStableId,
            OrderedSemanticStableIds = value.OrderedSemanticStableIds?.ToArray()
                ?? Array.Empty<string>(),
            TravelTypeCodes = value.TravelTypeCodes?.ToArray() ?? Array.Empty<string>(),
            MinimumWidthMeters = value.MinimumWidthMeters,
            ContinuityRequired = value.ContinuityRequired,
            RouteSignature = value.RouteSignature,
        };

        private static SimulationBattlefieldRelationConstraintSnapshot Relation(
            SimulationBattlefieldRelationConstraintSnapshot value) => new()
        {
            RelationConstraintStableId = value.RelationConstraintStableId,
            FromAnchorStableId = value.FromAnchorStableId,
            ToAnchorStableId = value.ToAnchorStableId,
            RelationCode = value.RelationCode,
            Priority = value.Priority,
        };

        private static SimulationBattlefieldAnchorPlacementSnapshot Placement(
            SimulationBattlefieldAnchorPlacementSnapshot value) => new()
        {
            BattlefieldAnchorStableId = value.BattlefieldAnchorStableId,
            BattlePose = Pose(value.BattlePose),
            WidthMeters = value.WidthMeters,
            DepthMeters = value.DepthMeters,
            SizeVariantCode = value.SizeVariantCode,
        };

        private static SimulationBattlefieldZoneSnapshot Zone(
            SimulationBattlefieldZoneSnapshot value) => new()
        {
            ZoneStableId = value.ZoneStableId,
            ZoneKindCode = value.ZoneKindCode,
            CenterPose = Pose(value.CenterPose),
            WidthMeters = value.WidthMeters,
            DepthMeters = value.DepthMeters,
            SourceAnchorStableId = value.SourceAnchorStableId,
        };

        private static SimulationBattlefieldTerrainCellSnapshot TerrainCell(
            SimulationBattlefieldTerrainCellSnapshot value) => new()
        {
            CellX = value.CellX,
            CellZ = value.CellZ,
            HeightCentimeters = value.HeightCentimeters,
            MovementCostPermille = value.MovementCostPermille,
            TerrainCode = value.TerrainCode,
            Walkable = value.Walkable,
        };

        private static SimulationBattleUnitSnapshot Unit(
            SimulationBattleUnitSnapshot value) => new()
        {
            UnitStableId = value.UnitStableId,
            SideCode = value.SideCode,
            MemberActorStableIds = value.MemberActorStableIds?.ToArray() ?? Array.Empty<string>(),
            ThreatTypeCode = value.ThreatTypeCode,
            MemberCount = value.MemberCount,
            CombatStrength = value.CombatStrength,
            HealthPermille = value.HealthPermille,
            StaminaPermille = value.StaminaPermille,
            MoralePermille = value.MoralePermille,
            RoleCodes = value.RoleCodes?.ToArray() ?? Array.Empty<string>(),
            EquipmentCodes = value.EquipmentCodes?.ToArray() ?? Array.Empty<string>(),
            CapabilityCodes = value.CapabilityCodes?.ToArray() ?? Array.Empty<string>(),
            FormationCode = value.FormationCode,
            InitialPose = Pose(value.InitialPose),
        };

        private static SimulationBattleCardModifierSnapshot CardModifier(
            SimulationBattleCardModifierSnapshot value) => new()
        {
            CardCopyStableId = value.CardCopyStableId,
            CardDefinitionStableId = value.CardDefinitionStableId,
            SourceCardRevision = value.SourceCardRevision,
            ApplicableControlModeCode = value.ApplicableControlModeCode,
            ActorStableId = value.ActorStableId,
            ModifierCode = value.ModifierCode,
            BasisPoints = value.BasisPoints,
            RuleRevision = value.RuleRevision,
        };

        private static SimulationBattleSpatialPoseSnapshot Pose(
            SimulationBattleSpatialPoseSnapshot value) => new()
        {
            CoordinateSpaceCode = value.CoordinateSpaceCode,
            XMeters = value.XMeters,
            ZMeters = value.ZMeters,
            RotationDegrees = value.RotationDegrees,
        };
    }
}
