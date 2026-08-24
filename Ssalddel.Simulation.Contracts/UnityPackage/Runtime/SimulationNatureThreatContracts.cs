using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// Nature 위협 압력과 조우가 소유하는 코드다.
    /// 기존 공개 진입점은 SimulationRegionalIncidentCodes에 호환 별칭으로 남긴다.
    /// </summary>
    public static class SimulationNatureThreatCodes
    {
        public const string Stable = "Stable";
        public const string Warning = "Warning";
        public const string Threatened = "Threatened";
        public const string Infested = "Infested";
        public const string EncounterBand = "EncounterBand";
        public const string Active = "Active";
    }

    public sealed class SimulationNatureThreatRouteSnapshot
    {
        public string NatureRouteCode { get; set; } = string.Empty;
        public int RootRemainingSeverity { get; set; }
        public int GlobalSpilloverPressure { get; set; }
        public int EffectivePressure { get; set; }
        public int IncidentPressure { get; set; }
        public int ThreatScoreModifier { get; set; }
        public int RecoveryScoreModifier { get; set; }
        public string PressureLevelCode { get; set; } = SimulationNatureThreatCodes.Stable;
        public string[] SourceIncidentStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNatureThreatEncounterSnapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public long EncounterRevision { get; set; }
        public string NatureRouteCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationNatureThreatCodes.Active;
        public string RiskBandCode { get; set; } = string.Empty;
        public int ThreatUnitCount { get; set; }
        public int OccurredWorldTick { get; set; }
        public int? ResolvedWorldTick { get; set; }
        public string[] SourceIncidentStableIds { get; set; } = Array.Empty<string>();
        public string PresentationKey { get; set; } = "survival.tactical.squad.zombie-pressure";
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureThreatStateSnapshot
    {
        public SimulationNatureThreatRouteSnapshot[] Routes { get; set; }
            = Array.Empty<SimulationNatureThreatRouteSnapshot>();
        public SimulationNatureThreatEncounterSnapshot[] Encounters { get; set; }
            = Array.Empty<SimulationNatureThreatEncounterSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureEncounterVictoryRequest
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
    }
}
