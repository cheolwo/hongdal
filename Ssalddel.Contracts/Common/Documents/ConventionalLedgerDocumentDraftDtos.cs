namespace Ssalddel.Contracts.Common.Documents;

public static class 원장관행문서정책코드
{
    public const string 검토초안 = "원장관행문서초안";
}

public static class 원장관행문서종류코드
{
    public const string 견적요청서 = "REQUEST_FOR_QUOTATION";
    public const string 구매주문서 = "PURCHASE_ORDER";
    public const string 같이주문집계표 = "GROUP_ORDER_SHEET";
    public const string 계약검토자료서 = "CONTRACT_REVIEW_SHEET";
    public const string 프로포마송장자료서 = "PROFORMA_INVOICE_DATA_SHEET";
    public const string 상업송장 = "COMMERCIAL_INVOICE";
    public const string 포장명세서 = "PACKING_LIST";
    public const string 선적인도지시서 = "SHIPPERS_LETTER_OF_INSTRUCTION";
    public const string 원산지증명준비자료서 = "CERTIFICATE_OF_ORIGIN_DATA_SHEET";
    public const string 수입통관서류점검표 = "IMPORT_CUSTOMS_DOCUMENT_CHECKLIST";
    public const string 수입식품서류점검표 = "IMPORT_FOOD_DOCUMENT_CHECKLIST";
    public const string 선적문서참조표 = "SHIPMENT_DOCUMENT_REFERENCE_SHEET";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(
        [
            견적요청서,
            구매주문서,
            같이주문집계표,
            계약검토자료서,
            프로포마송장자료서,
            상업송장,
            포장명세서,
            선적인도지시서,
            원산지증명준비자료서,
            수입통관서류점검표,
            수입식품서류점검표,
            선적문서참조표
        ],
        StringComparer.OrdinalIgnoreCase);
}

public static class 원장관행문서생성모드코드
{
    public const string 원장초안 = "LedgerDraft";
    public const string 발급자확인초안 = "IssuerConfirmationDraft";
    public const string 외부발급준비자료 = "ExternalIssuerPreparation";
}

public static class 원장관행문서발급주체코드
{
    public const string 주문자집단 = "BuyerGroup";
    public const string 판매자수출자 = "SellerOrExporter";
    public const string 운송사포워더 = "CarrierOrForwarder";
    public const string 관세사수입자 = "CustomsBrokerOrImporter";
    public const string 권한기관 = "AuthorizedAuthority";
}

public static class 원장관행문서초안상태코드
{
    public const string 입력필요 = "NeedsInput";
    public const string 전문가검토준비 = "ReadyForQualifiedReview";
}

public sealed class 원장관행문서초안묶음응답
{
    public string 원장Id { get; set; } = string.Empty;
    public long 원장Revision { get; set; }
    public string 원장템플릿Key { get; set; } = string.Empty;
    public DateTimeOffset 생성시각Utc { get; set; }
    public bool 운영문서여부 { get; set; }
    public bool 외부전송가능여부 { get; set; }
    public string 실행경계안내 { get; set; } = string.Empty;
    public IReadOnlyList<원장관행문서초안Dto> 문서목록 { get; set; } = [];
}

public sealed class 원장관행문서카탈로그응답
{
    public string? 원장Id { get; set; }
    public string? 원장템플릿Key { get; set; }
    public IReadOnlyList<원장관행문서카탈로그항목Dto> 문서종류목록 { get; set; } = [];
}

public sealed class 원장관행문서카탈로그항목Dto
{
    public string 문서종류코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 영문문서명 { get; set; } = string.Empty;
    public string 생성모드코드 { get; set; } = 원장관행문서생성모드코드.원장초안;
    public string 발급주체코드 { get; set; } = 원장관행문서발급주체코드.주문자집단;
    public bool 원장초안생성가능여부 { get; set; }
    public bool 외부발급원본대체가능여부 { get; set; }
    public IReadOnlyList<string> 지원원장템플릿Key목록 { get; set; } = [];
    public IReadOnlyList<string> 원천정보목록 { get; set; } = [];
    public IReadOnlyList<string> 연계모듈목록 { get; set; } = [];
    public IReadOnlyList<원장관행문서공식근거Dto> 공식근거목록 { get; set; } = [];
    public string 경계안내 { get; set; } = string.Empty;
}

public sealed class 원장관행문서공식근거Dto
{
    public string 기관명 { get; set; } = string.Empty;
    public string 자료명 { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string 적용범위 { get; set; } = string.Empty;
}

public sealed class 원장관행문서보관응답
{
    public long 저장문서Id { get; set; }
    public string 원장Id { get; set; } = string.Empty;
    public long 원장Revision { get; set; }
    public string 문서종류코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string 생성상태 { get; set; } = string.Empty;
    public string 문서분류코드 { get; set; } = string.Empty;
    public string 생명주기상태코드 { get; set; } = string.Empty;
    public string 내용Sha256 { get; set; } = string.Empty;
    public bool 암호화됨 { get; set; }
    public bool 다운로드허용여부 { get; set; }
}

public sealed class 원장관행문서초안Dto
{
    public string 문서종류코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 영문문서명 { get; set; } = string.Empty;
    public string 초안문서번호 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/html; charset=utf-8";
    public string 생성모드코드 { get; set; } = 원장관행문서생성모드코드.원장초안;
    public string 발급주체코드 { get; set; } = 원장관행문서발급주체코드.주문자집단;
    public bool 외부발급원본대체가능여부 { get; set; }
    public string 상태코드 { get; set; } = 원장관행문서초안상태코드.입력필요;
    public long 원천원장Revision { get; set; }
    public IReadOnlyList<원장관행문서필드Dto> 필드목록 { get; set; } = [];
    public IReadOnlyList<원장관행문서품목행Dto> 품목행목록 { get; set; } = [];
    public IReadOnlyList<원장관행문서금액합계Dto> 금액합계목록 { get; set; } = [];
    public IReadOnlyList<string> 필수입력누락목록 { get; set; } = [];
    public IReadOnlyList<string> 경고목록 { get; set; } = [];
    public string Html { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
}

public sealed class 원장관행문서필드Dto
{
    public string 필드코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 값 { get; set; } = string.Empty;
    public string 원천경로 { get; set; } = string.Empty;
    public bool 확인됨 { get; set; }
}

public sealed class 원장관행문서품목행Dto
{
    public int 순번 { get; set; }
    public string 품목키 { get; set; } = string.Empty;
    public string 품명 { get; set; } = string.Empty;
    public string Hs코드 { get; set; } = string.Empty;
    public bool Hs코드전문가확인여부 { get; set; }
    public string 원산지국가코드 { get; set; } = string.Empty;
    public decimal 수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public decimal? 단가 { get; set; }
    public string 통화코드 { get; set; } = string.Empty;
    public decimal? 금액 { get; set; }
    public string 포장조건 { get; set; } = string.Empty;
    public string 원천경로 { get; set; } = string.Empty;
}

public sealed class 원장관행문서금액합계Dto
{
    public string 합계코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public decimal 금액 { get; set; }
    public string 통화코드 { get; set; } = string.Empty;
}
