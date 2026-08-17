using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static partial class SimulationSaveReplayCloner
    {
        public static SimulationSessionSavePackage ClonePackage(SimulationSessionSavePackage source)
            => new SimulationSessionSavePackage
            {
                SchemaVersion = source.SchemaVersion,
                SaveStableId = source.SaveStableId,
                SessionStableId = source.SessionStableId,
                SavedWorldTick = source.SavedWorldTick,
                SavedWorldRevision = source.SavedWorldRevision,
                ReplayHashAlgorithmCode = source.ReplayHashAlgorithmCode,
                ReplayHash = source.ReplayHash,
                SessionCreateRequest = CloneCreateRequest(source.SessionCreateRequest),
                Snapshot = 경영SimulationSessionAggregate.Clone(source.Snapshot),
                WorldInventory = 경영SimulationSessionAggregate.CloneWorldInventory(
                    source.WorldInventory),
                SurvivalTarot = 경영SimulationSessionAggregate.CloneSurvivalTarotState(
                    source.SurvivalTarot),
                CommandLog = source.CommandLog.Select(CloneCommand).ToArray(),
                Battles = source.Battles.Select(
                    SimulationBattleInstanceState.CloneSaveRecord).ToArray(),
            };

        public static 경영SimulationSession생성Request CloneCreateRequest(
            경영SimulationSession생성Request source)
            => new 경영SimulationSession생성Request
            {
                ClientRequestId = source.ClientRequestId,
                ScenarioStableId = source.ScenarioStableId,
                ScenarioDataRevision = source.ScenarioDataRevision,
                ScenarioSeed = source.ScenarioSeed,
                RuleRevision = source.RuleRevision,
                DurationTicks = source.DurationTicks,
                WorldContext = new SimulationWorldContext생성Request
                {
                    FactionStableId = source.WorldContext.FactionStableId,
                    TerritoryStableId = source.WorldContext.TerritoryStableId,
                    SettlementStableId = source.WorldContext.SettlementStableId,
                    GameDateStartsOn = source.WorldContext.GameDateStartsOn,
                },
                Settlement = 경영SimulationSessionAggregate.CloneSettlementRequest(source.Settlement),
                NpcWorkforce = 경영SimulationSessionAggregate.CloneNpcWorkforceInitialState(
                    source.NpcWorkforce),
                SpatialWorld = 경영SimulationSessionAggregate.CloneSimulationSpatialInitialState(
                    source.SpatialWorld),
                WorldInventory = 경영SimulationSessionAggregate.CloneWorldInventoryInitialState(
                    source.WorldInventory),
                SurvivalTarot = 경영SimulationSessionAggregate.CloneSurvivalTarotInitialState(
                    source.SurvivalTarot),
                FarmSurvival = 경영SimulationSessionAggregate.CloneFarmSurvivalInitialState(
                    source.FarmSurvival),
                TeamRoleCards = 경영SimulationSessionAggregate
                    .CloneTeamRoleCardInitialStateOrNull(source.TeamRoleCards),
            };

        public static SimulationCommandLogEntrySnapshot CloneCommand(
            SimulationCommandLogEntrySnapshot source)
            => new SimulationCommandLogEntrySnapshot
            {
                Sequence = source.Sequence,
                CommandTypeCode = source.CommandTypeCode,
                AppliedWorldTick = source.AppliedWorldTick,
                ResultingWorldRevision = source.ResultingWorldRevision,
                TickRequest = source.TickRequest == null ? null : CloneTickRequest(source.TickRequest),
                DecisionConfirmRequest = source.DecisionConfirmRequest == null
                    ? null
                    : CloneConfirmRequest(source.DecisionConfirmRequest),
                HarvestDispositionImpactConfirmRequest = source.HarvestDispositionImpactConfirmRequest == null
                    ? null
                    : CloneHarvestDispositionImpactConfirmRequest(
                        source.HarvestDispositionImpactConfirmRequest),
                LogisticsMovementConfirmRequest = source.LogisticsMovementConfirmRequest == null
                    ? null
                    : CloneLogisticsMovementConfirmRequest(source.LogisticsMovementConfirmRequest),
                TurnClosingConfirmRequest = source.TurnClosingConfirmRequest == null
                    ? null
                    : CloneTurnClosingConfirmRequest(source.TurnClosingConfirmRequest),
                NpcPolicyChangeRequest = source.NpcPolicyChangeRequest == null
                    ? null
                    : CloneNpcPolicyChangeRequest(source.NpcPolicyChangeRequest),
                WorldItemAcquisitionConfirmRequest =
                    source.WorldItemAcquisitionConfirmRequest == null
                        ? null
                        : CloneWorldItemAcquisitionConfirmRequest(
                            source.WorldItemAcquisitionConfirmRequest),
                SurvivalTarotResponseConfirmRequest =
                    source.SurvivalTarotResponseConfirmRequest == null
                        ? null
                        : CloneSurvivalTarotResponseConfirmRequest(
                            source.SurvivalTarotResponseConfirmRequest),
                SurvivalTarotResolutionConfirmRequest =
                    source.SurvivalTarotResolutionConfirmRequest == null
                        ? null
                        : CloneSurvivalTarotResolutionConfirmRequest(
                            source.SurvivalTarotResolutionConfirmRequest),
                FarmWorkConfirmRequest = source.FarmWorkConfirmRequest == null
                    ? null
                    : CloneFarmWorkConfirmRequest(source.FarmWorkConfirmRequest),
                ThreatResponseConfirmRequest = source.ThreatResponseConfirmRequest == null
                    ? null
                    : CloneThreatResponseConfirmRequest(
                        source.ThreatResponseConfirmRequest),
                CombatPerspectiveConfirmRequest =
                    source.CombatPerspectiveConfirmRequest == null ? null
                        : CloneCombatPerspectiveConfirmRequest(
                            source.CombatPerspectiveConfirmRequest),
                CombatBeatStartRequest = source.CombatBeatStartRequest == null ? null
                    : CloneCombatBeatStartRequest(source.CombatBeatStartRequest),
                CombatReactionConfirmRequest =
                    source.CombatReactionConfirmRequest == null ? null
                        : CloneCombatReactionConfirmRequest(
                            source.CombatReactionConfirmRequest),
                TacticalOrderConfirmRequest =
                    source.TacticalOrderConfirmRequest == null ? null
                        : CloneTacticalOrderConfirmRequest(
                            source.TacticalOrderConfirmRequest),
                TeamRoleCardEquipRequest = source.TeamRoleCardEquipRequest == null
                    ? null : CloneTeamRoleCardEquipRequest(
                        source.TeamRoleCardEquipRequest),
                TeamActivityStartRequest = source.TeamActivityStartRequest == null
                    ? null : CloneTeamActivityStartRequest(
                        source.TeamActivityStartRequest),
                TeamActivityEndRequest = source.TeamActivityEndRequest == null
                    ? null : CloneTeamActivityEndRequest(
                        source.TeamActivityEndRequest),
                TileTraversalConfirmRequest = source.TileTraversalConfirmRequest == null
                    ? null : 경영SimulationSessionAggregate.CloneTileTraversalRequest(
                        source.TileTraversalConfirmRequest),
                CollectibleCardDrawRequest = source.CollectibleCardDrawRequest == null
                    ? null : 경영SimulationSessionAggregate.CloneCollectibleCardDrawRequest(
                        source.CollectibleCardDrawRequest),
                CollectibleCardTransferRequest = source.CollectibleCardTransferRequest == null
                    ? null : 경영SimulationSessionAggregate.CloneCollectibleCardTransferRequest(
                        source.CollectibleCardTransferRequest),
                TaskCancelRequest = source.TaskCancelRequest == null
                    ? null : CloneTaskCancelRequest(source.TaskCancelRequest),
                TaskStableId = source.TaskStableId,
            };

        public static SimulationTaskCancelRequest CloneTaskCancelRequest(
            SimulationTaskCancelRequest source)
            => new SimulationTaskCancelRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                ReasonCode = source.ReasonCode,
            };

        public static SimulationFarmWorkConfirmRequest CloneFarmWorkConfirmRequest(
            SimulationFarmWorkConfirmRequest source)
            => new SimulationFarmWorkConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                ActorStableId = source.ActorStableId,
                TargetStableId = source.TargetStableId,
                ActionCode = source.ActionCode,
                AssignmentKindCode = source.AssignmentKindCode,
                PreferredSpatialStableId = source.PreferredSpatialStableId,
            };

        public static SimulationThreatResponseConfirmRequest
            CloneThreatResponseConfirmRequest(
                SimulationThreatResponseConfirmRequest source)
            => new SimulationThreatResponseConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                EncounterStableId = source.EncounterStableId,
                ActorStableId = source.ActorStableId,
                ChoiceStableId = source.ChoiceStableId,
            };

        public static SimulationCombatPerspectiveConfirmRequest
            CloneCombatPerspectiveConfirmRequest(
                SimulationCombatPerspectiveConfirmRequest source)
            => new SimulationCombatPerspectiveConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                ActorStableId = source.ActorStableId,
                PerspectiveCode = source.PerspectiveCode,
            };

        public static SimulationCombatBeatStartRequest CloneCombatBeatStartRequest(
            SimulationCombatBeatStartRequest source)
            => new SimulationCombatBeatStartRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                EncounterStableId = source.EncounterStableId,
                ActorStableId = source.ActorStableId,
            };

        public static SimulationCombatReactionConfirmRequest
            CloneCombatReactionConfirmRequest(
                SimulationCombatReactionConfirmRequest source)
            => new SimulationCombatReactionConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                BeatStableId = source.BeatStableId,
                ActorStableId = source.ActorStableId,
                ReactionActionCode = source.ReactionActionCode,
                ReactionOffsetMs = source.ReactionOffsetMs,
            };

        public static SimulationTacticalOrderConfirmRequest
            CloneTacticalOrderConfirmRequest(
                SimulationTacticalOrderConfirmRequest source)
            => new SimulationTacticalOrderConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                OrderWindowStableId = source.OrderWindowStableId,
                FrontStableId = source.FrontStableId,
                ActorStableId = source.ActorStableId,
                OrderCode = source.OrderCode,
                OpportunityStableId = source.OpportunityStableId,
            };

        public static SimulationTeamRoleCardEquipRequest
            CloneTeamRoleCardEquipRequest(SimulationTeamRoleCardEquipRequest source)
            => new SimulationTeamRoleCardEquipRequest
            {
                ClientRequestId = source.ClientRequestId,
                ExpectedRevision = source.ExpectedRevision,
                ExpectedTeamPolicyRevision = source.ExpectedTeamPolicyRevision,
                RequestingActorStableId = source.RequestingActorStableId,
                TargetActorStableId = source.TargetActorStableId,
                CardCopyStableId = source.CardCopyStableId,
                SlotCode = source.SlotCode,
            };

        public static SimulationTeamActivityStartRequest
            CloneTeamActivityStartRequest(SimulationTeamActivityStartRequest source)
            => new SimulationTeamActivityStartRequest
            {
                ClientRequestId = source.ClientRequestId,
                ExpectedRevision = source.ExpectedRevision,
                ExpectedTeamPolicyRevision = source.ExpectedTeamPolicyRevision,
                ActorStableId = source.ActorStableId,
                CardCopyStableId = source.CardCopyStableId,
                ActivityRoleCode = source.ActivityRoleCode,
                ActivityStableId = source.ActivityStableId,
                LocationStableId = source.LocationStableId,
            };

        public static SimulationTeamActivityEndRequest
            CloneTeamActivityEndRequest(SimulationTeamActivityEndRequest source)
            => new SimulationTeamActivityEndRequest
            {
                ClientRequestId = source.ClientRequestId,
                ExpectedRevision = source.ExpectedRevision,
                ActorStableId = source.ActorStableId,
                ActivityStableId = source.ActivityStableId,
            };

        public static SimulationWorldItemAcquisitionConfirmRequest
            CloneWorldItemAcquisitionConfirmRequest(
                SimulationWorldItemAcquisitionConfirmRequest source)
            => new SimulationWorldItemAcquisitionConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                PlayerStableId = source.PlayerStableId,
                BuildingStableId = source.BuildingStableId,
                ContainerStableId = source.ContainerStableId,
                ItemStackStableId = source.ItemStackStableId,
                Quantity = source.Quantity,
            };

        public static SimulationSurvivalTarotResponseConfirmRequest
            CloneSurvivalTarotResponseConfirmRequest(
                SimulationSurvivalTarotResponseConfirmRequest source)
            => new SimulationSurvivalTarotResponseConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                OpportunityStableId = source.OpportunityStableId,
                PlayerStableId = source.PlayerStableId,
                OfferStableId = source.OfferStableId,
            };

        public static SimulationSurvivalTarotResolutionConfirmRequest
            CloneSurvivalTarotResolutionConfirmRequest(
                SimulationSurvivalTarotResolutionConfirmRequest source)
            => new SimulationSurvivalTarotResolutionConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                OpportunityStableId = source.OpportunityStableId,
                PlayerStableId = source.PlayerStableId,
                OfferStableId = source.OfferStableId,
            };

        public static SimulationNpcPolicyChangeRequest CloneNpcPolicyChangeRequest(
            SimulationNpcPolicyChangeRequest source)
            => new SimulationNpcPolicyChangeRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                PolicyStableId = source.PolicyStableId,
                AutomationEnabled = source.AutomationEnabled,
                Priority = source.Priority,
                PreferredActorStableId = source.PreferredActorStableId,
                AutoDelegationEnabled = source.AutoDelegationEnabled,
            };

        public static SimulationTurnClosingConfirmRequest CloneTurnClosingConfirmRequest(
            SimulationTurnClosingConfirmRequest source)
            => new SimulationTurnClosingConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Preview = new SimulationTurnClosingPreviewRequest
                {
                    ExpectedRevision = source.Preview.ExpectedRevision,
                    SelectedCardStableIds = source.Preview.SelectedCardStableIds.ToArray(),
                    SelectedTarotCard = source.Preview.SelectedTarotCard == null
                        ? null
                        : new Simulation타로CardSelectionRequest
                        {
                            OfferStableId = source.Preview.SelectedTarotCard.OfferStableId,
                            CardStableId = source.Preview.SelectedTarotCard.CardStableId,
                            OrientationCode = source.Preview.SelectedTarotCard.OrientationCode,
                        },
                },
            };

        public static 경영SimulationTick진행Request CloneTickRequest(
            경영SimulationTick진행Request source)
            => new 경영SimulationTick진행Request
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                TickCount = source.TickCount,
            };

        public static SimulationDecisionConfirmRequest CloneConfirmRequest(
            SimulationDecisionConfirmRequest source)
            => new SimulationDecisionConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Preview = ClonePreviewRequest(source.Preview),
            };

        public static SimulationHarvestDispositionImpactConfirmRequest
            CloneHarvestDispositionImpactConfirmRequest(
                SimulationHarvestDispositionImpactConfirmRequest source)
            => new SimulationHarvestDispositionImpactConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Impact = new SimulationHarvestDispositionImpactPreviewRequest
                {
                    DispositionDecisionStableId = source.Impact.DispositionDecisionStableId,
                    DispositionDecisionRevision = source.Impact.DispositionDecisionRevision,
                    HarvestLotStableId = source.Impact.HarvestLotStableId,
                    HarvestLotRevision = source.Impact.HarvestLotRevision,
                    ProductStableId = source.Impact.ProductStableId,
                    Quantity = source.Impact.Quantity,
                    UnitCode = source.Impact.UnitCode,
                    ChoiceCode = source.Impact.ChoiceCode,
                    NextWorkflowCode = source.Impact.NextWorkflowCode,
                    ActorStableId = source.Impact.ActorStableId,
                    SourceStableIds = source.Impact.SourceStableIds.ToArray(),
                },
            };

        public static SimulationLogisticsMovementConfirmRequest
            CloneLogisticsMovementConfirmRequest(
                SimulationLogisticsMovementConfirmRequest source)
            => new SimulationLogisticsMovementConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Movement = new SimulationLogisticsMovementPreviewRequest
                {
                    CargoStableId = source.Movement.CargoStableId,
                    CargoRevision = source.Movement.CargoRevision,
                    SourceExportCargoHandoffStableId =
                        source.Movement.SourceExportCargoHandoffStableId,
                    SourceAllocationStableId = source.Movement.SourceAllocationStableId,
                    HarvestLotStableId = source.Movement.HarvestLotStableId,
                    PackageLotStableId = source.Movement.PackageLotStableId,
                    ProductStableId = source.Movement.ProductStableId,
                    Quantity = source.Movement.Quantity,
                    UnitCode = source.Movement.UnitCode,
                    RouteStableId = source.Movement.RouteStableId,
                    OriginFacilityStableId = source.Movement.OriginFacilityStableId,
                    DestinationFacilityStableId = source.Movement.DestinationFacilityStableId,
                    ActorStableId = source.Movement.ActorStableId,
                    PreferredOriginSpatialStableId = source.Movement.PreferredOriginSpatialStableId,
                    PreferredRouteSpatialStableId = source.Movement.PreferredRouteSpatialStableId,
                    PreferredDestinationSpatialStableId = source.Movement.PreferredDestinationSpatialStableId,
                    RequiredRouteTicks = source.Movement.RequiredRouteTicks,
                    FreightTransport = 경영SimulationSessionAggregate.CloneFreightTransportBinding(
                        source.Movement.FreightTransport),
                    SourceStableIds = source.Movement.SourceStableIds.ToArray(),
                },
            };

        private static SimulationDecisionPreviewRequest ClonePreviewRequest(
            SimulationDecisionPreviewRequest source)
            => new SimulationDecisionPreviewRequest
            {
                DecisionStableId = source.DecisionStableId,
                DecisionTypeCode = source.DecisionTypeCode,
                ActorStableId = source.ActorStableId,
                TargetStableIds = source.TargetStableIds.ToArray(),
                ExpectedCosts = source.ExpectedCosts.Select(CloneValue).ToArray(),
                ExpectedEffects = source.ExpectedEffects.Select(CloneValue).ToArray(),
                Uncertainties = source.Uncertainties.ToArray(),
                BlockReasonCodes = source.BlockReasonCodes.ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = source.Task.TaskStableId,
                    TaskTypeCode = source.Task.TaskTypeCode,
                    FacilityStableId = source.Task.FacilityStableId,
                    ActionCode = source.Task.ActionCode,
                    AssignedActorStableId = source.Task.AssignedActorStableId,
                    PreferredSpatialStableId = source.Task.PreferredSpatialStableId,
                    PreferredOriginSpatialStableId = source.Task.PreferredOriginSpatialStableId,
                    PreferredRouteSpatialStableId = source.Task.PreferredRouteSpatialStableId,
                    PreferredDestinationSpatialStableId = source.Task.PreferredDestinationSpatialStableId,
                    RouteStableId = source.Task.RouteStableId,
                    DestinationFacilityStableId = source.Task.DestinationFacilityStableId,
                    AssignedCapacity = source.Task.AssignedCapacity,
                    AssignedCapacityUnitCode = source.Task.AssignedCapacityUnitCode,
                    DurationTicks = source.Task.DurationTicks,
                    InputLotStableIds = source.Task.InputLotStableIds.ToArray(),
                    OutputCandidateCodes = source.Task.OutputCandidateCodes.ToArray(),
                    SourceStableIds = source.Task.SourceStableIds.ToArray(),
                },
            };

        private static SimulationValueProjection CloneValue(SimulationValueProjection source)
            => new SimulationValueProjection
            {
                ValueTypeCode = source.ValueTypeCode,
                TargetLedgerStableId = source.TargetLedgerStableId,
                BeforeValue = source.BeforeValue,
                Delta = source.Delta,
                AfterValue = source.AfterValue,
                UnitCode = source.UnitCode,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }
}
