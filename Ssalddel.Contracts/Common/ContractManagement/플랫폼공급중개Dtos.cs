using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.ContractManagement;

public static class 공급이용조직유형코드
{
    public const string 음식점 = "Restaurant";
    public const string 살들마트 = "SsalddelMart";

    public static bool 지원됨(string? value)
        => string.Equals(value, 음식점, StringComparison.Ordinal)
           || string.Equals(value, 살들마트, StringComparison.Ordinal);
}

public static class 공급조직접근ClaimTypes
{
    public const string 살들마트Id = "ssalddel:mart_id";
}

public static class 플랫폼공급계약상태코드
{
    public const string 초안 = "Draft";
    public const string 활성 = "Active";
    public const string 일시중지 = "Suspended";
    public const string 종료 = "Terminated";
}

public static class 공급계약이용상태코드
{
    public const string 이용중 = "Active";
    public const string 중지 = "Suspended";
    public const string 해지 = "Cancelled";
}

public static class 개별공급발주상태코드
{
    public const string 공급자제출됨 = "SubmittedToSupplier";
    public const string 공급자수락 = "SupplierAccepted";
    public const string 공급자부분수락 = "SupplierPartiallyAccepted";
    public const string 공급자거절 = "SupplierRejected";
    public const string 철회 = "Withdrawn";

    public static bool 공급자응답상태(string? value)
        => string.Equals(value, 공급자수락, StringComparison.Ordinal)
           || string.Equals(value, 공급자부분수락, StringComparison.Ordinal)
           || string.Equals(value, 공급자거절, StringComparison.Ordinal);
}

public static class 공급중개역할코드
{
    public const string 개별발주중개 = "IndividualOrderBroker";
}

public static class 공급중개안내
{
    public const string 현재버전 = "2026-07-28";

    public static IReadOnlyList<string> 문구 { get; } =
    [
        "플랫폼은 공급조건 계약과 개별 발주 전달을 중개하며 상품의 판매자나 재판매자가 아닙니다.",
        "개별 발주의 매수인은 이용 음식점 또는 살들마트이고 판매자는 공급자입니다.",
        "발주 제출만으로 공급자 수락, 결제, 재고 예약, 입고 또는 소유권 이전이 발생하지 않습니다.",
        "공급자 응답과 실제 검수·인수 완료는 별도 상태와 원장으로 기록합니다."
    ];
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Contract,
    "플랫폼이 판매 당사자가 되지 않는 공급조건 계약과 조직별 개별 발주 중개 계약을 정의합니다.",
    FlowOrder = 10,
    Boundary = "공급조건·이용동의·개별발주 계약만 정의하며 결제·재고·입고를 실행하지 않습니다.")]
public sealed class 플랫폼공급계약등록요청
{
    public Guid 클라이언트요청Id { get; set; }

    public string 계약번호 { get; set; } = string.Empty;

    public string 공급자Key { get; set; } = string.Empty;

    public string 공급자명 { get; set; } = string.Empty;

    public string 계약문서버전 { get; set; } = string.Empty;

    public DateTime 유효시작Utc { get; set; }

    public DateTime 유효종료Utc { get; set; }

    public string 통화코드 { get; set; } = "KRW";

    public string 정산조건 { get; set; } = string.Empty;

    public string 반품조건 { get; set; } = string.Empty;

    public bool 플랫폼중개전용확인 { get; set; }

    public IReadOnlyList<플랫폼공급계약품목등록요청> 품목목록 { get; set; } = [];
}

public sealed class 플랫폼공급계약품목등록요청
{
    public string 계약품목Key { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string 품목명 { get; set; } = string.Empty;

    public string 공급단위 { get; set; } = string.Empty;

    public decimal 계약단가 { get; set; }

    public decimal 최소발주수량 { get; set; }

    public decimal? 최대발주수량 { get; set; }

    public string 원산지표시 { get; set; } = string.Empty;

    public string 보관조건 { get; set; } = string.Empty;

    public IReadOnlyList<string> 허용조직유형목록 { get; set; } = [];
}

public sealed class 플랫폼공급계약활성화요청
{
    public string 기대상태코드 { get; set; } = 플랫폼공급계약상태코드.초안;

    public string 계약문서버전 { get; set; } = string.Empty;

    public string 계약체결근거참조 { get; set; } = string.Empty;

    public bool 공급자체결확인 { get; set; }

    public bool 플랫폼중개전용확인 { get; set; }
}

public sealed class 플랫폼공급계약응답
{
    public Guid 공급계약Id { get; set; }

    public string 계약번호 { get; set; } = string.Empty;

    public string 공급자Key { get; set; } = string.Empty;

    public string 공급자명 { get; set; } = string.Empty;

    public string 계약문서버전 { get; set; } = string.Empty;

    public string 상태코드 { get; set; } = string.Empty;

    public DateTime 유효시작Utc { get; set; }

    public DateTime 유효종료Utc { get; set; }

    public string 통화코드 { get; set; } = string.Empty;

    public string 정산조건 { get; set; } = string.Empty;

    public string 반품조건 { get; set; } = string.Empty;

