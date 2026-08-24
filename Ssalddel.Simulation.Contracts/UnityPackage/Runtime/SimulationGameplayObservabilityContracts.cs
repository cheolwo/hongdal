using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class SimulationGameplayTraceSnapshot
    {
        public string HostedSessionStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string PermissionDecisionId { get; set; } = string.Empty;
        public string RequestIdempotencyId { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string EffectStableId { get; set; } = string.Empty;
        public string ProjectStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int AppliedWorldTick { get; set; }
        public string InterpretationHash { get; set; } = string.Empty;
        public string ProjectionCode { get; set; } = "SimulationWorldShell";
        public string TraceHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationGameplayObservabilitySnapshot
    {
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public SimulationGameplayTraceSnapshot[] Traces { get; set; }
            = Array.Empty<SimulationGameplayTraceSnapshot>();
        public bool RawFactSeparatedFromInterpretation { get; set; } = true;
        public bool MoodProjectionChangesRules { get; set; }
        public string SnapshotHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
