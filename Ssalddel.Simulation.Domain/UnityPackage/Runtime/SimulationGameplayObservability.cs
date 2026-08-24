using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationGameplayObservabilitySnapshot GetGameplayObservability()
        {
            lock (gate)
            {
                var mind = CreateNatureMindStateSnapshot();
                var traces = coopContributions.Values.OrderBy(value =>
                        value.ContributionStableId, StringComparer.Ordinal)
                    .Select(contribution =>
                    {
                        var taskId = "task:" + contribution.ContributionStableId
                            .Substring("coop-contribution:".Length);
                        var task = tasks[taskId];
                        var effect = effects.Values.First(value =>
                            value.CausedByTaskStableId == taskId);
                        var grant = contribution.PlayerStableId
                            == SimulationAreaAccessCodes.PlayerOwner
                            ? "permission:world-owner"
                            : FindHostedGrant(contribution.PlayerStableId,
                                SimulationAreaAccessCodes.FarmAreaSet,
                                SimulationHostedWorldCodes.PerformWork)
                                ?.GrantHashSha256 ?? string.Empty;
                        var interpretation = mind.Balances.FirstOrDefault(value =>
                            value.PlayerStableId == contribution.PlayerStableId)
                            ?.BalanceHashSha256 ?? string.Empty;
                        var result = new SimulationGameplayTraceSnapshot
                        {
                            HostedSessionStableId = hostedSessionStableId,
                            WorldStableId = SessionStableId,
                            PlayerStableId = contribution.PlayerStableId,
                            PermissionDecisionId = grant,
                            RequestIdempotencyId = task.TaskStableId,
                            DecisionStableId = task.CausedByDecisionStableId,
                            TaskStableId = task.TaskStableId,
                            EffectStableId = effect.EffectStableId,
                            ProjectStableId = contribution.ProjectStableId,
                            WorldRevision = Revision,
                            AppliedWorldTick = contribution.AppliedWorldTick,
                            InterpretationHash = interpretation,
                            ProjectionCode = "SimulationWorldShell",
                        };
                        result.TraceHashSha256 = HashCoop(string.Join("|",
                            result.HostedSessionStableId, result.WorldStableId,
                            result.PlayerStableId, result.PermissionDecisionId,
                            result.RequestIdempotencyId, result.DecisionStableId,
                            result.TaskStableId, result.EffectStableId,
                            result.ProjectStableId, result.WorldRevision,
                            result.AppliedWorldTick, result.InterpretationHash,
                            result.ProjectionCode));
                        return result;
                    }).ToArray();
                var snapshot = new SimulationGameplayObservabilitySnapshot
                {
                    WorldRevision = Revision,
                    WorldTick = CurrentTick,
                    Traces = traces,
                    RawFactSeparatedFromInterpretation = true,
                    MoodProjectionChangesRules = false,
                };
                snapshot.SnapshotHashSha256 = HashCoop(string.Join("|",
                    snapshot.WorldRevision, snapshot.WorldTick,
                    string.Join(",", traces.Select(value => value.TraceHashSha256)),
                    snapshot.RawFactSeparatedFromInterpretation,
                    snapshot.MoodProjectionChangesRules));
                return snapshot;
            }
        }
    }
}
