using System;

namespace Ssalddel.Simulation.Contracts
{
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
}
