using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수출선적계획상태Codes
    {
        public const string Scheduled = "Scheduled";
        public const string PlannedCandidate = "PlannedCandidate";
    }

    public static class Simulation수출운송방식Codes
    {
        public const string Ocean = "Ocean";
        public const string Air = "Air";
    }

    public static class Simulation수출위험수준Codes
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
    }

    public sealed class Simulation수출선적계획PreviewRequest
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string SourceReadinessReviewStableId { get; set; } = string.Empty;
        public string DestinationCountryCode { get; set; } = string.Empty;
        public string DestinationMarketStableId { get; set; } = string.Empty;
        public string TransportModeCode { get; set; } = string.Empty;
        public string PlanningFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public decimal ExpectedGrossRevenue { get; set; }
        public decimal ExpectedInternationalLogisticsCost { get; set; }
        public decimal ExpectedHandlingCost { get; set; }
        public decimal ExpectedOtherCost { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int EstimatedTransitTicks { get; set; }
        public int RiskScore { get; set; }
        public int RequiredPlanningTicks { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출선적계획ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출선적계획PreviewRequest Plan { get; set; }
            = new Simulation수출선적계획PreviewRequest();
    }

    public sealed class Simulation수출선적계획PreviewSnapshot
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string SourceReadinessReviewStableId { get; set; } = string.Empty;
        public string SourcePortReceiptStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string DestinationCountryCode { get; set; } = string.Empty;
        public string DestinationMarketStableId { get; set; } = string.Empty;
        public string TransportModeCode { get; set; } = string.Empty;
        public string PlanningFacilityStableId { get; set; } = string.Empty;
        public decimal ExpectedGrossRevenue { get; set; }
        public decimal ExpectedTotalCost { get; set; }
        public decimal ExpectedNetRevenue { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int EstimatedTransitTicks { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevelCode { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotChangeTreasury { get; set; }
        public bool DoesNotCreateOperationalShipment { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation수출선적계획Snapshot
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation수출선적계획상태Codes.Scheduled;
        public long Revision { get; set; }
        public string SourceReadinessReviewStableId { get; set; } = string.Empty;
        public string SourcePortReceiptStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string DestinationCountryCode { get; set; } = string.Empty;
        public string DestinationMarketStableId { get; set; } = string.Empty;
        public string TransportModeCode { get; set; } = string.Empty;
        public string PlanningFacilityStableId { get; set; } = string.Empty;
        public decimal ExpectedGrossRevenue { get; set; }
        public decimal ExpectedInternationalLogisticsCost { get; set; }
        public decimal ExpectedHandlingCost { get; set; }
        public decimal ExpectedOtherCost { get; set; }
        public decimal ExpectedTotalCost { get; set; }
        public decimal ExpectedNetRevenue { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int EstimatedTransitTicks { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevelCode { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int RequiredPlanningTicks { get; set; }
        public int ScheduledTick { get; set; }
        public int? CompletedTick { get; set; }
        public string? ExecutionStableId { get; set; }
        public int? ExecutionCompletedTick { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
