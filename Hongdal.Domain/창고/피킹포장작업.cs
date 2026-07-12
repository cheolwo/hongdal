namespace 홍달.도메인.창고;

public class 피킹포장작업
{
    public long Id { get; set; }

    public string 작업Key { get; set; } = string.Empty;

    public string 작업유형 { get; set; } = 피킹포장작업유형.피킹;

    public string 처리방식 { get; set; } = string.Empty;

    public string 상태 { get; set; } = 피킹포장작업상태.대기;

    public long? 출고묶음Id { get; set; }

    public long? 출고예정Id { get; set; }

    public long? 입고상품Id { get; set; }

    public long 창고Id { get; set; }

    public string 창고명 { get; set; } = string.Empty;

    public string 작업자UserId { get; set; } = string.Empty;

    public string 작업자표시명 { get; set; } = string.Empty;

    public string? 상대작업자UserId { get; set; }

    public string? 이전작업Key { get; set; }

    public string? 다음작업Key { get; set; }

    public string 주문참조번호 { get; set; } = string.Empty;

    public string 라인Key { get; set; } = string.Empty;

    public string 상품명 { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int 수량 { get; set; }

    public string? 적재대코드 { get; set; }

    public string? 보관위치코드 { get; set; }

    public string? 묶음바코드 { get; set; }

    public string? 할당사유 { get; set; }

    public string? 커뮤니티원장Id { get; set; }

    public string? 커뮤니티원장블록Id { get; set; }

    public DateTime? 시작일시Utc { get; set; }

    public DateTime? 완료일시Utc { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public 출고묶음? 출고묶음 { get; set; }
}

public static class 피킹포장작업유형
{
    public const string 피킹 = "피킹";
    public const string 포장 = "포장";
}

public static class 피킹포장작업상태
{
    public const string 대기 = "대기";
    public const string 진행중 = "진행중";
    public const string 완료 = "완료";
    public const string 취소 = "취소";
}
