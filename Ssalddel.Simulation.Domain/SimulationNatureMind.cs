using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const decimal NatureMindEnterThreshold = .51m;
        private const decimal NatureMindExitThreshold = .50m;
        private SimulationNatureMindInitialStateRequest? natureMindCreationState;
        private string natureMindInitialPayloadKey = string.Empty;
        private readonly Dictionary<string, NatureMindPlayerState> natureMindPlayers =
            new Dictionary<string, NatureMindPlayerState>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationMindImpactEffectSnapshot>
            natureMindEffects = new Dictionary<string,
                SimulationMindImpactEffectSnapshot>(StringComparer.Ordinal);

        private void InitializeNatureMind(SimulationNatureMindInitialStateRequest? request)
        {
            natureMindCreationState = CloneNatureMindInitialState(request);
            natureMindInitialPayloadKey = BuildNatureMindInitialPayloadKey(request);
            if (request == null) return;
            ValidateNatureMindInitialState(request);
            foreach (var player in request.Players.OrderBy(value =>
                         value.PlayerStableId, StringComparer.Ordinal))
            {
                var state = new NatureMindPlayerState
                {
                    PlayerStableId = player.PlayerStableId.Trim(),
                    RecoveryBaseOutput = player.RecoveryBaseOutput,
                    ThreatBaseOutput = player.ThreatBaseOutput,
                    InterpretationBandCode = SimulationNatureMindCodes.MixedBand,
                };
                state.InterpretationBandCode = ResolveInterpretationBand(state,
                    state.InterpretationBandCode);
                natureMindPlayers.Add(state.PlayerStableId, state);
                InitializeNaturePeriodState(state);
            }
        }

        public SimulationNatureMindStateSnapshot GetNatureMindState()
        {
            lock (gate)
            {
                EnsureNatureMindConfigured();
                return CreateNatureMindStateSnapshot();
            }
        }

        public SimulationNatureFarmInterpretationSnapshot GetNatureFarmInterpretation(
            string playerStableId)
        {
            RequireStableId(playerStableId, "SimulationNatureMindPlayerStableIdInvalid");
            lock (gate)
            {
                EnsureNatureMindConfigured();
                if (!natureMindPlayers.TryGetValue(playerStableId.Trim(), out var player))
                    throw new SimulationNotFoundException(
                        "SimulationNatureMindPlayerNotFound");
                var settlement = CreateSettlementSnapshot()
                    ?? throw new SimulationContractException(
                        "SimulationSettlementRequiredForNatureMind");
                var utilization = settlement.StorageCapacity <= 0m ? 0m
                    : decimal.Round(settlement.StorageOccupied
                        / settlement.StorageCapacity, 6,
                        MidpointRounding.AwayFromZero);
                var factHash = Sha256(string.Join("\u001e", new[]
                {
                    SimulationNatureMindCodes.FarmStorageFact,
                    Revision.ToString(CultureInfo.InvariantCulture),
                    settlement.StorageOccupied.ToString(CultureInfo.InvariantCulture),
                    settlement.StorageCapacity.ToString(CultureInfo.InvariantCulture),
                    settlement.StorageUnitCode,
                }));
                var balance = CreateNatureMindBalanceSnapshot(player);
                var inference = balance.InterpretationBandCode switch
                {
                    SimulationNatureMindCodes.RecoveryDominantBand => new[]
                    {
                        "FarmStorageOpportunity",
                        "남은 저장 여력으로 다음 수확을 준비할 수 있습니다.",
                        "NatureMood.Recovery",
                    },
                    SimulationNatureMindCodes.ThreatDominantBand => new[]
                    {
                        "FarmStorageRisk",
                        "저장 여력 소진 위험을 먼저 점검해야 합니다.",
                        "NatureMood.Threat",
                    },
                    _ => new[]
                    {
                        "FarmStorageMixed",
                        "현재 저장량과 남은 여력을 함께 확인하세요.",
                        "NatureMood.Mixed",
                    },
                };
                return new SimulationNatureFarmInterpretationSnapshot
                {
                    PlayerStableId = player.PlayerStableId,
                    WorldRevision = Revision,
                    FactStableId = SimulationNatureMindCodes.FarmStorageFact,
                    FactValue = utilization,
                    FactUnitCode = "ratio",
                    FactStateHashSha256 = factHash,
                    InferenceCode = inference[0],
                    InferenceText = inference[1],
                    MoodProjectionCode = inference[2],
                    PrioritizedCardStableIds = PrioritizedCards(
                        balance.InterpretationBandCode),
                    Balance = balance,
                    Period = CreateNaturePeriodSnapshot(
                        naturePeriodPlayers[player.PlayerStableId]),
                    ChangesSharedFact = false,
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        private void ApplyNatureMindImpactForHarvestDisposition(
            SimulationHarvestLotAllocationSnapshot allocation,
            int appliedWorldTick)
        {
            ApplyNatureMindImpactForAllPlayers(
                "mind-impact:farm-disposition:" + allocation.AllocationStableId
                + ":recovery",
                SimulationNatureMindCodes.FarmHarvestDispositionCompleted,
                allocation.AllocationStableId,
                SimulationNatureMindCodes.RecoveryAxis,
                2m,
                appliedWorldTick);
            ApplyNatureMindImpactForAllPlayers(
                "mind-impact:farm-disposition:" + allocation.AllocationStableId
                + ":threat-buffer",
                SimulationNatureMindCodes.FarmHarvestDispositionCompleted,
                allocation.AllocationStableId,
                SimulationNatureMindCodes.ThreatAxis,
                -1m,
                appliedWorldTick);
        }

        private void ApplyNatureMindImpactForRegionalCausality(
            string changeStableId,
            string sourceCode,
            int threatDelta,
            int recoveryDelta,
            int appliedWorldTick,
            string sourceStableId)
        {
            ApplyNatureMindImpactForAllPlayers(
                "mind-impact:regional:" + changeStableId + ":recovery",
                sourceCode, sourceStableId,
                SimulationNatureMindCodes.RecoveryAxis,
                recoveryDelta, appliedWorldTick);
            ApplyNatureMindImpactForAllPlayers(
                "mind-impact:regional:" + changeStableId + ":threat",
                sourceCode, sourceStableId,
                SimulationNatureMindCodes.ThreatAxis,
                threatDelta, appliedWorldTick);
        }

        private void ApplyNatureMindImpactForAllPlayers(
            string effectStableIdPrefix,
            string sourceCode,
            string sourceStableId,
            string axisCode,
            decimal magnitude,
            int appliedWorldTick)
        {
            if (natureMindCreationState == null || magnitude == 0m) return;
            foreach (var player in natureMindPlayers.Values.OrderBy(value =>
                         value.PlayerStableId, StringComparer.Ordinal))
            {
                var effectStableId = effectStableIdPrefix + ":" + player.PlayerStableId;
                if (natureMindEffects.ContainsKey(effectStableId)) continue;
                natureMindEffects.Add(effectStableId,
                    new SimulationMindImpactEffectSnapshot
                    {
                        EffectStableId = effectStableId,
                        PlayerStableId = player.PlayerStableId,
                        SourceCode = sourceCode,
                        SourceStableId = sourceStableId,
                        AxisCode = axisCode,
                        Magnitude = magnitude,
                        AppliedWorldTick = appliedWorldTick,
                        RuleRevision = natureMindCreationState.RuleRevision,
                    });
                player.Revision++;
                player.InterpretationBandCode = ResolveInterpretationBand(
                    player, player.InterpretationBandCode);
                RefreshNaturePeriodState(player, appliedWorldTick);
            }
        }

        private SimulationNatureMindStateSnapshot CreateNatureMindStateSnapshot()
            => new SimulationNatureMindStateSnapshot
            {
                RuleRevision = natureMindCreationState?.RuleRevision ?? string.Empty,
                Balances = natureMindPlayers.Values.OrderBy(value =>
                        value.PlayerStableId, StringComparer.Ordinal)
                    .Select(CreateNatureMindBalanceSnapshot).ToArray(),
                Effects = natureMindEffects.Values.OrderBy(value =>
                        value.EffectStableId, StringComparer.Ordinal)
                    .Select(CloneMindImpact).ToArray(),
                Periods = naturePeriodPlayers.Values.OrderBy(value =>
                        value.PlayerStableId, StringComparer.Ordinal)
                    .Select(CreateNaturePeriodSnapshot).ToArray(),
                PeriodHistory = naturePeriodHistory.OrderBy(value =>
                        value.PeriodInstanceStableId, StringComparer.Ordinal)
                    .Select(CloneNaturePeriodHistory).ToArray(),
                PeriodTransitionEffects = naturePeriodTransitionEffects
                    .OrderBy(value => value.EffectStableId, StringComparer.Ordinal)
                    .Select(CloneNaturePeriodTransitionEffect).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private SimulationNatureMindBalanceSnapshot CreateNatureMindBalanceSnapshot(
            NatureMindPlayerState player)
        {
            var effects = natureMindEffects.Values.Where(value =>
                    value.PlayerStableId == player.PlayerStableId).ToArray();
            var recovery = Math.Max(0m, player.RecoveryBaseOutput
                + effects.Where(value => value.AxisCode
                        == SimulationNatureMindCodes.RecoveryAxis)
                    .Sum(value => value.Magnitude));
            var threat = Math.Max(0m, player.ThreatBaseOutput
                + effects.Where(value => value.AxisCode
                        == SimulationNatureMindCodes.ThreatAxis)
                    .Sum(value => value.Magnitude));
            var total = recovery + threat;
            var recoveryShare = total == 0m ? .5m
                : decimal.Round(recovery / total, 6,
                    MidpointRounding.AwayFromZero);
            var threatShare = total == 0m ? .5m : 1m - recoveryShare;
            var recoveryContributors = Contributors(effects,
                SimulationNatureMindCodes.RecoveryAxis);
            var threatContributors = Contributors(effects,
                SimulationNatureMindCodes.ThreatAxis);
            var hash = Sha256(string.Join("\u001e", new[]
            {
                natureMindCreationState?.RuleRevision ?? string.Empty,
                player.PlayerStableId,
                recovery.ToString(CultureInfo.InvariantCulture),
                threat.ToString(CultureInfo.InvariantCulture),
                recoveryShare.ToString(CultureInfo.InvariantCulture),
                threatShare.ToString(CultureInfo.InvariantCulture),
                player.InterpretationBandCode,
                player.Revision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", effects.OrderBy(value => value.EffectStableId,
                    StringComparer.Ordinal).Select(value => string.Join("~", new[]
                    {
                        value.EffectStableId,
                        value.AxisCode,
                        value.Magnitude.ToString(CultureInfo.InvariantCulture),
                        value.AppliedWorldTick.ToString(CultureInfo.InvariantCulture),
                    }))),
            }));
            return new SimulationNatureMindBalanceSnapshot
            {
                PlayerStableId = player.PlayerStableId,
                RecoveryOutput = recovery,
                ThreatOutput = threat,
                RecoveryShare = recoveryShare,
                ThreatShare = threatShare,
                InterpretationStrength = decimal.Round(
                    Math.Abs(recoveryShare - .5m) * 2m, 6,
                    MidpointRounding.AwayFromZero),
                InterpretationBandCode = player.InterpretationBandCode,
                TopRecoveryContributors = recoveryContributors,
                TopThreatContributors = threatContributors,
                Revision = player.Revision,
                BalanceHashSha256 = hash,
            };
        }

        private string ResolveInterpretationBand(NatureMindPlayerState player,
            string currentBand)
        {
            var effects = natureMindEffects.Values.Where(value =>
                    value.PlayerStableId == player.PlayerStableId).ToArray();
            var recovery = Math.Max(0m, player.RecoveryBaseOutput
                + effects.Where(value => value.AxisCode
                        == SimulationNatureMindCodes.RecoveryAxis)
                    .Sum(value => value.Magnitude));
            var threat = Math.Max(0m, player.ThreatBaseOutput
                + effects.Where(value => value.AxisCode
                        == SimulationNatureMindCodes.ThreatAxis)
                    .Sum(value => value.Magnitude));
            var total = recovery + threat;
            var recoveryShare = total == 0m ? .5m : recovery / total;
            var threatShare = total == 0m ? .5m : threat / total;
            if (currentBand == SimulationNatureMindCodes.RecoveryDominantBand
                && recoveryShare >= NatureMindExitThreshold)
                return currentBand;
            if (currentBand == SimulationNatureMindCodes.ThreatDominantBand
                && threatShare >= NatureMindExitThreshold)
                return currentBand;
            if (recoveryShare > NatureMindEnterThreshold)
                return SimulationNatureMindCodes.RecoveryDominantBand;
            if (threatShare > NatureMindEnterThreshold)
                return SimulationNatureMindCodes.ThreatDominantBand;
            return SimulationNatureMindCodes.MixedBand;
        }

        private static SimulationNatureMindContributorSnapshot[] Contributors(
            IEnumerable<SimulationMindImpactEffectSnapshot> effects,
            string axisCode) => effects
            .Where(value => value.AxisCode == axisCode && value.Magnitude != 0m)
            .OrderByDescending(value => Math.Abs(value.Magnitude))
            .ThenBy(value => value.EffectStableId, StringComparer.Ordinal)
            .Take(3)
            .Select(value => new SimulationNatureMindContributorSnapshot
            {
                EffectStableId = value.EffectStableId,
                SourceCode = value.SourceCode,
                SourceStableId = value.SourceStableId,
                Magnitude = value.Magnitude,
            }).ToArray();

        private static string[] PrioritizedCards(string bandCode)
            => bandCode switch
            {
                SimulationNatureMindCodes.RecoveryDominantBand => new[]
                {
                    "card:mind.farm-opportunity",
                    "card:mind.shared-fact",
                },
                SimulationNatureMindCodes.ThreatDominantBand => new[]
                {
                    "card:mind.farm-risk-readiness",
                    "card:mind.shared-fact",
                },
                _ => new[]
                {
                    "card:mind.shared-fact",
                    "card:mind.check-both-signals",
                },
            };

        private void EnsureNatureMindConfigured()
        {
            if (natureMindCreationState == null)
                throw new SimulationNotFoundException(
                    "SimulationNatureMindNotConfigured");
        }

        private static void ValidateNatureMindInitialState(
            SimulationNatureMindInitialStateRequest request)
        {
            if (!string.Equals(request.RuleRevision,
                    SimulationNatureMindCodes.RuleRevision, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationNatureMindRuleRevisionUnsupported");
            if (request.Players == null || request.Players.Length == 0)
                throw new SimulationContractException(
                    "SimulationNatureMindPlayersMissing");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var player in request.Players)
            {
                RequireStableId(player.PlayerStableId,
                    "SimulationNatureMindPlayerStableIdInvalid");
                if (!ids.Add(player.PlayerStableId.Trim()))
                    throw new SimulationContractException(
                        "SimulationNatureMindPlayerStableIdDuplicate");
                if (player.RecoveryBaseOutput < 0m || player.ThreatBaseOutput < 0m
                    || player.RecoveryBaseOutput > 1000m
                    || player.ThreatBaseOutput > 1000m)
                    throw new SimulationContractException(
                        "SimulationNatureMindBaseOutputInvalid");
            }
        }

        internal static string BuildNatureMindInitialPayloadKey(
            SimulationNatureMindInitialStateRequest? request)
        {
            if (request == null) return string.Empty;
            return string.Join("\u001e", new[]
            {
                request.RuleRevision ?? string.Empty,
                string.Join("|", (request.Players ?? Array.Empty<
                        SimulationNatureMindPlayerInitialStateRequest>())
                    .OrderBy(value => value.PlayerStableId, StringComparer.Ordinal)
                    .Select(value => string.Join("~", new[]
                    {
                        value.PlayerStableId?.Trim() ?? string.Empty,
                        value.RecoveryBaseOutput.ToString(CultureInfo.InvariantCulture),
                        value.ThreatBaseOutput.ToString(CultureInfo.InvariantCulture),
                    }))),
            });
        }

        internal static SimulationNatureMindInitialStateRequest?
            CloneNatureMindInitialState(SimulationNatureMindInitialStateRequest? source)
            => source == null ? null : new SimulationNatureMindInitialStateRequest
            {
                RuleRevision = source.RuleRevision,
                Players = (source.Players ?? Array.Empty<
                        SimulationNatureMindPlayerInitialStateRequest>())
                    .Select(value => new SimulationNatureMindPlayerInitialStateRequest
                    {
                        PlayerStableId = value.PlayerStableId,
                        RecoveryBaseOutput = value.RecoveryBaseOutput,
                        ThreatBaseOutput = value.ThreatBaseOutput,
                    }).ToArray(),
            };

        internal static SimulationNatureMindStateSnapshot CloneNatureMindState(
            SimulationNatureMindStateSnapshot? source)
        {
            source ??= new SimulationNatureMindStateSnapshot();
            return new SimulationNatureMindStateSnapshot
            {
                RuleRevision = source.RuleRevision,
                Balances = (source.Balances ?? Array.Empty<
                        SimulationNatureMindBalanceSnapshot>())
                    .Select(CloneNatureMindBalance).ToArray(),
                Effects = (source.Effects ?? Array.Empty<
                        SimulationMindImpactEffectSnapshot>())
                    .Select(CloneMindImpact).ToArray(),
                Periods = (source.Periods ?? Array.Empty<
                        SimulationNaturePeriodStateSnapshot>())
                    .Select(CloneNaturePeriod).ToArray(),
                PeriodHistory = (source.PeriodHistory ?? Array.Empty<
                        SimulationNaturePeriodHistorySnapshot>())
                    .Select(CloneNaturePeriodHistory).ToArray(),
                PeriodTransitionEffects = (source.PeriodTransitionEffects
                        ?? Array.Empty<SimulationNaturePeriodTransitionEffectSnapshot>())
                    .Select(CloneNaturePeriodTransitionEffect).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };
        }

        internal static SimulationNatureMindBalanceSnapshot CloneNatureMindBalance(
            SimulationNatureMindBalanceSnapshot source) => new()
        {
            PlayerStableId = source.PlayerStableId,
            RecoveryOutput = source.RecoveryOutput,
            ThreatOutput = source.ThreatOutput,
            RecoveryShare = source.RecoveryShare,
            ThreatShare = source.ThreatShare,
            InterpretationStrength = source.InterpretationStrength,
            InterpretationBandCode = source.InterpretationBandCode,
            TopRecoveryContributors = source.TopRecoveryContributors.Select(value =>
                new SimulationNatureMindContributorSnapshot
                {
                    EffectStableId = value.EffectStableId,
                    SourceCode = value.SourceCode,
                    SourceStableId = value.SourceStableId,
                    Magnitude = value.Magnitude,
                }).ToArray(),
            TopThreatContributors = source.TopThreatContributors.Select(value =>
                new SimulationNatureMindContributorSnapshot
                {
                    EffectStableId = value.EffectStableId,
                    SourceCode = value.SourceCode,
                    SourceStableId = value.SourceStableId,
                    Magnitude = value.Magnitude,
                }).ToArray(),
            Revision = source.Revision,
            BalanceHashSha256 = source.BalanceHashSha256,
        };

        private static SimulationMindImpactEffectSnapshot CloneMindImpact(
            SimulationMindImpactEffectSnapshot source) => new()
        {
            EffectStableId = source.EffectStableId,
            PlayerStableId = source.PlayerStableId,
            SourceCode = source.SourceCode,
            SourceStableId = source.SourceStableId,
            AxisCode = source.AxisCode,
            Magnitude = source.Magnitude,
            AppliedWorldTick = source.AppliedWorldTick,
            RuleRevision = source.RuleRevision,
        };

        private static SimulationNaturePeriodStateSnapshot CloneNaturePeriod(
            SimulationNaturePeriodStateSnapshot source) => new()
        {
            PlayerStableId = source.PlayerStableId,
            PeriodStateCode = source.PeriodStateCode,
            PeriodInstanceStableId = source.PeriodInstanceStableId,
            SourceBalanceRevision = source.SourceBalanceRevision,
            SourceBalanceHashSha256 = source.SourceBalanceHashSha256,
            EnteredAtWorldTick = source.EnteredAtWorldTick,
            EnterReasonCode = source.EnterReasonCode,
            ExitThresholdPolicyRevision = source.ExitThresholdPolicyRevision,
            Revision = source.Revision,
            PeriodStateHashSha256 = source.PeriodStateHashSha256,
            BaseRecoveryWorkDurationTicks = source.BaseRecoveryWorkDurationTicks,
            EffectiveRecoveryWorkDurationTicks =
                source.EffectiveRecoveryWorkDurationTicks,
            WorkDurationModifierTicks = source.WorkDurationModifierTicks,
            CandidateStableIds = source.CandidateStableIds.ToArray(),
        };

        private static SimulationNaturePeriodHistorySnapshot CloneNaturePeriodHistory(
            SimulationNaturePeriodHistorySnapshot source) => new()
        {
            PlayerStableId = source.PlayerStableId,
            PeriodInstanceStableId = source.PeriodInstanceStableId,
            StateCode = source.StateCode,
            EnterTick = source.EnterTick,
            ExitTick = source.ExitTick,
            MajorOutcomeRefs = source.MajorOutcomeRefs.ToArray(),
        };

        private static SimulationNaturePeriodTransitionEffectSnapshot
            CloneNaturePeriodTransitionEffect(
                SimulationNaturePeriodTransitionEffectSnapshot source) => new()
            {
                EffectStableId = source.EffectStableId,
                EffectTypeCode = source.EffectTypeCode,
                PlayerStableId = source.PlayerStableId,
                PeriodInstanceStableId = source.PeriodInstanceStableId,
                StateCode = source.StateCode,
                AppliedWorldTick = source.AppliedWorldTick,
                SourceBalanceHashSha256 = source.SourceBalanceHashSha256,
            };

        private static string Sha256(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private sealed class NatureMindPlayerState
        {
            public string PlayerStableId { get; set; } = string.Empty;
            public decimal RecoveryBaseOutput { get; set; }
            public decimal ThreatBaseOutput { get; set; }
            public string InterpretationBandCode { get; set; } = string.Empty;
            public long Revision { get; set; }
        }
    }
}
