using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation음식배달DecisionTypeCodes
    {
        public const string 주문접수 = "FoodDeliveryOrderAcceptance";
        public const string 수령확인 = "FoodDeliveryReceiptConfirmation";
    }

    public sealed class Simulation음식배달PreviewRequest
    {
        public string FoodOrderStableId { get; set; } = string.Empty;
        public string MenuItemStableId { get; set; } = string.Empty;
        public string RestaurantFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string DeliveryScopeStableId { get; set; } = string.Empty;
        public string OrdererStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = "serving";
        public int PreparationDurationTicks { get; set; } = 2;
        public int DeliveryDurationTicks { get; set; } = 2;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation음식배달ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation음식배달PreviewRequest FoodDelivery { get; set; }
            = new Simulation음식배달PreviewRequest();
    }

    public sealed class Simulation음식배달수령PreviewRequest
    {
        public string FoodOrderStableId { get; set; } = string.Empty;
        public long FoodOrderRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public int ReceiptDurationTicks { get; set; } = 1;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation음식배달수령ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation음식배달수령PreviewRequest Receipt { get; set; }
            = new Simulation음식배달수령PreviewRequest();
    }

    public sealed class Simulation음식배달PreviewSnapshot
    {
        public string FoodOrderStableId { get; set; } = string.Empty;
        public string SuggestedStateCode { get; set; } = string.Empty;
        public int TotalDurationTicks { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] ExcludedOperationalEffectCodes { get; set; } = Array.Empty<string>();
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation음식배달상태전이Snapshot
    {
        public string FromStateCode { get; set; } = string.Empty;
        public string ToStateCode { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public string CauseStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation음식배달Snapshot
    {
        public string FoodOrderStableId { get; set; } = string.Empty;
        public string MenuItemStableId { get; set; } = string.Empty;
        public string RestaurantFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string DeliveryScopeStableId { get; set; } = string.Empty;
        public string OrdererStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public int PreparationDurationTicks { get; set; }
        public int DeliveryDurationTicks { get; set; }
        public int TotalDurationTicks { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string? ReceiptDecisionStableId { get; set; }
        public string? ReceiptTaskStableId { get; set; }
        public int AcceptedTick { get; set; }
        public int? CookingStartedTick { get; set; }
        public int? ReadyForPickupTick { get; set; }
        public int? DispatchCandidateTick { get; set; }
        public int? PickedUpTick { get; set; }
        public int? DeliveredTick { get; set; }
        public int? ReceivedTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] ExcludedOperationalEffectCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public Simulation음식배달상태전이Snapshot[] StateHistory { get; set; }
            = Array.Empty<Simulation음식배달상태전이Snapshot>();
    }
}
