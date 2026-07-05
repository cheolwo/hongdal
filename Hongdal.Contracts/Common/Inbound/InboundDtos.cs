namespace Hongdal.Contracts.Common.Inbound;

public sealed class 입고요청항목응답
{
    public long Id { get; set; }
    public long 창고Id { get; set; }
    public long? 주문Id { get; set; }
    public string 주문참조번호 { get; set; } = string.Empty;
    public string 주문자UserId { get; set; } = string.Empty;
    public string 판매자UserId { get; set; } = string.Empty;
    public long? 출고예정Id { get; set; }
    public string? 운송의뢰Id { get; set; }
    public string 공급처명 { get; set; } = string.Empty;
    public string 원주문참조번호 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 예정도착일 { get; set; }
    public DateTime? 입고완료일시 { get; set; }
    public 입고계약스냅샷 계약정보 { get; set; } = 입고계약스냅샷.Default();
}

public sealed class 입고요청목록응답
{
    public IReadOnlyList<입고요청항목응답> Items { get; set; } = [];
}

public sealed class 입고요청저장요청
{
    public long 창고Id { get; set; }
    public long? 주문Id { get; set; }
    public string 주문참조번호 { get; set; } = string.Empty;
    public string 판매자UserId { get; set; } = string.Empty;
    public long? 출고예정Id { get; set; }
    public string? 운송의뢰Id { get; set; }
    public string 공급처명 { get; set; } = string.Empty;
    public string 원주문참조번호 { get; set; } = string.Empty;
    public DateTime? 예정도착일 { get; set; }
    public string 비고 { get; set; } = string.Empty;
    public 입고계약스냅샷 계약정보 { get; set; } = 입고계약스냅샷.Default();
}

public sealed class 입고완료요청
{
    public IReadOnlyList<입고상품저장요청> Items { get; set; } = [];
}

public sealed class 입고상품항목응답
{
    public long Id { get; set; }
    public long 입고요청Id { get; set; }
    public long 창고Id { get; set; }
    public string 소유자UserId { get; set; } = string.Empty;
    public string 판매자UserId { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string 옵션명 { get; set; } = string.Empty;
    public int 입고수량 { get; set; }
    public int 가용수량 { get; set; }
    public int 불량수량 { get; set; }
    public string 보관위치 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 입고완료일시 { get; set; }
    public 입고계약스냅샷 계약정보 { get; set; } = 입고계약스냅샷.Default();
}

public sealed class 입고상품목록응답
{
    public IReadOnlyList<입고상품항목응답> Items { get; set; } = [];
}

public sealed class 입고상품저장요청
{
    public string 상품명 { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string 옵션명 { get; set; } = string.Empty;
    public int 입고수량 { get; set; }
    public int 불량수량 { get; set; }
    public string 보관위치 { get; set; } = string.Empty;
}
