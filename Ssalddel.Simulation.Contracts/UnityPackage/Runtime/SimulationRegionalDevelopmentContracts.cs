using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationRegionalDevelopmentCodes
    {
        public const string RuleRevision = "regional-development.r1";

        public const string Available = "Available";
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";

        public const string NotStarted = "NotStarted";
        public const string Developing = "Developing";
        public const string IndependentReady = "IndependentReady";

        public const string Locked = "Locked";

        public const string FarmIncidentContainmentH2 =
            "h2-candidate:farm-incident-containment";
        public const string FarmExposureInspectionH1 =
            "h1-stock:farm-exposure-inspection";
        public const string FarmIncidentQuarantineH1 =
            "h1-stock:farm-incident-quarantine";
        public const string FarmWeatherProtectionH1 =
            "h1-stock:farm-weather-protection";
        public const string NatureFarmSafetyConnector =
            "connector:nature-farm:safety-development";
    }

    public sealed class SimulationRegionalDevelopmentOpportunitySnapshot
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public long OpportunityRevision { get; set; }
        public string SourceIncidentStableId { get; set; } = string.Empty;
        public string SourceEncounterStableId { get; set; } = string.Empty;
        public string SourceBattleStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public string TargetAreaCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationRegionalDevelopmentCodes.Available;
        public int EarnedWorldTick { get; set; }
        public int? ReservedWorldTick { get; set; }
        public int? ConsumedWorldTick { get; set; }
        public string ReservedProjectStableId { get; set; } = string.Empty;
        public string OperationalFacilityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationRegionalRouteSafetySnapshot
    {
        public string NatureRouteCode { get; set; } = string.Empty;
        public int SecuredFromWorldTick { get; set; }
        public int SecuredUntilWorldTick { get; set; }
        public string SourceEncounterStableId { get; set; } = string.Empty;
        public string SourceBattleStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationRegionalDevelopmentAreaSnapshot
    {
        public string AreaCode { get; set; } = string.Empty;
        public string TargetH2StableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationRegionalDevelopmentCodes.NotStarted;
        public string[] RequiredH1StableIds { get; set; } = Array.Empty<string>();
        public string[] OperationalH1StableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationRegionalDevelopmentConnectorSnapshot
    {
        public string ConnectorStableId { get; set; } = string.Empty;
        public string FromAreaCode { get; set; } = string.Empty;
        public string ToAreaCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationRegionalDevelopmentCodes.Locked;
        public string RequiredAreaCode { get; set; } = string.Empty;
    }

    public sealed class SimulationRegionalDevelopmentStateSnapshot
    {
        public long Revision { get; set; }
        public string RuleRevision { get; set; } = SimulationRegionalDevelopmentCodes.RuleRevision;
        public SimulationRegionalDevelopmentOpportunitySnapshot[] Opportunities { get; set; }
            = Array.Empty<SimulationRegionalDevelopmentOpportunitySnapshot>();
        public SimulationRegionalRouteSafetySnapshot[] RouteSafeties { get; set; }
            = Array.Empty<SimulationRegionalRouteSafetySnapshot>();
        public SimulationRegionalDevelopmentAreaSnapshot[] Areas { get; set; }
            = Array.Empty<SimulationRegionalDevelopmentAreaSnapshot>();
        public SimulationRegionalDevelopmentConnectorSnapshot[] Connectors { get; set; }
            = Array.Empty<SimulationRegionalDevelopmentConnectorSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
