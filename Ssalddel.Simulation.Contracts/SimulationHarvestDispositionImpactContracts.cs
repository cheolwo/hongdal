using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationHarvestDispositionChoiceCodes
    {
        public const string CooperativeShipment = "CooperativeShipment";
        public const string DirectOnlineSale = "DirectOnlineSale";
        public const string ExportAgent = "ExportAgent";
        public const string ReserveStorage = "ReserveStorage";
    }

    public static class SimulationHarvestDispositionWorkflowCodes
    {
        public const string CooperativeIntakeCandidate = "CooperativeIntakeCandidate";
        public const string ProducerPackingCandidate = "ProducerPackingCandidate";
        public const string ExportReadinessCandidate = "ExportReadinessCandidate";
        public const string ReserveStockLotCandidate = "ReserveStockLotCandidate";
    }

    public sealed class SimulationHarvestDispositionImpactPreviewRequest
    {
        public string DispositionDecisionStableId { get; set; } = string.Empty;
        public long DispositionDecisionRevision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long HarvestLotRevision { get; set; }
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationHarvestDispositionImpactConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationHarvestDispositionImpactPreviewRequest Impact { get; set; }
            = new SimulationHarvestDispositionImpactPreviewRequest();
    }

    public sealed class SimulationHarvestDispositionImpactPreviewSnapshot
    {
        public string DispositionDecisionStableId { get; set; } = string.Empty;
        public long DispositionDecisionRevision { get; set; }
        public string ChoiceCode { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string SourceUnitCode { get; set; } = string.Empty;
        public string CanonicalQuantityUnitCode { get; set; } = string.Empty;
        public decimal RequiredLabor { get; set; }
        public decimal SimulationCost { get; set; }
        public decimal? ProjectedRevenue { get; set; }
        public int DurationTicks { get; set; }
        public decimal FoodSecurityDaysBefore { get; set; }
        public decimal FoodSecurityDaysCandidate { get; set; }
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotApplySettlementState { get; set; }
        public string PolicyRevision { get; set; } = string.Empty;
        public string[] RiskCodes { get; set; } = Array.Empty<string>();
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationReserveStorageCandidateSnapshot? StorageCandidate { get; set; }
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class SimulationReserveStorageCandidateSnapshot
    {
        public string StorageFacilityStableId { get; set; } = string.Empty;
        public decimal StorageCapacity { get; set; }
        public decimal StorageOccupiedBefore { get; set; }
        public decimal StorageAvailableBefore { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal ExpectedShrinkageQuantity { get; set; }
        public decimal ExpectedStoredQuantity { get; set; }
        public decimal FoodEquivalentAddedCandidate { get; set; }
        public decimal FoodReserveEquivalentBefore { get; set; }
        public decimal FoodReserveEquivalentCandidate { get; set; }
        public decimal FoodSecurityDaysBefore { get; set; }
        public decimal FoodSecurityDaysCandidate { get; set; }
        public decimal ShrinkageRate { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string FoodEquivalentUnitCode { get; set; } = string.Empty;
        public string FoodEquivalentRuleRevision { get; set; } = string.Empty;
        public string CandidateStockLotStableId { get; set; } = string.Empty;
    }
}
