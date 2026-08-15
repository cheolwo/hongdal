using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class SimulationWarehousePutAwayPreviewRequest
    {
        public string InventoryStableId { get; set; } = string.Empty;
        public long InventoryRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public int PutAwayDurationTicks { get; set; } = 2;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWarehousePutAwayConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationWarehousePutAwayPreviewRequest PutAway { get; set; }
            = new SimulationWarehousePutAwayPreviewRequest();
    }
}
