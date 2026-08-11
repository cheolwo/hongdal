using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationSettlementFacilityTypeCodes
    {
        public const string Storage = "Storage";
        public const string Market = "Market";
    }

    public static class SimulationFoodSecurityFormulaCodes
    {
        public const string AvailableFoodEquivalentDividedByDemand =
            "AvailableFoodEquivalentDividedByDemand";
    }

    public static class SimulationHarvestLotAllocationStateCodes
    {
        public const string Reserved = "Reserved";
        public const string Applied = "Applied";
    }

    public sealed class SimulationSettlementInitialStateRequest
    {
        public decimal TreasuryBalance { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal LaborCapacityTotal { get; set; }
        public decimal LaborReserved { get; set; }
        public decimal StorageCapacity { get; set; }
        public decimal StorageOccupied { get; set; }
        public string StorageUnitCode { get; set; } = string.Empty;
        public int PopulationCount { get; set; }
        public decimal PopulationFoodDemandPerTick { get; set; }
        public int GarrisonCount { get; set; }
        public decimal GarrisonFoodDemandPerTick { get; set; }
        public string FoodEquivalentUnitCode { get; set; } = string.Empty;
        public string FoodEquivalentRuleRevision { get; set; } = string.Empty;
        public SimulationSettlementDistrictRequest[] Districts { get; set; }
            = Array.Empty<SimulationSettlementDistrictRequest>();
        public SimulationSettlementFacilityRequest[] Facilities { get; set; }
            = Array.Empty<SimulationSettlementFacilityRequest>();
        public SimulationMarketSupplyRequest[] MarketSupplyByProduct { get; set; }
            = Array.Empty<SimulationMarketSupplyRequest>();
        public SimulationReserveStockLotRequest[] ReserveStockLots { get; set; }
            = Array.Empty<SimulationReserveStockLotRequest>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationSettlementDistrictRequest
    {
        public string DistrictStableId { get; set; } = string.Empty;
        public string DistrictTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationSettlementFacilityRequest
    {
        public string FacilityStableId { get; set; } = string.Empty;
        public string FacilityTypeCode { get; set; } = string.Empty;
        public string DistrictStableId { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationMarketSupplyRequest
    {
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationReserveStockLotRequest
    {
        public string StockLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string StorageFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal OutboundReservedQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal FoodEquivalentQuantity { get; set; }
        public decimal OutboundReservedFoodEquivalentQuantity { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationSettlementEconomySnapshot
    {
        public string SettlementStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long Revision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public decimal TreasuryBalance { get; set; }
        public decimal TreasuryReserved { get; set; }
        public decimal TreasuryAvailable { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal LaborCapacityTotal { get; set; }
        public decimal LaborReserved { get; set; }
        public decimal LaborAvailable { get; set; }
        public decimal StorageCapacity { get; set; }
        public decimal StorageOccupied { get; set; }
        public decimal StorageReserved { get; set; }
        public decimal StorageAvailable { get; set; }
        public string StorageUnitCode { get; set; } = string.Empty;
        public int PopulationCount { get; set; }
        public decimal PopulationFoodDemandPerTick { get; set; }
        public int GarrisonCount { get; set; }
        public decimal GarrisonFoodDemandPerTick { get; set; }
        public decimal FoodReserveEquivalent { get; set; }
        public decimal FoodDemandPerTick { get; set; }
        public decimal FoodSecurityDays { get; set; }
        public string FoodEquivalentUnitCode { get; set; } = string.Empty;
        public string FoodEquivalentRuleRevision { get; set; } = string.Empty;
        public string FoodSecurityFormulaCode { get; set; }
            = SimulationFoodSecurityFormulaCodes.AvailableFoodEquivalentDividedByDemand;
        public SimulationSettlementDistrictSnapshot[] Districts { get; set; }
            = Array.Empty<SimulationSettlementDistrictSnapshot>();
        public SimulationSettlementFacilitySnapshot[] Facilities { get; set; }
            = Array.Empty<SimulationSettlementFacilitySnapshot>();
        public SimulationMarketSupplySnapshot[] MarketSupplyByProduct { get; set; }
            = Array.Empty<SimulationMarketSupplySnapshot>();
        public SimulationResidentConsumptionSummarySnapshot[] ResidentConsumptionByProduct { get; set; }
            = Array.Empty<SimulationResidentConsumptionSummarySnapshot>();
        public SimulationReserveStockLotSnapshot[] ReserveStockLots { get; set; }
            = Array.Empty<SimulationReserveStockLotSnapshot>();
        public SimulationHarvestLotAllocationSnapshot[] HarvestLotAllocations { get; set; }
            = Array.Empty<SimulationHarvestLotAllocationSnapshot>();
        public string[] ActiveTaskStableIds { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationHarvestLotAllocationSnapshot
    {
        public string AllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long HarvestLotRevision { get; set; }
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public decimal RequiredLabor { get; set; }
        public decimal TreasuryCost { get; set; }
        public decimal? ProjectedRevenue { get; set; }
        public string StateCode { get; set; } = SimulationHarvestLotAllocationStateCodes.Reserved;
        public int ReservedTick { get; set; }
        public int? AppliedTick { get; set; }
        public string? ReserveStockLotStableId { get; set; }
        public decimal StoredQuantity { get; set; }
        public decimal FoodEquivalentQuantity { get; set; }
        public decimal OutboundReservedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationSettlementDistrictSnapshot
    {
        public string DistrictStableId { get; set; } = string.Empty;
        public string DistrictTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationSettlementFacilitySnapshot
    {
        public string FacilityStableId { get; set; } = string.Empty;
        public string FacilityTypeCode { get; set; } = string.Empty;
        public string DistrictStableId { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationMarketSupplySnapshot
    {
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationResidentConsumptionSummarySnapshot
    {
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public int ConsumptionCount { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationReserveStockLotSnapshot
    {
        public string StockLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string StorageFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal OutboundReservedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal FoodEquivalentQuantity { get; set; }
        public decimal OutboundReservedFoodEquivalentQuantity { get; set; }
        public decimal AvailableFoodEquivalentQuantity { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

}
