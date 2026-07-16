namespace Hongdal.Contracts.Common.Inbound;

public static class 입고흐름유형코드
{
    public const string 계약기반입고 = "ContractBased";
    public const string 현장임시입고 = "Unplanned";
    public const string 주문자동입고예정 = "OrderAutoExpected";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            현장임시입고 => 현장임시입고,
            주문자동입고예정 => 주문자동입고예정,
            _ => 계약기반입고
        };

    public static string GetDisplayName(string? value)
        => Normalize(value) switch
        {
            현장임시입고 => "현장 임시 입고",
            주문자동입고예정 => "주문 자동 입고 예정",
            _ => "계약 기반 입고"
        };

    public static string GetDescription(string? value)
        => Normalize(value) switch
        {
            현장임시입고 => "계약이나 입고 예정이 아직 정리되지 않았지만, 현장에서 먼저 물건을 받아 임시로 시스템에 편입합니다.",
            주문자동입고예정 => "주문자 또는 판매자의 구매/판매 흐름에서 자동으로 입고 예정이 생성되어 창고 업무로 이어집니다.",
            _ => "계약서와 관리 정보가 먼저 등록되고, 그 계약을 기준으로 입고 예정과 검수가 진행됩니다."
        };

    public static bool RequiresExistingContract(string? value)
        => Normalize(value) == 계약기반입고;

    public static bool IsOrderGenerated(string? value)
        => Normalize(value) == 주문자동입고예정;
}

public sealed class 입고요청항목응답
{
    public long Id { get; set; }
    public long 창고Id { get; set; }
    public string? 커뮤니티원장Id { get; set; }
    public string? 커뮤니티원장템플릿Key { get; set; }
    public string? 커뮤니티원장상태 { get; set; }
    public string 입고흐름유형 { get; set; } = 입고흐름유형코드.계약기반입고;
    public string 입고생성경로 { get; set; } = string.Empty;
    public bool 계약선행여부 { get; set; } = true;
    public bool 자동생성여부 { get; set; }
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
    public string 입고흐름유형 { get; set; } = 입고흐름유형코드.계약기반입고;
    public string 입고생성경로 { get; set; } = string.Empty;
    public bool 계약선행여부 { get; set; } = true;
    public bool 자동생성여부 { get; set; }
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
    public string? 커뮤니티원장Id { get; set; }
    public string? 커뮤니티원장템플릿Key { get; set; }
    public string? 커뮤니티원장상태 { get; set; }
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
