using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationLogisticsMovementStateCodes
    {
        public const string Reserved = "Reserved";
        public const string InTransit = "InTransit";
        public const string ArrivedAtDestination = "ArrivedAtDestination";
    }

    public sealed class SimulationLogisticsMovementPreviewRequest
    {
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public string? SourceExportCargoHandoffStableId { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string RouteStableId { get; set; } = string.Empty;
        public string OriginFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public int RequiredRouteTicks { get; set; }
        public SimulationFreightTransportBindingRequest? FreightTransport { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationLogisticsMovementConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationLogisticsMovementPreviewRequest Movement { get; set; }
            = new SimulationLogisticsMovementPreviewRequest();
    }

    public sealed class SimulationLogisticsMovementPreviewSnapshot
    {
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public string? SourceExportCargoHandoffStableId { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string RouteStableId { get; set; } = string.Empty;
        public string OriginFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public int RequiredRouteTicks { get; set; }
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotApplySettlementState { get; set; }
        public bool ReusesExistingOutboundReservation { get; set; }
        public string DestinationStockCandidateStableId { get; set; } = string.Empty;
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class SimulationLogisticsMovementSnapshot
    {
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public string? SourceExportCargoHandoffStableId { get; set; }
        public string StateCode { get; set; } = SimulationLogisticsMovementStateCodes.Reserved;
        public long Revision { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ReservedQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string RouteStableId { get; set; } = string.Empty;
        public string OriginFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int RequiredRouteTicks { get; set; }
        public int CompletedRouteTicks { get; set; }
        public int ReservedTick { get; set; }
        public int? DepartedTick { get; set; }
        public int? ArrivedTick { get; set; }
        public string DestinationStockCandidateStableId { get; set; } = string.Empty;
        public string? DestinationReceiptStableId { get; set; }
        public int? DestinationReceiptCompletedTick { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
