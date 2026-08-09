using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationModeCodes
    {
        public const string Simulation = "Simulation";
    }

    public sealed class 경영SimulationSession생성Request
    {
        public Guid ClientRequestId { get; set; }
        public string ScenarioStableId { get; set; } = string.Empty;
        public string ScenarioDataRevision { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public int DurationTicks { get; set; } = 28;
    }

    public sealed class 경영SimulationTick진행Request
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public int TickCount { get; set; } = 1;
    }

    public sealed class 경영SimulationSessionSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public Guid ClientRequestId { get; set; }
        public string ScenarioStableId { get; set; } = string.Empty;
        public string ScenarioDataRevision { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public int CurrentTick { get; set; }
        public int DurationTicks { get; set; }
        public long Revision { get; set; }
        public bool IsCompleted { get; set; }
        public string ModeCode { get; set; } = SimulationModeCodes.Simulation;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationErrorResponse
    {
        public string ErrorCode { get; set; } = string.Empty;
    }
}
