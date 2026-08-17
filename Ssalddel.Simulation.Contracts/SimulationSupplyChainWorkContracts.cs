using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationSupplyChainActionCodes
    {
        public const string WarehouseOutboundFlow = "WarehouseOutboundFlow";
        public const string MarketInspection = "MarketInspection";
        public const string MarketBackroomPutAway = "MarketBackroomPutAway";
        public const string MarketDisplayReplenishment = "MarketDisplayReplenishment";
    }

    public sealed class SimulationSupplyChainWorkPreviewRequest
    {
        public string InventoryStableId { get; set; } = string.Empty;
        public long InventoryRevision { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
        public int DurationTicks { get; set; } = 2;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationSupplyChainWorkConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationSupplyChainWorkPreviewRequest Work { get; set; }
            = new SimulationSupplyChainWorkPreviewRequest();
    }
}
