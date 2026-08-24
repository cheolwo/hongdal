using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class Simulation운송자원효과Request
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string EffectLineStableIdPrefix { get; set; } = string.Empty;
        public SimulationLogisticsMovementSnapshot Movement { get; set; }
            = new SimulationLogisticsMovementSnapshot();
        public SimulationFreightTransportSnapshot Freight { get; set; }
            = new SimulationFreightTransportSnapshot();
        public string OriginCargoLedgerStableId { get; set; } = string.Empty;
        public string VehicleCargoLedgerStableId { get; set; } = string.Empty;
        public string DestinationStagingLedgerStableId { get; set; } = string.Empty;
        public string ReceivedCargoLedgerStableId { get; set; } = string.Empty;
        public string TransportLossLedgerStableId { get; set; } = string.Empty;
        public string FuelLedgerStableId { get; set; } = string.Empty;
        public string LaborLedgerStableId { get; set; } = string.Empty;
        public decimal OriginCargoBefore { get; set; }
        public decimal VehicleCargoBefore { get; set; }
        public decimal DestinationStagingBefore { get; set; }
        public decimal ReceivedCargoBefore { get; set; }
        public decimal TransportLossBefore { get; set; }
        public decimal FuelBefore { get; set; }
        public decimal FuelConsumption { get; set; }
        public string FuelUnitCode { get; set; } = string.Empty;
        public decimal LaborBefore { get; set; }
        public decimal LaborConsumption { get; set; }
        public string LaborUnitCode { get; set; } = string.Empty;
        public decimal CargoLossQuantity { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation운송자원효과Result
    {
        public string StageCode { get; set; } = string.Empty;
        public decimal LoadedQuantity { get; set; }
        public decimal LostQuantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public Simulation자원효과묶음Snapshot PendingEffectBundle { get; set; }
            = new Simulation자원효과묶음Snapshot();
    }
}
