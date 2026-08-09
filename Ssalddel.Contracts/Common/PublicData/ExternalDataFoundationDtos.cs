namespace Ssalddel.Contracts.Common.PublicData;

public enum ExternalDataAccessMethod
{
    HttpApi,
    DownloadFile,
    ManualImport,
    ObjectStorageDrop,
    OgcWcs,
    WebDav,
}

public enum ExternalDataCredentialType
{
    None,
    ApiKeyHeader,
    ApiKeyQuery,
    BearerToken,
    OAuth,
}

public sealed record ExternalDataSourceDefinition
{
    public string SourceId { get; init; } = string.Empty;
    public string DatasetId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string DataDomain { get; init; } = string.Empty;
    public string OfficialSourceUrl { get; init; } = string.Empty;
    public string DocumentationUrl { get; init; } = string.Empty;
    public ExternalDataAccessMethod AccessMethod { get; init; }
    public ExternalDataCredentialType CredentialType { get; init; }
    public bool RequiresCredential { get; init; }
    public bool DefaultCollectionEnabled { get; init; }
    public bool ApiAvailable { get; init; }
    public string DataFormat { get; init; } = string.Empty;
    public string SpatialResolution { get; init; } = string.Empty;
    public string TemporalResolution { get; init; } = string.Empty;
    public string RefreshCadence { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public bool RedistributionAllowed { get; init; }
    public string AttributionRequirement { get; init; } = string.Empty;
    public string UsageLimitations { get; init; } = string.Empty;
    public DateOnly? LastVerifiedDate { get; init; }
    public IReadOnlyList<string> CredentialReferences { get; init; } = [];
}

public sealed record ExternalDataSourceCatalogResponse
{
    public IReadOnlyList<ExternalDataSourceDefinition> Items { get; init; } = [];
}

public sealed record ExternalDataEvidence
{
    public string SourceId { get; init; } = string.Empty;
    public string DatasetId { get; init; } = string.Empty;
    public DateTimeOffset EvidenceAsOfUtc { get; init; }
    public DateTimeOffset CollectedAtUtc { get; init; }
    public string SourceVersion { get; init; } = string.Empty;
    public string SpatialPrecisionCode { get; init; } = string.Empty;
    public string TemporalPrecisionCode { get; init; } = string.Empty;
    public string QualityCode { get; init; } = string.Empty;
    public string LimitationCode { get; init; } = string.Empty;
    public string RawContentHashSha256 { get; init; } = string.Empty;
}
