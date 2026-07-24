namespace Ssalddel.Services.FoodCulture;

public sealed record JapanRegionalDataSourceDefinition(
    string Key,
    string Provider,
    string DisplayName,
    string DocumentationUrl,
    string AcquisitionType,
    bool RequiresApiKey,
    string ApiKeySetting,
    string RegionMeaning,
    string UpdateGuidance);

public static class JapanRegionalDataSourceCatalog
{
    public static IReadOnlyList<JapanRegionalDataSourceDefinition> All { get; } =
    [
        new(
            "maff-regional-cuisine",
            "일본 농림수산성(MAFF)",
            "うちの郷土料理",
            "https://www.maff.go.jp/e/policies/market/k_ryouri/",
            "HTML",
            false,
            string.Empty,
            "향토요리 페이지에 명시된 도도부현·전승지역",
            "월 1회 변경 확인. 원문·출처 시각을 보존하고 이미지는 별도 권리 검토 없이 저장하지 않습니다."),
        new(
            "maff-gi-products",
            "일본 농림수산성(MAFF)",
            "지리적 표시(GI) 등록산품",
            "https://www.maff.go.jp/j/shokusan/gi_act/register/index.html",
            "HTML/PDF",
            false,
            string.Empty,
            "등록 명세에 적힌 생산지역·보호지역",
            "월 1회 등록·말소 상태와 기준일을 함께 갱신합니다."),
        new(
            "maff-export-production-areas",
            "일본 농림수산성(MAFF)",
            "플래그십 수출산지·수출사업계획",
            "https://www.maff.go.jp/j/shokusan/export/gfp/240807.html",
            "HTML/PDF",
            false,
            string.Empty,
            "인증 산지 또는 사업계획에 명시된 지역과 운영 주체",
            "분기 1회 확인. 인증은 실제 수출액이나 거래 가능 상태로 해석하지 않습니다."),
        new(
            "japan-customs-trade-statistics",
            "일본 재무성 관세국",
            "Trade Statistics of Japan",
            "https://www.customs.go.jp/toukei/info/tsdl_e.htm",
            "CSV/e-Stat download",
            false,
            string.Empty,
            "품목·상대국·세관 단위 통관 실적. 세관 소재지를 생산지역으로 해석하지 않음",
            "월별 확정 단계와 정정 여부를 함께 저장합니다."),
        new(
            "e-stat-regional-production",
            "일본 정부통계 종합창구(e-Stat)",
            "도도부현·시정촌 농림수산 생산통계",
            "https://www.e-stat.go.jp/api/api-info/api-spec",
            "REST API v3",
            true,
            "PublicData:Japan:EStat:AppId",
            "통계표가 정의한 도도부현·시정촌 생산지역",
            "통계표 ID, 조사연도, 단위, 공표·갱신일을 함께 저장합니다."),
        new(
            "resas-regional-production",
            "일본 내각관방·내각부 RESAS",
            "지역경제분석시스템 농업생산액",
            "https://opendata.resas-portal.go.jp/docs/api/v1/agriculture/sales/forLine.html",
            "REST API",
            true,
            "PublicData:Japan:Resas:ApiKey",
            "API 응답의 도도부현·시정촌 코드 기준 지역 생산액",
            "제공연도와 단위를 보존하며 최신성은 e-Stat 원표와 대조합니다."),
        new(
            "korea-customs-hs-country-trade",
            "대한민국 관세청",
            "품목별 국가별 수출입실적",
            "https://www.data.go.kr/data/15100475/openapi.do",
            "REST API/XML",
            true,
            "PublicData:CustomsTradeStatistics:ServiceKey 또는 PublicData:DataGoKrServiceKey",
            "일본을 원산·수출국 조건으로 조회한 한국 통관 실적. 일본 도도부현은 제공하지 않음",
            "HSK 10단위 원문과 국제 공통 HS 6단위 연결을 함께 보존합니다.")
    ];
}
