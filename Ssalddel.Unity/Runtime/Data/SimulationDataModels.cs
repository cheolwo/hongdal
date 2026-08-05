using System;

namespace Ssalddel.Unity.Data
{
    public static class 데이터품질Codes
    {
        public const string Valid = "Valid";
        public const string Stale = "Stale";
        public const string Missing = "Missing";
        public const string NotApplicable = "NotApplicable";
        public const string IncompatibleUnit = "IncompatibleUnit";
        public const string AmbiguousMapping = "AmbiguousMapping";
        public const string Rejected = "Rejected";
        public const string Fixture = "Fixture";
    }

    public static class 데이터SourceTypes
    {
        public const string PublicObservation = "PublicObservation";
        public const string Fixture = "Fixture";
        public const string Derived = "Derived";
    }

    public static class 판매방식Codes
    {
        public const string General = "GeneralSale";
        public const string Collective = "CollectiveSale";
    }

    public static class 비용발생Codes
    {
        public const string Planting = "Planting";
        public const string DailyCare = "DailyCare";
        public const string Watering = "Watering";
        public const string Harvest = "Harvest";
    }

    public static class 농업CommandCodes
    {
        public const string PlantCrop = "PlantCrop";
        public const string WaterTile = "WaterTile";
        public const string AdvanceDay = "AdvanceDay";
        public const string HarvestCrop = "HarvestCrop";
        public const string CompareSales = "CompareSales";
    }

    public static class 농업EventCodes
    {
        public const string CropPlanted = "CropPlanted";
        public const string TileWatered = "TileWatered";
        public const string DayAdvanced = "DayAdvanced";
        public const string CropGrowthAdvanced = "CropGrowthAdvanced";
        public const string GrowthStageChanged = "GrowthStageChanged";
        public const string CostAccrued = "CostAccrued";
        public const string HarvestCompleted = "HarvestCompleted";
        public const string SalesCompared = "SalesCompared";
    }

    public sealed class ScenarioManifest
    {
        public string ScenarioKey { get; set; } = string.Empty;

        public string ScenarioVersion { get; set; } = string.Empty;

        public string SchemaVersion { get; set; } = string.Empty;

        public string RuleSetKey { get; set; } = string.Empty;

        public string RuleSetVersion { get; set; } = string.Empty;

        public int DefaultRandomSeed { get; set; }

        public string ExpectedDataHash { get; set; } = string.Empty;

        public string Mode { get; set; } = "SIMULATED";
    }

    public sealed class 데이터근거Envelope
    {
        public string EvidenceId { get; set; } = string.Empty;

        public string SourceType { get; set; } = string.Empty;

        public string SourceKey { get; set; } = string.Empty;

        public string SourceRecordId { get; set; } = string.Empty;

        public string DatasetKey { get; set; } = string.Empty;

        public string DatasetVersion { get; set; } = string.Empty;

        public DateTimeOffset ObservedAt { get; set; }

        public DateTimeOffset IngestedAt { get; set; }

        public string RegionKey { get; set; } = string.Empty;

        public string MarketStageKey { get; set; } = string.Empty;

        public decimal OriginalValue { get; set; }

        public string OriginalUnit { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = string.Empty;

        public decimal NormalizedValue { get; set; }

        public string NormalizedUnit { get; set; } = string.Empty;

        public string QualityCode { get; set; } = string.Empty;

        public string FreshnessCode { get; set; } = string.Empty;

        public string LicenseOrTermsReference { get; set; } = string.Empty;

        public string[] Limitations { get; set; } = Array.Empty<string>();

        public string PayloadHash { get; set; } = string.Empty;
    }

    public sealed class ExternalCodeMapping
    {
        public string MappingKey { get; set; } = string.Empty;

        public string MappingVersion { get; set; } = string.Empty;

        public string ExternalSourceKey { get; set; } = string.Empty;

        public string ExternalCode { get; set; } = string.Empty;

        public string GameDataKey { get; set; } = string.Empty;

        public string QualityCode { get; set; } = string.Empty;

        public string EvidenceReference { get; set; } = string.Empty;
    }

    public sealed class 작물Definition
    {
        public string CropKey { get; set; } = string.Empty;

        public string VarietyKey { get; set; } = string.Empty;

        public decimal BaseDailyGrowthPoint { get; set; }

        public decimal HarvestGrowthPoint { get; set; }

        public decimal BaseYieldKg { get; set; }

        public decimal OptimalTemperatureMinC { get; set; }

        public decimal OptimalTemperatureMaxC { get; set; }

        public decimal MinimumMoistureRatio { get; set; }

        public decimal MaximumMoistureRatio { get; set; }
    }

    public sealed class 성장단계Definition
    {
        public string StageKey { get; set; } = string.Empty;

        public decimal MinimumGrowthPoint { get; set; }
    }

    public sealed class 토양Definition
    {
        public string SoilKey { get; set; } = string.Empty;

        public decimal InitialMoistureRatio { get; set; }

        public decimal RainRetentionRatioPerMillimeter { get; set; }

        public decimal DailyMoistureLossRatio { get; set; }
    }

    public sealed class 일별날씨Snapshot
    {
        public int GameDay { get; set; }

        public decimal MeanTemperatureC { get; set; }

        public decimal RainfallMm { get; set; }

        public 데이터근거Envelope TemperatureEvidence { get; set; } = new 데이터근거Envelope();

        public 데이터근거Envelope RainfallEvidence { get; set; } = new 데이터근거Envelope();
    }

    public sealed class 비용항목Definition
    {
        public string CostKey { get; set; } = string.Empty;

        public string TriggerCode { get; set; } = string.Empty;

        public long AmountKrw { get; set; }
    }

    public sealed class 시장가격관측Snapshot
    {
        public string ObservationKey { get; set; } = string.Empty;

        public string CropKey { get; set; } = string.Empty;

        public decimal PriceKrwPerKg { get; set; }

        public 데이터근거Envelope Evidence { get; set; } = new 데이터근거Envelope();
    }

    public sealed class 판매방식Rule
    {
        public string SalesChannelKey { get; set; } = string.Empty;

        public decimal PriceFactor { get; set; }

        public decimal SaleableQuantityFactor { get; set; }

        public long AdditionalCostKrw { get; set; }

        public string[] LaborDisclosureCodes { get; set; } = Array.Empty<string>();

        public string[] RiskDisclosureCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 농업ScenarioPackage
    {
        public ScenarioManifest Manifest { get; set; } = new ScenarioManifest();

        public 작물Definition Crop { get; set; } = new 작물Definition();

        public 성장단계Definition[] GrowthStages { get; set; } = Array.Empty<성장단계Definition>();

        public 토양Definition Soil { get; set; } = new 토양Definition();

        public 일별날씨Snapshot[] Weather { get; set; } = Array.Empty<일별날씨Snapshot>();

        public 비용항목Definition[] Costs { get; set; } = Array.Empty<비용항목Definition>();

        public 시장가격관측Snapshot MarketObservation { get; set; } = new 시장가격관측Snapshot();

        public 판매방식Rule[] SalesChannels { get; set; } = Array.Empty<판매방식Rule>();

        public ExternalCodeMapping[] ExternalMappings { get; set; } = Array.Empty<ExternalCodeMapping>();
    }
}
