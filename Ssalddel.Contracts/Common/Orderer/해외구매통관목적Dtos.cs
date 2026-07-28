namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 계정 종류가 아니라 이번 해외 구매 물품의 실제 사용 목적입니다.
/// 개인통관고유부호는 개인 자가사용 경로의 식별수단일 뿐 면세 증명이나 판매용 수입 신고 수단이 아닙니다.
/// </summary>
public static class 해외구매통관목적코드
{
    public const string 개인자가사용 = "PersonalSelfUse";
    public const string 사업판매사용 = "CommercialResale";

    public static IReadOnlyList<string> 지원목록 { get; } = [개인자가사용, 사업판매사용];

    public static string 거래유형에서변환(string? 거래유형)
        => 공동구매거래유형코드.정규화(거래유형) == 공동구매거래유형코드.B2B
            ? 사업판매사용
            : 개인자가사용;

    public static bool 지원여부(string? value)
        => 지원목록.Contains(value?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase);
}

public static class 해외구매소액면세판정코드
{
    public const string 추가정보필요 = "MoreInformationRequired";
    public const string 자가사용면세후보 = "SelfUseExemptionCandidate";
    public const string 과세검토필요 = "TaxAssessmentRequired";
    public const string 사업수입경로 = "CommercialImportRequired";
}

public static class 주문자수입3PL권유수준코드
{
    public const string 직접수령가능 = "DirectReceiptAvailable";
    public const string 이용검토 = "Consider3PL";
    public const string 이용권유 = "Recommend3PL";
}

public sealed class 주문자해외구매통관안내
{
    public string 수입목적코드 { get; set; } = 해외구매통관목적코드.개인자가사용;
    public string 수입목적표시명 { get; set; } = string.Empty;
    public string 판정코드 { get; set; } = 해외구매소액면세판정코드.추가정보필요;
    public bool 개인통관고유부호입력대상 { get; set; }
    public bool 자가사용소액면세검토대상 { get; set; }
    public bool 관세부가세예상비용검토필요 { get; set; }
    public decimal 일반자가사용기준금액Usd { get; set; }
    public decimal 미국발목록통관조건부기준금액Usd { get; set; }
    public string 핵심안내 { get; set; } = string.Empty;
    public IReadOnlyList<string> 확인사항 { get; set; } = [];
    public string 관세청예상세액조회Url { get; set; } = string.Empty;
    public string 관세법소액물품면세Url { get; set; } = string.Empty;
    public DateOnly 기준일 { get; set; }
}

public sealed class 주문자수입물류경로검토요청
{
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public decimal? 물품가격Usd { get; set; }
    public bool 식품류여부 { get; set; }
    public bool 냉장냉동보관필요 { get; set; }
    public bool 미국발목록통관조건충족 { get; set; }
}

public sealed class 주문자수입물류경로검토응답
{
    public string 판정코드 { get; set; } = 해외구매소액면세판정코드.추가정보필요;
    public bool 목록통관검토가능 { get; set; }
    public bool 일반수입신고필요 { get; set; }
    public bool 과세검토필요 { get; set; }
    public bool 식품수입요건검토필요 { get; set; }
    public string 물류3PL권유수준코드 { get; set; } = 주문자수입3PL권유수준코드.직접수령가능;
    public string 물류3PL권유제목 { get; set; } = string.Empty;
    public string 물류3PL권유이유 { get; set; } = string.Empty;
    public IReadOnlyList<string> 확인할3PL역량목록 { get; set; } = [];
    public IReadOnlyList<string> 주의사항목록 { get; set; } = [];
}

