namespace Ssalddel.Domain.PublicData.Agriculture;

/// <summary>공급자 형식과 무관한 국가·연도 단위 농업 토지 사실입니다.</summary>
public sealed record 국가농업토지Data
{
    public string StableId { get; init; } = string.Empty;
    public string CountryRegionStableId { get; init; } = string.Empty;
    public string MetricCode { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public string UnitCode { get; init; } = string.Empty;
    public int ReferenceYear { get; init; }
    public DateTimeOffset EvidenceAsOfUtc { get; init; }
    public string TemporalPrecisionCode { get; init; } = "annual";
    public string SourceId { get; init; } = string.Empty;
    public string DatasetId { get; init; } = string.Empty;
    public string SourceVersion { get; init; } = string.Empty;
    public string DataRevision { get; init; } = string.Empty;
    public string QualityCode { get; init; } = string.Empty;
    public string LimitationCode { get; init; } = string.Empty;
}

public sealed record 토양DepthInterval(decimal StartCm, decimal EndCm)
{
    public bool IsValid => StartCm >= 0 && EndCm > StartCm;
}

/// <summary>
/// bounded region·grid·coverage에서 읽은 토양 사실입니다. 전 세계 raster cell 전체를 DB row로
/// 펼치지 않고 coverage reference와 공간 정밀도를 유지합니다.
/// </summary>
public sealed record 지역토양Data
{
    public string StableId { get; init; } = string.Empty;
    public string SpatialReferenceId { get; init; } = string.Empty;
    public string SpatialPrecisionCode { get; init; } = string.Empty;
    public string CrsCode { get; init; } = string.Empty;
    public decimal? GridResolutionMeters { get; init; }
    public string CoverageId { get; init; } = string.Empty;
    public string MetricCode { get; init; } = string.Empty;
    public 토양DepthInterval? Depth { get; init; }
    public string StatisticCode { get; init; } = string.Empty;
    public decimal SourceMappedValue { get; init; }
    public string SourceMappedUnitCode { get; init; } = string.Empty;
    public decimal ConversionDivisor { get; init; } = 1m;
    public decimal Value { get; init; }
    public string UnitCode { get; init; } = string.Empty;
    public DateTimeOffset EvidenceAsOfUtc { get; init; }
    public string TemporalPrecisionCode { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string DatasetId { get; init; } = string.Empty;
    public string SourceVersion { get; init; } = string.Empty;
    public string DataRevision { get; init; } = string.Empty;
    public string QualityCode { get; init; } = string.Empty;
    public string LimitationCode { get; init; } = string.Empty;
}
