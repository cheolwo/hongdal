using System;

namespace Ssalddel.Unity.Data
{
    public sealed class 농업SimulationCommand
    {
        public string CommandId { get; set; } = string.Empty;

        public string CommandCode { get; set; } = string.Empty;

        public int GameDay { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class 농업SimulationEvent
    {
        public long Sequence { get; set; }

        public string EventCode { get; set; } = string.Empty;

        public int GameDay { get; set; }

        public decimal Amount { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string ExplanationCode { get; set; } = string.Empty;
    }

    public sealed class 파생값Lineage
    {
        public string DerivedValueId { get; set; } = string.Empty;

        public string[] InputEvidenceIds { get; set; } = Array.Empty<string>();

        public string RuleSetKey { get; set; } = string.Empty;

        public string RuleSetVersion { get; set; } = string.Empty;

        public string RuleParametersHash { get; set; } = string.Empty;

        public int RandomSeed { get; set; }

        public int CalculatedAtGameTime { get; set; }

        public string[] ExplanationCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 판매비교Result
    {
        public string SalesChannelKey { get; set; } = string.Empty;

        public decimal HarvestQuantityKg { get; set; }

        public decimal ExpectedSaleQuantityKg { get; set; }

        public long ExpectedUnitPriceKrwPerKg { get; set; }

        public long ExpectedRevenueKrw { get; set; }

        public long ProductionCostKrw { get; set; }

        public long AdditionalSalesCostKrw { get; set; }

        public long ExpectedProfitKrw { get; set; }

        public string[] LaborDisclosureCodes { get; set; } = Array.Empty<string>();

        public string[] RiskDisclosureCodes { get; set; } = Array.Empty<string>();

        public 파생값Lineage Lineage { get; set; } = new 파생값Lineage();
    }

    public sealed class 농업SimulationState
    {
        public string ScenarioKey { get; set; } = string.Empty;

        public string ScenarioVersion { get; set; } = string.Empty;

        public int CurrentGameDay { get; set; }

        public bool IsPlanted { get; set; }

        public bool IsHarvested { get; set; }

        public decimal MoistureRatio { get; set; }

        public decimal GrowthPoint { get; set; }

        public string GrowthStageKey { get; set; } = string.Empty;

        public decimal HarvestQuantityKg { get; set; }

        public long ProductionCostKrw { get; set; }

        public long LastEventSequence { get; set; }

        public 판매비교Result[] SalesComparisons { get; set; } = Array.Empty<판매비교Result>();
    }

    public sealed class 농업SimulationRunResult
    {
        public 농업SimulationState State { get; set; } = new 농업SimulationState();

        public 농업SimulationEvent[] Events { get; set; } = Array.Empty<농업SimulationEvent>();

        public string FinalStateHash { get; set; } = string.Empty;
    }
}
