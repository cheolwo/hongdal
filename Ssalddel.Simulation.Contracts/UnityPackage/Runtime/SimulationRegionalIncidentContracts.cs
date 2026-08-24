using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 업무 영역 사건의 기존 공개 코드다. 저장·API 호환 때문에 이름과 값은 유지한다.
    /// 새 Nature 상호작용과 지역 발전 코드는 각 전용 계약에 둔다.
    /// </summary>
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

        // 아래 별칭은 기존 API·저장 소비자를 위한 호환 표면이다.
        public const string Stable = SimulationNatureThreatCodes.Stable;
        public const string Warning = SimulationNatureThreatCodes.Warning;
        public const string Threatened = SimulationNatureThreatCodes.Threatened;
        public const string Infested = SimulationNatureThreatCodes.Infested;
        public const string EncounterBand = SimulationNatureThreatCodes.EncounterBand;
        public const string Active = SimulationNatureThreatCodes.Active;

        public const string NormalOutcome = SimulationRegionalCausalityCodes.NormalOutcome;
        public const string OpportunityOutcome = SimulationRegionalCausalityCodes.OpportunityOutcome;
        public const string ThreatOutcome = SimulationRegionalCausalityCodes.ThreatOutcome;
        public const string RecoveryOutcome = SimulationRegionalCausalityCodes.RecoveryOutcome;

        public const string SafeIncidentResponse = SimulationRegionalCausalityCodes.SafeIncidentResponse;
        public const string UnsafeIncidentResponse = SimulationRegionalCausalityCodes.UnsafeIncidentResponse;
        public const string IncidentDeadlineMissed = SimulationRegionalCausalityCodes.IncidentDeadlineMissed;
        public const string NatureRestorationCompleted = SimulationRegionalCausalityCodes.NatureRestorationCompleted;
        public const string NaturePartyRecoveryCompleted = SimulationRegionalCausalityCodes.NaturePartyRecoveryCompleted;
        public const string PositiveTurnCard = SimulationRegionalCausalityCodes.PositiveTurnCard;
        public const string ReversedTurnCard = SimulationRegionalCausalityCodes.ReversedTurnCard;
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
}
