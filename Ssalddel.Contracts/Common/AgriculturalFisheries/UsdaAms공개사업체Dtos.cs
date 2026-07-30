namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public sealed class UsdaAms공개사업체수집요청
{
    public IReadOnlyList<string> DirectoryTypes { get; init; } = [];
}

public sealed class UsdaAms공개사업체수집응답
{
    public bool Success { get; init; } = true;

    public long CollectionRunId { get; init; }

    public IReadOnlyList<string> DirectoryTypes { get; init; } = [];

    public int CompletedDirectoryCount { get; init; }

    public long FetchedCount { get; init; }

    public long InsertedCount { get; init; }

    public long UpdatedCount { get; init; }

    public long UnchangedCount { get; init; }

    public long NoLongerListedCount { get; init; }

    public long RejectedCount { get; init; }

    public DateTime CollectedAtUtc { get; init; }

    public string SourceUrl { get; init; } = string.Empty;

    public IReadOnlyList<string> Notices { get; init; } = [];
}

public sealed class UsdaAms공개사업체조회요청
{
    public string? SearchText { get; init; }

    public string? DirectoryTypeCode { get; init; }

    public string? StateCode { get; init; }

    public string? ProductKey { get; init; }

    public bool CurrentOnly { get; init; } = true;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 30;
}

public sealed class UsdaAms공개사업체항목
{
    public string ProfileKey { get; init; } = string.Empty;

    public string SourceKey { get; init; } = string.Empty;

    public string DirectoryTypeCode { get; init; } = string.Empty;

    public string ExternalListingId { get; init; } = string.Empty;

    public string BusinessName { get; init; } = string.Empty;

    public string CityName { get; init; } = string.Empty;

    public string StateCode { get; init; } = string.Empty;

    public string LocationPrecisionCode { get; init; } = string.Empty;

    public int? EstablishedYear { get; init; }

    public string LegalStatus { get; init; } = string.Empty;

    public IReadOnlyList<string> Products { get; init; } = [];

    public bool HasRetailChannel { get; init; }

    public bool HasWholesaleChannel { get; init; }

    public bool HasProducerService { get; init; }

    public bool HasProcurementService { get; init; }

    public bool IsCurrentlyListed { get; init; }

    public DateTime? SourceUpdatedAt { get; init; }

    public DateTime LastSeenAtUtc { get; init; }

    public string OfficialListingUrl { get; init; } = string.Empty;

    public bool DiscoveryOnly { get; init; } = true;

    public bool RequiresLiveRecheck { get; init; } = true;
}

public sealed class UsdaAms공개사업체조회응답
{
    public bool Success { get; init; } = true;

    public string MarketCode { get; init; } = "US";

    public string SourceKey { get; init; } = string.Empty;

    public bool DiscoveryOnly { get; init; } = true;

    public bool IsCertificationOrPermitRegistry { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public long TotalCount { get; init; }

    public IReadOnlyList<string> Notices { get; init; } = [];

    public IReadOnlyList<UsdaAms공개사업체항목> Items { get; init; } = [];
}
