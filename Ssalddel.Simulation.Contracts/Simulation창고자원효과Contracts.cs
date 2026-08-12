using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation창고작업상태Codes
    {
        public const string ReceivedAtDock = "ReceivedAtDock";
        public const string InspectionCompleted = "InspectionCompleted";
        public const string Stored = "Stored";
        public const string Picked = "Picked";
        public const string OutboundCompleted = "OutboundCompleted";
    }

    public sealed class Simulation창고작업Snapshot
    {
        public string WarehouseWorkStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string WarehouseFacilityStableId { get; set; } = string.Empty;
        public decimal ReceivedQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public decimal PickedQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string DecisionStateCode { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string TaskStateCode { get; set; } = string.Empty;
        public int CompletedTick { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation창고자원효과Request
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string EffectLineStableIdPrefix { get; set; } = string.Empty;
        public Simulation창고작업Snapshot Work { get; set; } = new Simulation창고작업Snapshot();
        public string ReceivedCargoLedgerStableId { get; set; } = string.Empty;
        public string InspectionPendingLedgerStableId { get; set; } = string.Empty;
        public string InspectionAcceptedLedgerStableId { get; set; } = string.Empty;
        public string InspectionRejectedLedgerStableId { get; set; } = string.Empty;
        public string WarehouseStockLedgerStableId { get; set; } = string.Empty;
        public string WarehouseLossLedgerStableId { get; set; } = string.Empty;
        public string OutboundReservedLedgerStableId { get; set; } = string.Empty;
        public string OutboundHandoffLedgerStableId { get; set; } = string.Empty;
        public string StorageAvailableLedgerStableId { get; set; } = string.Empty;
        public string StorageOccupiedLedgerStableId { get; set; } = string.Empty;
        public decimal ReceivedCargoBefore { get; set; }
        public decimal InspectionPendingBefore { get; set; }
        public decimal InspectionAcceptedBefore { get; set; }
        public decimal InspectionRejectedBefore { get; set; }
        public decimal WarehouseStockBefore { get; set; }
        public decimal WarehouseLossBefore { get; set; }
        public decimal OutboundReservedBefore { get; set; }
        public decimal OutboundHandoffBefore { get; set; }
        public decimal StorageAvailableBefore { get; set; }
        public decimal StorageOccupiedBefore { get; set; }
        public decimal StorageCapacity { get; set; }
        public decimal StorageLossQuantity { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation창고자원효과Result
    {
        public string StageCode { get; set; } = string.Empty;
        public decimal InputQuantity { get; set; }
        public decimal OutputQuantity { get; set; }
        public decimal LossQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public Simulation자원효과묶음Snapshot PendingEffectBundle { get; set; }
            = new Simulation자원효과묶음Snapshot();
    }
}
