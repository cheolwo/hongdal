namespace Ssalddel.Contracts.Common.Orderer;

public static class 같이주문용어
{
    public const string 표시명 = "같이 주문";

    // 기존 API route, 저장 코드와 외부 연동에서는 GroupPurchase를 호환 식별자로 유지합니다.
    public const string 기술호환코드 = "GroupPurchase";
}

public static class 주문방식비교신호코드
{
    public const string 같이모집마감 = "group-recruitment-closed";
    public const string 개별비용우위 = "individual-cost-not-higher";
    public const string 같이비용절감대기초과 = "group-cost-lower-wait-exceeds";
    public const string 같이비용절감성립대기 = "group-cost-lower-if-formed";
    public const string 같이비용절감가능 = "group-cost-lower";
}

/// <summary>
/// 한 주문자가 같은 상품과 수량을 지금 개별 주문할 때와 같이 주문 성립을 기다릴 때의
/// 예상 비용·시간·성립 조건을 비교하기 위한 읽기 전용 요청입니다.
/// </summary>
public sealed class 주문방식비교요청
{
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public decimal 요청수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public string 통화코드 { get; set; } = "KRW";
    public DateTimeOffset? 기준시각Utc { get; set; }
    public DateTimeOffset? 최대대기가능시각Utc { get; set; }
    public 개별주문비용입력 개별주문 { get; set; } = new();
    public 같이주문비용입력 같이주문 { get; set; } = new();
}

public sealed class 개별주문비용입력
{
    public decimal 상품단가 { get; set; }
    public decimal 배송비 { get; set; }
    public decimal 기타비용 { get; set; }
    public DateTimeOffset? 예상수령시각Utc { get; set; }
    public string 가격근거 { get; set; } = string.Empty;
}

public sealed class 같이주문비용입력
{
    public int 현재참여자수 { get; set; }
    public int 목표참여자수 { get; set; }
    public decimal 현재확정수량 { get; set; }
    public decimal 현재잠재수량 { get; set; }
    public decimal 최소성립수량 { get; set; }
    public decimal 최대안전수량 { get; set; }
    public decimal 계산증분 { get; set; } = 1m;
    public decimal 목표절감률 { get; set; }
    public decimal 위험예비비율 { get; set; }
    public DateTimeOffset? 모집마감시각Utc { get; set; }
    public DateTimeOffset? 예상수령시각Utc { get; set; }
    public List<같이주문공급가격구간입력> 공급가격구간 { get; set; } = [];
    public List<같이주문비용항목입력> 비용항목 { get; set; } = [];
}

public sealed class 같이주문공급가격구간입력
{
    public string 이름 { get; set; } = string.Empty;
    public decimal 최소수량 { get; set; }
    public decimal 상품단가 { get; set; }
    public string 근거 { get; set; } = string.Empty;
    public DateTimeOffset? 유효시각Utc { get; set; }
}

public sealed class 같이주문비용항목입력
{
    public string 코드 { get; set; } = string.Empty;
    public string 이름 { get; set; } = string.Empty;
    public string 비용분류코드 { get; set; } = string.Empty;
    public string 계산방식코드 { get; set; } = "fixed";
    public decimal 금액 { get; set; }
    public decimal? 용량수량 { get; set; }
    public string 근거 { get; set; } = string.Empty;
    public DateTimeOffset? 유효시각Utc { get; set; }
}

public sealed class 주문방식비교응답
{
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public decimal 요청수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public string 통화코드 { get; set; } = string.Empty;
    public DateTimeOffset 기준시각Utc { get; set; }
    public string 같이주문표시명 { get; set; } = 같이주문용어.표시명;
    public bool 기본선택없음 { get; set; } = true;
    public bool 자동같이주문금지 { get; set; } = true;
    public bool 같이주문별도동의필수 { get; set; } = true;
    public 주문방식비용응답 개별주문 { get; set; } = new();
    public 주문방식비용응답 같이주문 { get; set; } = new();
    public 같이주문모집비교응답 같이주문모집 { get; set; } = new();
    public 주문방식비교판단응답 판단 { get; set; } = new();
    public IReadOnlyList<string> 계산근거 { get; set; } = [];
    public IReadOnlyList<string> 주의사항 { get; set; } = [];
}

public sealed class 주문방식비용응답
{
    public decimal 상품금액 { get; set; }
    public decimal 물류및부대비용 { get; set; }
    public decimal 총예상비용 { get; set; }
    public decimal 단위당예상비용 { get; set; }
    public DateTimeOffset? 예상수령시각Utc { get; set; }
    public bool 추정치 { get; set; } = true;
}

public sealed class 같이주문모집비교응답
{
    public int 현재참여자수 { get; set; }
    public int 목표참여자수 { get; set; }
    public decimal 현재잠재수량 { get; set; }
    public decimal 비교기준모집수량 { get; set; }
    public decimal 최소성립수량 { get; set; }
    public decimal 추가필요수량 { get; set; }
    public decimal 모집진척률 { get; set; }
    public bool 최소성립조건충족 { get; set; }
    public bool 모집마감 { get; set; }
    public DateTimeOffset? 모집마감시각Utc { get; set; }
}

public sealed class 주문방식비교판단응답
{
    public string 신호코드 { get; set; } = string.Empty;
    public string 안내 { get; set; } = string.Empty;
    public decimal 예상절감액 { get; set; }
    public decimal 예상절감률 { get; set; }
    /// <summary>개별주문 예상 수령시각보다 더 기다리는 시간 수입니다.</summary>
    public decimal? 추가대기시간Hours { get; set; }
    public bool 같이비용절감가능 { get; set; }
    public bool? 최대대기허용범위안 { get; set; }
    public bool 개별주문계속가능 { get; set; } = true;
    public bool 같이주문검토가능 { get; set; }
}
