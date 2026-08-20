using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Infrastructure
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.Infrastructure,
        "활성 전투와 Simulation 자원 예약을 프로세스 수명 동안 보관한다.",
        StepKey = "infrastructure.battle-store",
        DependsOnStepKeys = new string[] { "domain.battle-state" },
        ExecutionStage = SsalddelCodeExecutionStage.Persistence,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 50,
        Boundary = "실제 창고 재고나 인력을 잠그지 않는 process-local Simulation 저장소다.")]
    public sealed class InMemorySimulationBattleInstanceStore
        : ISimulationBattleInstanceStore, ISimulationBattleResourceLockReader,
            ISimulationBattleReservationReader
    {
        private readonly object gate = new object();
        private readonly ConcurrentDictionary<string, SimulationBattleInstanceState> values =
            new ConcurrentDictionary<string, SimulationBattleInstanceState>(StringComparer.Ordinal);

        public SimulationBattleInstanceState? Find(string battleStableId)
            => !string.IsNullOrWhiteSpace(battleStableId)
                && values.TryGetValue(battleStableId.Trim(), out var value) ? value : null;

        public SimulationBattleInstanceState[] FindBySession(string sessionStableId)
            => values.Values.Where(value => value.SessionStableId == sessionStableId.Trim())
                .OrderBy(value => value.BattleStableId, StringComparer.Ordinal).ToArray();

        public bool CanReserve(string sessionStableId, string areaStableId,
            IEnumerable<string> resourceStableIds)
        {
            var requested = new HashSet<string>(resourceStableIds, StringComparer.Ordinal);
            return FindBySession(sessionStableId).All(value =>
            {
                var snapshot = value.Snapshot();
                if (snapshot.PhaseCode == SimulationBattleInstanceCodes.Reconciled) return true;
                if (snapshot.AreaStableId == areaStableId) return false;
                return !snapshot.ResourceReservations.Any(reservation =>
                    reservation.StateCode == SimulationBattleInstanceCodes.Reserved
                    && requested.Contains(reservation.ResourceStableId))
                    && !snapshot.ParticipationReservations.Any(reservation =>
                        reservation.StateCode ==
                            SimulationBattlefieldDerivationCodes.CommittedToBattle
                        && requested.Contains(reservation.ActorStableId));
            });
        }

        public bool IsLocked(string sessionStableId, string resourceStableId)
            => FindBySession(sessionStableId).Any(value =>
            {
                var snapshot = value.Snapshot();
                return snapshot.ResourceReservations.Any(reservation =>
                        reservation.ResourceStableId == resourceStableId.Trim()
                        && reservation.StateCode == SimulationBattleInstanceCodes.Reserved)
                    || snapshot.ParticipationReservations.Any(reservation =>
                        reservation.ActorStableId == resourceStableId.Trim()
                        && reservation.StateCode ==
                            SimulationBattlefieldDerivationCodes.CommittedToBattle);
            });

        public bool IsActorCommitted(string sessionStableId, string actorStableId)
            => FindBySession(sessionStableId).Any(value => value.Snapshot()
                .ParticipationReservations.Any(reservation =>
                    reservation.ActorStableId == actorStableId.Trim()
                    && reservation.StateCode ==
                        SimulationBattlefieldDerivationCodes.CommittedToBattle));

        public bool HasWorldTargetConflict(string sessionStableId,
            string worldEffectTargetStableId, string capabilityCode)
            => FindBySession(sessionStableId).Any(value => value.Snapshot()
                .WorldTargetReservations.Any(reservation =>
                    reservation.WorldEffectTargetStableId ==
                        worldEffectTargetStableId.Trim()
                    && reservation.StateCode ==
                        SimulationBattlefieldDerivationCodes.Reserved
                    && reservation.ConflictCapabilityCodes.Contains(
                        capabilityCode.Trim(), StringComparer.Ordinal)));

        public SimulationBattleInstanceState CreateOrGet(SimulationBattleCreationContext context)
        {
            lock (gate)
            {
                if (values.TryGetValue(context.BattleStableId, out var existing))
                {
                    var snapshot = existing.Snapshot();
                    if (snapshot.SessionStableId != context.SessionStableId
                        || snapshot.EncounterStableId != context.EncounterStableId
                        || snapshot.AreaStableId != context.AreaStableId
                        || snapshot.StartedWorldRevision != context.StartedWorldRevision)
                        throw new SimulationConflictException("SimulationBattleCreatePayloadConflict");
                    return existing;
                }
                if (!CanReserve(context.SessionStableId, context.AreaStableId,
                    context.InitialResourceStableIds))
                    throw new SimulationConflictException("BattleResourceLocked");
                var created = new SimulationBattleInstanceState(context);
                if (!values.TryAdd(context.BattleStableId, created))
                    throw new SimulationConflictException("SimulationBattleCreateConflict");
                return created;
            }
        }

        public SimulationBattleSaveRecordSnapshot[] CaptureSession(string sessionStableId)
            => FindBySession(sessionStableId)
                .Select(value => value.CreateSaveRecord()).ToArray();

        public void RestoreSession(string sessionStableId,
            SimulationBattleSaveRecordSnapshot[] records)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId) || records == null)
                throw new SimulationContractException("SimulationBattleSaveRecordInvalid");
            var id = sessionStableId.Trim();
            var restored = records.Select(SimulationBattleInstanceState.Restore)
                .OrderBy(value => value.BattleStableId, StringComparer.Ordinal).ToArray();
            if (restored.Any(value => value.SessionStableId != id)
                || restored.Select(value => value.BattleStableId)
                    .Distinct(StringComparer.Ordinal).Count() != restored.Length)
                throw new SimulationConflictException(
                    "SimulationBattleSaveSessionIdentityMismatch");

            lock (gate)
            {
                if (FindBySession(id).Length > 0
                    || restored.Any(value => values.ContainsKey(value.BattleStableId)))
                    throw new SimulationConflictException("SimulationBattleAlreadyActive");
                for (var left = 0; left < restored.Length; left++)
                for (var right = left + 1; right < restored.Length; right++)
                {
                    var first = restored[left].Snapshot();
                    var second = restored[right].Snapshot();
                    if (first.PhaseCode == SimulationBattleInstanceCodes.Reconciled
                        || second.PhaseCode == SimulationBattleInstanceCodes.Reconciled)
                        continue;
                    var firstResources = new HashSet<string>(first.ResourceReservations
                        .Where(value => value.StateCode == SimulationBattleInstanceCodes.Reserved)
                        .Select(value => value.ResourceStableId), StringComparer.Ordinal);
                    firstResources.UnionWith(first.ParticipationReservations
                        .Where(value => value.StateCode ==
                            SimulationBattlefieldDerivationCodes.CommittedToBattle)
                        .Select(value => value.ActorStableId));
                    if (first.AreaStableId == second.AreaStableId
                        || second.ResourceReservations.Any(value =>
                            value.StateCode == SimulationBattleInstanceCodes.Reserved
                            && firstResources.Contains(value.ResourceStableId))
                        || second.ParticipationReservations.Any(value =>
                            value.StateCode ==
                                SimulationBattlefieldDerivationCodes.CommittedToBattle
                            && firstResources.Contains(value.ActorStableId)))
                        throw new SimulationConflictException("BattleResourceLocked");
                }
                foreach (var battle in restored)
                    if (!values.TryAdd(battle.BattleStableId, battle))
                        throw new SimulationConflictException("SimulationBattleRestoreConflict");
            }
        }
    }
}
