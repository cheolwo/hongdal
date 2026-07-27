using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Common.Documents;

public static class 원장관행문서카탈로그
{
    private const string TradeGovCommonDocuments = "https://www.trade.gov/common-export-documents";
    private const string TradeGovSpecialDocuments = "https://www.trade.gov/special-documents";
    private const string KoreaCustomsImportDocuments = "https://unipass.customs.go.kr/clip/lworsrch/openULS0101032Q.do?lworAdmnRulNo=36893&lworUtNo=15";
    private const string MfdsImportFoodDocuments = "https://impfood.mfds.go.kr/CFCII04F01";
    private const string AphisPhytosanitaryCertificates = "https://www.aphis.usda.gov/plant-exports/certification";

    public static IReadOnlyList<원장관행문서카탈로그항목Dto> 전체목록 { get; } =
    [
        항목(
            원장관행문서종류코드.견적요청서,
            "견적요청서",
            "REQUEST FOR QUOTATION",
            원장관행문서생성모드코드.원장초안,
            원장관행문서발급주체코드.주문자집단,
            [CommunityLedgerTemplateKeys.GroupOrder],
            ["상품명", "집계 수량", "거래 단위", "희망 납기·응답기한"],
            ["공동구매개별주문원장Service", "문서관리Service"]),
        항목(
            원장관행문서종류코드.구매주문서,
            "구매주문서",
            "PURCHASE ORDER",
            원장관행문서생성모드코드.원장초안,
            원장관행문서발급주체코드.주문자집단,
            [CommunityLedgerTemplateKeys.GroupOrder],
            ["구매자·공급자", "품목", "수량·거래 단위", "단가·통화", "납품·결제 조건"],
            ["공동구매개별주문원장Service", "주문원장서명UseCase", "문서관리Service"]),
        항목(
            원장관행문서종류코드.같이주문집계표,
            "같이 주문 집계표",
            "GROUP ORDER SHEET",
            원장관행문서생성모드코드.원장초안,
            원장관행문서발급주체코드.주문자집단,
            [CommunityLedgerTemplateKeys.GroupOrder],
            ["확정 주문자 수", "합산 수량", "거래 단위", "도착 창고 수"],
            ["공동구매개별주문원장Service", "SsalddelDocumentOutputService", "문서관리Service"]),
        항목(
            원장관행문서종류코드.계약검토자료서,
            "계약 검토 자료서",
            "CONTRACT REVIEW SHEET",
            원장관행문서생성모드코드.원장초안,
            원장관행문서발급주체코드.주문자집단,
            [CommunityLedgerTemplateKeys.GroupOrder],
            ["계약 당사자 후보", "수량·가격", "납품·결제", "취소·환불", "서명 상태"],
            ["수입식품공동주문계약검토계획기", "ContractElectronicSignaturePlanner", "주문원장서명UseCase"]),
        항목(
            원장관행문서종류코드.프로포마송장자료서,
            "프로포마 송장 발급 자료서",
            "PRO FORMA INVOICE DATA SHEET",
            원장관행문서생성모드코드.발급자확인초안,
            원장관행문서발급주체코드.판매자수출자,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["판매자·수입자", "품목·수량·가격", "Incoterms", "유효기한", "결제 조건"],
            ["같이수입준비원장Service", "문서관리Service"],
            [TradeGov공통("프로포마 송장은 선적 전 협상·견적 문서이며 최종 상업송장의 기초가 됩니다.")]),
        항목(
            원장관행문서종류코드.상업송장,
            "상업송장",
            "COMMERCIAL INVOICE",
            원장관행문서생성모드코드.발급자확인초안,
            원장관행문서발급주체코드.판매자수출자,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["판매자·수입자", "품목·수량·단가·금액", "통화", "원산지", "거래 조건"],
            ["같이수입준비원장Service", "공동구매수입물류정규화Service", "문서관리Service"],
            [TradeGov공통("세관 과세가격 판단에 쓰이는 주요 거래 문서입니다."), 관세청근거()]),
        항목(
            원장관행문서종류코드.포장명세서,
            "포장명세서",
            "PACKING LIST",
            원장관행문서생성모드코드.발급자확인초안,
            원장관행문서발급주체코드.판매자수출자,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["포장별 품목·수량", "포장 개수·종류", "순·총중량", "치수·용적", "Shipping Mark"],
            ["같이수입준비원장Service", "SsalddelDocumentOutputService", "문서관리Service"],
            [TradeGov공통("상업송장을 대체하지 않으며 실제 포장·중량과 일치해야 합니다."), 관세청근거()]),
        항목(
            원장관행문서종류코드.선적인도지시서,
            "선적인도지시서",
            "SHIPPER'S LETTER OF INSTRUCTION",
            원장관행문서생성모드코드.발급자확인초안,
            원장관행문서발급주체코드.판매자수출자,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["수출자·수하인", "포워더", "출발·도착지", "운송 방식", "화물·서류 지시"],
            ["같이수입준비ProcessManager", "공동구매해외선적추적UseCase", "문서관리Service"],
            [TradeGov특수("수출자가 포워더에게 전달하는 선적 지시 문서입니다.")]),
        항목(
            원장관행문서종류코드.원산지증명준비자료서,
            "원산지증명 준비 자료서",
            "CERTIFICATE OF ORIGIN DATA SHEET",
            원장관행문서생성모드코드.외부발급준비자료,
            원장관행문서발급주체코드.권한기관,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["생산자·수출자", "품목·HS", "원산지 판정 근거", "FTA·비특혜 구분"],
            ["같이수입준비원장Service", "화주HS코드검토조회UseCase", "문서관리Service"],
            [TradeGov특수("원산지증명서는 수입국 요구와 협정에 따라 권한 있는 주체의 검증이 필요합니다.")]),
        항목(
            원장관행문서종류코드.수입통관서류점검표,
            "수입통관 서류 점검표",
            "IMPORT CUSTOMS DOCUMENT CHECKLIST",
            원장관행문서생성모드코드.외부발급준비자료,
            원장관행문서발급주체코드.관세사수입자,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["송품장", "가격신고 자료", "B/L·AWB", "포장명세서", "원산지·수입요건 서류"],
            ["화주HS코드검토조회UseCase", "공동구매해외선적추적UseCase", "공동구매해외선적통관동기화Service"],
            [관세청근거()]),
        항목(
            원장관행문서종류코드.수입식품서류점검표,
            "수입식품 서류 점검표",
            "IMPORT FOOD DOCUMENT CHECKLIST",
            원장관행문서생성모드코드.외부발급준비자료,
            원장관행문서발급주체코드.관세사수입자,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["품목·국가별 수입요건", "위생·검사·검역 증명", "해외제조업소·원재료 근거", "보관 서류"],
            ["같이수입준비원장Service", "화주HS코드검토조회UseCase", "수입식품해외제조업소조회Service"],
            [식약처근거(), Aphis근거()]),
        항목(
            원장관행문서종류코드.선적문서참조표,
            "선적 문서 등록·참조표",
            "SHIPMENT DOCUMENT REFERENCE SHEET",
            원장관행문서생성모드코드.외부발급준비자료,
            원장관행문서발급주체코드.운송사포워더,
            [CommunityLedgerTemplateKeys.GroupImport],
            ["문서관리번호", "B/L·AWB 유형·번호", "운송사", "선박·항공편", "출발·도착항"],
            ["공동구매해외선적추적UseCase", "공동구매수입물류정규화Service", "문서관리Service"],
            [TradeGov공통("B/L은 화주와 운송인 간 운송계약 문서이고 AWB는 항공운송사가 발행합니다."), 관세청근거()])
    ];

