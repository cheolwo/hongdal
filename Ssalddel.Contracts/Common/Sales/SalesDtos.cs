namespace Ssalddel.Contracts.Common.Sales;

public sealed class 판매채널계정항목응답
{
    public long Id { get; set; }
    public string 채널종류 { get; set; } = string.Empty;
    public string 상점명 { get; set; } = string.Empty;
    public string 연결상태 { get; set; } = string.Empty;
    public DateTime? 마지막동기화일시 { get; set; }
    public DateTime 등록일시 { get; set; }
    public DateTime 수정일시 { get; set; }
    public bool 인증정보설정됨 { get; set; }
    public IReadOnlyList<판매채널인증필드상태> 인증필드상태 { get; set; } = [];
}

public sealed class 판매채널계정목록응답
{
    public IReadOnlyList<판매채널계정항목응답> Items { get; set; } = [];
}

public sealed class 판매채널계정저장요청
{
    public string 채널종류 { get; set; } = string.Empty;
    public string 상점명 { get; set; } = string.Empty;
    public Dictionary<string, string> 인증정보 { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [Obsolete("인증정보를 사용하세요. 이 값은 새 연결에서 저장하지 않습니다.")]
    public string 인증메모 { get; set; } = string.Empty;
}

public sealed class 판매채널인증필드상태
{
    public string Key { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public bool 필수 { get; set; }
    public bool 비밀값 { get; set; }
    public bool 설정됨 { get; set; }
    public string 마스킹값 { get; set; } = string.Empty;
}

public sealed class 판매상품항목응답
{
    public long Id { get; set; }
    public long 입고상품Id { get; set; }
    public string 대표상품명 { get; set; } = string.Empty;
    public string 판매SKU { get; set; } = string.Empty;
    public decimal 판매가 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public bool 샘플데이터여부 { get; set; }
    public string? 샘플데이터코드 { get; set; }
    public string? Image_Url { get; set; }
    public string 이미지생성상태 { get; set; } = string.Empty;
}

public sealed class 판매상품목록응답
{
    public IReadOnlyList<판매상품항목응답> Items { get; set; } = [];
}

public sealed class 판매상품저장요청
{
    public long 입고상품Id { get; set; }
    public string 대표상품명 { get; set; } = string.Empty;
    public string 판매SKU { get; set; } = string.Empty;
    public decimal 판매가 { get; set; }
    public bool 샘플데이터여부 { get; set; }
    public string? 샘플데이터코드 { get; set; }
}

public sealed class 판매상품샘플시드요청
{
    public int 최대건수 { get; set; } = 20;
}

public sealed class 채널출품항목응답
{
    public long Id { get; set; }
    public long 판매상품Id { get; set; }
    public long 판매채널계정Id { get; set; }
    public string 채널상품번호 { get; set; } = string.Empty;
    public string 출품상태 { get; set; } = string.Empty;
    public string 동기화상태 { get; set; } = string.Empty;
    public string 에러메시지 { get; set; } = string.Empty;
}

public sealed class 채널출품목록응답
{
    public IReadOnlyList<채널출품항목응답> Items { get; set; } = [];
}

public sealed class 채널출품저장요청
{
    public long 판매상품Id { get; set; }
    public long 판매채널계정Id { get; set; }
}
