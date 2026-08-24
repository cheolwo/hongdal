using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation시장소비DecisionTypeCodes
    {
        public const string 주민수령소비 = "MarketResidentConsumption";
    }

    public static class Simulation시장소비StateCodes
    {
        public const string Scheduled = "Scheduled";
        public const string Consumed = "Consumed";
    }

    public sealed class Simulation시장소비PreviewRequest
    {
        public string ConsumptionStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public long OrderRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public int ConsumptionDurationTicks { get; set; } = 1;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation시장소비ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation시장소비PreviewRequest Consumption { get; set; }
            = new Simulation시장소비PreviewRequest();
    }

    public sealed class Simulation시장소비PreviewSnapshot
    {
        public string ConsumptionStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string MarketFacilityStableId { get; set; } = string.Empty;
        public decimal ConsumptionQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal MarketSupplyAfterOrderFulfillment { get; set; }
        public decimal MarketSupplyAfterConsumption { get; set; }
        public bool AdditionalMarketSupplyDeductionRequired { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation시장소비Snapshot
    {
        public string ConsumptionStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public string ReservationStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string MarketFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation시장소비StateCodes.Scheduled;
        public long Revision { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int ScheduledTick { get; set; }
        public int? ConsumedTick { get; set; }
        public decimal MarketSupplyAfterOrderFulfillment { get; set; }
        public decimal? MarketSupplyObservedAtConsumption { get; set; }
        public bool AdditionalMarketSupplyDeductionApplied { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
