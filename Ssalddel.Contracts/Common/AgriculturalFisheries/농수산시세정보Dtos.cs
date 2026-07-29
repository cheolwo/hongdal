namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class 농수산시세정보원Keys
{
    public const string Kamis도매조사가격 = "kamis-wholesale-survey-prices";
    public const string Kamis소매조사가격 = "kamis-retail-survey-prices";
    public const string Mafra도매시장경락가격 =
        국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;
    public const string UsdaNass생산자수취가격 =
        미국농수산가격출처Keys.UsdaNassQuickStats;
    public const string UsdaAms산지출하가격 =
        "usda-ams-shipping-point-market-prices";
    public const string UsdaAms도매터미널가격 =
        "usda-ams-terminal-market-prices";
    public const string UsdaAms소매광고가격 =
        "usda-ams-retail-advertised-prices";
    public const string Bls소비자평균소매가격 =
        "bls-average-retail-food-prices";
    public const string StatCan소비자평균소매가격 =
        국제농수산가격SourceKeys.StatCan소비자평균소매가격;
    public const string Eurostat농산물절대생산자가격 =
        국제농수산가격SourceKeys.Eurostat농산물절대생산자가격;
    public const string FranceAgriMerRnm시장가격 =
        "franceagrimer-rnm-market-prices";
    public const string MexicoSniim도매시장가격 =
        "mexico-sniim-wholesale-market-prices";
    public const string SpainMapa산지도매가격 =
        "spain-mapa-origin-wholesale-prices";
}

public static class 농수산시세시장단계Codes
{
    public const string 생산자수취 = "ProducerReceived";
    public const string 산지출하 = "ShippingPoint";
    public const string 도매시장경락 = "AuctionSettlement";
    public const string 도매유통조사 = "WholesaleSurvey";
    public const string 도매터미널 = "TerminalWholesale";
    public const string 소매유통조사 = "RetailSurvey";
    public const string 소매광고 = "AdvertisedRetail";
    public const string 소비자평균소매 = "ConsumerAverageRetail";
}

public static class 농수산시세연동상태Codes
{
    public const string Archive연동됨 = "ArchiveIntegrated";
    public const string Connector구현필요 = "ConnectorRequired";
}

public static class 농수산시세수집정책Codes
{
    public const string 명시적활성화 = "ExplicitOptIn";
}

public static class 농수산시세발행정책Codes
{
    public const string 검토후발행 = "ReviewBeforePublish";
}

public static class 농수산시세비교판정Codes
{
    public const string 차원검증필요 = "DimensionMatchRequired";
    public const string 참고병렬표시 = "ReferenceOnly";
    public const string 정보원없음 = "SourceNotFound";
    public const string 잘못된요청 = "InvalidRequest";
}

public sealed record 농수산시세정보원응답(
    string SourceKey,
    string ArchiveSourceKey,
    string CountryCode,
    string Provider,
    string DisplayName,
    string MarketStageCode,
    string MarketStageLabel,
    string PriceBasisCode,
    string PriceMeaning,
    string UpdateCycle,
    string GeographyLevel,
    string DocumentationUrl,
    string ApiBaseUrl,
    bool RequiresCredential,
    bool SupportsStructuredApi,
    string IntegrationStateCode,
    string CollectionPolicyCode,
    string PublicationPolicyCode,
    IReadOnlyList<string> RequiredComparisonDimensions,
    IReadOnlyList<string> Limitations);

public sealed record 농수산시세정보원목록응답(
    DateTime GeneratedAtUtc,
    IReadOnlyList<농수산시세정보원응답> Sources,
    string BoundaryNotice);

public sealed record 농수산시세비교판정응답(
    bool Success,
    string StatusCode,
    string LeftSourceKey,
    string RightSourceKey,
    bool CanBecomeDirectlyComparable,
    bool AllowsDifferenceCalculation,
    string DisplayModeCode,
    IReadOnlyList<string> RequiredDimensions,
    IReadOnlyList<string> Notices);
