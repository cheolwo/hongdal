namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class 농수산물포장근거수준코드
{
    public const string 공식품목규격 = "OfficialProductSpecification";
    public const string 공식대표규격 = "OfficialRepresentativeSpecification";
    public const string 품목군추론 = "CategoryInference";
    public const string 공급자확인필요 = "SupplierConfirmationRequired";
}

public static class 농수산물포장온도코드
{
    public const string 상온 = "Ambient";
    public const string 냉장 = "Chilled";
    public const string 냉동 = "Frozen";
}

public sealed class 농수산물포장Fcl분석목록Response
{
    public string ProfileVersion { get; init; } = string.Empty;

    public int SourceYear { get; init; }

    public int TotalCount { get; init; }

    public DateTime? LatestAnalyzedAtUtc { get; init; }

    public IReadOnlyList<농수산물포장Fcl분석항목Response> Items { get; init; } = [];

    public IReadOnlyList<string> Notices { get; init; } = [];
}

public sealed class 농수산물포장Fcl분석항목Response
{
    public string CategoryCode { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string ItemCode { get; init; } = string.Empty;

    public string ItemName { get; init; } = string.Empty;

    public IReadOnlyList<string> KamisPriceComparisonUnits { get; init; } = [];

    public IReadOnlyList<string> KamisKindNames { get; init; } = [];

    public 농수산물대표포장Response RepresentativePackage { get; init; } = new();

    public IReadOnlyList<농수산물Fcl적재추정Response> ContainerEstimates { get; init; } = [];

    public string EvidenceLevelCode { get; init; } = 농수산물포장근거수준코드.공급자확인필요;

    public decimal ConfidenceScore { get; init; }

    public bool IsEstimate { get; init; } = true;

    public bool RequiresSupplierConfirmation { get; init; } = true;

    public string AssumptionNote { get; init; } = string.Empty;

    public IReadOnlyList<농수산물포장근거Response> Evidence { get; init; } = [];

    public DateTime AnalyzedAtUtc { get; init; }
}

public sealed class 농수산물대표포장Response
{
    public string PackageTypeCode { get; init; } = string.Empty;

    public string PackageUnitLabel { get; init; } = string.Empty;

    public decimal NetContentWeightKg { get; init; }

    public decimal GrossWeightKg { get; init; }

    public int? UnitsPerPackage { get; init; }

    public string? UnitCountLabel { get; init; }

    public int LengthMm { get; init; }

    public int WidthMm { get; init; }

    public int HeightMm { get; init; }

    public string TemperatureCode { get; init; } = 농수산물포장온도코드.상온;

    public bool Stackable { get; init; } = true;

    public int MaxStackLayers { get; init; }

    public string PackingMethodCode { get; init; } = string.Empty;
}

public sealed class 농수산물Fcl적재추정Response
{
    public string ContainerCode { get; init; } = string.Empty;

    public string ContainerName { get; init; } = string.Empty;

    public string TemperatureCode { get; init; } = string.Empty;

    public int InternalLengthMm { get; init; }

    public int InternalWidthMm { get; init; }

    public int InternalHeightMm { get; init; }

    public decimal NominalCapacityCbm { get; init; }

    public decimal OceanEquipmentPayloadKg { get; init; }

    public decimal? UnitedStatesRoadCargoWeightLimitKg { get; init; }

    public decimal LoadingEfficiencyRate { get; init; }

    public int OceanMaximumPackageCount { get; init; }

    public decimal OceanMaximumNetWeightKg { get; init; }

    public decimal OceanMaximumGrossWeightKg { get; init; }

    public int PracticalMaximumPackageCount { get; init; }

    public decimal PracticalMaximumNetWeightKg { get; init; }

    public decimal PracticalMaximumGrossWeightKg { get; init; }

    public long? PracticalMaximumUnitCount { get; init; }

    public decimal PlanningFillRate { get; init; }

    public int PlanningFclPackageCount { get; init; }

    public decimal PlanningFclNetWeightKg { get; init; }

    public decimal PlanningFclGrossWeightKg { get; init; }

    public long? PlanningFclUnitCount { get; init; }

    public string LimitingFactorCode { get; init; } = string.Empty;

    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed class 농수산물포장근거Response
{
    public string SourceKey { get; init; } = string.Empty;

    public string SourceName { get; init; } = string.Empty;

    public string SourceUrl { get; init; } = string.Empty;

    public string Supports { get; init; } = string.Empty;

    public string Limitation { get; init; } = string.Empty;

    public DateTime RetrievedAtUtc { get; init; }
}
