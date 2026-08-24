using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 업무 사건과 Nature 행동 사이의 위협·회복 계보가 소유하는 코드다.
    /// 기존 공개 진입점은 SimulationRegionalIncidentCodes에 호환 별칭으로 남긴다.
    /// </summary>
    public static class SimulationRegionalCausalityCodes
    {
        public const string NormalOutcome = "Normal";
        public const string OpportunityOutcome = "Opportunity";
        public const string ThreatOutcome = "Threat";
        public const string RecoveryOutcome = "Recovery";

        public const string SafeIncidentResponse = "SafeIncidentResponse";
        public const string UnsafeIncidentResponse = "UnsafeIncidentResponse";
        public const string IncidentDeadlineMissed = "IncidentDeadlineMissed";
        public const string NatureRestorationCompleted = "NatureRestorationCompleted";
        public const string NaturePartyRecoveryCompleted = "NaturePartyRecoveryCompleted";
        public const string PositiveTurnCard = "PositiveTurnCard";
        public const string ReversedTurnCard = "ReversedTurnCard";
    }

    public sealed class SimulationRegionalCausalityChangeSnapshot
    {
        public string ChangeStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public int ThreatDelta { get; set; }
        public int RecoveryDelta { get; set; }
        public int AppliedWorldTick { get; set; }
        public string SourceStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
    }

    public sealed class SimulationRegionalCausalityStateSnapshot
    {
        public long Revision { get; set; }
        public int ThreatScore { get; set; }
        public int RecoveryScore { get; set; }
        public int NetPressureModifier { get; set; }
        public string OutcomeCode { get; set; } =
            SimulationRegionalCausalityCodes.NormalOutcome;
        public int LastChangedWorldTick { get; set; }
        public SimulationRegionalCausalityChangeSnapshot[] Changes { get; set; } =
            Array.Empty<SimulationRegionalCausalityChangeSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
