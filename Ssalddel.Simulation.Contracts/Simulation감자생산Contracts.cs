using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation생산규칙SourceTypeCodes
    {
        public const string Fixture = "Fixture";
    }

    public static class Simulation재배단위상태Codes
    {
        public const string Growing = "Growing";
        public const string HarvestReady = "HarvestReady";
        public const string Harvested = "Harvested";
    }

    public sealed class Simulation재배단위Snapshot
    {
        public string CultivationUnitStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string TileStableId { get; set; } = string.Empty;
        public string CultivationStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string CropVariantStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public decimal PhysicalAreaSquareMeters { get; set; }
        public decimal EffectiveCultivationAreaRatio { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation감자생산RuleSnapshot
    {
        public string RuleStableId { get; set; } = string.Empty;
        public long RuleRevision { get; set; }
        public string SourceTypeCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string CropVariantStableId { get; set; } = string.Empty;
        public decimal BaseYieldKilogramsPerSquareMeter { get; set; }
        public decimal MinimumEnvironmentFactor { get; set; }
        public decimal MaximumEnvironmentFactor { get; set; }
        public decimal MinimumInputFactor { get; set; }
        public decimal MaximumInputFactor { get; set; }
        public decimal MinimumFacilityFactor { get; set; }
        public decimal MaximumFacilityFactor { get; set; }
        public decimal MinimumLossFactor { get; set; }
        public decimal MaximumLossFactor { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation감자생산Request
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string EffectLineStableId { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string DecisionStateCode { get; set; } = string.Empty;
        public string CompletedTaskStableId { get; set; } = string.Empty;
        public string TaskStateCode { get; set; } = string.Empty;
        public int CompletedTick { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string HarvestLedgerStableId { get; set; } = string.Empty;
        public string EnvironmentSnapshotStableId { get; set; } = string.Empty;
        public decimal EnvironmentFactor { get; set; }
        public decimal InputFactor { get; set; }
        public decimal FacilityFactor { get; set; }
        public decimal LossFactor { get; set; }
        public Simulation재배단위Snapshot CultivationUnit { get; set; }
            = new Simulation재배단위Snapshot();
        public Simulation감자생산RuleSnapshot Rule { get; set; }
            = new Simulation감자생산RuleSnapshot();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation감자생산PreviewResult
    {
        public string CultivationUnitStableId { get; set; } = string.Empty;
        public string TileStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public int CompletedTick { get; set; }
        public decimal EffectiveCultivationAreaSquareMeters { get; set; }
        public decimal BaseHarvestQuantityKilograms { get; set; }
        public decimal ExpectedHarvestQuantityKilograms { get; set; }
        public Simulation자원효과묶음Snapshot PendingEffectBundle { get; set; }
            = new Simulation자원효과묶음Snapshot();
    }
}