/// <summary>
/// 주문자가 구매 목적을 선택하는 시점의 안내 정책입니다.
/// 실제 면세·세액은 세관 신고 시 물품 종류, 수량, 가격, 운임, 보험료, 원산지와 적용 법령으로 확정됩니다.
/// </summary>
public static class 주문자해외구매통관정책
{
    public const decimal 일반자가사용기준금액Usd = 150m;
    public const decimal 미국발목록통관조건부기준금액Usd = 200m;
    public const string 관세청예상세액조회Url = "https://www.customs.go.kr/kcs/ad/tax/BuyTaxCalculation.do";
    public const string 관세법소액물품면세Url = "https://www.law.go.kr/LSW/lsLawLinkInfo.do?chrClsCd=010202&lsJoLnkSeq=1000695420";
    public const string 관세청목록통관배제안내Url = "https://www.customs.go.kr/download/UNIPASS_FAQ_221031.pdf";
    public const string 식약처수입식품정보Url = "https://www.mfds.go.kr/mfdssearch/main.do";

    public static 주문자해외구매통관안내 안내(string? 거래유형)
    {
        var purpose = 해외구매통관목적코드.거래유형에서변환(거래유형);
        var commercial = purpose == 해외구매통관목적코드.사업판매사용;

        return new 주문자해외구매통관안내
        {
            수입목적코드 = purpose,
            수입목적표시명 = commercial ? "사업·판매 목적 수입" : "개인 자가사용 목적",
            판정코드 = commercial
                ? 해외구매소액면세판정코드.사업수입경로
                : 해외구매소액면세판정코드.추가정보필요,
            개인통관고유부호입력대상 = !commercial,
            자가사용소액면세검토대상 = !commercial,
            관세부가세예상비용검토필요 = commercial,
            일반자가사용기준금액Usd = 일반자가사용기준금액Usd,
            미국발목록통관조건부기준금액Usd = 미국발목록통관조건부기준금액Usd,
            핵심안내 = commercial
                ? "판매·영업에 사용할 물품은 개인통관고유부호만으로 처리하거나 자가사용 소액면세를 적용할 수 없습니다. 사업 수입 신고와 품목별 관세·부가세·요건을 검토합니다."
                : "개인통관고유부호는 본인 확인 수단이며 면세 보증이 아닙니다. 실제 자가사용, 수량·품목과 과세가격 기준을 모두 충족할 때만 소액면세 후보가 됩니다.",
            확인사항 = commercial
                ?
                [
                    "HS 코드·원산지·과세가격에 따라 관세율과 수입 부가세 등 예상비용을 계산합니다.",
                    "식품 등은 가격과 별개로 수입자·영업 등록, 신고·검사 등 품목별 요건을 확인합니다.",
                    "같이 주문한 물량을 개인 명의로 나누어 판매용 물품처럼 통관하지 않습니다."
                ]
                :
                [
                    "일반적인 자가사용 소액물품 기준은 미화 150달러 이하이며, 미국발 200달러 기준은 목록통관 대상 등 조건을 충족할 때만 검토합니다.",
                    "기준을 초과하면 초과분만이 아니라 전체 과세가격을 기준으로 세액을 검토할 수 있습니다.",
                    "같이 주문·같이 수입이라는 이유만으로 참여자별 면세가 자동 적용되지 않습니다."
                ],
            관세청예상세액조회Url = 관세청예상세액조회Url,
            관세법소액물품면세Url = 관세법소액물품면세Url,
            기준일 = new DateOnly(2026, 7, 28)
        };
    }

    public static string 소액면세후보판정(
        string? 거래유형,
        decimal? 물품가격Usd,
        bool 미국발목록통관조건충족)
    {
        if (해외구매통관목적코드.거래유형에서변환(거래유형)
            == 해외구매통관목적코드.사업판매사용)
        {
            return 해외구매소액면세판정코드.사업수입경로;
        }

        if (물품가격Usd is null || 물품가격Usd < 0)
        {
            return 해외구매소액면세판정코드.추가정보필요;
        }

        var threshold = 미국발목록통관조건충족
            ? 미국발목록통관조건부기준금액Usd
            : 일반자가사용기준금액Usd;

        return 물품가격Usd <= threshold
            ? 해외구매소액면세판정코드.자가사용면세후보
            : 해외구매소액면세판정코드.과세검토필요;
    }

