using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static partial class SimulationReplayHasher
    {
        public static string Calculate(SimulationSessionSavePackage package)
        {
            var canonical = new StringBuilder();
            Add(canonical, package.SchemaVersion);
            var includesRealityContext = string.Equals(package.SchemaVersion,
                SimulationSaveSchemaVersions.V8, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V9, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V10, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V11, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V12, StringComparison.Ordinal);
            AddCreateRequest(canonical, package.SessionCreateRequest,
                includesRealityContext);
            var includesTarotJourneyRoot = string.Equals(package.SchemaVersion,
                SimulationSaveSchemaVersions.V7, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V8, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V9, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V10, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V11, StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V12, StringComparison.Ordinal);
            AddSnapshot(canonical, package.Snapshot, includesTarotJourneyRoot);
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V5,
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
                    StringComparison.Ordinal))
                AddRegionalCausality(canonical, package.Snapshot.RegionalCausality);
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V6,
                    StringComparison.Ordinal)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V7,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null)
                || (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                        StringComparison.Ordinal)
                    && package.SessionCreateRequest.IntegratedWorld != null))
            {
                AddIntegratedWorldInitialState(canonical,
                    package.SessionCreateRequest.IntegratedWorld!);
                AddIntegratedWorldSnapshot(canonical, package.Snapshot.IntegratedWorld);
            }
            if ((string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V8,
                     StringComparison.Ordinal)
                 || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                     StringComparison.Ordinal)
                 || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                     StringComparison.Ordinal)
                 || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                     StringComparison.Ordinal)
                 || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                     StringComparison.Ordinal)) && package.RealityContext != null)
                AddRealityContext(canonical, package.RealityContext);
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V9,
                    StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                    StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                    StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                    StringComparison.Ordinal))
                AddNatureMind(canonical, package.Snapshot.NatureMind);
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V10,
                    StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                    StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                    StringComparison.Ordinal))
                AddAreaAccess(canonical, package.Snapshot.AreaAccess);
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V11,
                    StringComparison.Ordinal)
                || string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                    StringComparison.Ordinal))
                AddHostedWorld(canonical, package.Snapshot.HostedWorld);
            if (string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V12,
                    StringComparison.Ordinal))
                AddCoopConstruction(canonical, package.Snapshot.CoopConstruction);
            if (package.SessionCreateRequest.WorldInventory != null)
                AddWorldInventory(canonical, package.WorldInventory);
            if (package.SessionCreateRequest.SurvivalTarot != null)
                AddSurvivalTarot(canonical, package.SurvivalTarot);
            if (package.LhWorld != null)
            {
                Add(canonical, package.LhWorld.WorldSeed);
                Add(canonical, package.LhWorld.GeneratorVersion);
                Add(canonical, package.LhWorld.AreaSetStableId);
                Add(canonical, package.LhWorld.AreaSetRevision);
                Add(canonical, package.LhWorld.AreaSetBoundaryHashSha256);
                if (!string.IsNullOrWhiteSpace(package.LhWorld.WorldLayoutStableId))
                {
                    Add(canonical, package.LhWorld.WorldLayoutStableId);
                    Add(canonical, package.LhWorld.WorldLayoutRevision);
                    Add(canonical, package.LhWorld.WorldLayoutHashSha256);
                    Add(canonical, package.LhWorld.PlacementAuthorityCode);
                    Add(canonical, package.LhWorld.WorldGroundingStateCode);
                    Add(canonical, package.LhWorld.GroundingEvidenceHashSha256);
                }
                Add(canonical, package.LhWorld.LastL3CellKey);
                Add(canonical, package.LhWorld.Deltas.Length);
                foreach (var delta in package.LhWorld.Deltas.OrderBy(
                             value => value.GeneratedStableId, StringComparer.Ordinal))
                {
                    Add(canonical, delta.GeneratedStableId);
                    Add(canonical, delta.DeltaKindCode);
                    Add(canonical, delta.StateCode);
                    Add(canonical, delta.AppliedWorldRevision);
                    Add(canonical, delta.Tombstone);
                }
            }
            if (package.Battles.Length > 0)
            {
                Add(canonical, package.Battles.Length);
                foreach (var battle in package.Battles.OrderBy(value =>
                    value.State.BattleStableId, StringComparer.Ordinal))
                {
                    Add(canonical, battle.State.BattleStableId);
                    Add(canonical, battle.IntegrityHashSha256);
                }
            }
            Add(canonical, package.CommandLog.Length);
            foreach (var entry in package.CommandLog.OrderBy(value => value.Sequence))
            {
                Add(canonical, entry.Sequence);
                Add(canonical, entry.CommandTypeCode);
                Add(canonical, entry.AppliedWorldTick);
                Add(canonical, entry.ResultingWorldRevision);
                if (entry.TickRequest != null)
                {
                    Add(canonical, entry.TickRequest.CommandId);
                    Add(canonical, entry.TickRequest.ExpectedRevision);
                    Add(canonical, entry.TickRequest.TickCount);
                }
                if (entry.IntegratedWorldConfirmRequest != null)
                    AddIntegratedWorldCommand(canonical,
                        entry.IntegratedWorldConfirmRequest);
                if (entry.FacilityDamageQueueRequest != null)
                {
                    Add(canonical, entry.FacilityDamageQueueRequest.BattleStableId);
                    Add(canonical, entry.FacilityDamageQueueRequest.FacilityStableId);
                    Add(canonical, entry.FacilityDamageQueueRequest.SeverityCode);
                }
                if (entry.DecisionConfirmRequest != null)
                {
                    Add(canonical, entry.DecisionConfirmRequest.CommandId);
                    Add(canonical, entry.DecisionConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildDecisionPayloadKey(
                        entry.DecisionConfirmRequest.Preview));
                }
                if (entry.RegionalIncidentResponseConfirmRequest != null)
                {
                    var request = entry.RegionalIncidentResponseConfirmRequest;
                    Add(canonical, entry.WorldEventStableId ?? string.Empty);
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, request.ActorStableId);
                    Add(canonical, request.ChoiceStableId);
                }
                if (entry.NatureEncounterVictoryRequest != null)
                {
                    Add(canonical, entry.NatureEncounterVictoryRequest.BattleStableId);
                    Add(canonical, entry.NatureEncounterVictoryRequest.EncounterStableId);
                }
                if (entry.HarvestDispositionImpactConfirmRequest != null)
                {
                    Add(canonical, entry.HarvestDispositionImpactConfirmRequest.CommandId);
                    Add(canonical, entry.HarvestDispositionImpactConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildHarvestDispositionImpactPayloadKey(
                        entry.HarvestDispositionImpactConfirmRequest.Impact));
                }
                if (entry.LogisticsMovementConfirmRequest != null)
                {
                    Add(canonical, entry.LogisticsMovementConfirmRequest.CommandId);
                    Add(canonical, entry.LogisticsMovementConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildLogisticsMovementPayloadKey(
                        entry.LogisticsMovementConfirmRequest.Movement));
                }
                if (entry.TurnClosingConfirmRequest != null)
                {
                    Add(canonical, entry.TurnClosingConfirmRequest.CommandId);
                    Add(canonical, entry.TurnClosingConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildTurnClosingPayloadKey(
                        entry.TurnClosingConfirmRequest.Preview));
                }
                if (entry.NpcPolicyChangeRequest != null)
                {
                    Add(canonical, entry.NpcPolicyChangeRequest.CommandId);
                    Add(canonical, entry.NpcPolicyChangeRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildNpcPolicyPayloadKey(
                        entry.NpcPolicyChangeRequest));
                }
                if (entry.WorldItemAcquisitionConfirmRequest != null)
                {
                    var request = entry.WorldItemAcquisitionConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, request.PlayerStableId);
                    Add(canonical, request.BuildingStableId);
                    Add(canonical, request.ContainerStableId);
                    Add(canonical, request.ItemStackStableId);
                    Add(canonical, request.Quantity);
                }
                if (entry.SurvivalTarotResponseConfirmRequest != null)
                {
                    var request = entry.SurvivalTarotResponseConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildSurvivalTarotResponsePayloadKey(request));
                }
                if (entry.SurvivalTarotResolutionConfirmRequest != null)
                {
                    var request = entry.SurvivalTarotResolutionConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildSurvivalTarotResolutionPayloadKey(request));
                }
                if (entry.FarmWorkConfirmRequest != null)
                {
                    var request = entry.FarmWorkConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildFarmWorkPayloadKey(request));
                }
                if (entry.FarmWorkPlanConfirmRequest != null)
                {
                    var request = entry.FarmWorkPlanConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildFarmWorkPlanPayloadKey(request));
                }
                if (entry.ThreatResponseConfirmRequest != null)
                {
                    var request = entry.ThreatResponseConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildThreatResponsePayloadKey(request));
                }
                if (entry.CombatPerspectiveConfirmRequest != null)
                {
                    var request = entry.CombatPerspectiveConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildCombatPerspectivePayloadKey(request));
                }
                if (entry.CombatBeatStartRequest != null)
                {
                    var request = entry.CombatBeatStartRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildCombatBeatPayloadKey(request));
                }
                if (entry.CombatReactionConfirmRequest != null)
                {
                    var request = entry.CombatReactionConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildCombatReactionPayloadKey(request));
                }
                if (entry.TacticalOrderConfirmRequest != null)
                {
                    var request = entry.TacticalOrderConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildTacticalOrderPayloadKey(request));
                }
                if (entry.TeamRoleCardEquipRequest != null)
                {
                    var request = entry.TeamRoleCardEquipRequest;
                    Add(canonical, request.ClientRequestId.ToString("N"));
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, request.ExpectedTeamPolicyRevision);
                    Add(canonical, request.RequestingActorStableId);
                    Add(canonical, request.TargetActorStableId);
                    Add(canonical, request.CardCopyStableId);
                    Add(canonical, request.SlotCode);
                }
                if (entry.CombatCardLoadoutSetRequest != null)
                {
                    var request = entry.CombatCardLoadoutSetRequest;
                    Add(canonical, request.ClientRequestId.ToString("N"));
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, request.ExpectedTeamPolicyRevision);
                    Add(canonical, request.RequestingActorStableId);
                    Add(canonical, request.TargetActorStableId);
                    Add(canonical, request.CombatControlModeCode);
                    foreach (var slot in request.Slots.OrderBy(value =>
                                 value.SlotCode, StringComparer.Ordinal))
                    {
                        Add(canonical, slot.SlotCode);
                        Add(canonical, slot.CardCopyStableId);
                    }
                }
                if (entry.TeamActivityStartRequest != null)
                {
                    var request = entry.TeamActivityStartRequest;
                    Add(canonical, request.ClientRequestId.ToString("N"));
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, request.ExpectedTeamPolicyRevision);
                    Add(canonical, request.ActorStableId);
                    Add(canonical, request.CardCopyStableId);
                    Add(canonical, request.ActivityRoleCode);
                    Add(canonical, request.ActivityStableId);
                    Add(canonical, request.LocationStableId);
                }
                if (entry.TeamActivityEndRequest != null)
                {
                    var request = entry.TeamActivityEndRequest;
                    Add(canonical, request.ClientRequestId.ToString("N"));
                    Add(canonical, request.ExpectedRevision);
                    Add(canonical, request.ActorStableId);
                    Add(canonical, request.ActivityStableId);
                }
                if (entry.TileTraversalConfirmRequest != null)
                {
                    var request = entry.TileTraversalConfirmRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildTileTraversalPayloadKey(request));
                }
                if (entry.CollectibleCardDrawRequest != null)
                {
                    var request = entry.CollectibleCardDrawRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildCollectibleCardDrawPayloadKey(request));
                }
                if (entry.CollectibleCardTransferRequest != null)
                {
                    var request = entry.CollectibleCardTransferRequest;
                    Add(canonical, request.CommandId);
                    Add(canonical, 경영SimulationSessionAggregate
                        .BuildCollectibleCardTransferPayloadKey(request));
                }
                if (entry.TaskCancelRequest != null)
                {
                    Add(canonical, entry.TaskStableId ?? string.Empty);
                    Add(canonical, 경영SimulationSessionAggregate.BuildTaskCancelPayloadKey(
                        entry.TaskStableId ?? string.Empty,
                        entry.TaskCancelRequest));
                }
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(canonical.ToString());
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }

        private static void AddCreateRequest(
            StringBuilder target,
            경영SimulationSession생성Request request,
            bool includesRealityContext)
        {
            Add(target, request.ClientRequestId.ToString("N"));
            Add(target, request.ScenarioStableId);
            Add(target, request.ScenarioDataRevision);
            Add(target, request.ScenarioSeed);
            Add(target, request.RuleRevision);
            if (includesRealityContext)
                Add(target, request.RealityContextProfileStableId);
            Add(target, request.DurationTicks);
            Add(target, request.WorldContext.FactionStableId);
            Add(target, request.WorldContext.TerritoryStableId);
            Add(target, request.WorldContext.SettlementStableId);
            Add(target, request.WorldContext.GameDateStartsOn.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            Add(target, 경영SimulationSessionAggregate.BuildSettlementPayloadKey(request.Settlement));
            if (request.NpcWorkforce != null)
                Add(target, 경영SimulationSessionAggregate.BuildNpcWorkforcePayloadKey(request.NpcWorkforce));
            if (request.SpatialWorld != null)
                Add(target, 경영SimulationSessionAggregate.BuildSimulationSpatialPayloadKey(
                    request.SpatialWorld));
            if (request.WorldInventory != null)
                Add(target, 경영SimulationSessionAggregate.BuildWorldInventoryPayloadKey(
                    request.WorldInventory));
            if (request.SurvivalTarot != null)
                Add(target, 경영SimulationSessionAggregate.BuildSurvivalTarotPayloadKey(
                    request.SurvivalTarot));
            if (request.FarmSurvival != null)
                Add(target, 경영SimulationSessionAggregate.BuildFarmSurvivalPayloadKey(
                    request.FarmSurvival));
            if (request.TeamRoleCards != null)
                Add(target, 경영SimulationSessionAggregate.BuildTeamRoleCardPayloadKey(
                    request.TeamRoleCards));
            if (request.NatureMind != null)
                Add(target, 경영SimulationSessionAggregate.BuildNatureMindInitialPayloadKey(
                    request.NatureMind));
        }

        private static void AddNatureMind(StringBuilder target,
            SimulationNatureMindStateSnapshot value)
        {
            Add(target, "NatureMindExtensionV1");
            Add(target, value.RuleRevision);
            Add(target, value.Balances.Length);
            foreach (var balance in value.Balances.OrderBy(item =>
                         item.PlayerStableId, StringComparer.Ordinal))
            {
                Add(target, balance.PlayerStableId);
                Add(target, balance.RecoveryOutput);
                Add(target, balance.ThreatOutput);
                Add(target, balance.RecoveryShare);
                Add(target, balance.ThreatShare);
                Add(target, balance.InterpretationStrength);
                Add(target, balance.InterpretationBandCode);
                Add(target, balance.Revision);
                Add(target, balance.BalanceHashSha256);
            }
            Add(target, value.Effects.Length);
            foreach (var effect in value.Effects.OrderBy(item =>
                         item.EffectStableId, StringComparer.Ordinal))
            {
                Add(target, effect.EffectStableId);
                Add(target, effect.PlayerStableId);
                Add(target, effect.SourceCode);
                Add(target, effect.SourceStableId);
                Add(target, effect.AxisCode);
                Add(target, effect.Magnitude);
                Add(target, effect.AppliedWorldTick);
                Add(target, effect.RuleRevision);
            }
            Add(target, value.Periods.Length);
            foreach (var period in value.Periods.OrderBy(item =>
                         item.PlayerStableId, StringComparer.Ordinal))
            {
                Add(target, period.PlayerStableId);
                Add(target, period.PeriodStateCode);
                Add(target, period.PeriodInstanceStableId);
                Add(target, period.SourceBalanceRevision);
                Add(target, period.SourceBalanceHashSha256);
                Add(target, period.EnteredAtWorldTick);
                Add(target, period.EnterReasonCode);
                Add(target, period.ExitThresholdPolicyRevision);
                Add(target, period.Revision);
                Add(target, period.PeriodStateHashSha256);
                Add(target, period.BaseRecoveryWorkDurationTicks);
                Add(target, period.EffectiveRecoveryWorkDurationTicks);
                Add(target, period.WorkDurationModifierTicks);
                AddStrings(target, period.CandidateStableIds);
            }
            Add(target, value.PeriodHistory.Length);
            foreach (var history in value.PeriodHistory.OrderBy(item =>
                         item.PeriodInstanceStableId, StringComparer.Ordinal))
            {
                Add(target, history.PlayerStableId);
                Add(target, history.PeriodInstanceStableId);
                Add(target, history.StateCode);
                Add(target, history.EnterTick);
                Add(target, history.ExitTick?.ToString(
                    CultureInfo.InvariantCulture) ?? string.Empty);
                AddStrings(target, history.MajorOutcomeRefs);
            }
            Add(target, value.PeriodTransitionEffects.Length);
            foreach (var effect in value.PeriodTransitionEffects.OrderBy(item =>
                         item.EffectStableId, StringComparer.Ordinal))
            {
                Add(target, effect.EffectStableId);
                Add(target, effect.EffectTypeCode);
                Add(target, effect.PlayerStableId);
                Add(target, effect.PeriodInstanceStableId);
                Add(target, effect.StateCode);
                Add(target, effect.AppliedWorldTick);
                Add(target, effect.SourceBalanceHashSha256);
            }
        }

        private static void AddAreaAccess(StringBuilder target,
            SimulationPlayerAreaAccessStateSnapshot value)
        {
            Add(target, "PlayerAreaAccessExtensionV1");
            Add(target, value.RuleRevision);
            Add(target, value.WorldRevision);
            Add(target, value.WorldTick);
            Add(target, value.CurrentAreaSetStableId);
            Add(target, value.MutatesStaticHDefinitions);
            Add(target, value.AccessEntries.Length);
            foreach (var entry in value.AccessEntries.OrderBy(item =>
                         item.AreaSetStableId, StringComparer.Ordinal))
            {
                Add(target, entry.PlayerStableId);
                Add(target, entry.AreaSetStableId);
                Add(target, entry.AccessLevelCode);
                Add(target, entry.AccessStateCode);
                AddStrings(target, entry.GrantedByEvidenceIds);
                Add(target, entry.GrantedAtWorldRevision);
                Add(target, entry.RevocationPolicyCode);
                Add(target, entry.SourceHDefinitionHashSha256);
                AddStrings(target, entry.AvailableWorldInteractionIds);
                Add(target, entry.Revision);
                Add(target, entry.AccessHashSha256);
            }
        }

        private static void AddHostedWorld(StringBuilder target,
            SimulationHostedWorldStateSnapshot value)
        {
            Add(target, "HostedWorldExtensionV1");
            Add(target, value.WorldRevision);
            Add(target, value.WorldTick);
            Add(target, value.HostedSessionStableId);
            Add(target, value.WorldStableId);
            Add(target, value.OwnerPlayerStableId);
            Add(target, value.SessionModeCode);
            Add(target, value.JoinPolicyCode);
            Add(target, value.DefaultGuestPermissionProfileCode);
            Add(target, value.Participants.Length);
            foreach (var participant in value.Participants.OrderBy(item =>
                         item.PlayerStableId, StringComparer.Ordinal))
            {
                Add(target, participant.PlayerStableId);
                Add(target, participant.ParticipantStateCode);
                Add(target, participant.CurrentAreaSetStableId);
                Add(target, participant.JoinedAtWorldRevision);
            }
            Add(target, value.PermissionGrants.Length);
            foreach (var grant in value.PermissionGrants.OrderBy(item =>
                         item.TargetPlayerStableId, StringComparer.Ordinal).ThenBy(item =>
                         item.ScopeStableId, StringComparer.Ordinal).ThenBy(item =>
                         item.CapabilityCode, StringComparer.Ordinal))
            {
                Add(target, grant.TargetPlayerStableId);
                Add(target, grant.ScopeStableId);
                Add(target, grant.CapabilityCode);
                Add(target, grant.GrantStateCode);
                Add(target, grant.ActionRiskPolicyCode);
                Add(target, grant.GrantedByPlayerStableId);
                Add(target, grant.Revision);
                Add(target, grant.GrantHashSha256);
            }
            Add(target, value.AuditTrail.Length);
            foreach (var audit in value.AuditTrail.OrderBy(item =>
                         item.EffectStableId, StringComparer.Ordinal))
            {
                Add(target, audit.EffectStableId);
                Add(target, audit.EffectTypeCode);
                Add(target, audit.ChangedByPlayerStableId);
                Add(target, audit.TargetPlayerStableId);
                Add(target, audit.ScopeStableId);
                Add(target, audit.CapabilityCode);
                Add(target, audit.AppliedWorldTick);
            }
            Add(target, value.PermissionRevision);
            Add(target, value.CreatedAtWorldRevision);
            Add(target, value.HostLossBlocksMutation);
            Add(target, value.EscPausesWorld);
            Add(target, value.SessionHashSha256);
            Add(target, value.SimulationOnly);
            Add(target, value.IsOperationalState);
        }

        private static void AddCoopConstruction(StringBuilder target,
            SimulationCoopConstructionStateSnapshot value)
        {
            Add(target, "CoopConstructionExtensionV1");
            Add(target, value.RuleRevision);
            Add(target, value.ProtectionRuleRevision);
            Add(target, value.WorldRevision);
            Add(target, value.WorldTick);
            Add(target, value.Projects.Length);
            foreach (var project in value.Projects.OrderBy(item =>
                         item.ProjectStableId, StringComparer.Ordinal))
            {
                Add(target, project.ProjectStableId);
                Add(target, project.BlueprintStableId);
                Add(target, project.BuildSiteH1StableId);
                Add(target, project.TargetFacilityStableId);
                Add(target, project.StageCode);
                Add(target, project.RequiredMaterialQuantity);
                Add(target, project.ContributedMaterialQuantity);
                Add(target, project.ProgressValue);
                Add(target, project.UnitCode);
                Add(target, project.Revision);
                Add(target, project.CompletedWorldTick ?? -1);
                AddStrings(target, project.OpenedCapabilityCodes);
                AddStrings(target, project.OpenedWorldInteractionIds);
                Add(target, project.ProjectHashSha256);
            }
            Add(target, value.Contributions.Length);
            foreach (var contribution in value.Contributions.OrderBy(item =>
                         item.ContributionStableId, StringComparer.Ordinal))
            {
                Add(target, contribution.ContributionStableId);
                Add(target, contribution.ProjectStableId);
                Add(target, contribution.PlayerStableId);
                Add(target, contribution.SourceLotStableId);
                Add(target, contribution.SourceLotRevisionBefore);
                Add(target, contribution.MaterialQuantity);
                Add(target, contribution.EffectiveWork);
                Add(target, contribution.UnitCode);
                Add(target, contribution.StateCode);
                Add(target, contribution.AppliedWorldTick);
            }
            Add(target, value.SourceLots.Length);
            foreach (var lot in value.SourceLots.OrderBy(item => item.LotStableId,
                         StringComparer.Ordinal))
            {
                Add(target, lot.LotStableId);
                Add(target, lot.Revision);
                Add(target, lot.ReservedQuantity);
                Add(target, lot.RemainingQuantity);
                Add(target, lot.UnitCode);
            }
            Add(target, value.ProtectionCheckpoints.Length);
            foreach (var checkpoint in value.ProtectionCheckpoints.OrderBy(item =>
                         item.CheckpointStableId, StringComparer.Ordinal))
            {
                Add(target, checkpoint.CheckpointStableId);
                Add(target, checkpoint.CheckpointKindCode);
                Add(target, checkpoint.WorldStableId);
                AddStrings(target, checkpoint.TargetStableIds);
                Add(target, checkpoint.BeforeWorldRevision);
                Add(target, checkpoint.SpatialStateHashSha256);
                AddStrings(target, checkpoint.RelatedResourceRefs);
                AddStrings(target, checkpoint.RelatedConnectorRefs);
                Add(target, checkpoint.CreatedByActionRequestId);
                Add(target, checkpoint.HistoricalEffectsDeleted);
            }
            Add(target, value.RestoreEffects.Length);
            foreach (var effect in value.RestoreEffects.OrderBy(item =>
                         item.EffectStableId, StringComparer.Ordinal))
            {
                Add(target, effect.EffectStableId);
                Add(target, effect.CheckpointStableId);
                Add(target, effect.TargetStableId);
                Add(target, effect.EffectTypeCode);
                Add(target, effect.AppliedWorldTick);
                Add(target, effect.DeletesHistoricalEffects);
                Add(target, effect.DuplicatesResources);
            }
            Add(target, value.UsesCompensatingEffects);
            Add(target, value.MutatesStaticHDefinitions);
            Add(target, value.StateHashSha256);
            Add(target, value.SimulationOnly);
            Add(target, value.IsOperationalState);
        }

        private static void AddRealityContext(StringBuilder target,
            SimulationRealityContextSnapshot value)
        {
            Add(target, "RealityContextExtensionV1");
            Add(target, value.SchemaVersion);
            Add(target, value.ContextSnapshotStableId);
            Add(target, value.ProfileStableId);
            Add(target, value.ProfileRevision);
            Add(target, value.SignalRuleRevision);
            Add(target, value.AreaSetStableId);
            Add(target, value.FrozenAtUtc.ToUniversalTime().ToString("O",
                CultureInfo.InvariantCulture));
            Add(target, value.AvailabilityCode);
            Add(target, value.InputHashSha256);
            Add(target, value.ChangesSimulationRules);
            Add(target, value.MovesSpatialDefinitions);
            Add(target, value.CreatesIncidentOrEffect);
            Add(target, value.SourceEvidence.Length);
            foreach (var source in value.SourceEvidence.OrderBy(item =>
                         item.SourceEvidenceStableId, StringComparer.Ordinal))
            {
                Add(target, source.SourceEvidenceStableId);
                Add(target, source.SourceName);
                Add(target, source.DatasetCode);
                Add(target, source.AvailabilityCode);
                Add(target, source.QualityCode);
                Add(target, source.FreshnessCode);
                Add(target, source.ObservedAtUtc?.ToUniversalTime().ToString("O",
                    CultureInfo.InvariantCulture) ?? string.Empty);
                Add(target, source.RetrievedAtUtc?.ToUniversalTime().ToString("O",
                    CultureInfo.InvariantCulture) ?? string.Empty);
                Add(target, source.SpatialPrecisionCode);
                AddStrings(target, source.UnitCodes);
                Add(target, source.SourceHashSha256);
                Add(target, source.LicenseCode);
                Add(target, source.SourceHref);
                AddStrings(target, source.LimitationCodes);
            }
            Add(target, value.SemanticSignals.Length);
            foreach (var signal in value.SemanticSignals.OrderBy(item =>
                         item.SignalStableId, StringComparer.Ordinal))
            {
                Add(target, signal.SignalStableId);
                Add(target, signal.SignalCode);
                Add(target, signal.SignalRuleRevision);
                AddStrings(target, signal.H3StableIds);
                AddStrings(target, signal.AdvisoryCodes);
                AddStrings(target, signal.SourceEvidenceStableIds);
            }
        }

        private static void AddWorldInventory(
            StringBuilder target,
            SimulationWorldInventorySnapshot value)
        {
            Add(target, "WorldInventoryExtensionV1");
            Add(target, value.SessionStableId);
            Add(target, value.WorldRevision);
            Add(target, value.WorldTick);
            Add(target, value.RuleRevision);
            Add(target, value.Buildings.Length);
            foreach (var item in value.Buildings)
            {
                Add(target, item.BuildingStableId);
                Add(target, item.TileKey);
                Add(target, item.RegionStableId);
                Add(target, item.BuildingEvidenceKindCode);
                Add(target, item.SourceRecordStableId);
                Add(target, item.InteriorSpaceStableId);
                Add(target, item.InteriorEvidenceKindCode);
            }
            Add(target, value.Containers.Length);
            foreach (var item in value.Containers)
            {
                Add(target, item.ContainerStableId);
                Add(target, item.BuildingStableId);
                Add(target, item.InteriorSpaceStableId);
                Add(target, item.AccessPolicyCode);
                Add(target, item.CapacityUnits);
                AddStrings(target, item.ManagerPlayerStableIds);
                Add(target, item.EvidenceKindCode);
            }
            Add(target, value.ContainerItemStacks.Length);
            foreach (var item in value.ContainerItemStacks)
            {
                Add(target, item.ItemStackStableId);
                Add(target, item.ContainerStableId);
                Add(target, item.ItemCode);
                Add(target, item.KoreanName);
                Add(target, item.Quantity);
                Add(target, item.UnitCode);
                Add(target, item.BuildingItemRelationStableId);
                Add(target, item.EvidenceKindCode);
            }
            Add(target, value.Players.Length);
            foreach (var player in value.Players)
            {
                Add(target, player.PlayerStableId);
                Add(target, player.CurrentBuildingStableId);
                Add(target, player.InventoryCapacityUnits);
                AddStrings(target, player.ManagedContainerStableIds);
                Add(target, player.Items.Length);
                foreach (var item in player.Items)
                {
                    Add(target, item.ItemCode);
                    Add(target, item.KoreanName);
                    Add(target, item.Quantity);
                    Add(target, item.UnitCode);
                }
            }
            Add(target, value.Transfers.Length);
            foreach (var item in value.Transfers)
            {
                Add(target, item.TransferStableId);
                Add(target, item.CommandId);
                Add(target, item.PlayerStableId);
                Add(target, item.BuildingStableId);
                Add(target, item.SourceContainerStableId);
                Add(target, item.SourceItemStackStableId);
                Add(target, item.ItemCode);
                Add(target, item.Quantity);
                Add(target, item.UnitCode);
                Add(target, item.AppliedWorldTick);
                Add(target, item.AppliedWorldRevision);
                Add(target, item.EvidenceKindCode);
                Add(target, item.SimulationOnly);
            }
            Add(target, value.SimulationOnly);
            Add(target, value.IsOperationalState);
        }

        private static void AddSurvivalTarot(
            StringBuilder target,
            SimulationSurvivalTarotStateSnapshot value)
        {
            Add(target, "SurvivalTarotExtensionV1");
            Add(target, value.SessionStableId);
            Add(target, value.RuleRevision);
            Add(target, value.WorldTick);
            Add(target, value.WorldRevision);
            Add(target, value.PeriodicIntervalTicks);
            Add(target, value.FoodCrisisThresholdPersonDays);
            Add(target, value.CurrentFoodReservePersonDays);
            if (value.FarmScopeConfigured)
            {
                Add(target, "FarmExitExtensionV1");
                Add(target, value.FarmExitThresholdPersonDays);
                Add(target, value.CurrentFarmFoodReservePersonDays);
                Add(target, value.RequiresExternalExpedition);
            }
            Add(target, value.CalendarRuleCode);
            Add(target, value.PendingOpportunity == null);
            if (value.PendingOpportunity != null)
                AddSurvivalTarotOpportunity(target, value.PendingOpportunity,
                    value.FarmScopeConfigured);
            Add(target, value.OpportunityHistory.Length);
            foreach (var opportunity in value.OpportunityHistory)
                AddSurvivalTarotOpportunity(target, opportunity,
                    value.FarmScopeConfigured);
            Add(target, value.ActiveModifierLines.Length);
            foreach (var line in value.ActiveModifierLines)
                AddTarotModifierLine(target, line);
            Add(target, value.SimulationOnly);
            Add(target, value.IsOperationalState);
        }

        private static void AddSurvivalTarotOpportunity(
            StringBuilder target,
            SimulationSurvivalTarotOpportunitySnapshot value,
            bool farmScopeConfigured)
        {
            Add(target, value.OpportunityStableId);
            Add(target, value.TriggerCode);
            Add(target, value.StatusCode);
            Add(target, value.TriggeredWorldTick);
            Add(target, value.FoodReservePersonDays);
            if (farmScopeConfigured)
            {
                Add(target, value.FarmFoodReservePersonDays);
                Add(target, value.RequiresExternalExpedition);
            }
            Add(target, value.SafeBuildingStableId);
            AddStrings(target, value.ParticipantPlayerStableIds);
            Add(target, value.Draw.DrawStableId);
            Add(target, value.Draw.DeckStableId);
            Add(target, value.Draw.DeckRevision);
            Add(target, value.Draw.DrawRuleRevision);
            Add(target, value.Draw.TurnNumber);
            Add(target, value.Draw.TurnHistoryHash);
            Add(target, value.Draw.Offers.Length);
            foreach (var offer in value.Draw.Offers)
            {
                Add(target, offer.OfferStableId);
                Add(target, offer.OfferSlotNumber);
                Add(target, offer.CardCopyStableId);
                Add(target, offer.OrientationCode);
                AddTurnCard(target, offer.Card);
            }
            Add(target, value.Responses.Length);
            foreach (var response in value.Responses)
            {
                Add(target, response.PlayerStableId);
                Add(target, response.OfferStableId);
                Add(target, response.RespondedWorldTick);
                Add(target, response.RespondedWorldRevision);
            }
            Add(target, value.SelectedOfferStableId);
            Add(target, value.ResolvedWorldTick?.ToString(CultureInfo.InvariantCulture)
                ?? string.Empty);
            Add(target, value.ResolvedWorldRevision?.ToString(CultureInfo.InvariantCulture)
                ?? string.Empty);
            Add(target, value.ModifierLines.Length);
            foreach (var line in value.ModifierLines)
                AddTarotModifierLine(target, line);
        }

        private static void AddTarotModifierLine(
            StringBuilder target,
            Simulation타로규칙보정선Snapshot value)
        {
            Add(target, value.ModifierLineStableId);
            Add(target, value.UpperRuleStableId);
            Add(target, value.UpperRuleRevision);
            Add(target, value.SourceCardStableId);
            Add(target, value.SourceCardRevision);
            Add(target, value.CardOrientationCode);
            Add(target, value.ResponseStableId);
            Add(target, value.TargetConnectionPointStableId);
            Add(target, value.TargetRuleDomainCode);
            Add(target, value.CompatibleLowerRuleStableId);
            Add(target, value.CompatibleLowerRuleRevision);
            Add(target, value.CalculationKindCode);
            Add(target, value.ModifierValue);
            Add(target, value.ModifierUnitCode);
            Add(target, value.MeaningCode);
            Add(target, value.ActiveFromTurnNumber);
            Add(target, value.ActiveThroughTurnNumber);
            Add(target, value.SourceTurnClosingStableId);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddSnapshot(StringBuilder target,
            경영SimulationSessionSnapshot value, bool includesTarotJourneyRoot)
        {
            Add(target, value.SessionStableId);
            Add(target, value.ClientRequestId.ToString("N"));
            Add(target, value.ScenarioStableId);
            Add(target, value.ScenarioDataRevision);
            Add(target, value.ScenarioSeed);
            Add(target, value.RuleRevision);
            Add(target, value.CurrentTick);
            Add(target, value.DurationTicks);
            Add(target, value.Revision);
            Add(target, value.IsCompleted);
            Add(target, value.ModeCode);
            Add(target, value.IsOperationalState);
            Add(target, value.WorldContext.FactionStableId);
            Add(target, value.WorldContext.TerritoryStableId);
            Add(target, value.WorldContext.SettlementStableId);
            Add(target, value.WorldContext.WorldTick);
            Add(target, value.WorldContext.WorldRevision);
            Add(target, value.WorldContext.GameDateStartsOn.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            Add(target, value.WorldContext.GameDate.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            Add(target, value.WorldContext.CalendarRuleCode);
            Add(target, value.Decisions.Length);
            foreach (var decision in value.Decisions)
                AddDecision(target, decision);
            Add(target, value.Tasks.Length);
            foreach (var task in value.Tasks)
                AddTask(target, task);
            Add(target, value.Effects.Length);
            foreach (var effect in value.Effects)
                AddEffect(target, effect);
            Add(target, value.LogisticsMovements.Length);
            foreach (var movement in value.LogisticsMovements)
                AddLogisticsMovement(target, movement);
            Add(target, value.FreightTransports.Length);
            foreach (var freight in value.FreightTransports)
                AddFreightTransport(target, freight);
            Add(target, value.GroupOrders.Length);
            foreach (var groupOrder in value.GroupOrders)
                AddGroupOrder(target, groupOrder);
            Add(target, value.FoodDeliveries.Length);
            foreach (var foodDelivery in value.FoodDeliveries)
                AddFoodDelivery(target, foodDelivery);
            Add(target, value.MarketConsumptions.Length);
            foreach (var consumption in value.MarketConsumptions)
                AddMarketConsumption(target, consumption);
            Add(target, value.IndividualOrders.Length);
            foreach (var order in value.IndividualOrders)
                AddIndividualOrder(target, order);
            Add(target, value.StockReservations.Length);
            foreach (var reservation in value.StockReservations)
                AddStockReservation(target, reservation);
            Add(target, value.ExportPreparations.Length);
            foreach (var preparation in value.ExportPreparations)
                Add수출준비(target, preparation);
            Add(target, value.ExportCargoPreparations.Length);
            foreach (var preparation in value.ExportCargoPreparations)
                Add수출Cargo준비(target, preparation);
            Add(target, value.ExportCargoHandoffs.Length);
            foreach (var handoff in value.ExportCargoHandoffs)
                Add수출Cargo인계(target, handoff);
            Add(target, value.ExportPortReceipts.Length);
            foreach (var receipt in value.ExportPortReceipts)
                Add수출항만인수(target, receipt);
            Add(target, value.ExportReadinessReviews.Length);
            foreach (var review in value.ExportReadinessReviews)
                Add수출준비성검토(target, review);
            Add(target, value.ExportShipmentPlans.Length);
            foreach (var plan in value.ExportShipmentPlans)
                Add수출선적계획(target, plan);
            Add(target, value.ExportShipmentExecutions.Length);
            foreach (var execution in value.ExportShipmentExecutions)
                Add수출선적실행(target, execution);
            if (value.TurnClosings.Length > 0 || value.ActiveTurnCardEffects.Length > 0)
            {
                Add(target, "TurnClosingExtensionV1");
                Add(target, value.TurnClosings.Length);
                foreach (var closing in value.TurnClosings)
                {
                    Add(target, closing.TurnClosingStableId);
                    Add(target, closing.ClosedTurnNumber);
                    Add(target, closing.ClosedGameDate.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
                    Add(target, closing.ResultingWorldTick);
                    Add(target, closing.ResultingRevision);
                    Add(target, closing.SelectedCards.Length);
                    foreach (var card in closing.SelectedCards)
                        AddTurnCard(target, card);
                }
                Add(target, value.ActiveTurnCardEffects.Length);
                foreach (var effect in value.ActiveTurnCardEffects)
                {
                    Add(target, effect.CardStableId);
                    Add(target, effect.CardRevision);
                    Add(target, effect.CardKindCode);
                    Add(target, effect.CardCopyStableId);
                    Add(target, effect.OfferStableId);
                    Add(target, effect.OrientationCode);
                    Add(target, effect.EffectCode);
                    Add(target, effect.TargetStatCode);
                    Add(target, effect.StatDelta);
                    Add(target, effect.ActiveTurnNumber);
                    Add(target, effect.SourceTurnClosingStableId);
                    Add(target, effect.SourceStableId);
                    Add(target, effect.RegionKey);
                    Add(target, effect.CalendarRevision);
                    Add(target, effect.EffectRuleRevision);
                    Add(target, effect.SourceUrl);
                    Add(target, effect.EvidenceCheckedAtUtc?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
                }
            }
            if (value.TarotContext != null
                && ((includesTarotJourneyRoot
                        && !string.IsNullOrWhiteSpace(
                            value.TarotContext.FrameSet.JourneyRoot.CardStableId))
                    || value.TarotContext.FrameSet.ActiveFrames.Length > 0
                    || value.TarotContext.Proposals.Length > 0
                    || value.TarotContext.IncidentEvaluations.Length > 0))
            {
                Add(target, includesTarotJourneyRoot
                    ? "TarotContextExtensionV2" : "TarotContextExtensionV1");
                Add(target, includesTarotJourneyRoot
                    ? 경영SimulationSessionAggregate
                        .BuildTarotContextStatePayloadKey(value.TarotContext)
                    : 경영SimulationSessionAggregate
                        .BuildLegacyTarotContextStatePayloadKey(value.TarotContext));
                Add(target, includesTarotJourneyRoot
                    ? value.TarotContext.ContextStateHashSha256
                    : 경영SimulationSessionAggregate
                        .BuildLegacyTarotContextStateHash(value.TarotContext));
            }
            AddNpcWorkforceSnapshot(target, value);
            AddSimulationSpatialSnapshot(target, value);
            AddSettlement(target, value.Settlement);
            if (value.FarmSurvival != null)
                Add(target, 경영SimulationSessionAggregate
                    .BuildFarmSurvivalStatePayloadKey(value.FarmSurvival));
            if (value.TeamRoleCards != null)
                Add(target, 경영SimulationSessionAggregate
                    .BuildTeamRoleCardStatePayloadKey(value.TeamRoleCards));
            if (value.Exploration != null)
                Add(target, 경영SimulationSessionAggregate
                    .BuildWorldExplorationStatePayloadKey(value.Exploration));
            if (value.CollectibleCardRewards != null)
                Add(target, 경영SimulationSessionAggregate
                    .BuildCollectibleCardRewardStatePayloadKey(
                        value.CollectibleCardRewards));
            if (value.RegionalIncidents.Length > 0
                || value.NatureThreat.Encounters.Length > 0)
            {
                Add(target, "RegionalIncidentExtensionV1");
                Add(target, value.RegionalIncidents.Length);
                foreach (var incident in value.RegionalIncidents.OrderBy(
                    item => item.IncidentStableId, StringComparer.Ordinal))
                {
                    Add(target, incident.IncidentStableId);
                    Add(target, incident.EventStableId);
                    Add(target, incident.IncidentRevision);
                    Add(target, incident.SourceInstanceStableId);
                    Add(target, incident.NatureRouteCode);
                    Add(target, incident.IncidentTypeCode);
                    Add(target, incident.StateCode);
                    Add(target, incident.OutcomeCode);
                    Add(target, incident.Severity);
                    Add(target, incident.RemainingSeverity);
                    Add(target, incident.OccurredWorldTick);
                    Add(target, incident.DeadlineWorldTick);
                    Add(target, incident.SourceTargetStableId);
                    Add(target, incident.FacilityStableId);
                    Add(target, incident.SelectedChoiceStableId);
                    AddStrings(target, incident.RequiredWorldInteractionIds);
                    AddStrings(target, incident.RequiredActionCodes);
                    AddStrings(target, incident.CompletedActionCodes);
                    AddStrings(target, incident.SourceStableIds);
                }
                Add(target, value.NatureThreat.Routes.Length);
                foreach (var route in value.NatureThreat.Routes.OrderBy(
                    item => item.NatureRouteCode, StringComparer.Ordinal))
                {
                    Add(target, route.NatureRouteCode);
                    Add(target, route.RootRemainingSeverity);
                    Add(target, route.GlobalSpilloverPressure);
                    Add(target, route.EffectivePressure);
                    Add(target, route.PressureLevelCode);
                    AddStrings(target, route.SourceIncidentStableIds);
                }
                Add(target, value.NatureThreat.Encounters.Length);
                foreach (var encounter in value.NatureThreat.Encounters.OrderBy(
                    item => item.EncounterStableId, StringComparer.Ordinal))
                {
                    Add(target, encounter.EncounterStableId);
                    Add(target, encounter.EncounterRevision);
                    Add(target, encounter.NatureRouteCode);
                    Add(target, encounter.StateCode);
                    Add(target, encounter.RiskBandCode);
                    Add(target, encounter.ThreatUnitCount);
                    Add(target, encounter.OccurredWorldTick);
                    Add(target, encounter.ResolvedWorldTick ?? -1);
                    AddStrings(target, encounter.SourceIncidentStableIds);
                    Add(target, encounter.PresentationKey);
                }
            }
        }

        private static void AddNpcWorkforceSnapshot(
            StringBuilder target,
            경영SimulationSessionSnapshot value)
        {
            if (value.NpcOrganizations.Length == 0
                && value.NpcActors.Length == 0
                && value.NpcCapabilityGrants.Length == 0
                && value.NpcWorkPolicies.Length == 0
                && value.NpcTaskAssignments.Length == 0
                && value.NpcWorkRecords.Length == 0
                && value.NpcActionProjections.Length == 0
                && value.NpcFacilityInventories.Length == 0)
                return;
            Add(target, "NpcWorkforceExtensionV1");
            Add(target, value.NpcOrganizations.Length);
            foreach (var organization in value.NpcOrganizations)
            {
                Add(target, organization.OrganizationStableId);
                Add(target, organization.DisplayName);
                AddStrings(target, organization.FacilityStableIds);
                AddStrings(target, organization.AllowedCapabilityCodes);
                AddStrings(target, organization.SourceStableIds);
            }
            Add(target, value.NpcActors.Length);
            foreach (var actor in value.NpcActors)
            {
                Add(target, actor.ActorStableId);
                Add(target, actor.OrganizationStableId);
                Add(target, actor.DisplayName);
                Add(target, actor.HomeFacilityStableId);
                Add(target, actor.ReferenceRoleCode);
                Add(target, actor.MaximumConcurrentTasks);
                AddStrings(target, actor.AssignableCapabilityCodes);
                Add(target, actor.Skills.Length);
                foreach (var skill in actor.Skills)
                {
                    Add(target, skill.CapabilityCode);
                    Add(target, skill.Score);
                }
                AddStrings(target, actor.SourceStableIds);
            }
            Add(target, value.NpcCapabilityGrants.Length);
            foreach (var grant in value.NpcCapabilityGrants)
            {
                Add(target, grant.GrantStableId);
                Add(target, grant.OrganizationStableId);
                Add(target, grant.ActorStableId);
                Add(target, grant.FacilityStableId);
                Add(target, grant.CapabilityCode);
                Add(target, grant.GrantedByActorStableId);
                Add(target, grant.GrantKindCode);
                Add(target, grant.CanDelegate);
                Add(target, grant.Active);
                Add(target, grant.GrantedTick);
                Add(target, grant.Revision);
                AddStrings(target, grant.SourceStableIds);
            }
            Add(target, value.NpcWorkPolicies.Length);
            foreach (var policy in value.NpcWorkPolicies)
            {
                Add(target, policy.PolicyStableId);
                Add(target, policy.OrganizationStableId);
                Add(target, policy.FacilityStableId);
                Add(target, policy.ActionCode);
                Add(target, policy.RequiredCapabilityCode);
                Add(target, policy.AutomationEnabled);
                Add(target, policy.Priority);
                Add(target, policy.PreferredActorStableId);
                Add(target, policy.AutoDelegationEnabled);
                Add(target, policy.AutoDelegationBacklogThreshold);
                Add(target, policy.TravelDurationTicks);
                Add(target, policy.WorkDurationTicks);
                Add(target, policy.InteractionPointKey);
                Add(target, policy.ActionVisualKey);
                Add(target, policy.Revision);
                AddStrings(target, policy.SourceStableIds);
            }
            Add(target, value.NpcTaskAssignments.Length);
            foreach (var assignment in value.NpcTaskAssignments)
            {
                Add(target, assignment.AssignmentStableId);
                Add(target, assignment.TaskStableId);
                Add(target, assignment.PolicyStableId);
                Add(target, assignment.OrganizationStableId);
                Add(target, assignment.FacilityStableId);
                Add(target, assignment.ActorStableId);
                Add(target, assignment.ActionCode);
                Add(target, assignment.RequiredCapabilityCode);
                Add(target, assignment.PhaseCode);
                Add(target, assignment.AssignedTick);
                Add(target, assignment.PhaseStartedTick);
                Add(target, assignment.TravelDurationTicks);
                Add(target, assignment.WorkDurationTicks);
                Add(target, assignment.CompletedTick?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                AddStrings(target, assignment.BlockReasonCodes);
                Add(target, assignment.Revision);
            }
            Add(target, value.NpcWorkRecords.Length);
            foreach (var record in value.NpcWorkRecords)
            {
                Add(target, record.WorkRecordStableId);
                Add(target, record.AssignmentStableId);
                Add(target, record.TaskStableId);
                Add(target, record.ActorStableId);
                Add(target, record.ActionCode);
                Add(target, record.FacilityStableId);
                Add(target, record.StartedTick);
                Add(target, record.CompletedTick);
                AddStrings(target, record.ResultCodes);
                AddStrings(target, record.SourceStableIds);
            }
            Add(target, value.NpcActionProjections.Length);
            foreach (var projection in value.NpcActionProjections)
            {
                Add(target, projection.ProjectionStableId);
                Add(target, projection.ActorStableId);
                Add(target, projection.TaskStableId);
                Add(target, projection.FacilityStableId);
                Add(target, projection.InteractionPointKey);
                Add(target, projection.ActionVisualKey);
                Add(target, projection.PhaseCode);
                Add(target, projection.ProgressRate);
                AddStrings(target, projection.BlockReasonCodes);
                Add(target, projection.Revision);
                Add(target, projection.WorldTick);
                Add(target, projection.PresentationOnly);
            }
            Add(target, value.NpcFacilityInventories.Length);
            foreach (var inventory in value.NpcFacilityInventories)
            {
                Add(target, inventory.InventoryStableId);
                Add(target, inventory.LotStableId);
                Add(target, inventory.FacilityStableId);
                if (!string.IsNullOrWhiteSpace(inventory.ProductStableId))
                    Add(target, inventory.ProductStableId);
                Add(target, inventory.StateCode);
                Add(target, inventory.Quantity);
                Add(target, inventory.UnitCode);
                Add(target, inventory.SourceTaskStableId);
                Add(target, inventory.UpdatedTick);
                Add(target, inventory.Revision);
                AddStrings(target, inventory.SourceStableIds);
            }
        }

        private static void AddRegionalCausality(
            StringBuilder target,
            SimulationRegionalCausalityStateSnapshot value)
        {
            Add(target, "RegionalCausalityExtensionV1");
            Add(target, value.Revision);
            Add(target, value.ThreatScore);
            Add(target, value.RecoveryScore);
            Add(target, value.NetPressureModifier);
            Add(target, value.OutcomeCode);
            Add(target, value.LastChangedWorldTick);
            Add(target, value.Changes.Length);
            foreach (var change in value.Changes.OrderBy(
                         item => item.ChangeStableId, StringComparer.Ordinal))
            {
                Add(target, change.ChangeStableId);
                Add(target, change.SourceCode);
                Add(target, change.ThreatDelta);
                Add(target, change.RecoveryDelta);
                Add(target, change.AppliedWorldTick);
                Add(target, change.SourceStableId);
                Add(target, change.NatureRouteCode);
            }
        }

        private static void AddSimulationSpatialSnapshot(
            StringBuilder target,
            경영SimulationSessionSnapshot value)
        {
            if (value.SpatialDefinitions.Length == 0
                && value.SpatialRuntimeStates.Length == 0
                && value.SpatialReservations.Length == 0)
                return;
            Add(target, "SimulationSpatialInteractionExtensionV1");
            Add(target, value.SpatialDefinitions.Length);
            foreach (var definition in value.SpatialDefinitions)
            {
                Add(target, definition.SpatialStableId);
                Add(target, definition.FacilityStableId);
                Add(target, definition.AreaStableId);
                Add(target, definition.AreaSetStableId);
                Add(target, definition.LandscapeGraphStableId);
                Add(target, definition.LandscapeNodeStableId);
                Add(target, definition.EvidenceKindCode);
                Add(target, definition.AccessStateCode);
                AddStrings(target, definition.CapabilityCodes);
                AddCapacities(target, definition.BaseCapacities);
                Add(target, definition.DefinitionRevision);
                Add(target, definition.DefinitionHashSha256);
                AddStrings(target, definition.SourceStableIds);
            }
            Add(target, value.SpatialRuntimeStates.Length);
            foreach (var runtime in value.SpatialRuntimeStates)
            {
                Add(target, runtime.SpatialStableId);
                Add(target, runtime.AccessStateCode);
                AddCapacities(target, runtime.OccupiedCapacities);
                AddCapacities(target, runtime.ReservedCapacities);
                AddStrings(target, runtime.ActiveTaskStableIds);
                Add(target, runtime.Revision);
            }
            Add(target, value.SpatialReservations.Length);
            var hasSpatialRoles = value.SpatialReservations.Any(reservation =>
                !string.IsNullOrWhiteSpace(reservation.RoleCode));
            if (hasSpatialRoles) Add(target, "SimulationSpatialReservationRolesV2");
            foreach (var reservation in value.SpatialReservations)
            {
                Add(target, reservation.ReservationStableId);
                Add(target, reservation.SpatialStableId);
                Add(target, reservation.TaskStableId);
                if (hasSpatialRoles) Add(target, reservation.RoleCode);
                Add(target, reservation.ReservationKindCode);
                Add(target, reservation.Quantity);
                Add(target, reservation.UnitCode);
                Add(target, reservation.StatusCode);
                Add(target, reservation.ReservedAtTick);
                Add(target, reservation.ConsumedAtTick ?? -1);
                Add(target, reservation.ReleasedAtTick ?? -1);
                Add(target, reservation.CreatedRevision);
                Add(target, reservation.FinalizedRevision ?? -1);
            }
        }

        private static void AddCapacities(
            StringBuilder target,
            Simulation공간용량Snapshot[] capacities)
        {
            Add(target, capacities.Length);
            foreach (var capacity in capacities)
            {
                Add(target, capacity.CapacityCode);
                Add(target, capacity.Quantity);
                Add(target, capacity.UnitCode);
            }
        }

        private static void AddTurnCard(StringBuilder target, SimulationTurnCardSnapshot card)
        {
            Add(target, card.CardStableId);
            Add(target, card.CardRevision);
            Add(target, card.CardKindCode);
            Add(target, card.CardCopyStableId);
            Add(target, card.OfferStableId);
            Add(target, card.OrientationCode);
            Add(target, card.Title);
            Add(target, card.Summary);
            Add(target, card.EffectTimingCode);
            Add(target, card.EffectCode);
            Add(target, card.TargetStatCode);
            Add(target, card.StatDelta);
            Add(target, card.SourceStableId);
            Add(target, card.RegionKey);
            Add(target, card.AvailableFromGameDate?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
            Add(target, card.AvailableThroughGameDate?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
            Add(target, card.CalendarRevision);
            Add(target, card.EffectRuleRevision);
            Add(target, card.SourceUrl);
            Add(target, card.EvidenceCheckedAtUtc?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
        }

        private static void Add수출준비(
            StringBuilder target,
            Simulation수출준비Snapshot value)
        {
            Add(target, value.PreparationStableId);
            Add(target, value.RootPreparationStableId);
            Add(target, value.PreviousPreparationStableId ?? string.Empty);
            Add(target, value.AttemptNumber);
            Add(target, value.IsReworkAttempt);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.PackingFacilityStableId);
            Add(target, value.HandoffFacilityStableId);
            Add(target, value.PackageLotCandidateStableId);
            Add(target, value.HandoffCandidateStableId);
            Add(target, value.InspectionOutcomeCode);
            Add(target, value.FailureReasonCode ?? string.Empty);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.PackagingTicks);
            Add(target, value.InspectionTicks);
            Add(target, value.ReservedTick);
            Add(target, value.PackagedTick ?? -1);
            Add(target, value.InspectedTick ?? -1);
            Add(target, value.HandoffCandidateReadyTick ?? -1);
            Add(target, value.CanRetry);
            Add(target, value.CargoPreparationStableId ?? string.Empty);
            Add(target, value.CargoStableId ?? string.Empty);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출Cargo준비(
            StringBuilder target,
            Simulation수출Cargo준비Snapshot value)
        {
            Add(target, value.CargoPreparationStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceExportPreparationStableId);
            Add(target, value.RootExportPreparationStableId);
            Add(target, value.ExportPreparationAttemptNumber);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.CargoStableId);
            Add(target, value.CargoRevision);
            Add(target, value.RouteStableId);
            Add(target, value.OriginFacilityStableId);
            Add(target, value.DestinationFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredPreparationTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.ReadyForHandoffTick ?? -1);
            Add(target, value.HandoffStableId ?? string.Empty);
            Add(target, value.HandoffCompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출Cargo인계(
            StringBuilder target,
            Simulation수출Cargo인계Snapshot value)
        {
            Add(target, value.HandoffStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceCargoPreparationStableId);
            Add(target, value.SourceExportPreparationStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.CargoStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.ReceivingFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredHandoffTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            Add(target, value.LogisticsMovementCargoStableId ?? string.Empty);
            Add(target, value.LogisticsMovementTaskStableId ?? string.Empty);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출항만인수(
            StringBuilder target,
            Simulation수출항만인수Snapshot value)
        {
            Add(target, value.ReceiptStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.CargoStableId);
            Add(target, value.SourceExportCargoHandoffStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.ReceivingFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredReceivingTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출선적실행(
            StringBuilder target,
            Simulation수출선적실행Snapshot value)
        {
            Add(target, value.ExecutionStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.OutcomeCode);
            Add(target, value.OutcomeRoll ?? -1);
            Add(target, value.SourceShipmentPlanStableId);
            Add(target, value.SourceReadinessReviewStableId);
            Add(target, value.SourcePortReceiptStableId);
            Add(target, value.CargoStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.DeliveredQuantity);
            Add(target, value.LostQuantity);
            Add(target, value.UnitCode);
            Add(target, value.DestinationCountryCode);
            Add(target, value.DestinationMarketStableId);
            Add(target, value.TransportModeCode);
            Add(target, value.ExecutionFacilityStableId);
            Add(target, value.EstimatedTransitTicks);
            Add(target, value.RiskScore);
            Add(target, value.SuccessProbabilityPercent);
            Add(target, value.ExpectedGrossRevenue);
            Add(target, value.ExpectedTotalCost);
            Add(target, value.PreviouslyRecognizedProjectedRevenue);
            Add(target, value.SuccessTreasuryDeltaCandidate);
            Add(target, value.LossTreasuryDeltaCandidate);
            Add(target, value.RequiredLossCapacityReservation);
            Add(target, value.AppliedTreasuryDelta ?? 0m);
            Add(target, value.TreasuryBeforeApplication ?? 0m);
            Add(target, value.TreasuryAfterApplication ?? 0m);
            Add(target, value.CurrencyCode);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.ScheduledTick);
            Add(target, value.DepartedTick ?? -1);
            Add(target, value.CompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출준비성검토(
            StringBuilder target,
            Simulation수출준비성검토Snapshot value)
        {
            Add(target, value.ReviewStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourcePortReceiptStableId);
            Add(target, value.ParentReviewStableId ?? string.Empty);
            Add(target, value.AttemptNumber);
            Add(target, value.CargoStableId);
            Add(target, value.SourceExportCargoHandoffStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.ReviewingFacilityStableId);
            Add(target, value.DocumentsPrepared);
            Add(target, value.InspectionPreparationReady);
            Add(target, value.OutcomeCode);
            AddStrings(target, value.MissingRequirementCodes);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredReviewTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출선적계획(
            StringBuilder target,
            Simulation수출선적계획Snapshot value)
        {
            Add(target, value.PlanStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceReadinessReviewStableId);
            Add(target, value.SourcePortReceiptStableId);
            Add(target, value.CargoStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.DestinationCountryCode);
            Add(target, value.DestinationMarketStableId);
            Add(target, value.TransportModeCode);
            Add(target, value.PlanningFacilityStableId);
            Add(target, value.ExpectedGrossRevenue);
            Add(target, value.ExpectedInternationalLogisticsCost);
            Add(target, value.ExpectedHandlingCost);
            Add(target, value.ExpectedOtherCost);
            Add(target, value.ExpectedTotalCost);
            Add(target, value.ExpectedNetRevenue);
            Add(target, value.CurrencyCode);
            Add(target, value.EstimatedTransitTicks);
            Add(target, value.RiskScore);
            Add(target, value.RiskLevelCode);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredPlanningTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            Add(target, value.ExecutionStableId ?? string.Empty);
            Add(target, value.ExecutionCompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddIndividualOrder(
            StringBuilder target,
            SimulationIndividualOrderSnapshot value)
        {
            Add(target, value.OrderStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.ActorStableId);
            Add(target, value.ProductStableId);
            Add(target, value.MarketFacilityStableId);
            Add(target, value.OrderedQuantity);
            Add(target, value.FulfilledQuantity);
            Add(target, value.UnitCode);
            Add(target, value.TotalPrice);
            Add(target, value.CurrencyCode);
            Add(target, value.RequiredLabor);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.CancellationTaskStableId ?? string.Empty);
            Add(target, value.ReservedTick);
            Add(target, value.ReadyForPickupTick.HasValue
                ? value.ReadyForPickupTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            if (value.PickedTick.HasValue || value.PackedTick.HasValue
                || value.PickupTaskStableId != null || value.FulfilledTick.HasValue)
            {
                Add(target, "IndividualOrderLifecycleV2");
                Add(target, value.ConfirmedTick);
                Add(target, value.StockReservedTick);
                Add(target, value.PickedTick?.ToString(CultureInfo.InvariantCulture)
                    ?? string.Empty);
                Add(target, value.PackedTick?.ToString(CultureInfo.InvariantCulture)
                    ?? string.Empty);
                Add(target, value.PickupDecisionStableId ?? string.Empty);
                Add(target, value.PickupTaskStableId ?? string.Empty);
                Add(target, value.FulfilledTick?.ToString(CultureInfo.InvariantCulture)
                    ?? string.Empty);
            }
            Add(target, value.ConsumptionDecisionStableId ?? string.Empty);
            Add(target, value.ConsumptionTaskStableId ?? string.Empty);
            Add(target, value.ConsumedTick.HasValue
                ? value.ConsumedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.CancelledTick.HasValue
                ? value.CancelledTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddStockReservation(
            StringBuilder target,
            SimulationStockReservationSnapshot value)
        {
            Add(target, value.ReservationStableId);
            Add(target, value.OrderStableId);
            Add(target, value.MarketFacilityStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.StateCode);
            Add(target, value.ReservedTick);
            Add(target, value.ConsumedTick.HasValue
                ? value.ConsumedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.ReleasedTick.HasValue
                ? value.ReleasedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddLogisticsMovement(
            StringBuilder target,
            SimulationLogisticsMovementSnapshot value)
        {
            Add(target, value.CargoStableId);
            Add(target, value.CargoRevision);
            Add(target, value.SourceExportCargoHandoffStableId ?? string.Empty);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.ReservedQuantity);
            Add(target, value.UnitCode);
            Add(target, value.RouteStableId);
            Add(target, value.OriginFacilityStableId);
            Add(target, value.DestinationFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredRouteTicks);
            Add(target, value.CompletedRouteTicks);
            Add(target, value.ReservedTick);
            Add(target, value.DepartedTick ?? -1);
            Add(target, value.ArrivedTick ?? -1);
            Add(target, value.DestinationStockCandidateStableId);
            Add(target, value.DestinationReceiptStableId ?? string.Empty);
            Add(target, value.DestinationReceiptCompletedTick ?? -1);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddFreightTransport(
            StringBuilder target,
            SimulationFreightTransportSnapshot value)
        {
            Add(target, value.TransportRequestStableId);
            Add(target, value.DispatchOfferStableId);
            Add(target, value.RequestStateCode);
            Add(target, value.DispatchStateCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.CargoStableId);
            Add(target, value.CarrierCandidateStableId);
            Add(target, value.VehicleStableId);
            Add(target, value.VehicleCapacity);
            Add(target, value.VehicleCapacityUnitCode);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.LogisticsTaskStableId);
            Add(target, value.ReceiptDecisionStableId ?? string.Empty);
            Add(target, value.ReceiptTaskStableId ?? string.Empty);
            Add(target, value.RequestedTick);
            Add(target, value.DispatchedTick ?? -1);
            Add(target, value.PickedUpTick ?? -1);
            Add(target, value.ArrivedAtDropoffTick ?? -1);
            Add(target, value.ReceivedTick ?? -1);
            Add(target, value.RuleRevision);
            AddFreightDispatchDecision(target, value.DispatchDecision);
            AddStrings(target, value.ExcludedOperationalEffectCodes);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.StateHistory.Length);
            foreach (var transition in value.StateHistory)
            {
                Add(target, transition.FromStateCode);
                Add(target, transition.ToStateCode);
                Add(target, transition.WorldTick);
                Add(target, transition.CauseStableId);
                Add(target, transition.RuleRevision);
            }
        }

        private static void AddFreightDispatchDecision(
            StringBuilder target,
            SimulationFreightDispatchDecisionSnapshot? value)
        {
            Add(target, value == null ? 0 : 1);
            if (value == null) return;
            Add(target, value.DispatchOfferStableId);
            Add(target, value.TransportRequestStableId);
            Add(target, value.RecommendedCarrierCandidateStableId ?? string.Empty);
            Add(target, value.SelectedCarrierCandidateStableId ?? string.Empty);
            Add(target, value.SelectedVehicleStableId ?? string.Empty);
            Add(target, value.RuleRevision);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.CandidateEvaluations.Length);
            foreach (var candidate in value.CandidateEvaluations)
            {
                Add(target, candidate.CarrierCandidateStableId);
                Add(target, candidate.VehicleStableId);
                Add(target, candidate.IsEligible ? 1 : 0);
                Add(target, candidate.IsRecommended ? 1 : 0);
                Add(target, candidate.IsSelected ? 1 : 0);
                Add(target, candidate.Rank);
                Add(target, candidate.PickupDistanceKm ?? decimal.MinValue);
                Add(target, candidate.VehicleCapacity);
                Add(target, candidate.VehicleCapacityUnitCode);
                Add(target, candidate.Reason);
                AddStrings(target, candidate.BlockReasonCodes);
                Add(target, candidate.Score.ScheduleScore);
                Add(target, candidate.Score.ProfitScore);
                Add(target, candidate.Score.DelayScore);
                Add(target, candidate.Score.DistanceScore);
                Add(target, candidate.Score.RecommendationTypeScore);
                Add(target, candidate.Score.CargoSensitivityScore);
                Add(target, candidate.Score.ReturnBurdenScore);
                Add(target, candidate.Score.BaseScore);
                Add(target, candidate.Score.DriverWaitingScore);
                Add(target, candidate.Score.TotalScore);
            }
        }

        private static void AddGroupOrder(StringBuilder target, Simulation같이주문Snapshot value)
        {
            Add(target, value.GroupOrderStableId);
            Add(target, value.ProductStableId);
            Add(target, value.DeliveryScopeStableId);
            Add(target, value.AggregationFacilityStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.ParticipantCount);
            Add(target, value.TotalQuantity);
            Add(target, value.UnitCode);
            Add(target, value.TargetParticipantCount);
            Add(target, value.TargetQuantity);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.CreatedTick);
            Add(target, value.FinalizedTick ?? -1);
            Add(target, value.RuleRevision);
            AddStrings(target, value.ExcludedOperationalEffectCodes);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.Intents.Length);
            foreach (var intent in value.Intents)
            {
                Add(target, intent.IntentStableId);
                Add(target, intent.ParticipantStableId);
                Add(target, intent.Quantity);
                Add(target, intent.UnitCode);
                Add(target, intent.ExplicitParticipationConsent);
                AddStrings(target, intent.SourceStableIds);
            }
        }

        private static void AddFoodDelivery(
            StringBuilder target,
            Simulation음식배달Snapshot value)
        {
            Add(target, value.FoodOrderStableId);
            Add(target, value.MenuItemStableId);
            Add(target, value.RestaurantFacilityStableId);
            Add(target, value.DestinationFacilityStableId);
            Add(target, value.DeliveryScopeStableId);
            Add(target, value.OrdererStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.PreparationDurationTicks);
            Add(target, value.DeliveryDurationTicks);
            Add(target, value.TotalDurationTicks);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.ReceiptDecisionStableId ?? string.Empty);
            Add(target, value.ReceiptTaskStableId ?? string.Empty);
            Add(target, value.AcceptedTick);
            Add(target, value.CookingStartedTick ?? -1);
            Add(target, value.ReadyForPickupTick ?? -1);
            Add(target, value.DispatchCandidateTick ?? -1);
            Add(target, value.PickedUpTick ?? -1);
            Add(target, value.DeliveredTick ?? -1);
            Add(target, value.ReceivedTick ?? -1);
            Add(target, value.RuleRevision);
            AddStrings(target, value.ExcludedOperationalEffectCodes);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.StateHistory.Length);
            foreach (var transition in value.StateHistory)
            {
                Add(target, transition.FromStateCode);
                Add(target, transition.ToStateCode);
                Add(target, transition.WorldTick);
                Add(target, transition.CauseStableId);
                Add(target, transition.RuleRevision);
            }
        }

        private static void AddMarketConsumption(
            StringBuilder target,
            Simulation시장소비Snapshot value)
        {
            Add(target, value.ConsumptionStableId);
            Add(target, value.OrderStableId);
            Add(target, value.ReservationStableId);
            Add(target, value.ActorStableId);
            Add(target, value.ProductStableId);
            Add(target, value.MarketFacilityStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.ScheduledTick);
            Add(target, value.ConsumedTick ?? -1);
            Add(target, value.MarketSupplyAfterOrderFulfillment);
            Add(target, value.MarketSupplyObservedAtConsumption.HasValue
                ? value.MarketSupplyObservedAtConsumption.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty);
            Add(target, value.AdditionalMarketSupplyDeductionApplied);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddDecision(StringBuilder target, SimulationDecisionSnapshot value)
        {
            Add(target, value.DecisionStableId);
            Add(target, value.DecisionTypeCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SessionStableId);
            Add(target, value.FactionStableId);
            Add(target, value.TerritoryStableId);
            Add(target, value.SettlementStableId);
            Add(target, value.ActorStableId);
            AddStrings(target, value.TargetStableIds);
            Add(target, value.CreatedTick);
            Add(target, value.ConfirmedTick.HasValue ? value.ConfirmedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddValues(target, value.ExpectedCosts);
            AddValues(target, value.ExpectedEffects);
            AddStrings(target, value.Uncertainties);
            AddStrings(target, value.BlockReasonCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddTask(StringBuilder target, SimulationTaskSnapshot value)
        {
            Add(target, value.TaskStableId);
            Add(target, value.TaskTypeCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.CausedByDecisionStableId);
            Add(target, value.FacilityStableId);
            if (!string.IsNullOrWhiteSpace(value.ActionCode)
                || !string.IsNullOrWhiteSpace(value.AssignedActorStableId))
            {
                Add(target, "NpcTaskBindingV1");
                Add(target, value.ActionCode);
                Add(target, value.AssignedActorStableId);
            }
            if (!string.IsNullOrWhiteSpace(value.SelectedSpatialStableId))
            {
                Add(target, "SimulationSpatialTaskBindingV1");
                Add(target, value.SelectedSpatialStableId);
                Add(target, value.SpatialDefinitionRevision);
                Add(target, value.SpatialDefinitionHashSha256);
            }
            if (value.SpatialRoleBindings.Length > 0)
            {
                Add(target, "SimulationSpatialTaskBindingsV2");
                Add(target, value.SpatialRoleBindings.Length);
                foreach (var binding in value.SpatialRoleBindings)
                {
                    Add(target, binding.RoleCode);
                    Add(target, binding.PreferredSpatialStableId);
                    Add(target, binding.SelectedSpatialStableId);
                    Add(target, binding.DefinitionRevision);
                    Add(target, binding.DefinitionHashSha256);
                    Add(target, binding.EvidenceKindCode);
                    AddStrings(target, binding.RequiredCapabilityCodes);
                    AddCapacities(target, binding.RequiredCapacities);
                    AddStrings(target, binding.BlockReasonCodes);
                }
            }
            Add(target, value.AssignedCapacity);
            Add(target, value.AssignedCapacityUnitCode);
            Add(target, value.ScheduledStartTick);
            Add(target, value.ExpectedEndTick);
            Add(target, value.ActualEndTick.HasValue ? value.ActualEndTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddStrings(target, value.InputLotStableIds);
            AddStrings(target, value.OutputCandidateCodes);
            AddStrings(target, value.BlockReasonCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddEffect(StringBuilder target, SimulationEffectRecord value)
        {
            Add(target, value.EffectStableId);
            Add(target, value.EffectTypeCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.AppliedTick.HasValue ? value.AppliedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.CausedByDecisionStableId);
            Add(target, value.CausedByTaskStableId);
            Add(target, value.TargetLedgerStableId);
            Add(target, value.BeforeValue);
            Add(target, value.Delta);
            Add(target, value.AfterValue);
            Add(target, value.UnitCode);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddSettlement(
            StringBuilder target,
            SimulationSettlementEconomySnapshot? value)
        {
            Add(target, value == null ? "none" : "present");
            if (value == null) return;
            Add(target, value.SettlementStableId);
            Add(target, value.WorldTick);
            Add(target, value.Revision);
            Add(target, value.RuleRevision);
            Add(target, value.TreasuryBalance);
            Add(target, value.TreasuryReserved);
            Add(target, value.TreasuryAvailable);
            Add(target, value.CurrencyCode);
            Add(target, value.LaborCapacityTotal);
            Add(target, value.LaborReserved);
            Add(target, value.LaborAvailable);
            Add(target, value.StorageCapacity);
            Add(target, value.StorageOccupied);
            Add(target, value.StorageReserved);
            Add(target, value.StorageAvailable);
            Add(target, value.StorageUnitCode);
            Add(target, value.PopulationCount);
            Add(target, value.PopulationFoodDemandPerTick);
            Add(target, value.GarrisonCount);
            Add(target, value.GarrisonFoodDemandPerTick);
            Add(target, value.FoodReserveEquivalent);
            Add(target, value.FoodDemandPerTick);
            Add(target, value.FoodSecurityDays);
            Add(target, value.FoodEquivalentUnitCode);
            Add(target, value.FoodEquivalentRuleRevision);
            Add(target, value.FoodSecurityFormulaCode);
            Add(target, value.Districts.Length);
            foreach (var district in value.Districts)
            {
                Add(target, district.DistrictStableId);
                Add(target, district.DistrictTypeCode);
                AddStrings(target, district.SourceStableIds);
            }
            Add(target, value.Facilities.Length);
            foreach (var facility in value.Facilities)
            {
                Add(target, facility.FacilityStableId);
                Add(target, facility.FacilityTypeCode);
                Add(target, facility.DistrictStableId);
                AddStrings(target, facility.SourceStableIds);
            }
            Add(target, value.MarketSupplyByProduct.Length);
            foreach (var supply in value.MarketSupplyByProduct)
            {
                Add(target, supply.ProductStableId);
                Add(target, supply.Quantity);
                Add(target, supply.UnitCode);
                AddStrings(target, supply.SourceStableIds);
            }
            Add(target, value.ResidentConsumptionByProduct.Length);
            foreach (var consumption in value.ResidentConsumptionByProduct)
            {
                Add(target, consumption.ProductStableId);
                Add(target, consumption.Quantity);
                Add(target, consumption.UnitCode);
                Add(target, consumption.ConsumptionCount);
                AddStrings(target, consumption.SourceStableIds);
            }
            Add(target, value.ReserveStockLots.Length);
            foreach (var lot in value.ReserveStockLots)
            {
                Add(target, lot.StockLotStableId);
                Add(target, lot.ProductStableId);
                Add(target, lot.StorageFacilityStableId);
                Add(target, lot.Quantity);
                Add(target, lot.OutboundReservedQuantity);
                Add(target, lot.AvailableQuantity);
                Add(target, lot.UnitCode);
                Add(target, lot.FoodEquivalentQuantity);
                Add(target, lot.OutboundReservedFoodEquivalentQuantity);
                Add(target, lot.AvailableFoodEquivalentQuantity);
                AddStrings(target, lot.SourceStableIds);
            }
            Add(target, value.HarvestLotAllocations.Length);
            foreach (var allocation in value.HarvestLotAllocations)
            {
                Add(target, allocation.AllocationStableId);
                Add(target, allocation.HarvestLotStableId);
                Add(target, allocation.HarvestLotRevision);
                Add(target, allocation.ProductStableId);
                Add(target, allocation.Quantity);
                Add(target, allocation.UnitCode);
                Add(target, allocation.ChoiceCode);
                Add(target, allocation.NextWorkflowCode);
                Add(target, allocation.DecisionStableId);
                Add(target, allocation.TaskStableId);
                Add(target, allocation.FacilityStableId);
                Add(target, allocation.RequiredLabor);
                Add(target, allocation.TreasuryCost);
                Add(target, allocation.ProjectedRevenue.HasValue
                    ? allocation.ProjectedRevenue.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                Add(target, allocation.StateCode);
                Add(target, allocation.ReservedTick);
                Add(target, allocation.AppliedTick.HasValue
                    ? allocation.AppliedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                Add(target, allocation.ReserveStockLotStableId ?? string.Empty);
                Add(target, allocation.StoredQuantity);
                Add(target, allocation.FoodEquivalentQuantity);
                Add(target, allocation.OutboundReservedQuantity);
                Add(target, allocation.AvailableQuantity);
                AddStrings(target, allocation.SourceStableIds);
            }
            AddStrings(target, value.ActiveTaskStableIds);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddValues(StringBuilder target, SimulationValueProjection[] values)
        {
            Add(target, values.Length);
            foreach (var value in values)
            {
                Add(target, value.ValueTypeCode);
                Add(target, value.TargetLedgerStableId);
                Add(target, value.BeforeValue);
                Add(target, value.Delta);
                Add(target, value.AfterValue);
                Add(target, value.UnitCode);
                AddStrings(target, value.SourceStableIds);
            }
        }

        private static void AddStrings(StringBuilder target, string[] values)
        {
            Add(target, values.Length);
            foreach (var value in values)
                Add(target, value);
        }

        private static void Add(StringBuilder target, object value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture));
            target.Append(':');
            target.Append(text);
            target.Append('|');
        }
    }
}
