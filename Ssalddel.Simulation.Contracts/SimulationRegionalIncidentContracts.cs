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

    public static class SimulationNatureInteractionCodes
    {
        public const string RegionalThreatObservation = "RegionalThreatObservation";
        public const string ThreatObservationTask = "NatureThreatObservationTask";
        public const string NatureThreatObserved = "NatureThreatObserved";
        public const string ThreatObserved = "ThreatObserved";
        public const string NatureHomeFacility = "facility:nature-home";
        public const string PressurePointUnit = "pressure-point";
        public const string EmergencyRetreat = "EmergencyRetreat";
        public const string EmergencyRetreatTask = "NatureEmergencyRetreatTask";
        public const string PartyRetreatedToSafeCore = "PartyRetreatedToSafeCore";
        public const string RetreatedToSafeCore = "RetreatedToSafeCore";
        public const string PartyStateUnit = "party-state";
        public const string NatureRestoration = "NatureRestoration";
        public const string NatureRestorationTask = "NatureRestorationTask";
        public const string NatureRouteRestored = "NatureRouteRestored";
        public const string RestorationStateUnit = "restoration-state";
        public const string PartyRecovery = "PartyRecovery";
        public const string PartyRecoveryTask = "NaturePartyRecoveryTask";
        public const string PartyRecovered = "PartyRecovered";
    }

    public sealed class SimulationNatureEmergencyRetreatPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureEmergencyRetreatConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationNatureEmergencyRetreatPreviewRequest Preview { get; set; }
            = new SimulationNatureEmergencyRetreatPreviewRequest();
    }

    public sealed class SimulationNatureEmergencyRetreatPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public bool HasObservedThreat { get; set; }
        public bool HasActiveEncounter { get; set; }
        public string[] NextWorldInteractionIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot DecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureRestorationPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureRestorationConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationNatureRestorationPreviewRequest Preview { get; set; }
            = new SimulationNatureRestorationPreviewRequest();
    }

    public sealed class SimulationNatureRestorationPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string[] ResolvedCauseIncidentStableIds { get; set; } = Array.Empty<string>();
        public string[] NextWorldInteractionIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot DecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNaturePartyRecoveryPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationNaturePartyRecoveryConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationNaturePartyRecoveryPreviewRequest Preview { get; set; }
            = new SimulationNaturePartyRecoveryPreviewRequest();
    }

    public sealed class SimulationNaturePartyRecoveryPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public bool HasRetreatPredecessor { get; set; }
        public bool HasRestorationPredecessor { get; set; }
        public string NextPlayerActionCode { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string NaturePeriodStateCode { get; set; } = string.Empty;
        public int BaseDurationTicks { get; set; }
        public int EffectiveDurationTicks { get; set; }
        public SimulationDecisionPreviewSnapshot DecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureThreatObservationPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureThreatObservationConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationNatureThreatObservationPreviewRequest Preview { get; set; }
            = new SimulationNatureThreatObservationPreviewRequest();
    }

    public sealed class SimulationNatureThreatObservationPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public int EffectivePressure { get; set; }
        public string PressureLevelCode { get; set; } = string.Empty;
        public string[] SourceIncidentStableIds { get; set; } = Array.Empty<string>();
        public string[] NextWorldInteractionIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot DecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
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
        public int IncidentPressure { get; set; }
        public int ThreatScoreModifier { get; set; }
        public int RecoveryScoreModifier { get; set; }
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
            SimulationRegionalIncidentCodes.NormalOutcome;
        public int LastChangedWorldTick { get; set; }
        public SimulationRegionalCausalityChangeSnapshot[] Changes { get; set; } =
            Array.Empty<SimulationRegionalCausalityChangeSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureEncounterVictoryRequest
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
    }
}
