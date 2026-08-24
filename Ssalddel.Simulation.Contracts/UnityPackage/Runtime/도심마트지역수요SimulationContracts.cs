using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class 지역인구SimulationBasisDataSnapshot
    {
        public string SnapshotStableId { get; set; } = string.Empty;
        public string RegionStableId { get; set; } = string.Empty;
        public long? RegisteredPopulation { get; set; }
        public long? RegisteredHouseholdCount { get; set; }
        public string SourceKey { get; set; } = string.Empty;
        public DateTimeOffset EvidenceAsOfUtc { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string QualityStatusCode { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public bool IsPublicAggregate { get; set; }
        public bool IsSuppressed { get; set; }
    }

    public sealed class 도심마트4주DemandScenarioAssumptions
    {
        public string AssumptionStableId { get; set; } = string.Empty;
        public string AssumptionRevision { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal ProductSelectionRate { get; set; }
        public decimal SimulationMarketShareRate { get; set; }
        public string SeasonAssumptionCode { get; set; } = string.Empty;
        public string EventAssumptionCode { get; set; } = string.Empty;
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
        public 도심마트주간수요SimulationAssumption[] WeeklyDemand { get; set; } =
            Array.Empty<도심마트주간수요SimulationAssumption>();
    }

    public sealed class 도심마트주간수요SimulationAssumption
    {
        public int WeekIndex { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal MaximumQuantity { get; set; }
    }

    public sealed class 도심마트기본방문주문생성SimulationRule
    {
        public string RuleStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string QuantityBasisCode { get; set; } =
            도심마트주문생성수량기준코드.ExpectedQuantity;
        public string SplitStrategyCode { get; set; } =
            도심마트주문분할전략코드.EvenSplitWithSeededRemainder;
        public int QuantityDecimalPlaces { get; set; }
        public int FulfillmentWindowTicks { get; set; }
        public int[] OrdersPerTickPattern { get; set; } = Array.Empty<int>();
        public string LimitationText { get; set; } = string.Empty;
    }

    public static class 도심마트주문생성수량기준코드
    {
        public const string ExpectedQuantity = "ExpectedQuantity";
    }

    public static class 도심마트주문분할전략코드
    {
        public const string EvenSplitWithSeededRemainder =
            "EvenSplitWithSeededRemainder";
    }
}
