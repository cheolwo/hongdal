namespace Ssalddel.Domain.AgriculturalFisheries;

public sealed class 농수산물포장Fcl분석Snapshot
{
    public long Id { get; set; }

    public string AnalysisKey { get; set; } = string.Empty;

    public string ProfileVersion { get; set; } = string.Empty;

    public int SourceYear { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string KamisPriceComparisonUnitsJson { get; set; } = "[]";

    public string KamisKindNamesJson { get; set; } = "[]";

    public string PackageTypeCode { get; set; } = string.Empty;

    public string PackageUnitLabel { get; set; } = string.Empty;

    public decimal NetContentWeightKg { get; set; }

    public decimal GrossWeightKg { get; set; }

    public int? UnitsPerPackage { get; set; }

    public string UnitCountLabel { get; set; } = string.Empty;

    public int LengthMm { get; set; }

    public int WidthMm { get; set; }

    public int HeightMm { get; set; }

    public string TemperatureCode { get; set; } = string.Empty;

    public bool Stackable { get; set; }

    public int MaxStackLayers { get; set; }

    public string PackingMethodCode { get; set; } = string.Empty;

    public string EvidenceLevelCode { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }

    public bool IsEstimate { get; set; } = true;

    public bool RequiresSupplierConfirmation { get; set; } = true;

    public string AssumptionNote { get; set; } = string.Empty;

    public string EvidenceJson { get; set; } = "[]";

    public string ContainerEstimatesJson { get; set; } = "[]";

    public DateTime AnalyzedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
