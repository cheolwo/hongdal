using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.WorldBank;

public static class WorldBank경지면적Dataset
{
    public const string SourceId = "world-bank-indicators";
    public const string DatasetId = "wdi-ag-lnd-arbl-ha";
    public const string IndicatorCode = "AG.LND.ARBL.HA";
    public const string MetricCode = "agricultural-land.arable-area";
    public const string UnitCode = "ha";
}

public sealed class WorldBank경지면적Options
{
    public const string SectionName = "ExternalData:WorldBank:ArableLand";

    public string BaseUrl { get; set; } = "https://api.worldbank.org/v2/";

    // P6는 대한민국 하나로 제한한다. P7에서 지표 비교 가능성을 확인한 뒤 USA/CHN을 추가한다.
    public string[] CountryCodes { get; set; } = ["KOR"];

    // P6-B는 국가별 최신 비결측 관측 한 건만 수집한다.
    public int MostRecentValues { get; set; } = 1;

    public int MaxResponseBytes { get; set; } = 5 * 1024 * 1024;
}

public sealed class WorldBank경지면적SourceRegistration : IExternalDataSourceRegistration
{
    private static readonly ExternalDataSourceDefinition Definition = new()
    {
        SourceId = WorldBank경지면적Dataset.SourceId,
        DatasetId = WorldBank경지면적Dataset.DatasetId,
        Name = "World Development Indicators - Arable land (hectares)",
        Provider = "World Bank / Food and Agriculture Organization of the United Nations",
        CountryCode = "GLOBAL",
        DataDomain = "AgriculturalLand",
        OfficialSourceUrl = "https://data.worldbank.org/indicator/AG.LND.ARBL.HA",
        DocumentationUrl = "https://datahelpdesk.worldbank.org/knowledgebase/articles/889392",
        AccessMethod = ExternalDataAccessMethod.HttpApi,
        CredentialType = ExternalDataCredentialType.None,
        RequiresCredential = false,
        DefaultCollectionEnabled = false,
        ApiAvailable = true,
        DataFormat = "JSON",
        SpatialResolution = "Country",
        TemporalResolution = "Annual",
        RefreshCadence = "Provider-managed annual series; preserve API lastupdated metadata",
        License = "CC BY 4.0",
        RedistributionAllowed = true,
        AttributionRequirement = "World Bank Open Data, indicator AG.LND.ARBL.HA; original source FAO",
        UsageLimitations = "National annual aggregate. It does not identify parcels, soil quality, crop suitability or currently cultivated land.",
        LastVerifiedDate = new DateOnly(2026, 8, 9),
    };

    public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => [Definition];
}
