using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수출선적실행상태Codes
    {
        public const string Scheduled = "Scheduled";
        public const string InTransit = "InTransit";
        public const string DeliveredInSimulation = "DeliveredInSimulation";
        public const string DisruptedWithLossInSimulation = "DisruptedWithLossInSimulation";
    }

    public static class Simulation수출선적결과Codes
    {
        public const string Pending = "Pending";
        public const string Delivered = "Delivered";
        public const string DisruptedWithLoss = "DisruptedWithLoss";
    }

    public sealed class Simulation수출선적실행PreviewRequest
    {
        public string ExecutionStableId { get; set; } = string.Empty;
        public string SourceShipmentPlanStableId { get; set; } = string.Empty;
        public string ExecutionFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출선적실행ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출선적실행PreviewRequest Execution { get; set; }
            = new Simulation수출선적실행PreviewRequest();
    }

    public sealed class Simulation수출선적실행PreviewSnapshot
    {
        public string ExecutionStableId { get; set; } = string.Empty;
        public string SourceShipmentPlanStableId { get; set; } = string.Empty;
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
        public int EstimatedTransitTicks { get; set; }
        public int RiskScore { get; set; }
        public decimal SuccessProbabilityPercent { get; set; }
        public decimal TreasuryBefore { get; set; }
        public decimal PreviouslyRecognizedProjectedRevenue { get; set; }
        public decimal SuccessTreasuryDeltaCandidate { get; set; }
        public decimal LossTreasuryDeltaCandidate { get; set; }
        public decimal SuccessTreasuryAfterCandidate { get; set; }
        public decimal LossTreasuryAfterCandidate { get; set; }
        public decimal RequiredLossCapacityReservation { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool OutcomeHiddenUntilCompletion { get; set; }
        public bool DoesNotCreateOperationalShipment { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation수출선적실행Snapshot
    {
        public string ExecutionStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation수출선적실행상태Codes.Scheduled;
        public long Revision { get; set; }
        public string OutcomeCode { get; set; } = Simulation수출선적결과Codes.Pending;
        public int? OutcomeRoll { get; set; }
        public string SourceShipmentPlanStableId { get; set; } = string.Empty;
        public string SourceReadinessReviewStableId { get; set; } = string.Empty;
        public string SourcePortReceiptStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal LostQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string DestinationCountryCode { get; set; } = string.Empty;
        public string DestinationMarketStableId { get; set; } = string.Empty;
        public string TransportModeCode { get; set; } = string.Empty;
        public string ExecutionFacilityStableId { get; set; } = string.Empty;
        public int EstimatedTransitTicks { get; set; }
        public int RiskScore { get; set; }
        public decimal SuccessProbabilityPercent { get; set; }
        public decimal ExpectedGrossRevenue { get; set; }
        public decimal ExpectedTotalCost { get; set; }
        public decimal PreviouslyRecognizedProjectedRevenue { get; set; }
        public decimal SuccessTreasuryDeltaCandidate { get; set; }
        public decimal LossTreasuryDeltaCandidate { get; set; }
        public decimal RequiredLossCapacityReservation { get; set; }
        public decimal? AppliedTreasuryDelta { get; set; }
        public decimal? TreasuryBeforeApplication { get; set; }
        public decimal? TreasuryAfterApplication { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int ScheduledTick { get; set; }
        public int? DepartedTick { get; set; }
        public int? CompletedTick { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
