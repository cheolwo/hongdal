namespace 살뜰.Services.Options;

public sealed class ApifyInteriorProductsOptions
{
    public const string SectionName = "ApifyInteriorProducts";

    /// <summary>유료 외부 호출과 이용 조건 검토 전에는 항상 false다.</summary>
    public bool Enabled { get; set; }
    public int ActorTimeoutSeconds { get; set; } = 120;
    public int MemoryMegabytes { get; set; } = 1024;
    public int MaxDatasetItems { get; set; } = 50;
    public decimal MaxTotalChargeUsd { get; set; } = 1m;
    public string RawObservationDirectory { get; set; } = "App_Data/InteriorProductObservations";
    public ApifyInteriorProductSourceOptions[] Sources { get; set; } = [];
}

public sealed class ApifyInteriorProductSourceOptions
{
    public string SourceStableId { get; set; } = string.Empty;
    public string MarketplaceCode { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string ActorBuild { get; set; } = string.Empty;
    public string InputContractRevision { get; set; } = string.Empty;
    public string OutputContractRevision { get; set; } = string.Empty;
    public string NormalizerCode { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string TermsReviewStatus { get; set; } = "Pending";
}
