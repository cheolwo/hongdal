using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationModeCodes
    {
        public const string Simulation = "Simulation";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Contract,
        "Simulation 세션 생성 입력과 초기 World 문맥을 정의한다.",
        StepKey = "contract.session-create",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "운영 업무 생성 계약이 아니라 결정적 Simulation 세션 입력 계약이다.")]
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
        public SimulationNpcWorkforceInitialStateRequest? NpcWorkforce { get; set; }
        public Simulation공간세계InitialStateRequest? SpatialWorld { get; set; }
        public SimulationWorldInventoryInitialStateRequest? WorldInventory { get; set; }
        public SimulationSurvivalTarotInitialStateRequest? SurvivalTarot { get; set; }
        public SimulationFarmSurvivalInitialStateRequest? FarmSurvival { get; set; }
        public SimulationTeamRoleCardInitialState? TeamRoleCards { get; set; }
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
        public Simulation수출준비Snapshot[] ExportPreparations { get; set; }
            = Array.Empty<Simulation수출준비Snapshot>();
        public Simulation수출Cargo준비Snapshot[] ExportCargoPreparations { get; set; }
            = Array.Empty<Simulation수출Cargo준비Snapshot>();
        public Simulation수출Cargo인계Snapshot[] ExportCargoHandoffs { get; set; }
            = Array.Empty<Simulation수출Cargo인계Snapshot>();
        public Simulation수출항만인수Snapshot[] ExportPortReceipts { get; set; }
            = Array.Empty<Simulation수출항만인수Snapshot>();
        public Simulation수출준비성검토Snapshot[] ExportReadinessReviews { get; set; }
            = Array.Empty<Simulation수출준비성검토Snapshot>();
        public Simulation수출선적계획Snapshot[] ExportShipmentPlans { get; set; }
            = Array.Empty<Simulation수출선적계획Snapshot>();
        public Simulation수출선적실행Snapshot[] ExportShipmentExecutions { get; set; }
            = Array.Empty<Simulation수출선적실행Snapshot>();
        public SimulationTurnClosingSnapshot[] TurnClosings { get; set; }
            = Array.Empty<SimulationTurnClosingSnapshot>();
        public SimulationActiveTurnCardEffectSnapshot[] ActiveTurnCardEffects { get; set; }
            = Array.Empty<SimulationActiveTurnCardEffectSnapshot>();
        public SimulationNpcOrganizationSnapshot[] NpcOrganizations { get; set; }
            = Array.Empty<SimulationNpcOrganizationSnapshot>();
        public SimulationNpcActorSnapshot[] NpcActors { get; set; }
            = Array.Empty<SimulationNpcActorSnapshot>();
        public SimulationNpcCapabilityGrantSnapshot[] NpcCapabilityGrants { get; set; }
            = Array.Empty<SimulationNpcCapabilityGrantSnapshot>();
        public SimulationNpcWorkPolicySnapshot[] NpcWorkPolicies { get; set; }
            = Array.Empty<SimulationNpcWorkPolicySnapshot>();
        public SimulationNpcTaskAssignmentSnapshot[] NpcTaskAssignments { get; set; }
            = Array.Empty<SimulationNpcTaskAssignmentSnapshot>();
        public SimulationNpcWorkRecordSnapshot[] NpcWorkRecords { get; set; }
            = Array.Empty<SimulationNpcWorkRecordSnapshot>();
        public SimulationNpcActionProjection[] NpcActionProjections { get; set; }
            = Array.Empty<SimulationNpcActionProjection>();
        public SimulationNpcFacilityInventorySnapshot[] NpcFacilityInventories { get; set; }
            = Array.Empty<SimulationNpcFacilityInventorySnapshot>();
        public Simulation공간정의Snapshot[] SpatialDefinitions { get; set; }
            = Array.Empty<Simulation공간정의Snapshot>();
        public Simulation공간실행상태Snapshot[] SpatialRuntimeStates { get; set; }
            = Array.Empty<Simulation공간실행상태Snapshot>();
        public Simulation공간예약Snapshot[] SpatialReservations { get; set; }
            = Array.Empty<Simulation공간예약Snapshot>();
        public SimulationSettlementEconomySnapshot? Settlement { get; set; }
        public SimulationFarmSurvivalStateSnapshot? FarmSurvival { get; set; }
        public SimulationTeamRoleCardStateSnapshot? TeamRoleCards { get; set; }
        public SimulationWorldExplorationStateSnapshot? Exploration { get; set; }
        public SimulationCollectibleCardRewardStateSnapshot? CollectibleCardRewards { get; set; }
        public SimulationRegionalIncidentSnapshot[] RegionalIncidents { get; set; }
            = Array.Empty<SimulationRegionalIncidentSnapshot>();
        public SimulationNatureThreatStateSnapshot NatureThreat { get; set; }
            = new SimulationNatureThreatStateSnapshot();
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
