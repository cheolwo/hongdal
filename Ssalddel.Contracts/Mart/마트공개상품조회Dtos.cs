namespace Ssalddel.Contracts.Mart;

public sealed class 마트공개상품목록조회요청
{
    public string? 검색어 { get; set; }

    public bool 판매가능만 { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}

public sealed class 마트공개상품목록응답
{
    public IReadOnlyList<마트공개상품요약응답> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;

    public string 재고기준안내 { get; set; } = string.Empty;
}

public sealed class 마트공개상품요약응답
{
    public long Id { get; set; }

    public string 상품명 { get; set; } = string.Empty;

    public string 카테고리 { get; set; } = string.Empty;

    public string 짧은설명 { get; set; } = string.Empty;

    public string 판매단위 { get; set; } = string.Empty;

    public decimal 판매가 { get; set; }

    public string? 대표이미지Url { get; set; }

    public int 판매가능수량 { get; set; }

    public bool 판매가능여부 { get; set; }

    public DateTime 재고기준시각Utc { get; set; }

    public DateTime 수정일시Utc { get; set; }
}

public sealed class 마트공개상품상세응답
{
    public long Id { get; set; }

    public string 상품명 { get; set; } = string.Empty;

    public string 카테고리 { get; set; } = string.Empty;

    public string 설명 { get; set; } = string.Empty;

    public string 판매단위 { get; set; } = string.Empty;

    public decimal 판매가 { get; set; }

    public string? 대표이미지Url { get; set; }

    public int 판매가능수량 { get; set; }

    public bool 판매가능여부 { get; set; }

    public DateTime 재고기준시각Utc { get; set; }

    public DateTime 수정일시Utc { get; set; }

    public string 재고기준안내 { get; set; } = string.Empty;
}
