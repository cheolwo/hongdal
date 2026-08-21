using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const decimal NaturePeriodEntryThreshold = .80m;
        private const decimal NaturePeriodExitThreshold = .75m;
        private const int NaturePeriodBaseRecoveryWorkDurationTicks = 2;
        private readonly Dictionary<string, NaturePeriodPlayerState>
            naturePeriodPlayers = new Dictionary<string, NaturePeriodPlayerState>(
                StringComparer.Ordinal);
        private readonly List<SimulationNaturePeriodHistorySnapshot>
            naturePeriodHistory = new List<SimulationNaturePeriodHistorySnapshot>();
        private readonly List<SimulationNaturePeriodTransitionEffectSnapshot>
            naturePeriodTransitionEffects =
                new List<SimulationNaturePeriodTransitionEffectSnapshot>();

        private void InitializeNaturePeriodState(NatureMindPlayerState player)
        {
            var state = new NaturePeriodPlayerState
            {
                PlayerStableId = player.PlayerStableId,
                PeriodStateCode = SimulationNaturePeriodCodes.OrdinaryPeriod,
                PeriodInstanceStableId = BuildPeriodInstanceStableId(
                    player.PlayerStableId,
                    SimulationNaturePeriodCodes.OrdinaryPeriod, 0),
                EnterReasonCode = SimulationNaturePeriodCodes.OrdinaryFallbackReason,
                EnteredAtWorldTick = 0,
            };
            naturePeriodPlayers.Add(player.PlayerStableId, state);
            RefreshNaturePeriodState(player, 0);
        }

        private void RefreshNaturePeriodState(NatureMindPlayerState player,
            int worldTick)
        {
            var balance = CreateNatureMindBalanceSnapshot(player);
            var state = naturePeriodPlayers[player.PlayerStableId];
            state.SourceBalanceRevision = balance.Revision;
            state.SourceBalanceHashSha256 = balance.BalanceHashSha256;
            state.Revision++;

            if (state.PeriodStateCode == SimulationNaturePeriodCodes.GwangbokPeriod
                && balance.RecoveryShare < NaturePeriodExitThreshold)
                ExitSpecialPeriod(state, worldTick,
                    SimulationNaturePeriodCodes.GwangbokExitReason);
            else if (state.PeriodStateCode == SimulationNaturePeriodCodes.DarkAgePeriod
                     && balance.ThreatShare < NaturePeriodExitThreshold)
                ExitSpecialPeriod(state, worldTick,
                    SimulationNaturePeriodCodes.DarkAgeExitReason);

            if (state.PeriodStateCode == SimulationNaturePeriodCodes.OrdinaryPeriod)
            {
                if (balance.RecoveryShare >= NaturePeriodEntryThreshold)
                    EnterSpecialPeriod(state,
                        SimulationNaturePeriodCodes.GwangbokPeriod,
                        SimulationNaturePeriodCodes.GwangbokEntryReason, worldTick);
                else if (balance.ThreatShare >= NaturePeriodEntryThreshold)
                    EnterSpecialPeriod(state,
                        SimulationNaturePeriodCodes.DarkAgePeriod,
                        SimulationNaturePeriodCodes.DarkAgeEntryReason, worldTick);
            }
        }

        private void EnterSpecialPeriod(NaturePeriodPlayerState state,
            string periodStateCode, string reasonCode, int worldTick)
        {
            state.InstanceSequence++;
            state.PeriodStateCode = periodStateCode;
            state.PeriodInstanceStableId = BuildPeriodInstanceStableId(
                state.PlayerStableId, periodStateCode, state.InstanceSequence);
            state.EnteredAtWorldTick = worldTick;
            state.EnterReasonCode = reasonCode;
            state.Revision++;
            naturePeriodHistory.Add(new SimulationNaturePeriodHistorySnapshot
            {
                PlayerStableId = state.PlayerStableId,
                PeriodInstanceStableId = state.PeriodInstanceStableId,
                StateCode = periodStateCode,
                EnterTick = worldTick,
            });
            naturePeriodTransitionEffects.Add(new
                SimulationNaturePeriodTransitionEffectSnapshot
                {
                    EffectStableId = "period-effect:entered:"
                        + state.PeriodInstanceStableId,
                    EffectTypeCode = SimulationNaturePeriodCodes.EnteredEffect,
                    PlayerStableId = state.PlayerStableId,
                    PeriodInstanceStableId = state.PeriodInstanceStableId,
                    StateCode = periodStateCode,
                    AppliedWorldTick = worldTick,
                    SourceBalanceHashSha256 = state.SourceBalanceHashSha256,
                });
        }

        private void ExitSpecialPeriod(NaturePeriodPlayerState state,
            int worldTick, string reasonCode)
        {
            var exitedInstanceId = state.PeriodInstanceStableId;
            var exitedStateCode = state.PeriodStateCode;
            var history = naturePeriodHistory.Last(value =>
                value.PeriodInstanceStableId == exitedInstanceId);
            history.ExitTick = worldTick;
            history.MajorOutcomeRefs = natureMindEffects.Values
                .Where(value => value.PlayerStableId == state.PlayerStableId
                    && value.AppliedWorldTick >= history.EnterTick)
                .OrderBy(value => value.EffectStableId, StringComparer.Ordinal)
                .Select(value => value.SourceStableId).Distinct(StringComparer.Ordinal)
                .ToArray();
            naturePeriodTransitionEffects.Add(new
                SimulationNaturePeriodTransitionEffectSnapshot
                {
                    EffectStableId = "period-effect:exited:" + exitedInstanceId,
                    EffectTypeCode = SimulationNaturePeriodCodes.ExitedEffect,
                    PlayerStableId = state.PlayerStableId,
                    PeriodInstanceStableId = exitedInstanceId,
                    StateCode = exitedStateCode,
                    AppliedWorldTick = worldTick,
                    SourceBalanceHashSha256 = state.SourceBalanceHashSha256,
                });
            state.InstanceSequence++;
            state.PeriodStateCode = SimulationNaturePeriodCodes.OrdinaryPeriod;
            state.PeriodInstanceStableId = BuildPeriodInstanceStableId(
                state.PlayerStableId, SimulationNaturePeriodCodes.OrdinaryPeriod,
                state.InstanceSequence);
            state.EnteredAtWorldTick = worldTick;
            state.EnterReasonCode = reasonCode;
            state.Revision++;
        }

        private SimulationNaturePeriodStateSnapshot CreateNaturePeriodSnapshot(
            NaturePeriodPlayerState state)
        {
            var modifier = ResolveNaturePeriodWorkDurationModifier(
                state.PeriodStateCode);
            var candidates = state.PeriodStateCode switch
            {
                SimulationNaturePeriodCodes.GwangbokPeriod => new[]
                {
                    SimulationNaturePeriodCodes.GwangbokRevelationCandidate,
                },
                SimulationNaturePeriodCodes.DarkAgePeriod => new[]
                {
                    SimulationNaturePeriodCodes.DarkAgeRecoveryWorldInteraction,
                },
                _ => Array.Empty<string>(),
            };
            var stateHash = Sha256(string.Join("\u001e", new[]
            {
                SimulationNaturePeriodCodes.RuleRevision,
                state.PlayerStableId,
                state.PeriodStateCode,
                state.PeriodInstanceStableId,
                state.SourceBalanceRevision.ToString(CultureInfo.InvariantCulture),
                state.SourceBalanceHashSha256,
                state.EnteredAtWorldTick.ToString(CultureInfo.InvariantCulture),
                state.EnterReasonCode,
                SimulationNaturePeriodCodes.ExitThresholdPolicyRevision,
                state.Revision.ToString(CultureInfo.InvariantCulture),
                modifier.ToString(CultureInfo.InvariantCulture),
                string.Join("|", candidates),
            }));
            return new SimulationNaturePeriodStateSnapshot
            {
                PlayerStableId = state.PlayerStableId,
                PeriodStateCode = state.PeriodStateCode,
                PeriodInstanceStableId = state.PeriodInstanceStableId,
                SourceBalanceRevision = state.SourceBalanceRevision,
                SourceBalanceHashSha256 = state.SourceBalanceHashSha256,
                EnteredAtWorldTick = state.EnteredAtWorldTick,
                EnterReasonCode = state.EnterReasonCode,
                ExitThresholdPolicyRevision =
                    SimulationNaturePeriodCodes.ExitThresholdPolicyRevision,
                Revision = state.Revision,
                PeriodStateHashSha256 = stateHash,
                BaseRecoveryWorkDurationTicks =
                    NaturePeriodBaseRecoveryWorkDurationTicks,
                EffectiveRecoveryWorkDurationTicks = Math.Max(1,
                    NaturePeriodBaseRecoveryWorkDurationTicks + modifier),
                WorkDurationModifierTicks = modifier,
                CandidateStableIds = candidates,
            };
        }

        private int ResolveNaturePeriodRecoveryWorkDuration(string actorStableId)
        {
            if (natureMindCreationState == null)
                return 1;
            var playerId = naturePeriodPlayers.ContainsKey(actorStableId)
                ? actorStableId
                : naturePeriodPlayers.ContainsKey(
                    SimulationNatureMindCodes.DefaultPlayerStableId)
                    ? SimulationNatureMindCodes.DefaultPlayerStableId
                    : naturePeriodPlayers.Count == 1
                        ? naturePeriodPlayers.Keys.Single()
                        : throw new SimulationConflictException(
                            "SimulationNaturePeriodPlayerAmbiguous");
            return CreateNaturePeriodSnapshot(naturePeriodPlayers[playerId])
                .EffectiveRecoveryWorkDurationTicks;
        }

        private SimulationNaturePeriodStateSnapshot ResolveNaturePeriodForActor(
            string actorStableId)
        {
            var playerId = naturePeriodPlayers.ContainsKey(actorStableId)
                ? actorStableId
                : naturePeriodPlayers.ContainsKey(
                    SimulationNatureMindCodes.DefaultPlayerStableId)
                    ? SimulationNatureMindCodes.DefaultPlayerStableId
                    : naturePeriodPlayers.Count == 1
                        ? naturePeriodPlayers.Keys.Single()
                        : throw new SimulationConflictException(
                            "SimulationNaturePeriodPlayerAmbiguous");
            return CreateNaturePeriodSnapshot(naturePeriodPlayers[playerId]);
        }

        private static int ResolveNaturePeriodWorkDurationModifier(
            string periodStateCode) => periodStateCode switch
            {
                SimulationNaturePeriodCodes.GwangbokPeriod => -1,
                SimulationNaturePeriodCodes.DarkAgePeriod => 1,
                _ => 0,
            };

        private static string BuildPeriodInstanceStableId(string playerStableId,
            string periodStateCode, long sequence) => "period-instance:"
                + playerStableId + ":" + periodStateCode.ToLowerInvariant()
                + ":" + sequence.ToString(CultureInfo.InvariantCulture);

        private sealed class NaturePeriodPlayerState
        {
            public string PlayerStableId { get; set; } = string.Empty;
            public string PeriodStateCode { get; set; } = string.Empty;
            public string PeriodInstanceStableId { get; set; } = string.Empty;
            public long SourceBalanceRevision { get; set; }
            public string SourceBalanceHashSha256 { get; set; } = string.Empty;
            public int EnteredAtWorldTick { get; set; }
            public string EnterReasonCode { get; set; } = string.Empty;
            public long Revision { get; set; }
            public long InstanceSequence { get; set; }
        }
    }
}
