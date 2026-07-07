namespace Hongdal.Contracts.Common.Orderer;

public sealed class 공동구매자동수요등록Command
{
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = "상온";
    public string 물류방식 { get; set; } = "LCL";
    public string 주문자키 { get; set; } = string.Empty;
    public string 주문자표시명 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = "kg";
    public decimal? 예약결제금액 { get; set; }
    public string 수요유형 { get; set; } = 공동구매자동수요유형코드.관심표시;
    public string 결제상태 { get; set; } = 공동구매자동결제상태코드.미결제;
    public string 메모 { get; set; } = string.Empty;
}

public sealed class 공동구매자동수요응답
{
    public string 수요Id { get; set; } = string.Empty;
    public string 자동집단Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 수요유형 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public decimal? 예약결제금액 { get; set; }
    public DateTime 생성시각Utc { get; set; }
}

public sealed class 공동구매자동집단응답
{
    public string 자동집단Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 현재상태 { get; set; } = 공동구매자동집단상태코드.수요수집중;
    public int 수요건수 { get; set; }
    public int 예약결제건수 { get; set; }
    public decimal 총희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public decimal 예약결제합계 { get; set; }
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
    public IReadOnlyList<공동구매자동수요응답> 수요목록 { get; set; } = [];
    public IReadOnlyList<공동구매자동집단이벤트응답> 이벤트목록 { get; set; } = [];
}

public sealed class 공동구매자동집단이벤트응답
{
    public string 이벤트유형 { get; set; } = string.Empty;
    public string 요약 { get; set; } = string.Empty;
    public DateTime 발생시각Utc { get; set; }
}

public sealed class 공동구매자동집단조회조건
{
    public string? 상품키 { get; set; }
    public string? 배송권키 { get; set; }
    public string? 현재상태 { get; set; }
}

public static class 공동구매자동수요유형코드
{
    public const string 관심표시 = "InterestOnly";
    public const string 예약결제 = "PaidReservation";
}

public static class 공동구매자동결제상태코드
{
    public const string 미결제 = "NotPaid";
    public const string 예약됨 = "Reserved";
    public const string 결제확정 = "Captured";
}

public static class 공동구매자동집단상태코드
{
    public const string 수요수집중 = "CollectingDemand";
    public const string 확정대기 = "ReadyToConfirm";
    public const string 확정 = "Confirmed";
}