    public static IReadOnlyList<원장관행문서카탈로그항목Dto> 원장종류별(string 원장템플릿Key)
        => 전체목록
            .Where(item => item.지원원장템플릿Key목록.Contains(원장템플릿Key, StringComparer.OrdinalIgnoreCase))
            .ToArray();

    public static 원장관행문서카탈로그항목Dto? 찾기(string 문서종류코드)
        => 전체목록.FirstOrDefault(item => string.Equals(
            item.문서종류코드,
            문서종류코드,
            StringComparison.OrdinalIgnoreCase));

    private static 원장관행문서카탈로그항목Dto 항목(
        string 코드,
        string 문서명,
        string 영문명,
        string 생성모드,
        string 발급주체,
        IReadOnlyList<string> 지원원장,
        IReadOnlyList<string> 원천정보,
        IReadOnlyList<string> 연계모듈,
        IReadOnlyList<원장관행문서공식근거Dto>? 공식근거 = null)
        => new()
        {
            문서종류코드 = 코드,
            문서명 = 문서명,
            영문문서명 = 영문명,
            생성모드코드 = 생성모드,
            발급주체코드 = 발급주체,
            원장초안생성가능여부 = true,
            외부발급원본대체가능여부 = false,
            지원원장템플릿Key목록 = 지원원장,
            원천정보목록 = 원천정보,
            연계모듈목록 = 연계모듈,
            공식근거목록 = 공식근거 ?? [],
            경계안내 = 생성모드 == 원장관행문서생성모드코드.외부발급준비자료
                ? "플랫폼은 준비자료와 누락 점검표만 만듭니다. 운송사·관세사·상공회의소·검역기관 등 권한 있는 발급자의 원본을 대체하지 않습니다."
                : "원장 근거를 재사용한 검토용 초안입니다. 발급 주체 확인, 서명 또는 전문 검토 전에는 운영 문서로 사용할 수 없습니다."
        };

    private static 원장관행문서공식근거Dto TradeGov공통(string 적용범위)
        => 근거("U.S. International Trade Administration", "Common Export Documents", TradeGovCommonDocuments, 적용범위);

    private static 원장관행문서공식근거Dto TradeGov특수(string 적용범위)
        => 근거("U.S. International Trade Administration", "Special Export Documents", TradeGovSpecialDocuments, 적용범위);

    private static 원장관행문서공식근거Dto 관세청근거()
        => 근거("대한민국 관세청", "수입통관 사무처리에 관한 고시 제15조", KoreaCustomsImportDocuments, "서류제출 대상 수입신고의 송품장, 가격신고서, B/L·AWB, 포장명세서 및 해당 서류");

    private static 원장관행문서공식근거Dto 식약처근거()
        => 근거("대한민국 식품의약품안전처", "수입식품 수입신고 증명서·구비서류 안내", MfdsImportFoodDocuments, "품목·국가·시점별 수입식품 신고 구비서류");

    private static 원장관행문서공식근거Dto Aphis근거()
        => 근거("USDA APHIS", "Plant and Plant Product Export Certificates", AphisPhytosanitaryCertificates, "식물·식물제품의 검역증명 발급 주체와 검사 요건");

    private static 원장관행문서공식근거Dto 근거(string 기관, string 자료, string url, string 적용범위)
        => new() { 기관명 = 기관, 자료명 = 자료, Url = url, 적용범위 = 적용범위 };
}
