using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class SimulationSessionReplay
    {
        public static 경영SimulationSessionAggregate Restore(SimulationSessionSavePackage package)
        {
            ValidatePackage(package);
            var aggregate = new 경영SimulationSessionAggregate(
                SimulationSaveReplayCloner.CloneCreateRequest(package.SessionCreateRequest),
                package.RealityContext == null ? null
                    : 경영SimulationSessionAggregate.CloneRealityContext(
                        package.RealityContext));
            if (!string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V5,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V6,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V7,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                    StringComparison.Ordinal))
                aggregate.UseLegacyRegionalCausalityRules();
            if (!string.Equals(aggregate.SessionStableId, package.SessionStableId, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationSaveSessionIdentityMismatch");

            for (var index = 0; index < package.CommandLog.Length; index++)
            {
                var entry = package.CommandLog[index];
                if (entry.Sequence != index + 1L)
                    throw new SimulationConflictException("SimulationCommandLogSequenceInvalid");
                EnsureSingleCommandPayload(entry);

                if (entry.CommandTypeCode == SimulationCommandTypeCodes.DecisionConfirm)
                {
                    if (entry.DecisionConfirmRequest == null || entry.TickRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmDecision(
                        SimulationSaveReplayCloner.CloneConfirmRequest(entry.DecisionConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.HarvestDispositionImpactConfirm)
                {
                    if (entry.HarvestDispositionImpactConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmHarvestDispositionImpact(
                        SimulationSaveReplayCloner.CloneHarvestDispositionImpactConfirmRequest(
                            entry.HarvestDispositionImpactConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.LogisticsMovementConfirm)
                {
                    if (entry.LogisticsMovementConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmLogisticsMovement(
                        SimulationSaveReplayCloner.CloneLogisticsMovementConfirmRequest(
                            entry.LogisticsMovementConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TurnClosingConfirm)
                {
                    if (entry.TurnClosingConfirmRequest == null || entry.TickRequest != null
                        || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmTurnClosing(
                        SimulationSaveReplayCloner.CloneTurnClosingConfirmRequest(
                            entry.TurnClosingConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.NpcPolicyChange)
                {
                    if (entry.NpcPolicyChangeRequest == null || entry.TickRequest != null
                        || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.UpdateNpcPolicy(
                        SimulationSaveReplayCloner.CloneNpcPolicyChangeRequest(
                            entry.NpcPolicyChangeRequest));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.WorldItemAcquisitionConfirm)
                {
                    aggregate.ConfirmWorldItemAcquisition(
                        SimulationSaveReplayCloner.CloneWorldItemAcquisitionConfirmRequest(
                            entry.WorldItemAcquisitionConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.SurvivalTarotResponseConfirm)
                {
                    aggregate.ConfirmSurvivalTarotResponse(
                        SimulationSaveReplayCloner.CloneSurvivalTarotResponseConfirmRequest(
                            entry.SurvivalTarotResponseConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.SurvivalTarotResolutionConfirm)
                {
                    aggregate.ConfirmSurvivalTarotResolution(
                        SimulationSaveReplayCloner.CloneSurvivalTarotResolutionConfirmRequest(
                            entry.SurvivalTarotResolutionConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.FarmWorkConfirm)
                {
                    aggregate.ConfirmFarmWork(
                        SimulationSaveReplayCloner.CloneFarmWorkConfirmRequest(
                            entry.FarmWorkConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.FarmWorkPlanConfirm)
                {
                    aggregate.ConfirmFarmWorkPlan(
                        SimulationSaveReplayCloner.CloneFarmWorkPlanConfirmRequest(
                            entry.FarmWorkPlanConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.ThreatResponseConfirm)
                {
                    aggregate.ConfirmThreatResponse(
                        SimulationSaveReplayCloner.CloneThreatResponseConfirmRequest(
                            entry.ThreatResponseConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatPerspectiveConfirm)
                {
                    aggregate.ConfirmCombatPerspective(
                        SimulationSaveReplayCloner.CloneCombatPerspectiveConfirmRequest(
                            entry.CombatPerspectiveConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatBeatStart)
                {
                    aggregate.StartCombatBeat(
                        SimulationSaveReplayCloner.CloneCombatBeatStartRequest(
                            entry.CombatBeatStartRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatReactionConfirm)
                {
                    aggregate.ConfirmCombatReaction(
                        SimulationSaveReplayCloner.CloneCombatReactionConfirmRequest(
                            entry.CombatReactionConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TacticalOrderConfirm)
                {
                    aggregate.ConfirmTacticalOrder(
                        SimulationSaveReplayCloner.CloneTacticalOrderConfirmRequest(
                            entry.TacticalOrderConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TeamRoleCardEquip)
                {
                    aggregate.EquipTeamRoleCard(
                        SimulationSaveReplayCloner.CloneTeamRoleCardEquipRequest(
                            entry.TeamRoleCardEquipRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TeamActivityStart)
                {
                    aggregate.StartTeamActivity(
                        SimulationSaveReplayCloner.CloneTeamActivityStartRequest(
                            entry.TeamActivityStartRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatCardLoadoutSet)
                {
                    aggregate.SetTeamCombatCardLoadout(
                        SimulationSaveReplayCloner.CloneCombatCardLoadoutSetRequest(
                            entry.CombatCardLoadoutSetRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TeamActivityEnd)
                {
                    aggregate.EndTeamActivity(
                        SimulationSaveReplayCloner.CloneTeamActivityEndRequest(
                            entry.TeamActivityEndRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TileTraversalConfirm)
                {
                    aggregate.ConfirmTileTraversal(
                        경영SimulationSessionAggregate.CloneTileTraversalRequest(
                            entry.TileTraversalConfirmRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CollectibleCardDraw)
                {
                    aggregate.DrawCollectibleCard(
                        경영SimulationSessionAggregate.CloneCollectibleCardDrawRequest(
                            entry.CollectibleCardDrawRequest!));
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CollectibleCardTransfer)
                {
                    aggregate.TransferCollectibleCard(
                        경영SimulationSessionAggregate.CloneCollectibleCardTransferRequest(
                        entry.CollectibleCardTransferRequest!));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TaskCancel)
                {
                    aggregate.CancelTask(
                        entry.TaskStableId!,
                        SimulationSaveReplayCloner.CloneTaskCancelRequest(
                            entry.TaskCancelRequest!));
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.RegionalIncidentResponseConfirm)
                {
                    aggregate.ConfirmRegionalIncidentResponse(
                        entry.WorldEventStableId!,
                        SimulationSaveReplayCloner.CloneRegionalIncidentResponseConfirmRequest(
                            entry.RegionalIncidentResponseConfirmRequest!));
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.NatureEncounterVictory)
                {
                    aggregate.ApplyNatureEncounterVictory(
                        entry.NatureEncounterVictoryRequest!.BattleStableId,
                        entry.NatureEncounterVictoryRequest.EncounterStableId);
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.IntegratedWorldConfirm)
                {
                    aggregate.ConfirmIntegratedWorldCommand(
                        경영SimulationSessionAggregate.CloneIntegratedWorldCommand(
                            entry.IntegratedWorldConfirmRequest!));
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.IntegratedWorldEffectEnqueued)
                {
                    aggregate.QueueFacilityBattleDamage(
                        entry.FacilityDamageQueueRequest!.BattleStableId,
                        entry.FacilityDamageQueueRequest.FacilityStableId,
                        entry.FacilityDamageQueueRequest.SeverityCode);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TickAdvance)
                {
                    if (entry.TickRequest == null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.Advance(
                        SimulationSaveReplayCloner.CloneTickRequest(entry.TickRequest));
                }
                else
                {
                    throw new SimulationConflictException("SimulationCommandTypeUnsupported");
                }

                var current = aggregate.Snapshot();
                if (current.CurrentTick != entry.AppliedWorldTick
                    || current.Revision != entry.ResultingWorldRevision)
                {
                    throw new SimulationConflictException("SimulationCommandReplayResultMismatch");
                }
            }

            aggregate.RestoreLhWorldState(package.LhWorld);
            var replayed = aggregate.CreateSavePackage(new SimulationSessionSaveRequest
            {
                SaveStableId = package.SaveStableId,
                ExpectedRevision = aggregate.Revision,
            });
            replayed = SimulationBattleSaveReplay.AttachToPackage(
                replayed,
                package.Battles);
            if (!string.Equals(replayed.ReplayHash, package.ReplayHash, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationReplayHashMismatch");
            if (replayed.SavedWorldTick != package.SavedWorldTick
                || replayed.SavedWorldRevision != package.SavedWorldRevision)
            {
                throw new SimulationConflictException("SimulationSavePositionMismatch");
            }

            return aggregate;
        }

        private static void ValidatePackage(SimulationSessionSavePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (!string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V1, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V2,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V3,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V4,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V5,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V6,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V7,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                    StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                    StringComparison.Ordinal))
                throw new SimulationContractException("SimulationSaveSchemaUnsupported");
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V3,
                    StringComparison.Ordinal) && package.LhWorld == null)
                throw new SimulationContractException("SimulationLhWorldStateMissing");
            if (!string.Equals(
                package.ReplayHashAlgorithmCode,
                SimulationReplayHashAlgorithmCodes.Sha256,
                StringComparison.Ordinal))
            {
                throw new SimulationContractException("SimulationReplayHashAlgorithmUnsupported");
            }
            if (string.IsNullOrWhiteSpace(package.SaveStableId)
                || string.IsNullOrWhiteSpace(package.SessionStableId)
                || string.IsNullOrWhiteSpace(package.ReplayHash)
                || package.SessionCreateRequest == null
                || package.SessionCreateRequest.WorldContext == null
                || package.Snapshot == null
                || package.WorldInventory == null
                || package.SurvivalTarot == null
                || package.Snapshot.WorldContext == null
                || package.Snapshot.Decisions == null
                || package.Snapshot.Tasks == null
                || package.Snapshot.Effects == null
                || package.Snapshot.LogisticsMovements == null
                || package.Snapshot.FreightTransports == null
                || package.Snapshot.GroupOrders == null
                || package.Snapshot.FoodDeliveries == null
                || package.Snapshot.MarketConsumptions == null
                || package.Snapshot.IndividualOrders == null
                || package.Snapshot.StockReservations == null
                || package.Snapshot.ExportPreparations == null
                || package.Snapshot.ExportCargoPreparations == null
                || package.Snapshot.ExportCargoHandoffs == null
                || package.Snapshot.ExportPortReceipts == null
                || package.Snapshot.ExportReadinessReviews == null
                || package.Snapshot.ExportShipmentPlans == null
                || package.Snapshot.ExportShipmentExecutions == null
                || package.Snapshot.NpcOrganizations == null
                || package.Snapshot.NpcActors == null
                || package.Snapshot.NpcCapabilityGrants == null
                || package.Snapshot.NpcWorkPolicies == null
                || package.Snapshot.NpcTaskAssignments == null
                || package.Snapshot.NpcWorkRecords == null
                || package.Snapshot.NpcActionProjections == null
                || package.Snapshot.NpcFacilityInventories == null
                || ((string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V5,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V6,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V7,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                        StringComparison.Ordinal)
                    || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                        StringComparison.Ordinal)
                    ) && package.Snapshot.RegionalCausality == null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V6,
                        StringComparison.Ordinal)
                    && (package.SessionCreateRequest.IntegratedWorld == null
                        || package.Snapshot.IntegratedWorld == null))
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V7,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null
                    && package.Snapshot.IntegratedWorld == null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null
                    && package.Snapshot.IntegratedWorld == null)
                || ((string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                         StringComparison.Ordinal))
                    && package.SessionCreateRequest.IntegratedWorld != null
                    && package.Snapshot.IntegratedWorld == null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                        StringComparison.Ordinal)
                    && (package.RealityContext == null
                        || string.IsNullOrWhiteSpace(
                            package.SessionCreateRequest.RealityContextProfileStableId)))
                || ((string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                         StringComparison.Ordinal))
                    && package.Snapshot.NatureMind == null)
                || ((string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                         StringComparison.Ordinal))
                    && package.Snapshot.AreaAccess == null)
                || ((string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                         StringComparison.Ordinal)
                     || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                         StringComparison.Ordinal))
                    && package.Snapshot.HostedWorld == null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                        StringComparison.Ordinal)
                    && package.Snapshot.CoopConstruction == null)
                || package.CommandLog == null
                || package.Battles == null)
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (package.Snapshot.Settlement != null
                && (package.Snapshot.Settlement.Districts == null
                    || package.Snapshot.Settlement.Facilities == null
                    || package.Snapshot.Settlement.MarketSupplyByProduct == null
                    || package.Snapshot.Settlement.ResidentConsumptionByProduct == null
                    || package.Snapshot.Settlement.ReserveStockLots == null
                    || package.Snapshot.Settlement.HarvestLotAllocations == null
                    || package.Snapshot.Settlement.ActiveTaskStableIds == null
                    || package.Snapshot.Settlement.SourceStableIds == null))
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (package.Snapshot.FarmSurvival != null
                && (package.Snapshot.FarmSurvival.Actors == null
                    || package.Snapshot.FarmSurvival.SoilTiles == null
                    || package.Snapshot.FarmSurvival.Defenses == null
                    || package.Snapshot.FarmSurvival.WorkOrders == null
                    || package.Snapshot.FarmSurvival.Encounters == null
                    || package.Snapshot.FarmSurvival.DayReports == null
                    || package.Snapshot.FarmSurvival.Combat == null
                    || package.Snapshot.FarmSurvival.Combat.Perspectives == null
                    || package.Snapshot.FarmSurvival.Combat.Beats == null
                    || package.Snapshot.FarmSurvival.Combat.Reactions == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical.Fronts == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical.Squads == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical.Opportunities == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical.OrderWindows == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical.Orders == null
                    || package.Snapshot.FarmSurvival.Combat.Tactical.Resolutions == null))
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (package.Snapshot.TeamRoleCards != null
                && (package.Snapshot.TeamRoleCards.MemberActorStableIds == null
                    || package.Snapshot.TeamRoleCards.Cards == null
                    || package.Snapshot.TeamRoleCards.ActiveActivities == null
                    || package.Snapshot.TeamRoleCards.MemberRoles == null))
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (package.Snapshot.Exploration != null
                && (package.Snapshot.Exploration.ActorTilePositions == null
                    || package.Snapshot.Exploration.RevealedL2TileKeys == null
                    || package.Snapshot.Exploration.RevealedL1AreaKeys == null
                    || package.Snapshot.Exploration.DiscoveryEvents == null))
                throw new SimulationContractException("SimulationSavePackageInvalid");
            if (package.Snapshot.CollectibleCardRewards != null
                && (package.Snapshot.CollectibleCardRewards.ProbabilityProfile == null
                    || package.Snapshot.CollectibleCardRewards.Definitions == null
                    || package.Snapshot.CollectibleCardRewards.DrawOpportunities == null
                    || package.Snapshot.CollectibleCardRewards.Cards == null
                    || package.Snapshot.CollectibleCardRewards.PityStates == null
                    || package.Snapshot.CollectibleCardRewards.Evaluations == null
                    || package.Snapshot.CollectibleCardRewards.Transfers == null))
                throw new SimulationContractException("SimulationSavePackageInvalid");

            for (var index = 0; index < package.CommandLog.Length; index++)
            {
                var entry = package.CommandLog[index];
                if (entry == null || entry.Sequence != index + 1L)
                    throw new SimulationConflictException("SimulationCommandLogSequenceInvalid");
                EnsureSingleCommandPayload(entry);
                if (entry.CommandTypeCode == SimulationCommandTypeCodes.DecisionConfirm)
                {
                    if (entry.DecisionConfirmRequest == null || entry.TickRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateDecisionConfirm(
                        entry.DecisionConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.HarvestDispositionImpactConfirm)
                {
                    if (entry.HarvestDispositionImpactConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateHarvestDispositionImpactConfirmRequestForReplay(
                        entry.HarvestDispositionImpactConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.LogisticsMovementConfirm)
                {
                    if (entry.LogisticsMovementConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateLogisticsMovementConfirmRequestForReplay(
                        entry.LogisticsMovementConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TurnClosingConfirm)
                {
                    if (entry.TurnClosingConfirmRequest == null || entry.TickRequest != null
                        || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateTurnClosingConfirmRequest(
                        entry.TurnClosingConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.NpcPolicyChange)
                {
                    if (entry.NpcPolicyChangeRequest == null || entry.TickRequest != null
                        || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateNpcPolicyChangeRequest(
                        entry.NpcPolicyChangeRequest);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.WorldItemAcquisitionConfirm)
                {
                    경영SimulationSessionAggregate.ValidateWorldItemAcquisitionConfirmRequest(
                        entry.WorldItemAcquisitionConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.SurvivalTarotResponseConfirm)
                {
                    경영SimulationSessionAggregate.ValidateSurvivalTarotResponseRequest(
                        entry.SurvivalTarotResponseConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.SurvivalTarotResolutionConfirm)
                {
                    경영SimulationSessionAggregate.ValidateSurvivalTarotResolutionRequest(
                        entry.SurvivalTarotResolutionConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.FarmWorkConfirm)
                {
                    경영SimulationSessionAggregate.ValidateFarmWorkConfirmRequest(
                        entry.FarmWorkConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.FarmWorkPlanConfirm)
                {
                    경영SimulationSessionAggregate.ValidateFarmWorkPlanConfirmRequest(
                        entry.FarmWorkPlanConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.ThreatResponseConfirm)
                {
                    경영SimulationSessionAggregate.ValidateThreatResponseRequest(
                        entry.ThreatResponseConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatPerspectiveConfirm)
                {
                    경영SimulationSessionAggregate.ValidateCombatPerspectiveRequest(
                        entry.CombatPerspectiveConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatBeatStart)
                {
                    경영SimulationSessionAggregate.ValidateCombatBeatStartRequest(
                        entry.CombatBeatStartRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatReactionConfirm)
                {
                    경영SimulationSessionAggregate.ValidateCombatReactionRequest(
                        entry.CombatReactionConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TacticalOrderConfirm)
                {
                    경영SimulationSessionAggregate.ValidateTacticalOrderConfirmRequest(
                        entry.TacticalOrderConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TeamRoleCardEquip)
                {
                    SimulationTeamRoleCardState.ValidateEquip(
                        entry.TeamRoleCardEquipRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TeamActivityStart)
                {
                    SimulationTeamRoleCardState.ValidateStart(
                        entry.TeamActivityStartRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CombatCardLoadoutSet)
                {
                    SimulationTeamRoleCardState.ValidateCombatLoadout(
                        entry.CombatCardLoadoutSetRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TeamActivityEnd)
                {
                    SimulationTeamRoleCardState.ValidateEnd(
                        entry.TeamActivityEndRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.TileTraversalConfirm)
                {
                    경영SimulationSessionAggregate.ValidateTileTraversalRequest(
                        entry.TileTraversalConfirmRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CollectibleCardDraw)
                {
                    경영SimulationSessionAggregate.ValidateCollectibleCardDrawRequest(
                        entry.CollectibleCardDrawRequest!);
                }
                else if (entry.CommandTypeCode
                    == SimulationCommandTypeCodes.CollectibleCardTransfer)
                {
                    경영SimulationSessionAggregate.ValidateCollectibleCardTransferRequest(
                        entry.CollectibleCardTransferRequest!);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TaskCancel)
                {
                    if (string.IsNullOrWhiteSpace(entry.TaskStableId))
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateTaskCancel(
                        entry.TaskStableId,
                        entry.TaskCancelRequest!);
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.RegionalIncidentResponseConfirm)
                {
                    if (string.IsNullOrWhiteSpace(entry.WorldEventStableId))
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateRegionalIncidentConfirmRequest(
                        entry.WorldEventStableId!,
                        entry.RegionalIncidentResponseConfirmRequest!);
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.NatureEncounterVictory)
                {
                    if (entry.NatureEncounterVictoryRequest == null
                        || string.IsNullOrWhiteSpace(
                            entry.NatureEncounterVictoryRequest.BattleStableId)
                        || string.IsNullOrWhiteSpace(
                            entry.NatureEncounterVictoryRequest.EncounterStableId))
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.IntegratedWorldConfirm)
                {
                    if (entry.IntegratedWorldConfirmRequest == null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                }
                else if (entry.CommandTypeCode ==
                    SimulationCommandTypeCodes.IntegratedWorldEffectEnqueued)
                {
                    if (entry.FacilityDamageQueueRequest == null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TickAdvance)
                {
                    if (entry.TickRequest == null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null
                        || entry.NpcPolicyChangeRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateAdvance(entry.TickRequest);
                }
                else
                {
                    throw new SimulationConflictException("SimulationCommandTypeUnsupported");
                }
            }

            SimulationBattleSaveReplay.ValidatePackage(package);

            경영SimulationSessionAggregate.ValidateCreate(package.SessionCreateRequest);
            string packageHash;
            try
            {
                packageHash = SimulationReplayHasher.Calculate(package);
            }
            catch (Exception error) when (
                error is NullReferenceException
                || error is ArgumentException
                || error is InvalidOperationException)
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (!string.Equals(packageHash, package.ReplayHash, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationReplayHashMismatch");
        }

        private static void EnsureSingleCommandPayload(SimulationCommandLogEntrySnapshot entry)
        {
            var payloadCount = 0;
            if (entry.TickRequest != null) payloadCount++;
            if (entry.DecisionConfirmRequest != null) payloadCount++;
            if (entry.HarvestDispositionImpactConfirmRequest != null) payloadCount++;
            if (entry.LogisticsMovementConfirmRequest != null) payloadCount++;
            if (entry.TurnClosingConfirmRequest != null) payloadCount++;
            if (entry.NpcPolicyChangeRequest != null) payloadCount++;
            if (entry.WorldItemAcquisitionConfirmRequest != null) payloadCount++;
            if (entry.SurvivalTarotResponseConfirmRequest != null) payloadCount++;
            if (entry.SurvivalTarotResolutionConfirmRequest != null) payloadCount++;
            if (entry.FarmWorkConfirmRequest != null) payloadCount++;
            if (entry.FarmWorkPlanConfirmRequest != null) payloadCount++;
            if (entry.ThreatResponseConfirmRequest != null) payloadCount++;
            if (entry.CombatPerspectiveConfirmRequest != null) payloadCount++;
            if (entry.CombatBeatStartRequest != null) payloadCount++;
            if (entry.CombatReactionConfirmRequest != null) payloadCount++;
            if (entry.TacticalOrderConfirmRequest != null) payloadCount++;
            if (entry.TeamRoleCardEquipRequest != null) payloadCount++;
            if (entry.CombatCardLoadoutSetRequest != null) payloadCount++;
            if (entry.TeamActivityStartRequest != null) payloadCount++;
            if (entry.TeamActivityEndRequest != null) payloadCount++;
            if (entry.TileTraversalConfirmRequest != null) payloadCount++;
            if (entry.CollectibleCardDrawRequest != null) payloadCount++;
            if (entry.CollectibleCardTransferRequest != null) payloadCount++;
            if (entry.TaskCancelRequest != null) payloadCount++;
            if (entry.RegionalIncidentResponseConfirmRequest != null) payloadCount++;
            if (entry.NatureEncounterVictoryRequest != null) payloadCount++;
            if (entry.IntegratedWorldConfirmRequest != null) payloadCount++;
            if (entry.FacilityDamageQueueRequest != null) payloadCount++;
            if (payloadCount != 1)
                throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
        }
    }
}
