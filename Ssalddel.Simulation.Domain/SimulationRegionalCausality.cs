using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const int MaximumRegionalCausalityScore = 12;
        private int regionalThreatScore;
        private int regionalRecoveryScore;
        private long regionalCausalityRevision;
        private int regionalCausalityLastChangedWorldTick;
        private bool regionalCausalitySchemaEnabled = true;
        private readonly List<SimulationRegionalCausalityChangeSnapshot>
            regionalCausalityChanges = new List<SimulationRegionalCausalityChangeSnapshot>();
        private readonly HashSet<string> appliedRegionalCausalityChangeIds =
            new HashSet<string>(StringComparer.Ordinal);

        internal void UseLegacyRegionalCausalityRules() =>
            regionalCausalitySchemaEnabled = false;

        internal void ObserveRegionalCausalityTaskCompletion(
            SimulationTaskSnapshot task,
            int completedWorldTick)
        {
            if (task.ActionCode == SimulationNatureInteractionCodes.NatureRestoration)
            {
                ApplyRegionalCausality(
                    "regional-causality:nature-restoration:" + task.TaskStableId,
                    SimulationRegionalIncidentCodes.NatureRestorationCompleted,
                    -1, 1, completedWorldTick, task.TaskStableId,
                    FindNatureRouteCode(task));
            }
            else if (task.ActionCode == SimulationNatureInteractionCodes.PartyRecovery)
            {
                ApplyRegionalCausality(
                    "regional-causality:nature-party-recovery:" + task.TaskStableId,
                    SimulationRegionalIncidentCodes.NaturePartyRecoveryCompleted,
                    -1, 1, completedWorldTick, task.TaskStableId,
                    FindNatureRouteCode(task));
            }
        }

        internal void ObserveRegionalCausalityTurnCards(
            SimulationTurnClosingSnapshot closing)
        {
            foreach (var card in closing.SelectedCards)
            {
                var amount = Math.Max(1, Math.Abs(card.StatDelta));
                var sourceId = string.IsNullOrWhiteSpace(card.OfferStableId)
                    ? card.CardStableId : card.OfferStableId;
                if (card.OrientationCode == Simulation타로카드방향Codes.Reversed)
                {
                    ApplyRegionalCausality(
                        "regional-causality:turn-card:" + closing.TurnClosingStableId
                        + ":" + sourceId,
                        SimulationRegionalIncidentCodes.ReversedTurnCard,
                        amount, 0, closing.ResultingWorldTick, sourceId,
                        string.Empty);
                }
                else if (card.OrientationCode == Simulation타로카드방향Codes.Upright
                         || card.StatDelta > 0)
                {
                    ApplyRegionalCausality(
                        "regional-causality:turn-card:" + closing.TurnClosingStableId
                        + ":" + sourceId,
                        SimulationRegionalIncidentCodes.PositiveTurnCard,
                        0, amount, closing.ResultingWorldTick, sourceId,
                        string.Empty);
                }
            }
        }

        private void ObserveSafeRegionalIncidentOutcome(
            SimulationRegionalIncidentSnapshot incident,
            int completedWorldTick)
        {
            ApplyRegionalCausality(
                "regional-causality:incident:safe:" + incident.IncidentStableId,
                SimulationRegionalIncidentCodes.SafeIncidentResponse,
                -incident.Severity, incident.Severity,
                completedWorldTick, incident.IncidentStableId,
                incident.NatureRouteCode);
        }

        private void ObserveUnsafeRegionalIncidentOutcome(
            SimulationRegionalIncidentSnapshot incident,
            string sourceCode,
            int completedWorldTick)
        {
            ApplyRegionalCausality(
                "regional-causality:incident:unsafe:" + incident.IncidentStableId,
                sourceCode,
                incident.Severity, -incident.Severity,
                completedWorldTick, incident.IncidentStableId,
                incident.NatureRouteCode);
        }

        private void ApplyRegionalCausality(
            string changeStableId,
            string sourceCode,
            int threatDelta,
            int recoveryDelta,
            int appliedWorldTick,
            string sourceStableId,
            string natureRouteCode)
        {
            if (!regionalCausalitySchemaEnabled
                || !appliedRegionalCausalityChangeIds.Add(changeStableId)) return;
            var priorThreat = regionalThreatScore;
            var priorRecovery = regionalRecoveryScore;
            regionalThreatScore = ClampScore(regionalThreatScore + threatDelta);
            regionalRecoveryScore = ClampScore(regionalRecoveryScore + recoveryDelta);
            regionalCausalityRevision++;
            regionalCausalityLastChangedWorldTick = appliedWorldTick;
            regionalCausalityChanges.Add(new SimulationRegionalCausalityChangeSnapshot
            {
                ChangeStableId = changeStableId,
                SourceCode = sourceCode,
                ThreatDelta = regionalThreatScore - priorThreat,
                RecoveryDelta = regionalRecoveryScore - priorRecovery,
                AppliedWorldTick = appliedWorldTick,
                SourceStableId = sourceStableId,
                NatureRouteCode = natureRouteCode,
            });
        }

        private SimulationRegionalCausalityStateSnapshot CreateRegionalCausalitySnapshot()
            => new SimulationRegionalCausalityStateSnapshot
            {
                Revision = regionalCausalityRevision,
                ThreatScore = regionalThreatScore,
                RecoveryScore = regionalRecoveryScore,
                NetPressureModifier = regionalThreatScore - regionalRecoveryScore,
                OutcomeCode = ResolveRegionalOutcomeCode(
                    regionalThreatScore, regionalRecoveryScore),
                LastChangedWorldTick = regionalCausalityLastChangedWorldTick,
                Changes = regionalCausalityChanges
                    .Select(CloneRegionalCausalityChange).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        internal static SimulationRegionalCausalityStateSnapshot
            CloneRegionalCausalityState(SimulationRegionalCausalityStateSnapshot? source)
        {
            source ??= new SimulationRegionalCausalityStateSnapshot();
            return new SimulationRegionalCausalityStateSnapshot
            {
                Revision = source.Revision,
                ThreatScore = source.ThreatScore,
                RecoveryScore = source.RecoveryScore,
                NetPressureModifier = source.NetPressureModifier,
                OutcomeCode = source.OutcomeCode,
                LastChangedWorldTick = source.LastChangedWorldTick,
                Changes = (source.Changes ?? Array.Empty<
                    SimulationRegionalCausalityChangeSnapshot>())
                    .Select(CloneRegionalCausalityChange).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };
        }

        private static SimulationRegionalCausalityChangeSnapshot CloneRegionalCausalityChange(
            SimulationRegionalCausalityChangeSnapshot source) => new()
        {
            ChangeStableId = source.ChangeStableId,
            SourceCode = source.SourceCode,
            ThreatDelta = source.ThreatDelta,
            RecoveryDelta = source.RecoveryDelta,
            AppliedWorldTick = source.AppliedWorldTick,
            SourceStableId = source.SourceStableId,
            NatureRouteCode = source.NatureRouteCode,
        };

        private static string FindNatureRouteCode(SimulationTaskSnapshot task)
        {
            const string prefix = "nature-route:";
            var value = task.InputLotStableIds.Concat(task.SourceStableIds)
                .FirstOrDefault(item => item.StartsWith(prefix, StringComparison.Ordinal));
            return value == null ? string.Empty : value.Substring(prefix.Length);
        }

        private static string ResolveRegionalOutcomeCode(int threat, int recovery)
        {
            if (threat > recovery) return SimulationRegionalIncidentCodes.ThreatOutcome;
            if (recovery >= threat + 2) return SimulationRegionalIncidentCodes.RecoveryOutcome;
            if (recovery > threat) return SimulationRegionalIncidentCodes.OpportunityOutcome;
            return SimulationRegionalIncidentCodes.NormalOutcome;
        }

        private static int ClampScore(int value) =>
            Math.Max(0, Math.Min(MaximumRegionalCausalityScore, value));
    }
}
