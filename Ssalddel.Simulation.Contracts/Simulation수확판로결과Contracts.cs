using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수확판로선택상태Codes
    {
        public const string Selected = "Selected";
        public const string NotSelected = "NotSelected";
    }

    public static class Simulation수확판로단계Codes
    {
        public const string NotSelected = "NotSelected";
        public const string DispositionTaskScheduled = "DispositionTaskScheduled";
        public const string CooperativeIntakeCandidate = "CooperativeIntakeCandidate";
        public const string CooperativeCargoReserved = "CooperativeCargoReserved";
        public const string CooperativeCargoInTransit = "CooperativeCargoInTransit";
        public const string CooperativeCargoArrived = "CooperativeCargoArrived";
        public const string DirectMarketSupplyAvailable = "DirectMarketSupplyAvailable";
        public const string ReserveStored = "ReserveStored";
        public const string ExportPreparation = "ExportPreparation";
        public const string ExportCargoPreparation = "ExportCargoPreparation";
        public const string ExportCargoHandoff = "ExportCargoHandoff";
        public const string ExportPortMovement = "ExportPortMovement";
        public const string ExportPortReceipt = "ExportPortReceipt";
        public const string ExportReadinessReview = "ExportReadinessReview";
        public const string ExportShipmentPlan = "ExportShipmentPlan";
        public const string ExportShipmentScheduled = "ExportShipmentScheduled";
        public const string ExportShipmentInTransit = "ExportShipmentInTransit";
        public const string ExportDelivered = "ExportDelivered";
        public const string ExportDisruptedWithLoss = "ExportDisruptedWithLoss";
    }

    public sealed class Simulation수확판로결과Snapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public string SettlementStableId { get; set; } = string.Empty;
        public string AllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long HarvestLotRevision { get; set; }
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string SelectedChoiceCode { get; set; } = string.Empty;
        public string AllocationStateCode { get; set; } = string.Empty;
        public decimal CurrentTreasuryBalance { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal CurrentProductMarketSupplyQuantity { get; set; }
        public decimal CurrentProductReserveQuantity { get; set; }
        public Simulation수확판로선택지결과Snapshot[] Routes { get; set; }
            = Array.Empty<Simulation수확판로선택지결과Snapshot>();
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수확판로선택지결과Snapshot
    {
        public string ChoiceCode { get; set; } = string.Empty;
        public string SelectionStateCode { get; set; }
            = Simulation수확판로선택상태Codes.NotSelected;
        public bool IsSelected { get; set; }
        public string CurrentStageCode { get; set; } = Simulation수확판로단계Codes.NotSelected;
        public string SourceStateCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ResolvedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal MarketSuppliedQuantity { get; set; }
        public decimal StoredQuantity { get; set; }
        public decimal ExportDeliveredQuantity { get; set; }
        public decimal ExportLostQuantity { get; set; }
        public decimal OutboundReservedQuantity { get; set; }
        public decimal RecognizedTreasuryDelta { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string RiskResultCode { get; set; } = string.Empty;
        public string[] RiskCodes { get; set; } = Array.Empty<string>();
        public string[] RelatedStableIds { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
