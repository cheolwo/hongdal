using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal sealed record 국제농수산가격SourceDefinition(
    string SourceKey,
    string Provider,
    string DisplayName,
    string CountryScopeCode,
    string MarketStageCode,
    string FrequencyCode,
    string DocumentationUrl,
    string ApiBaseUrl,
    bool RequiresCredential,
    IReadOnlyList<string> DatasetCodes,
    string LatestVerifiedPeriod,
    IReadOnlyList<string> Limitations);

internal static class 국제농수산가격SourceCatalog
{
    public static IReadOnlyList<국제농수산가격SourceDefinition> All { get; } =
    [
        new(
            국제농수산가격SourceKeys.StatCan소비자평균소매가격,
            "Statistics Canada",
            "캐나다 월평균 식품 소매가격",
            "CA",
            농수산시세시장단계Codes.소비자평균소매,
            "Monthly",
            "https://www.statcan.gc.ca/en/developers/wds/user-guide",
            "https://www150.statcan.gc.ca/t1/wds/rest/",
            false,
            ["18100245"],
            "2026-05",
            [
                "공식 표의 110개 상품 중 개인위생·세제 4개를 제외한 식품 106개를 수집합니다.",
                "제품 회전, 품질·용량 변화와 지역별 브랜드 구성 차이 때문에 지역 간 가격 수준 비교에 주의해야 합니다.",
                "캐나다 달러와 원 포장단위를 보존하며 환율·중량 환산은 별도 검토 전 수행하지 않습니다."
            ]),
        new(
            국제농수산가격SourceKeys.Eurostat농산물절대생산자가격,
            "Eurostat",
            "유럽 국가별 농산물 절대 생산자가격",
            "EU",
            농수산시세시장단계Codes.생산자수취,
            "Annual",
            "https://ec.europa.eu/eurostat/web/agriculture/information-data",
            "https://ec.europa.eu/eurostat/api/dissemination/statistics/1.0/data/",
            false,
            ["apri_ap_crpouta", "apri_ap_anouta"],
            "2024",
            [
                "생산자에서 거래상으로 넘어가는 첫 유통단계의 연평균 절대가격이며 소비자 소매가격이 아닙니다.",
                "2026년 현재 공식 절대가격의 최신 공통 연도는 2024년으로 시차가 있습니다.",
                "유로 표시값을 수집하고 품목별 공식 원단위를 보존하며 KAMIS와 직접 차액을 계산하지 않습니다."
            ])
    ];

    public static 국제농수산가격SourceDefinition? Find(string? sourceKey)
        => string.IsNullOrWhiteSpace(sourceKey)
            ? null
            : All.FirstOrDefault(item => string.Equals(
                item.SourceKey,
                sourceKey.Trim(),
                StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<국제농수산가격Source응답> ToResponse()
        => All.Select(item => new 국제농수산가격Source응답(
                item.SourceKey,
                item.Provider,
                item.DisplayName,
                item.CountryScopeCode,
                item.MarketStageCode,
                item.FrequencyCode,
                item.DocumentationUrl,
                item.ApiBaseUrl,
                item.RequiresCredential,
                item.DatasetCodes,
                item.LatestVerifiedPeriod,
                item.Limitations))
            .ToArray();
}