    public static 주문자수입물류경로검토응답 수입물류경로검토(
        주문자수입물류경로검토요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var commercial = 해외구매통관목적코드.거래유형에서변환(request.거래유형)
                         == 해외구매통관목적코드.사업판매사용;
        var listClearanceEligible = !commercial
                                    && !request.식품류여부
                                    && request.물품가격Usd is >= 0;
        var effectiveUnitedStatesListClearance = listClearanceEligible
                                                 && request.미국발목록통관조건충족;
        var exemptionDecision = 소액면세후보판정(
            request.거래유형,
            request.물품가격Usd,
            effectiveUnitedStatesListClearance);
        var taxReviewRequired = commercial
                                || exemptionDecision == 해외구매소액면세판정코드.과세검토필요;
        var importDeclarationRequired = commercial
                                        || request.식품류여부
                                        || taxReviewRequired;

        var recommendation = commercial
            ? 주문자수입3PL권유수준코드.이용권유
            : taxReviewRequired || request.냉장냉동보관필요
                ? 주문자수입3PL권유수준코드.이용검토
                : 주문자수입3PL권유수준코드.직접수령가능;

        var capabilities = new List<string>();
        if (recommendation != 주문자수입3PL권유수준코드.직접수령가능)
        {
            capabilities.Add("관세사·수입신고 업무와의 연계 범위");
            capabilities.Add("통관 후 국내 운송, 입고·검수·보관·피킹·출고 범위");
        }

        if (request.식품류여부)
        {
            capabilities.Add("수입식품 신고·검사 지원 범위와 식품 취급 가능 여부");
            capabilities.Add("로트·소비기한·회수 추적 관리");
        }

        if (request.냉장냉동보관필요)
        {
            capabilities.Add("냉장·냉동 온도 기록과 이상 발생 대응");
        }

        return new 주문자수입물류경로검토응답
        {
            판정코드 = exemptionDecision,
            목록통관검토가능 = listClearanceEligible,
            일반수입신고필요 = importDeclarationRequired,
            과세검토필요 = taxReviewRequired,
            식품수입요건검토필요 = request.식품류여부,
            물류3PL권유수준코드 = recommendation,
            물류3PL권유제목 = recommendation switch
            {
                주문자수입3PL권유수준코드.이용권유 => "판매용 수입은 3PL 이용을 함께 검토해 보세요",
                주문자수입3PL권유수준코드.이용검토 => "통관 후 보관·분배가 필요하면 3PL을 비교해 보세요",
                _ => "개인 직접 수령 경로도 검토할 수 있습니다"
            },
            물류3PL권유이유 = commercial
                ? "판매용 물량은 통관 뒤 입고·검수·재고·판매채널 출고가 이어지므로, 수입자와 관세사의 책임을 유지하면서 해당 업무 범위를 맡을 3PL을 비교하는 편이 안전합니다."
                : taxReviewRequired
                    ? "면세 기준을 벗어난 물량은 세액 확인과 통관 후 국내 인계가 필요합니다. 3PL은 세금을 없애지 않지만 입고·보관·분배를 연결할 수 있습니다."
                    : request.식품류여부
                        ? "식품은 목록통관 대상이 아니지만 자가사용·150달러 이하라면 면세 가능성이 있습니다. 개인 수량이면 3PL이 필수는 아니며 신고·검역 요건을 먼저 확인합니다."
                        : "자가사용 소액면세 후보라면 개인 직접 수령이 가능할 수 있습니다. 실제 세관 판정 전에는 면세로 확정하지 않습니다.",
            확인할3PL역량목록 = capabilities.Distinct(StringComparer.Ordinal).ToArray(),
            주의사항목록 =
            [
                "3PL 이용은 관세·부가세를 면제하거나 줄여 주는 수단이 아닙니다.",
                "3PL이 수입자 또는 관세사를 자동으로 대신하지 않으므로 계약 전 책임 범위를 확인합니다.",
                "식품은 가격과 세금 여부와 별개로 자가사용 수량, 금지성분, 검역·식품 안전 요건을 확인합니다."
            ]
        };
    }
}
