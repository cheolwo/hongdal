using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationIndividualOrderDecisionTypeCodes
    {
        public const string IndividualOrder = "IndividualOrder";
    }

    public static class SimulationIndividualOrderStateCodes
    {
        public const string OrderConfirmed = "OrderConfirmed";
        public const string StockReserved = "StockReserved";
        public const string Picking = "Picking";
        public const string Packed = "Packed";
        public const string PickupScheduled = "PickupScheduled";
        public const string Fulfilled = "Fulfilled";
        public const string CancellationScheduled = "CancellationScheduled";
        public const string ReadyForPickup = "ReadyForPickup";
        public const string ConsumptionScheduled = "ConsumptionScheduled";
        public const string Consumed = "Consumed";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationStockReservationStateCodes
    {
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";
        public const string Released = "Released";
    }

    public sealed class SimulationIndividualOrderPreviewRequest
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string MarketFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal RequiredLabor { get; set; }
        public int FulfillmentDurationTicks { get; set; } = 1;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationIndividualOrderConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationIndividualOrderPreviewRequest Order { get; set; }
            = new SimulationIndividualOrderPreviewRequest();
    }

    public sealed class SimulationIndividualOrderCancelRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string OrderStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationIndividualOrderPreviewSnapshot
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal AvailableBeforeReservation { get; set; }
        public decimal AvailableAfterReservation { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal RequiredLabor { get; set; }
        public decimal LaborAvailableBeforeReservation { get; set; }
        public decimal LaborAvailableAfterReservation { get; set; }
        public int FulfillmentDurationTicks { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class SimulationIndividualOrderSnapshot
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string MarketFacilityStableId { get; set; } = string.Empty;
        public decimal OrderedQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal RequiredLabor { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string? CancellationTaskStableId { get; set; }
        public int ReservedTick { get; set; }
        public int ConfirmedTick { get; set; }
        public int StockReservedTick { get; set; }
        public int? PickedTick { get; set; }
        public int? PackedTick { get; set; }
        public int? ReadyForPickupTick { get; set; }
        public string? PickupDecisionStableId { get; set; }
        public string? PickupTaskStableId { get; set; }
        public int? FulfilledTick { get; set; }
        public string? ConsumptionDecisionStableId { get; set; }
        public string? ConsumptionTaskStableId { get; set; }
        public int? ConsumedTick { get; set; }
        public int? CancelledTick { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationIndividualOrderPickupPreviewRequest
    {
        public string OrderStableId { get; set; } = string.Empty;
        public long OrderRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
        public int PickupDurationTicks { get; set; } = 1;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationIndividualOrderPickupConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationIndividualOrderPickupPreviewRequest Pickup { get; set; }
            = new SimulationIndividualOrderPickupPreviewRequest();
    }

    public sealed class SimulationStockReservationSnapshot
    {
        public string ReservationStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public string MarketFacilityStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationStockReservationStateCodes.Reserved;
        public int ReservedTick { get; set; }
        public int? ConsumedTick { get; set; }
        public int? ReleasedTick { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
