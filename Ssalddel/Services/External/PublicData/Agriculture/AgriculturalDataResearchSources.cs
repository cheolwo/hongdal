using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.Agriculture;

/// <summary>P6-A 조사 완료 source입니다. 실제 collector 등록과 수집 활성화는 P6-B입니다.</summary>
public sealed class AgriculturalDataResearchSourceRegistration : IExternalDataSourceRegistration
{
    private static readonly IReadOnlyCollection<ExternalDataSourceDefinition> Definitions =
    [
        new()
        {
            SourceId = "fao-faostat",
            DatasetId = "land-use-rl",
            Name = "FAOSTAT Land Use",
            Provider = "Food and Agriculture Organization of the United Nations",
            CountryCode = "GLOBAL",
            DataDomain = "AgriculturalLand",
            OfficialSourceUrl = "https://www.fao.org/faostat/en/#data/RL",
            DocumentationUrl = "https://www.fao.org/statistics/events/events-detail/land-use.-june-2025-update/en",
            AccessMethod = ExternalDataAccessMethod.DownloadFile,
            CredentialType = ExternalDataCredentialType.None,
            RequiresCredential = false,
            DefaultCollectionEnabled = false,
            ApiAvailable = false,
            DataFormat = "CSV/ZIP bulk download; exact endpoint and schema require P6-B verification",
            SpatialResolution = "Country, region and global aggregate",
            TemporalResolution = "Annual",
            RefreshCadence = "Annual; June release calendar",
            License = "CC BY 4.0 with FAO database additional terms",
            RedistributionAllowed = true,
            AttributionRequirement = "FAO citation must include database/dataset, last update year, access date, URL and CC BY 4.0",
            UsageLimitations = "Land Use reports categories and indicators by country and year. Provider area/item/element codes require explicit mapping; land use is not parcel geometry or soil condition.",
            LastVerifiedDate = new DateOnly(2026, 8, 9),
        },
        new()
        {
            SourceId = "isric-soilgrids",
            DatasetId = "soilgrids-250m-properties",
            Name = "SoilGrids 250m soil property coverages",
            Provider = "ISRIC - World Soil Information",
            CountryCode = "GLOBAL",
            DataDomain = "Soil",
            OfficialSourceUrl = "https://docs.isric.org/globaldata/soilgrids/",
            DocumentationUrl = "https://docs.isric.org/globaldata/soilgrids/wcs.html",
            AccessMethod = ExternalDataAccessMethod.OgcWcs,
            CredentialType = ExternalDataCredentialType.None,
            RequiresCredential = false,
            DefaultCollectionEnabled = false,
            ApiAvailable = true,
            DataFormat = "WCS 2.0 XML metadata and GeoTIFF coverage; VRT/GeoTIFF via anonymous WebDAV",
            SpatialResolution = "250m raster grid; requested bounded coverage",
            TemporalResolution = "Rolling release model output, not a periodic field observation",
            RefreshCadence = "Rolling release; preserve coverage and source revision",
            License = "CC BY 4.0",
            RedistributionAllowed = true,
            AttributionRequirement = "ISRIC SoilGrids attribution and change indication",
            UsageLimitations = "Model predictions with uncertainty, six depth intervals and large raster volume. REST API is paused; use WCS metadata/subsets or WebDAV. Do not treat predictions as measured parcel soil tests.",
            LastVerifiedDate = new DateOnly(2026, 8, 9),
        },
    ];

    public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => Definitions;
}

public sealed record SoilGridsPropertyDefinition(
    string PropertyCode,
    string MetricCode,
    string Description,
    string MappedUnitCode,
    decimal ConversionDivisor,
    string ConventionalUnitCode);

public sealed record SoilGridsCoverageDefinition(
    string CoverageId,
    SoilGridsPropertyDefinition Property,
    decimal DepthStartCm,
    decimal DepthEndCm,
    string StatisticCode);

public static class SoilGridsMetadataCatalog
{
    public static readonly IReadOnlyDictionary<string, SoilGridsPropertyDefinition> Properties =
        new Dictionary<string, SoilGridsPropertyDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["bdod"] = new("bdod", "soil.bulk-density", "Bulk density", "cg/cm3", 100m, "kg/dm3"),
            ["cec"] = new("cec", "soil.cation-exchange-capacity", "CEC buffered at pH7", "mmol(c)/kg", 10m, "cmol(c)/kg"),
            ["cfvo"] = new("cfvo", "soil.coarse-fragments", "Coarse fragments", "cm3/dm3", 10m, "vol%"),
            ["clay"] = new("clay", "soil.clay-content", "Clay", "g/kg", 10m, "%"),
            ["nitrogen"] = new("nitrogen", "soil.total-nitrogen", "Total nitrogen", "cg/kg", 100m, "g/kg"),
            ["ocd"] = new("ocd", "soil.organic-carbon-density", "Organic carbon density", "hg/m3", 10m, "kg/m3"),
            ["ocs"] = new("ocs", "soil.organic-carbon-stock", "Organic carbon stock", "t/ha", 10m, "kg/m2"),
            ["soc"] = new("soc", "soil.organic-carbon", "Soil organic carbon", "dg/kg", 10m, "g/kg"),
            ["phh2o"] = new("phh2o", "soil.ph-water", "pH in water", "phx10", 10m, "ph"),
            ["sand"] = new("sand", "soil.sand-content", "Sand", "g/kg", 10m, "%"),
            ["silt"] = new("silt", "soil.silt-content", "Silt", "g/kg", 10m, "%"),
            ["wv0010"] = new("wv0010", "soil.volumetric-water-content-10kpa", "Volumetric water content at 10kPa", "1e-3-cm3/cm3", 10m, "%"),
        };

    public static readonly IReadOnlyCollection<(decimal StartCm, decimal EndCm)> DepthIntervals =
    [
        (0m, 5m),
        (5m, 15m),
        (15m, 30m),
        (30m, 60m),
        (60m, 100m),
        (100m, 200m),
    ];

    public static bool TryParseCoverageId(
        string coverageId,
        out SoilGridsCoverageDefinition? definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(coverageId)) return false;
        var parts = coverageId.Trim().Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || !Properties.TryGetValue(parts[0], out var property)) return false;
        var depthParts = parts[1].Replace("cm", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (depthParts.Length != 2
            || !decimal.TryParse(depthParts[0], out var start)
            || !decimal.TryParse(depthParts[1], out var end)
            || !DepthIntervals.Contains((start, end)))
            return false;
        var statisticCode = parts[2] switch
        {
            "Q0.05" => "quantile-0.05",
            "Q0.5" => "median",
            "Q0.95" => "quantile-0.95",
            "mean" => "mean",
            "uncertainty" => "uncertainty",
            _ => string.Empty,
        };
        if (statisticCode.Length == 0) return false;
        definition = new SoilGridsCoverageDefinition(
            coverageId.Trim(), property, start, end, statisticCode);
        return true;
    }
}
