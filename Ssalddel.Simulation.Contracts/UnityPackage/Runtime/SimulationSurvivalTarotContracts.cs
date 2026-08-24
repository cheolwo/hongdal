using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationSurvivalTarotCodes
    {
        public const string RuleRevision = "survival-tarot.consensus.r2";
        public const string Pending = "Pending";
        public const string Resolved = "Resolved";
        public const string Periodic = "Periodic";
        public const string FoodReserveCrisis = "FoodReserveCrisis";
        public const string ExternalExpeditionRequired = "ExternalExpeditionRequired";
        public const string OneTickOneDay = "OneTickOneDay";

        public const string OpportunityNotFound = "SimulationSurvivalTarotOpportunityNotFound";
        public const string OpportunityAlreadyResolved = "SimulationSurvivalTarotOpportunityAlreadyResolved";
        public const string ParticipantNotFound = "SimulationSurvivalTarotParticipantNotFound";
        public const string SafeBuildingRequired = "SimulationSurvivalTarotSafeBuildingRequired";
        public const string ParticipantsNotTogether = "SimulationSurvivalTarotParticipantsNotTogether";
        public const string OfferNotFound = "SimulationSurvivalTarotOfferNotFound";
        public const string UnanimousResponseRequired = "SimulationSurvivalTarotUnanimousResponseRequired";
    }

    /// <summary>
    /// 공공데이터 사실이 아니라 SimulationScenario가 정하는 생존 타로 규칙이다.
    /// 한 Tick은 하루이며, 주기와 첫 식량 위기를 함께 감시한다.
    /// </summary>
    public sealed class SimulationSurvivalTarotInitialStateRequest
    {
        public string RuleRevision { get; set; } = SimulationSurvivalTarotCodes.RuleRevision;
        public int PeriodicIntervalTicks { get; set; } = 3;
        public decimal FoodCrisisThresholdPersonDays { get; set; } = 2m;
        public decimal FarmExitThresholdPersonDays { get; set; } = 2m;
        public decimal FoodUnitsPerPlayerDay { get; set; } = 1m;
        public string[] FoodItemCodes { get; set; } = Array.Empty<string>();
        public string[] FarmBuildingStableIds { get; set; } = Array.Empty<string>();
        public string[] SafeBuildingStableIds { get; set; } = Array.Empty<string>();
        public string[] ParticipantPlayerStableIds { get; set; } = Array.Empty<string>();
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationSurvivalTarotResponseConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string OpportunityStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string OfferStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationSurvivalTarotResolutionConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string OpportunityStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string OfferStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationSurvivalTarotParticipantResponseSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string OfferStableId { get; set; } = string.Empty;
        public int RespondedWorldTick { get; set; }
        public long RespondedWorldRevision { get; set; }
    }

    public sealed class SimulationSurvivalTarotOpportunitySnapshot
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public int TriggeredWorldTick { get; set; }
        public decimal FoodReservePersonDays { get; set; }
        public decimal FarmFoodReservePersonDays { get; set; }
        public bool RequiresExternalExpedition { get; set; }
        public string SafeBuildingStableId { get; set; } = string.Empty;
        public string[] ParticipantPlayerStableIds { get; set; } = Array.Empty<string>();
        public Simulation타로DrawSnapshot Draw { get; set; } = new Simulation타로DrawSnapshot();
        public SimulationSurvivalTarotParticipantResponseSnapshot[] Responses { get; set; }
            = Array.Empty<SimulationSurvivalTarotParticipantResponseSnapshot>();
        public string SelectedOfferStableId { get; set; } = string.Empty;
        public int? ResolvedWorldTick { get; set; }
        public long? ResolvedWorldRevision { get; set; }
        public Simulation타로규칙보정선Snapshot[] ModifierLines { get; set; }
            = Array.Empty<Simulation타로규칙보정선Snapshot>();
    }

    public sealed class SimulationSurvivalTarotStateSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public int PeriodicIntervalTicks { get; set; }
        public decimal FoodCrisisThresholdPersonDays { get; set; }
        public decimal FarmExitThresholdPersonDays { get; set; }
        public decimal CurrentFoodReservePersonDays { get; set; }
        public decimal CurrentFarmFoodReservePersonDays { get; set; }
        public bool FarmScopeConfigured { get; set; }
        public bool RequiresExternalExpedition { get; set; }
        public string CalendarRuleCode { get; set; } = SimulationSurvivalTarotCodes.OneTickOneDay;
        public SimulationSurvivalTarotOpportunitySnapshot? PendingOpportunity { get; set; }
        public SimulationSurvivalTarotOpportunitySnapshot[] OpportunityHistory { get; set; }
            = Array.Empty<SimulationSurvivalTarotOpportunitySnapshot>();
        public Simulation타로규칙보정선Snapshot[] ActiveModifierLines { get; set; }
            = Array.Empty<Simulation타로규칙보정선Snapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationSurvivalTarotCommandResultSnapshot
    {
        public string CommandId { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public long AppliedWorldRevision { get; set; }
        public SimulationSurvivalTarotStateSnapshot State { get; set; }
            = new SimulationSurvivalTarotStateSnapshot();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
