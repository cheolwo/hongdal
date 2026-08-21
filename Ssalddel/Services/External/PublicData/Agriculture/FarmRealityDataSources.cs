using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.Agriculture;

public static class FarmRealityDataSourceIds
{
    public const string Nongsaro = "nongsaro";
    public const string NongsaroWorkSchedule = "farm-working-plan-new";
    public const string NongsaroDisasterPrevention = "crop-disaster-prevention";
    public const string Kamis = "kamis";
    public const string KamisPriceObservations = "price-observations";
    public const string UsdaAms = "usda-ams-market-news";
    public const string UsdaAmsPriceObservations = "market-price-observations";
}

public sealed class FarmRealityDataSourceRegistration : IExternalDataSourceRegistration
{
    private static readonly IReadOnlyCollection<ExternalDataSourceDefinition> Definitions =
    [
        Source(FarmRealityDataSourceIds.Nongsaro,
            FarmRealityDataSourceIds.NongsaroWorkSchedule,
            "농사로 농작업일정정보", "농촌진흥청", "KR", "CropWorkReference",
            "https://www.nongsaro.go.kr", "XML", "Content revision",
            "작업군과 콘텐츠번호는 상품코드가 아니며 사람 검토 전 Simulation 규칙으로 승격하지 않습니다."),
        Source(FarmRealityDataSourceIds.Nongsaro,
            FarmRealityDataSourceIds.NongsaroDisasterPrevention,
            "농사로 농작물재해예방정보", "농촌진흥청", "KR", "CropRiskReference",
            "https://www.nongsaro.go.kr", "XML", "Content revision",
            "재해예방 설명은 사건 발생이나 생산 손실을 자동 확정하지 않습니다."),
        Source(FarmRealityDataSourceIds.Kamis,
            FarmRealityDataSourceIds.KamisPriceObservations,
            "KAMIS 농수산물 가격 관측", "한국농수산식품유통공사", "KR", "MarketPriceObservation",
            "https://www.kamis.or.kr", "JSON", "Daily",
            "원 단위와 조사 시장을 보존하며 판매가나 수익성을 자동 계산하지 않습니다."),
        Source(FarmRealityDataSourceIds.UsdaAms,
            FarmRealityDataSourceIds.UsdaAmsPriceObservations,
            "USDA AMS Market News 가격 관측", "USDA Agricultural Marketing Service", "US", "MarketPriceObservation",
            "https://www.ams.usda.gov/market-news", "JSON", "Report cadence",
            "Commodity 후보 관계이며 통화, 단위, 시장 단계와 관측일 정렬 전 KAMIS와 직접 비교하지 않습니다.")
    ];

    public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => Definitions;

    private static ExternalDataSourceDefinition Source(
        string sourceId, string datasetId, string name, string provider,
        string countryCode, string domain, string url, string format,
        string cadence, string limitations) => new()
    {
        SourceId = sourceId,
        DatasetId = datasetId,
        Name = name,
        Provider = provider,
        CountryCode = countryCode,
        DataDomain = domain,
        OfficialSourceUrl = url,
        DocumentationUrl = url,
        AccessMethod = ExternalDataAccessMethod.HttpApi,
        CredentialType = ExternalDataCredentialType.ApiKeyQuery,
        RequiresCredential = true,
        DefaultCollectionEnabled = false,
        ApiAvailable = true,
        DataFormat = format,
        TemporalResolution = cadence,
        RefreshCadence = cadence,
        RedistributionAllowed = false,
        UsageLimitations = limitations,
    };
}
