using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// Solo LocalProcess가 HTTP나 Infrastructure assembly 없이 사용하는 전투 파생 원장이다.
    /// 운영 상태를 쓰지 않으며 Session 저장 시 전투 상태 사본을 함께 제공한다.
    /// </summary>
    internal sealed class LocalSimulationBattleInstanceStore
        : ISimulationBattleInstanceStore
    {
        private readonly object gate = new();
        private readonly ConcurrentDictionary<string, SimulationBattleInstanceState>
            values = new(StringComparer.Ordinal);

        public SimulationBattleInstanceState? Find(string battleStableId)
            => !string.IsNullOrWhiteSpace(battleStableId)
                && values.TryGetValue(battleStableId.Trim(), out var value)
                    ? value : null;

        public SimulationBattleInstanceState[] FindBySession(string sessionStableId)
            => values.Values.Where(value => value.SessionStableId ==
                    (sessionStableId ?? string.Empty).Trim())
                .OrderBy(value => value.BattleStableId, StringComparer.Ordinal)
                .ToArray();

        public bool CanReserve(string sessionStableId, string areaStableId,
            IEnumerable<string> resourceStableIds)
        {
            var requested = new HashSet<string>(resourceStableIds
                ?? Array.Empty<string>(), StringComparer.Ordinal);
            return FindBySession(sessionStableId).All(value =>
            {
                var snapshot = value.Snapshot();
                if (snapshot.PhaseCode == SimulationBattleInstanceCodes.Reconciled)
                    return true;
                if (snapshot.AreaStableId == (areaStableId ?? string.Empty).Trim())
                    return false;
                return !snapshot.ResourceReservations.Any(reservation =>
                        reservation.StateCode == SimulationBattleInstanceCodes.Reserved
                        && requested.Contains(reservation.ResourceStableId))
                    && !snapshot.ParticipationReservations.Any(reservation =>
                        reservation.StateCode ==
                            SimulationBattlefieldDerivationCodes.CommittedToBattle
                        && requested.Contains(reservation.ActorStableId));
            });
        }

        public SimulationBattleInstanceState CreateOrGet(
            SimulationBattleCreationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            lock (gate)
            {
                if (values.TryGetValue(context.BattleStableId, out var existing))
                {
                    var snapshot = existing.Snapshot();
                    if (snapshot.SessionStableId != context.SessionStableId
                        || snapshot.EncounterStableId != context.EncounterStableId
                        || snapshot.AreaStableId != context.AreaStableId
                        || snapshot.StartedWorldRevision != context.StartedWorldRevision)
                        throw new SimulationConflictException(
                            "SimulationBattleCreatePayloadConflict");
                    return existing;
                }
                if (!CanReserve(context.SessionStableId, context.AreaStableId,
                        context.InitialResourceStableIds))
                    throw new SimulationConflictException("BattleResourceLocked");
                var created = new SimulationBattleInstanceState(context);
                if (!values.TryAdd(context.BattleStableId, created))
                    throw new SimulationConflictException(
                        "SimulationBattleCreateConflict");
                return created;
            }
        }

        public SimulationBattleSaveRecordSnapshot[] CaptureSession(
            string sessionStableId)
            => FindBySession(sessionStableId).Select(value => value.CreateSaveRecord())
                .ToArray();

        public void RestoreSession(string sessionStableId,
            SimulationBattleSaveRecordSnapshot[] records)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId) || records == null)
                throw new SimulationContractException(
                    "SimulationBattleSaveRecordInvalid");
            var id = sessionStableId.Trim();
            var restored = records.Select(SimulationBattleInstanceState.Restore)
                .OrderBy(value => value.BattleStableId, StringComparer.Ordinal)
                .ToArray();
            if (restored.Any(value => value.SessionStableId != id)
                || restored.Select(value => value.BattleStableId)
                    .Distinct(StringComparer.Ordinal).Count() != restored.Length)
                throw new SimulationConflictException(
                    "SimulationBattleSaveSessionIdentityMismatch");
            lock (gate)
            {
                if (FindBySession(id).Length != 0
                    || restored.Any(value => values.ContainsKey(value.BattleStableId)))
                    throw new SimulationConflictException(
                        "SimulationBattleAlreadyActive");
                foreach (var state in restored)
                    if (!values.TryAdd(state.BattleStableId, state))
                        throw new SimulationConflictException(
                            "SimulationBattleRestoreConflict");
            }
        }
    }
}
