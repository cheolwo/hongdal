using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class SimulationDataLineage
    {
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceDataRevision { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public abstract class 도심마트SimulationDataSnapshot
    {
        public string SnapshotStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public int AsOfTick { get; set; }
        public string ModeCode { get; set; } = SimulationModeCodes.Simulation;
        public bool IsOperationalState { get; set; }
        public SimulationDataLineage[] SourceLineage { get; set; } = Array.Empty<SimulationDataLineage>();
    }

    public sealed class 도심마트공급경영SimulationDataSnapshot : 도심마트SimulationDataSnapshot
    {
        public 도심마트공급처SimulationData[] Suppliers { get; set; } = Array.Empty<도심마트공급처SimulationData>();
        public 도심마트공급제안SimulationData[] Offers { get; set; } = Array.Empty<도심마트공급제안SimulationData>();
        public 도심마트공급계약안SimulationData[] ContractDrafts { get; set; } = Array.Empty<도심마트공급계약안SimulationData>();
    }

    public sealed class 도심마트공급처SimulationData
    {
        public string SupplierStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SupplierTypeCode { get; set; } = string.Empty;
        public decimal MaximumWeeklyQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트공급제안SimulationData
    {
        public string OfferStableId { get; set; } = string.Empty;
        public string SupplierStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal MinimumOrderQuantity { get; set; }
        public decimal MaximumWeeklyQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public int DeliveriesPerWeek { get; set; }
        public int LeadTimeTicks { get; set; }
        public int PaymentDueTicks { get; set; }
        public string QualityStandardCode { get; set; } = string.Empty;
        public string OfferRevision { get; set; } = string.Empty;
    }

    public sealed class 도심마트공급계약안SimulationData
    {
        public string ContractDraftStableId { get; set; } = string.Empty;
        public string SourceOfferStableId { get; set; } = string.Empty;
        public string SupplierStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal MinimumOrderQuantity { get; set; }
        public decimal BaseWeeklyQuantity { get; set; }
        public decimal MaximumAdditionalWeeklyQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public int DeliveriesPerWeek { get; set; }
        public int LeadTimeTicks { get; set; }
        public int PaymentDueTicks { get; set; }
        public string QualityStandardCode { get; set; } = string.Empty;
        public int StartsAtTick { get; set; }
        public int EndsAtTick { get; set; }
        public string DraftRevision { get; set; } = string.Empty;
    }

    public sealed class 도심마트수요시나리오DataSnapshot : 도심마트SimulationDataSnapshot
    {
        public string RegionStableId { get; set; } = string.Empty;
        public string PopulationBasisRevision { get; set; } = string.Empty;
        public string DemandRuleRevision { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public decimal? ProductSelectionRate { get; set; }
        public decimal? SimulationMarketShareRate { get; set; }
        public string SeasonAssumptionCode { get; set; } = string.Empty;
        public string EventAssumptionCode { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
        public 도심마트기간별수요SimulationData[] DemandSegments { get; set; } = Array.Empty<도심마트기간별수요SimulationData>();
    }

    public sealed class 도심마트기간별수요SimulationData
    {
        public string DemandSegmentStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public int StartsAtTick { get; set; }
        public int EndsAtTick { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal MaximumQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트주문SimulationDataSnapshot : 도심마트SimulationDataSnapshot
    {
        public string DemandScenarioDataRevision { get; set; } = string.Empty;
        public string GenerationRuleRevision { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public string QuantityBasisCode { get; set; } = string.Empty;
        public string SplitStrategyCode { get; set; } = string.Empty;
        public string GenerationLimitationText { get; set; } = string.Empty;
        public 도심마트주문SimulationData[] Orders { get; set; } = Array.Empty<도심마트주문SimulationData>();
        public 도심마트주문재고할당SimulationData[] Allocations { get; set; } = Array.Empty<도심마트주문재고할당SimulationData>();
    }

    public sealed class 도심마트주문SimulationData
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string SourceDemandSegmentStableId { get; set; } = string.Empty;
        public string DemandSourceTypeCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string RegionStableId { get; set; } = string.Empty;
        public int CreatedTick { get; set; }
        public int FulfillmentDueTick { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal AllocatedQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal UnfulfilledQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트주문재고할당SimulationData
    {
        public string AllocationStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public string InventoryStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long AllocationRevision { get; set; }
    }

    public static class SimulationOrderStateCodes
    {
        public const string Pending = "Pending";
        public const string Allocated = "Allocated";
        public const string PartiallyFulfilled = "PartiallyFulfilled";
        public const string Fulfilled = "Fulfilled";
        public const string Unfulfilled = "Unfulfilled";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationDemandSourceTypeCodes
    {
        public const string BaseScenarioDemand = "BaseScenarioDemand";
        public const string GroupIntentDemand = "GroupIntentDemand";
        public const string GroupConfirmedDemand = "GroupConfirmedDemand";
    }

    public static class SimulationOrderAllocationStateCodes
    {
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";
        public const string Released = "Released";
    }
}