    public string 플랫폼역할코드 { get; set; } = 공급중개역할코드.개별발주중개;

    public bool 플랫폼판매자여부 { get; set; }

    public bool 플랫폼재판매자여부 { get; set; }

    public IReadOnlyList<플랫폼공급계약품목응답> 품목목록 { get; set; } = [];
}

public sealed class 플랫폼공급계약품목응답
{
    public Guid 공급계약품목Id { get; set; }

    public string 계약품목Key { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string 품목명 { get; set; } = string.Empty;

    public string 공급단위 { get; set; } = string.Empty;

    public decimal 계약단가 { get; set; }

    public decimal 최소발주수량 { get; set; }

    public decimal? 최대발주수량 { get; set; }

    public string 원산지표시 { get; set; } = string.Empty;

    public string 보관조건 { get; set; } = string.Empty;

    public IReadOnlyList<string> 허용조직유형목록 { get; set; } = [];
}

public sealed class 공급계약이용등록요청
{
    public Guid 클라이언트요청Id { get; set; }

    public string 조직유형코드 { get; set; } = string.Empty;

    public string 계약문서버전 { get; set; } = string.Empty;

    public bool 공급계약이용동의 { get; set; }

    public bool 개별발주별도확인동의 { get; set; }

    public string 안내버전 { get; set; } = string.Empty;
}

public sealed class 공급계약이용등록응답
{
    public Guid 공급계약이용등록Id { get; set; }

    public Guid 공급계약Id { get; set; }

    public string 조직유형코드 { get; set; } = string.Empty;

    public string 조직참조Key { get; set; } = string.Empty;

    public string 계약문서버전 { get; set; } = string.Empty;

    public string 상태코드 { get; set; } = 공급계약이용상태코드.이용중;

    public DateTime 등록시각Utc { get; set; }
}

public sealed class 개별공급발주등록요청
{
    public Guid 클라이언트요청Id { get; set; }

    public Guid 공급계약이용등록Id { get; set; }

    public Guid 공급계약품목Id { get; set; }

    public decimal 발주수량 { get; set; }

    public DateTime 희망납품일Utc { get; set; }

    public string 납품지참조Key { get; set; } = string.Empty;

    public string 계약문서버전 { get; set; } = string.Empty;

    public bool 개별발주확인 { get; set; }

    public bool 공급자판매자확인 { get; set; }

    public bool 플랫폼중개자확인 { get; set; }

    public string 안내버전 { get; set; } = string.Empty;
}

public sealed class 개별공급발주목록조회요청
{
    public string 조직유형코드 { get; set; } = string.Empty;

    public string? 상태코드 { get; set; }
}

public sealed class 개별공급발주철회요청
{
    public string 조직유형코드 { get; set; } = string.Empty;

    public string 기대상태코드 { get; set; } = 개별공급발주상태코드.공급자제출됨;
}

public sealed class 개별공급발주공급자응답기록요청
{
    public string 기대상태코드 { get; set; } = 개별공급발주상태코드.공급자제출됨;

    public string 공급자응답상태코드 { get; set; } = string.Empty;

    public decimal 수락수량 { get; set; }

    public string 공급자응답근거참조 { get; set; } = string.Empty;

    public bool 공급자응답확인 { get; set; }
}

public sealed class 개별공급발주응답
{
    public Guid 개별공급발주Id { get; set; }

    public Guid 공급계약Id { get; set; }

    public Guid 공급계약품목Id { get; set; }

    public string 계약번호Snapshot { get; set; } = string.Empty;

    public string 계약문서버전Snapshot { get; set; } = string.Empty;

    public string 공급자KeySnapshot { get; set; } = string.Empty;

    public string 공급자명Snapshot { get; set; } = string.Empty;

    public string 구매조직유형코드 { get; set; } = string.Empty;

    public string 구매조직참조Key { get; set; } = string.Empty;

    public string 품목명Snapshot { get; set; } = string.Empty;

    public string SKUSnapshot { get; set; } = string.Empty;

    public string 공급단위Snapshot { get; set; } = string.Empty;

    public decimal 발주수량 { get; set; }

    public decimal? 공급자수락수량 { get; set; }

    public decimal 계약단가Snapshot { get; set; }

    public decimal 발주금액Snapshot { get; set; }

    public string 통화코드Snapshot { get; set; } = string.Empty;

    public DateTime 희망납품일Utc { get; set; }

    public string 납품지참조Key { get; set; } = string.Empty;

    public string 상태코드 { get; set; } = 개별공급발주상태코드.공급자제출됨;

    public string 플랫폼역할코드 { get; set; } = 공급중개역할코드.개별발주중개;

    public bool 플랫폼판매자여부 { get; set; }

    public bool 플랫폼재판매자여부 { get; set; }

    public bool 결제실행됨 { get; set; }

    public bool 재고예약됨 { get; set; }

    public bool 입고생성됨 { get; set; }

    public string? 공급자응답근거참조 { get; set; }

    public DateTime 제출시각Utc { get; set; }

    public DateTime? 공급자응답시각Utc { get; set; }
}
