using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal static class 농수산시세정보Catalog
{
    private static readonly string[] CommonDimensions =
    [
        "기준 품목",
        "품종·등급·규격",
        "시장 단계",
        "지역 범위",
        "관측 기간",
        "통화",
        "원 거래단위 또는 근거가 확인된 환산단위"
    ];

    public static IReadOnlyList<농수산시세정보원응답> All { get; } =
    [
        Source(
            농수산시세정보원Keys.Kamis도매조사가격,
            CommunityInformationSourceKeys.KamisPriceObservations,
            "KR",
            "한국농수산식품유통공사 KAMIS",
            "KAMIS 국내 중도매 조사 가격",
            농수산시세시장단계Codes.도매유통조사,
            "중도매 유통 조사",
            "SurveyedDistributionPrice",
            "KAMIS가 조사한 국내 중도매 유통 단계의 품목·품종·등급별 가격",
            "일별 조사와 월평균 보완",
            "대한민국 조사 지역",
            "https://www.kamis.or.kr/customer/reference/openapi_list.do",
            "https://www.kamis.or.kr/service/price/xml.do",
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "개별 도매시장 경락·정산가격이 아니며 실제 발주 견적이 아닙니다.",
                "KAMIS 소매 조사값과 시장 단계를 분리해서 비교해야 합니다."
            ]),
        Source(
            농수산시세정보원Keys.Kamis소매조사가격,
            CommunityInformationSourceKeys.KamisPriceObservations,
            "KR",
            "한국농수산식품유통공사 KAMIS",
            "KAMIS 국내 소매 조사 가격",
            농수산시세시장단계Codes.소매유통조사,
            "소매 유통 조사",
            "SurveyedDistributionPrice",
            "KAMIS가 조사한 국내 소매 단계의 품목·품종·등급별 가격",
            "일별 조사와 월평균 보완",
            "대한민국 조사 지역",
            "https://www.kamis.or.kr/customer/reference/openapi_list.do",
            "https://www.kamis.or.kr/service/price/xml.do",
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "개별 소비자의 실제 결제 영수증 평균이 아니며 판매 제안 가격이 아닙니다.",
                "규격과 조사 지역이 다른 가격을 직접 비교하지 않습니다."
            ]),
        Source(
            농수산시세정보원Keys.Mafra도매시장경락가격,
            농수산시세정보원Keys.Mafra도매시장경락가격,
            "KR",
            "농림축산식품부 공영도매시장",
            "국내 공영도매시장 경락·정산 가격",
            농수산시세시장단계Codes.도매시장경락,
            "도매시장 경락·정산",
            "AuctionSettlementPrice",
            "공영도매시장 법인에서 발생한 품목·산지·등급·규격별 경락·정산 가격",
            "거래일별",
            "도매시장·법인·원산지",
            "https://www.data.go.kr/",
            string.Empty,
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "KAMIS 중도매 조사 가격과 다른 거래 단계입니다.",
                "출하자·생산자·중도매인 식별정보는 공개 비교에 사용하지 않습니다."
            ]),
        Source(
            농수산시세정보원Keys.UsdaNass생산자수취가격,
            CommunityInformationSourceKeys.UsdaNassPriceObservations,
            "US",
            "USDA National Agricultural Statistics Service",
            "USDA NASS 생산자 수취가격",
            농수산시세시장단계Codes.생산자수취,
            "생산자 수취",
            "ProducerPriceReceived",
            "미국 생산자가 농산물 판매로 받은 전국 또는 주 단위 공식 통계 가격",
            "주로 월별",
            "미국 전국·주",
            "https://quickstats.nass.usda.gov/api",
            "https://quickstats.nass.usda.gov/api/api_GET/",
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "도매가격이나 소비자 소매가격이 아닙니다.",
                "KAMIS 유통 조사값과 가격 수준의 우열·차액을 계산하지 않습니다."
            ]),
        Source(
            농수산시세정보원Keys.UsdaAms산지출하가격,
            농수산시세정보원Keys.UsdaAms산지출하가격,
            "US",
            "USDA Agricultural Marketing Service Market News",
            "USDA AMS 산지 출하 가격",
            농수산시세시장단계Codes.산지출하,
            "산지 출하",
            "ShippingPointMarketPrice",
            "미국 주요 생산지의 출하·선적 단계 품목별 시장 가격",
            "시장일별·보고서별",
            "생산지·출하지",
            "https://www.ams.usda.gov/market-news/fruit-and-vegetable-shipping-point-market-price-reports",
            "https://marsapi.ams.usda.gov/services/v1.2/reports",
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "보고서별 Slug와 Section을 매 수집 시 공식 목록에서 확인합니다.",
                "운임·관세·소매 마진이 포함된 도착지 가격이 아닙니다."
            ]),
        Source(
            농수산시세정보원Keys.UsdaAms도매터미널가격,
            농수산시세정보원Keys.UsdaAms도매터미널가격,
            "US",
            "USDA Agricultural Marketing Service Market News",
            "USDA AMS 도매 터미널 가격",
            농수산시세시장단계Codes.도매터미널,
            "도매 터미널",
            "TerminalWholesalePrice",
            "미국 주요 도시 터미널 시장에서 1차 수취자가 도매 물량으로 판매한 가격",
            "시장일별",
            "미국 도시별 터미널 시장",
            "https://www.ams.usda.gov/market-news/fruit-and-vegetable-terminal-markets-standard-reports",
            "https://marsapi.ams.usda.gov/services/v1.2/reports",
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "원산지·품종·크기·포장·등급별 가격 범위를 보존해야 합니다.",
                "전국 평균 도매가격이나 개별 구매자 견적이 아닙니다."
            ]),
        Source(
            농수산시세정보원Keys.UsdaAms소매광고가격,
            농수산시세정보원Keys.UsdaAms소매광고가격,
            "US",
            "USDA Agricultural Marketing Service Market News",
            "USDA AMS 주간 소매 광고 가격",
            농수산시세시장단계Codes.소매광고,
            "소매 광고·프로모션",
            "AdvertisedRetailPrice",
            "미국 주요 식료품 유통업체가 웹사이트와 전단에 공개한 주간 광고 가격",
            "주별",
            "미국 전국·지역",
            "https://www.ams.usda.gov/market-news/grocerystore",
            "https://marsapi.ams.usda.gov/services/v1.2/reports",
            requiresCredential: true,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "실제 결제 거래의 평균가격이 아니라 광고·프로모션 가격입니다.",
                "광고 수와 가중평균, 품종과 포장단위를 함께 보존해야 합니다."
            ]),
        Source(
            농수산시세정보원Keys.Bls소비자평균소매가격,
            농수산시세정보원Keys.Bls소비자평균소매가격,
            "US",
            "U.S. Bureau of Labor Statistics",
            "BLS 식품 소비자 평균 소매가격",
            농수산시세시장단계Codes.소비자평균소매,
            "소비자 평균 소매",
            "ConsumerAverageRetailPrice",
            "CPI 조사 표본으로 계산한 미국 소비자의 일부 식품 품목별 월평균 소매가격",
            "월별",
            "미국 전국(2026년 관측 확인 식품 56개 계열)",
            "https://www.bls.gov/cpi/factsheets/average-prices.htm",
            "https://api.bls.gov/publicAPI/v1/timeseries/data/",
            requiresCredential: false,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "현재 수집 범위는 BLS 공식 목록에서 2026년 관측이 확인된 미국 전국 식품 56개 계열입니다.",
                "등록 키가 필요 없는 BLS Public Data API v1의 요청량·기간 제한을 따릅니다.",
                "BLS 무등록 일일 한도에 도달하면 FRED가 배포하는 동일 BLS 계열 CSV를 사용하고 수집 경로를 기록합니다.",
                "CPI 표본은 가격 수준보다 가격 변동 측정에 최적화되어 있습니다."
            ]),
        Source(
            농수산시세정보원Keys.StatCan소비자평균소매가격,
            농수산시세정보원Keys.StatCan소비자평균소매가격,
            "CA",
            "Statistics Canada",
            "캐나다 식품 월평균 소매가격",
            농수산시세시장단계Codes.소비자평균소매,
            "소비자 평균 소매",
            "ConsumerAverageRetailPrice",
            "캐나다 전체와 주별 식품 제품 단위 월평균 소매가격",
            "월별",
            "캐나다 전체·주",
            "https://www.statcan.gc.ca/en/developers/wds/user-guide",
            "https://www150.statcan.gc.ca/t1/wds/rest/",
            requiresCredential: false,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "110개 상품 중 비식품 4개를 제외한 식품 106개만 수집합니다.",
                "제품·브랜드·품질·포장 차이 때문에 KAMIS와 자동 차액을 계산하지 않습니다.",
                "캐나다 달러와 원 포장단위를 보존합니다."
            ]),
        Source(
            농수산시세정보원Keys.Eurostat농산물절대생산자가격,
            농수산시세정보원Keys.Eurostat농산물절대생산자가격,
            "EU",
            "Eurostat",
            "유럽 국가별 농산물 절대 생산자가격",
            농수산시세시장단계Codes.생산자수취,
            "생산자 수취·첫 유통",
            "ProducerFirstMarketingAbsolutePrice",
            "유럽 국가별 생산자가 거래상에게 판매하는 첫 유통단계의 농산물 연평균 절대가격",
            "연별",
            "유럽 국가",
            "https://ec.europa.eu/eurostat/web/agriculture/information-data",
            "https://ec.europa.eu/eurostat/api/dissemination/statistics/1.0/data/",
            requiresCredential: false,
            integrationStateCode: 농수산시세연동상태Codes.Archive연동됨,
            limitations:
            [
                "소비자 소매가격이 아니며 현재 최신 공통 연도는 2024년입니다.",
                "유로 표시값과 품목별 100kg·100L·100개 등의 원단위를 보존합니다.",
                "국가별 조사 방법 차이를 확인하기 전 직접 순위나 차액을 계산하지 않습니다."
            ]),
        Source(
            농수산시세정보원Keys.FranceAgriMerRnm시장가격,
            농수산시세정보원Keys.FranceAgriMerRnm시장가격,
            "FR",
            "FranceAgriMer RNM",
            "프랑스 RNM 신선식품 시장가격",
            농수산시세시장단계Codes.도매유통조사,
            "산지·도매·소매 시장 조사",
            "MultiStageMarketQuotation",
            "프랑스 신선식품의 여러 유통 단계별 가격·시세·호가",
            "시장일별",
            "프랑스 지역·시장",
            "https://rnm.franceagrimer.fr/",
            "https://www.data.gouv.fr/api/1/datasets/cotations-du-reseau-des-nouvelles-des-marches/",
            requiresCredential: false,
            integrationStateCode: 농수산시세연동상태Codes.Connector구현필요,
            limitations:
            [
                "공개 ZIP resource가 현재 공식 통계 화면으로 재지정되어 고정 다운로드 URL 검증이 필요합니다.",
                "산지·도매·소매 단계를 하나의 평균가격으로 합치지 않습니다."
            ],
            supportsStructuredApi: false),
        Source(
            농수산시세정보원Keys.MexicoSniim도매시장가격,
            농수산시세정보원Keys.MexicoSniim도매시장가격,
            "MX",
            "Secretaría de Economía SNIIM",
            "멕시코 SNIIM 도매시장 가격",
            농수산시세시장단계Codes.도매터미널,
            "도매시장",
            "WholesaleMarketQuotation",
            "멕시코 도매시장별 품목·품질·포장·원산지의 최저·최고·빈도가격",
            "영업일별",
            "멕시코 도매시장",
            "https://www.economia-sniim.gob.mx/analisis/Precios.asp",
            string.Empty,
            requiresCredential: false,
            integrationStateCode: 농수산시세연동상태Codes.Connector구현필요,
            limitations:
            [
                "공식 문서화된 REST API가 없어 조회 화면 계약과 이용 조건 검토가 필요합니다.",
                "포장단위별 가격과 kg 환산값을 분리해야 합니다."
            ],
            supportsStructuredApi: false),
        Source(
            농수산시세정보원Keys.SpainMapa산지도매가격,
            농수산시세정보원Keys.SpainMapa산지도매가격,
            "ES",
            "Ministerio de Agricultura, Pesca y Alimentación",
            "스페인 MAPA 주간 산지·도매가격",
            농수산시세시장단계Codes.산지출하,
            "산지·도매",
            "OriginWholesaleWeeklyPrice",
            "스페인 주요 식품의 주간 전국 산지·도매 평균가격",
            "주별",
            "스페인 전국·대표 시장",
            "https://www.mapa.gob.es/es/alimentacion/temas/observatorio-cadena/cadenas-valor/sistema-de-precios-om",
            string.Empty,
            requiresCredential: false,
            integrationStateCode: 농수산시세연동상태Codes.Connector구현필요,
            limitations:
            [
                "공식 REST API보다 Power BI·다운로드 파일 중심이라 고정 파일 계약 확인이 필요합니다.",
                "산지가격과 도매가격을 별도 시장 단계 관측으로 보존해야 합니다."
            ],
            supportsStructuredApi: false)
    ];

    public static 농수산시세정보원응답? Find(string? sourceKey)
        => string.IsNullOrWhiteSpace(sourceKey)
            ? null
            : All.FirstOrDefault(source => string.Equals(
                source.SourceKey,
                sourceKey.Trim(),
                StringComparison.OrdinalIgnoreCase));

    private static 농수산시세정보원응답 Source(
        string sourceKey,
        string archiveSourceKey,
        string countryCode,
        string provider,
        string displayName,
        string marketStageCode,
        string marketStageLabel,
        string priceBasisCode,
        string priceMeaning,
        string updateCycle,
        string geographyLevel,
        string documentationUrl,
        string apiBaseUrl,
        bool requiresCredential,
        string integrationStateCode,
        IReadOnlyList<string> limitations,
        bool supportsStructuredApi = true)
        => new(
            sourceKey,
            archiveSourceKey,
            countryCode,
            provider,
            displayName,
            marketStageCode,
            marketStageLabel,
            priceBasisCode,
            priceMeaning,
            updateCycle,
            geographyLevel,
            documentationUrl,
            apiBaseUrl,
            requiresCredential,
            SupportsStructuredApi: supportsStructuredApi,
            integrationStateCode,
            농수산시세수집정책Codes.명시적활성화,
            농수산시세발행정책Codes.검토후발행,
            CommonDimensions,
            limitations);
}
