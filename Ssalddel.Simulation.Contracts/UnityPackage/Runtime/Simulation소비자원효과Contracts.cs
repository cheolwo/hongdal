using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class Simulation주문예약자원효과Request
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string AvailableEffectLineStableId { get; set; } = string.Empty;
        public string ReservedEffectLineStableId { get; set; } = string.Empty;
        public string AvailableLedgerStableId { get; set; } = string.Empty;
        public string ReservedLedgerStableId { get; set; } = string.Empty;
        public decimal AvailableBeforeReservation { get; set; }
        public SimulationIndividualOrderSnapshot Order { get; set; }
            = new SimulationIndividualOrderSnapshot();
        public SimulationStockReservationSnapshot Reservation { get; set; }
            = new SimulationStockReservationSnapshot();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation주문이행자원효과Request
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string ReservedEffectLineStableId { get; set; } = string.Empty;
        public string ResidentReceivedEffectLineStableId { get; set; } = string.Empty;
        public string ReservedLedgerStableId { get; set; } = string.Empty;
        public string ResidentReceivedLedgerStableId { get; set; } = string.Empty;
        public decimal ResidentReceivedBeforeFulfillment { get; set; }
        public SimulationIndividualOrderSnapshot Order { get; set; }
            = new SimulationIndividualOrderSnapshot();
        public SimulationStockReservationSnapshot Reservation { get; set; }
            = new SimulationStockReservationSnapshot();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation주민소비자원효과Request
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string ResidentReceivedEffectLineStableId { get; set; } = string.Empty;
        public string ConsumptionRecordEffectLineStableId { get; set; } = string.Empty;
        public string ResidentReceivedLedgerStableId { get; set; } = string.Empty;
        public string ConsumptionRecordLedgerStableId { get; set; } = string.Empty;
        public decimal ResidentReceivedBeforeConsumption { get; set; }
        public decimal ConsumptionRecordBefore { get; set; }
        public SimulationIndividualOrderSnapshot Order { get; set; }
            = new SimulationIndividualOrderSnapshot();
        public Simulation시장소비Snapshot Consumption { get; set; }
            = new Simulation시장소비Snapshot();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation소비흐름자원효과Result
    {
        public string StageCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public Simulation자원효과묶음Snapshot PendingEffectBundle { get; set; }
            = new Simulation자원효과묶음Snapshot();
    }
}
