using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldEventCodes
    {
        public const string AwaitingResponse = "AwaitingResponse";
        public const string Warning = "Warning";
        public const string Resolved = "Resolved";
        public const string SessionParticipants = "SessionParticipants";
        public const string SurvivalTarotConsensus = "SurvivalTarotConsensus";
        public const string SurvivalTarotOpportunity = "SurvivalTarotOpportunity";
        public const string FarmThreatChoice = "FarmThreatChoice";
        public const string FarmThreatEncounter = "FarmThreatEncounter";
        public const string RegionalIncident = "RegionalIncident";
        public const string NatureThreatWarning = "NatureThreatWarning";
        public const string NatureThreatEncounter = "NatureThreatEncounter";

        public const string ExternalExpeditionPresentation =
            "survival.external-expedition";
        public const string FoodReserveCrisisPresentation =
            "survival.food-reserve-crisis";
        public const string PeriodicTarotPresentation =
            "survival.periodic-tarot";
    }

    public sealed class SimulationWorldEventChoiceSnapshot
    {
        public string ChoiceStableId { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string KoreanSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simulation 서버가 확정한 사건을 Unity가 표현할 수 있도록 제공하는 관점별 조회 결과다.
    /// PresentationKey는 의미 키이며 Prefab·Material 경로나 업무 확정 권위가 아니다.
    /// </summary>
    public sealed class SimulationWorldEventSnapshot
    {
        public string EventStableId { get; set; } = string.Empty;
        public long EventRevision { get; set; }
        public long LastChangedWorldRevision { get; set; }
        public string EventTypeCode { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int OccurredWorldTick { get; set; }
        public int VisibleFromWorldTick { get; set; }
        public int? ExpiresAfterWorldTick { get; set; }
        public string AudienceScopeCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
        public string ResponseKindCode { get; set; } = string.Empty;
        public string SourceOpportunityStableId { get; set; } = string.Empty;
        public string ChoiceSetStableId { get; set; } = string.Empty;
        public SimulationWorldEventChoiceSnapshot[] Choices { get; set; }
            = Array.Empty<SimulationWorldEventChoiceSnapshot>();
        public string SelectedChoiceStableId { get; set; } = string.Empty;
        public string ActiveBuildingStableId { get; set; } = string.Empty;
        public string[] AnchorBuildingStableIds { get; set; } = Array.Empty<string>();
        public string[] TileKeys { get; set; } = Array.Empty<string>();
        public string[] RegionStableIds { get; set; } = Array.Empty<string>();
        public string[] ParticipantPlayerStableIds { get; set; } = Array.Empty<string>();
        public int RespondedParticipantCount { get; set; }
        public int RequiredParticipantCount { get; set; }
        public bool CanRespond { get; set; }
        public bool RequiresUnanimousResponse { get; set; }
        public bool RequiresExpectedRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string SourceInstanceStableId { get; set; } = string.Empty;
        public string NatureRouteCode { get; set; } = string.Empty;
        public int ProjectedThreatPressureDelta { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationWorldEventProjectionSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public long AfterWorldRevision { get; set; } = -1;
        public long NextAfterWorldRevision { get; set; }
        public bool HasMore { get; set; }
        public SimulationWorldEventSnapshot[] Events { get; set; }
            = Array.Empty<SimulationWorldEventSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }
}
