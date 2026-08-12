namespace Ssalddel.Contracts.Mart;

public static class 마트피킹작업상태코드
{
    public const string 대기 = "대기";
    public const string 진행중 = "진행중";
    public const string 완료 = "완료";
    public const string 취소 = "취소";
}

public sealed class 마트피킹주문목록조회요청
{
    public string? 검색어 { get; set; }

    public long? 창고Id { get; set; }

    public string? 작업상태 { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}

public sealed class 마트피킹주문목록응답
{
    public IReadOnlyList<마트피킹주문요약응답> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}

public sealed class 마트피킹주문요약응답
{
    public long 주문Id { get; set; }

    public string 주문참조번호 { get; set; } = string.Empty;

    public string 주문상태 { get; set; } = string.Empty;

    public string? 현재단계 { get; set; }

    public int 상품종류수 { get; set; }

    public int 주문수량 { get; set; }

    public int 작업수 { get; set; }

    public int 완료작업수 { get; set; }

    public int 작업수량 { get; set; }

    public int 완료작업수량 { get; set; }

    public IReadOnlyList<string> 창고목록 { get; set; } = [];

    public DateTime 최근수정일시Utc { get; set; }
}

public sealed class 마트피킹주문상세응답
{
    public long 주문Id { get; set; }

    public string 주문참조번호 { get; set; } = string.Empty;

    public string 주문상태 { get; set; } = string.Empty;

    public string? 현재단계 { get; set; }

    public DateTime 생성일시Utc { get; set; }

    public DateTime 수정일시Utc { get; set; }

    public IReadOnlyList<마트피킹주문상품응답> 상품목록 { get; set; } = [];

    public IReadOnlyList<마트피킹작업응답> 작업목록 { get; set; } = [];
}

public sealed class 마트피킹주문상품응답
{
    public long 상품라인Id { get; set; }

    public string 상품명 { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int 수량 { get; set; }

    public string 상태 { get; set; } = string.Empty;
}

public sealed class 마트피킹작업응답
{
    public long 작업Id { get; set; }

    public string 작업Key { get; set; } = string.Empty;

    public string 작업유형 { get; set; } = string.Empty;

    public string 처리방식 { get; set; } = string.Empty;

    public string 상태 { get; set; } = string.Empty;

    public long 창고Id { get; set; }

    public string 창고명 { get; set; } = string.Empty;

    public string 작업자표시명 { get; set; } = string.Empty;

    public long? 입고상품Id { get; set; }

    public string? 이전작업Key { get; set; }

    public string? 다음작업Key { get; set; }

    public string 라인Key { get; set; } = string.Empty;

    public string 상품명 { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int 수량 { get; set; }

    public string? 적재대코드 { get; set; }

    public string? 보관위치코드 { get; set; }

    public string? 묶음바코드 { get; set; }

    public DateTime? 시작일시Utc { get; set; }

    public DateTime? 완료일시Utc { get; set; }

    public DateTime 수정일시Utc { get; set; }
}
