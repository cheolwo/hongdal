using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationModeCodes
    {
        public const string Simulation = "Simulation";
    }

    public sealed class 경영SimulationSession생성Request
    {
        public Guid ClientRequestId { get; set; }
        public string ScenarioStableId { get; set; } = string.Empty;
        public string ScenarioDataRevision { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public int DurationTicks { get; set; } = 28;
        public SimulationWorldContext생성Request WorldContext { get; set; }
            = new SimulationWorldContext생성Request();
        public SimulationSettlementInitialStateRequest? Settlement { get; set; }
    }

    public sealed class SimulationWorldContext생성Request
    {
        public string FactionStableId { get; set; } = string.Empty;
        public string TerritoryStableId { get; set; } = string.Empty;
        public string SettlementStableId { get; set; } = string.Empty;
        public DateTimeOffset GameDateStartsOn { get; set; }
    }

    public sealed class 경영SimulationTick진행Request
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public int TickCount { get; set; } = 1;
    }

    public sealed class 경영SimulationSessionSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public Guid ClientRequestId { get; set; }
        public string ScenarioStableId { get; set; } = string.Empty;
        public string ScenarioDataRevision { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public int CurrentTick { get; set; }
        public int DurationTicks { get; set; }
        public long Revision { get; set; }
        public bool IsCompleted { get; set; }
        public string ModeCode { get; set; } = SimulationModeCodes.Simulation;
        public bool IsOperationalState { get; set; }
        public SimulationWorldContextSnapshot WorldContext { get; set; }
            = new SimulationWorldContextSnapshot();
        public SimulationDecisionSnapshot[] Decisions { get; set; }
            = Array.Empty<SimulationDecisionSnapshot>();
        public SimulationTaskSnapshot[] Tasks { get; set; }
            = Array.Empty<SimulationTaskSnapshot>();
        public SimulationEffectRecord[] Effects { get; set; }
            = Array.Empty<SimulationEffectRecord>();
        public SimulationLogisticsMovementSnapshot[] LogisticsMovements { get; set; }
            = Array.Empty<SimulationLogisticsMovementSnapshot>();
        public SimulationFreightTransportSnapshot[] FreightTransports { get; set; }
            = Array.Empty<SimulationFreightTransportSnapshot>();
        public Simulation같이주문Snapshot[] GroupOrders { get; set; }
            = Array.Empty<Simulation같이주문Snapshot>();
        public Simulation음식배달Snapshot[] FoodDeliveries { get; set; }
            = Array.Empty<Simulation음식배달Snapshot>();
        public Simulation시장소비Snapshot[] MarketConsumptions { get; set; }
            = Array.Empty<Simulation시장소비Snapshot>();
        public SimulationIndividualOrderSnapshot[] IndividualOrders { get; set; }
            = Array.Empty<SimulationIndividualOrderSnapshot>();
        public SimulationStockReservationSnapshot[] StockReservations { get; set; }
            = Array.Empty<SimulationStockReservationSnapshot>();
        public SimulationSettlementEconomySnapshot? Settlement { get; set; }
    }

    public sealed class SimulationWorldContextSnapshot
    {
        public string FactionStableId { get; set; } = string.Empty;
        public string TerritoryStableId { get; set; } = string.Empty;
        public string SettlementStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public DateTimeOffset GameDateStartsOn { get; set; }
        public DateTimeOffset GameDate { get; set; }
        public string CalendarRuleCode { get; set; } = "OneTickOneDay";
    }

    public sealed class SimulationErrorResponse
    {
        public string ErrorCode { get; set; } = string.Empty;
    }
}
