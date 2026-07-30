namespace Ssalddel.Domain.AgriculturalFisheries;

public static class UsdaAms공개사업체Archive상태Codes
{
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public static class UsdaAms공개사업체원천Keys
{
    public const string LocalFoodDirectories = "usda-local-food-directories";
}

public static class UsdaAms공개사업체Directory유형Codes
{
    public const string Agritourism = "Agritourism";
    public const string Csa = "Csa";
    public const string FarmersMarket = "FarmersMarket";
    public const string FoodHub = "FoodHub";
    public const string OnFarmMarket = "OnFarmMarket";

    public static IReadOnlyList<string> All { get; } =
    [
        Agritourism,
        Csa,
        FarmersMarket,
        FoodHub,
        OnFarmMarket
    ];
}

public static class UsdaAms공개사업체위치정밀도Codes
{
    public const string 도시주 = "CityState";
    public const string 주 = "StateOnly";
    public const string 미확인 = "Unparsed";
}

public sealed class UsdaAms공개사업체수집Run
{
    public long Id { get; set; }

    public string RunKey { get; set; } = Guid.NewGuid().ToString("N");

    public string StatusCode { get; set; } =
        UsdaAms공개사업체Archive상태Codes.실행중;

    public string RequestedDirectoryTypesJson { get; set; } = "[]";

    public int CompletedDirectoryCount { get; set; }

    public long FetchedCount { get; set; }

    public long InsertedCount { get; set; }

    public long UpdatedCount { get; set; }

    public long UnchangedCount { get; set; }

    public long NoLongerListedCount { get; set; }

    public long RejectedCount { get; set; }

    public string SourceUrl { get; set; } = string.Empty;

    public string SourceMessagesJson { get; set; } = "[]";

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;
}

public sealed class UsdaAms공개사업체Profile
{
    public long Id { get; set; }

    public long FirstCollectionRunId { get; set; }

    public UsdaAms공개사업체수집Run? FirstCollectionRun { get; set; }

    public long LastCollectionRunId { get; set; }

    public UsdaAms공개사업체수집Run? LastCollectionRun { get; set; }

    public string ProfileKey { get; set; } = string.Empty;

    public string SourceKey { get; set; } =
        UsdaAms공개사업체원천Keys.LocalFoodDirectories;

    public string DirectoryTypeCode { get; set; } = string.Empty;

    public string ExternalListingId { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessNameNormalized { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public string StateCode { get; set; } = string.Empty;

    public string LocationPrecisionCode { get; set; } =
        UsdaAms공개사업체위치정밀도Codes.미확인;

    public int? EstablishedYear { get; set; }

    public string LegalStatus { get; set; } = string.Empty;

    public string ProductSummary { get; set; } = string.Empty;

    public bool HasRetailChannel { get; set; }

    public bool HasWholesaleChannel { get; set; }

    public bool HasProducerService { get; set; }

    public bool HasProcurementService { get; set; }

    public bool IsCurrentlyListed { get; set; } = true;

    public DateTime? SourceUpdatedAt { get; set; }

    public string OfficialListingUrl { get; set; } = string.Empty;

    public string SourceFingerprint { get; set; } = string.Empty;

    public DateTime FirstSeenAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastChangedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<UsdaAms공개사업체취급품목> Products { get; set; } =
        new List<UsdaAms공개사업체취급품목>();
}

public sealed class UsdaAms공개사업체취급품목
{
    public long Id { get; set; }

    public long ProfileId { get; set; }

    public UsdaAms공개사업체Profile? Profile { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;
}
