using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationRegionalIncidentCodes
    {
        public const string NatureHome = "NatureHome";
        public const string Farm = "Farm";
        public const string Town = "Town";
        public const string CityHub = "CityHub";

        public const string NatureToFarm = "NatureToFarm";
        public const string NatureToTown = "NatureToTown";
        public const string NatureToCityHub = "NatureToCityHub";

        public const string FarmHarvestExposure = "FarmHarvestExposure";
        public const string TownMarketContamination = "TownMarketContamination";
        public const string CityHubCargoBacklog = "CityHubCargoBacklog";

        public const string AwaitingResponse = "AwaitingResponse";
        public const string RecoveryInProgress = "RecoveryInProgress";
        public const string AdverseOutcome = "AdverseOutcome";
        public const string Resolved = "Resolved";

        public const string Contained = "Contained";
        public const string UnsafeResponse = "UnsafeResponse";
        public const string DeadlineMissed = "DeadlineMissed";
        public const string Corrected = "Corrected";

        public const string FarmCollectAndPack = "FarmCollectAndPack";
        public const string FarmLeaveExposed = "FarmLeaveExposed";
        public const string TownQuarantineAndRestock = "TownQuarantineAndRestock";
        public const string TownDiscardOutside = "TownDiscardOutside";
        public const string HubInspectAndPutAway = "HubInspectAndPutAway";
        public const string HubOverflowOpenYard = "HubOverflowOpenYard";

        public const string Stable = "Stable";
        public const string Warning = "Warning";
        public const string Threatened = "Threatened";
        public const string Infested = "Infested";
        public const string EncounterBand = "EncounterBand";
        public const string Active = "Active";
    }

    public sealed class SimulationRegionalIncidentResponsePreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ChoiceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationRegionalIncidentResponseConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ChoiceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationRegionalIncidentResponsePreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string EventStableId { get; set; } = string.Empty;
        public string IncidentStableId { get; set; } = string.Empty;
        public string ChoiceStableId { get; set; } = string.Empty;
        public int DeadlineWorldTick { get; set; }
        public int ProjectedThreatSeverityDelta { get; set; }
        public string[] RequiredWorldInteractionIds { get; set; } = Array.Empty<string>();
        public string[] RequiredActionCodes { get; set; } = Array.Empty<string>();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationRegionalIncidentSnapshot
    {
        public string IncidentStableId { get; set; } = string.Empty;
        public string EventStableId { get; set; } = string.Empty;
        public long IncidentRevision { get; set; }
        public string SourceInstanceStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string IncidentTypeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string OutcomeCode { get; set; } = string.Empty;
        public int Severity { get; set; }
        public int RemainingSeverity { get; set; }
        public int OccurredWorldTick { get; set; }
        public int DeadlineWorldTick { get; set; }
        public string SourceTargetStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string SelectedChoiceStableId { get; set; } = string.Empty;
        public string[] RequiredWorldInteractionIds { get; set; } = Array.Empty<string>();
        public string[] RequiredActionCodes { get; set; } = Array.Empty<string>();
        public string[] CompletedActionCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureThreatRouteSnapshot
    {
        public string NatureRouteCode { get; set; } = string.Empty;
        public int RootRemainingSeverity { get; set; }
        public int GlobalSpilloverPressure { get; set; }
        public int EffectivePressure { get; set; }
        public string PressureLevelCode { get; set; } = SimulationRegionalIncidentCodes.Stable;
        public string[] SourceIncidentStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNatureThreatEncounterSnapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public long EncounterRevision { get; set; }
        public string NatureRouteCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationRegionalIncidentCodes.Active;
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
