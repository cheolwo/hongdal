using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFreightTransportDecisionTypeCodes
    {
        public const string FreightReceipt = "FreightReceipt";
    }

    public sealed class SimulationFreightTransportBindingRequest
    {
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string DispatchOfferStableId { get; set; } = string.Empty;
        public string CarrierCandidateStableId { get; set; } = string.Empty;
        public string VehicleStableId { get; set; } = string.Empty;
        public decimal VehicleCapacity { get; set; }
        public string VehicleCapacityUnitCode { get; set; } = string.Empty;
    }

    public sealed class SimulationFreightTransportPreviewRequest
    {
        public SimulationFreightTransportBindingRequest Transport { get; set; }
            = new SimulationFreightTransportBindingRequest();
        public SimulationLogisticsMovementPreviewRequest Movement { get; set; }
            = new SimulationLogisticsMovementPreviewRequest();
    }

    public sealed class SimulationFreightTransportConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationFreightTransportPreviewRequest Freight { get; set; }
            = new SimulationFreightTransportPreviewRequest();
    }

    public sealed class SimulationFreightTransportPreviewSnapshot
    {
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string DispatchOfferStableId { get; set; } = string.Empty;
        public string RequestStateCode { get; set; } = string.Empty;
        public string DispatchStateCode { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string[] ExcludedOperationalEffectCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationLogisticsMovementPreviewSnapshot LogisticsMovement { get; set; }
            = new SimulationLogisticsMovementPreviewSnapshot();
    }

    public sealed class SimulationFreightReceiptPreviewRequest
    {
        public string TransportRequestStableId { get; set; } = string.Empty;
        public long TransportRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public int ReceiptDurationTicks { get; set; } = 1;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFreightReceiptConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationFreightReceiptPreviewRequest Receipt { get; set; }
            = new SimulationFreightReceiptPreviewRequest();
    }

    public sealed class SimulationFreightTransportTransitionSnapshot
    {
        public string FromStateCode { get; set; } = string.Empty;
        public string ToStateCode { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public string CauseStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationFreightTransportSnapshot
    {
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string DispatchOfferStableId { get; set; } = string.Empty;
        public string RequestStateCode { get; set; } = string.Empty;
        public string DispatchStateCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public string CarrierCandidateStableId { get; set; } = string.Empty;
        public string VehicleStableId { get; set; } = string.Empty;
        public decimal VehicleCapacity { get; set; }
        public string VehicleCapacityUnitCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string LogisticsTaskStableId { get; set; } = string.Empty;
        public string? ReceiptDecisionStableId { get; set; }
        public string? ReceiptTaskStableId { get; set; }
        public int RequestedTick { get; set; }
        public int? DispatchedTick { get; set; }
        public int? PickedUpTick { get; set; }
        public int? ArrivedAtDropoffTick { get; set; }
        public int? ReceivedTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] ExcludedOperationalEffectCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationFreightTransportTransitionSnapshot[] StateHistory { get; set; }
            = Array.Empty<SimulationFreightTransportTransitionSnapshot>();
    }
}
