namespace Ssalddel.Contracts.Common.Community;

public static class 커뮤니티도매시장위치정밀도Codes
{
    public const string 시장대표점 = "MarketSiteRepresentative";
    public const string 도시중심점 = "CityCenter";
}

public static class 커뮤니티도매시장단계Codes
{
    public const string 공영도매시장경락 = "PublicWholesaleAuction";
    public const string 도매터미널보고 = "TerminalWholesaleReport";
}

public sealed record 커뮤니티도매시장MapDefinition(
    string Key,
    string CountryCode,
    string CountryName,
    string MarketName,
    string RegionName,
    double Latitude,
    double Longitude,
    string LocationPrecisionCode,
    string MarketStageCode,
    string MarketStageLabel,
    string SourceName,
    string SourceHref,
    string DetailHref,
    string UpdateCycle,
    DateTimeOffset EvidenceAsOfUtc);

public static class 커뮤니티도매시장MapCatalog
{
    public const string KoreaSourceHref = "https://www.nongnet.or.kr/front/index.do";
    public const string UnitedStatesSourceHref =
        "https://www.ams.usda.gov/market-news/fruit-and-vegetable-terminal-markets-standard-reports";

    private static readonly DateTimeOffset EvidenceAsOfUtc =
        new(2026, 8, 2, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<커뮤니티도매시장MapDefinition> All { get; } =
    [
        Korea("kr-seoul-garak", "서울 가락동 농수산물도매시장", "서울특별시 송파구", 37.4933, 127.1113),
        Korea("kr-seoul-gangseo", "서울 강서농산물도매시장", "서울특별시 강서구", 37.5534, 126.8207),
        Korea("kr-busan-eomgung", "부산 엄궁농산물도매시장", "부산광역시 사상구", 35.1285, 128.9560),
        Korea("kr-busan-banyeo", "부산 반여농산물도매시장", "부산광역시 해운대구", 35.2145, 129.1237),
        Korea("kr-daegu-bukbu", "대구 북부농수산물도매시장", "대구광역시 북구", 35.9018, 128.5434),
        Korea("kr-guri", "구리농수산물도매시장", "경기도 구리시", 37.6124, 127.1447),

        UnitedStates("us-asheville-nc", "Asheville Terminal Market", "Asheville, NC", 35.5951, -82.5515),
        UnitedStates("us-atlanta-ga", "Atlanta Terminal Market", "Atlanta, GA", 33.7490, -84.3880),
        UnitedStates("us-baltimore-md", "Baltimore Terminal Market", "Baltimore, MD", 39.2904, -76.6122),
        UnitedStates("us-boston-ma", "Boston Terminal Market", "Boston, MA", 42.3601, -71.0589),
        UnitedStates("us-chicago-il", "Chicago Terminal Market", "Chicago, IL", 41.8781, -87.6298),
        UnitedStates("us-columbia-sc", "Columbia Terminal Market", "Columbia, SC", 34.0007, -81.0348),
        UnitedStates("us-detroit-mi", "Detroit Terminal Market", "Detroit, MI", 42.3314, -83.0458),
        UnitedStates("us-los-angeles-ca", "Los Angeles Terminal Market", "Los Angeles, CA", 34.0522, -118.2437),
        UnitedStates("us-miami-fl", "Miami Terminal Market", "Miami, FL", 25.7617, -80.1918),
        UnitedStates("us-new-york-ny", "New York Terminal Market", "New York, NY", 40.7128, -74.0060),
        UnitedStates("us-philadelphia-pa", "Philadelphia Terminal Market", "Philadelphia, PA", 39.9526, -75.1652),
        UnitedStates("us-raleigh-nc", "Raleigh Terminal Market", "Raleigh, NC", 35.7796, -78.6382)
    ];

    public static IReadOnlyList<커뮤니티도매시장MapDefinition> ForCountry(string countryCode)
        => All.Where(item => string.Equals(item.CountryCode, countryCode, StringComparison.Ordinal))
            .ToArray();

    private static 커뮤니티도매시장MapDefinition Korea(
        string key,
        string marketName,
        string regionName,
        double latitude,
        double longitude)
        => new(
            key,
            "KR",
            "대한민국",
            marketName,
            regionName,
            latitude,
            longitude,
            커뮤니티도매시장위치정밀도Codes.시장대표점,
            커뮤니티도매시장단계Codes.공영도매시장경락,
            "공영도매시장 경락·정산",
            "농넷 · 농림축산식품부 공영도매시장 정보",
            KoreaSourceHref,
            "/information/produce-price-comparison",
            "시장별 거래일 단위",
            EvidenceAsOfUtc);

    private static 커뮤니티도매시장MapDefinition UnitedStates(
        string key,
        string marketName,
        string regionName,
        double latitude,
        double longitude)
        => new(
            key,
            "US",
            "미국",
            marketName,
            regionName,
            latitude,
            longitude,
            커뮤니티도매시장위치정밀도Codes.도시중심점,
            커뮤니티도매시장단계Codes.도매터미널보고,
            "USDA AMS 도매 터미널 보고",
            "USDA Agricultural Marketing Service",
            UnitedStatesSourceHref,
            UnitedStatesSourceHref,
            "영업일별 표준 보고서",
            EvidenceAsOfUtc);
}
