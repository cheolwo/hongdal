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

    public 마트공개상품구매근거응답 구매근거 { get; set; } = new();
}

/// <summary>
/// 공개 상품과 연결된 완료 원장·후기에서 개인정보와 거래 원문을 제거한 공개 투영입니다.
/// </summary>
public sealed class 마트공개상품구매근거응답
{
    public bool 완료원장확인여부 { get; set; }

    public string 원장근거상태 { get; set; } = string.Empty;

    public int 공개후기수 { get; set; }

    public DateTime? 완료확인시각Utc { get; set; }

    public DateTime? 근거기준시각Utc { get; set; }

    public bool 후기작성가능여부 { get; set; }

    public IReadOnlyList<마트공개상품구매후기응답> 구매후기목록 { get; set; } = [];

    public string 공개범위안내 { get; set; } = string.Empty;
}

public sealed class 마트공개상품구매후기응답
{
    public long 게시글Id { get; set; }

    public string 제목 { get; set; } = string.Empty;

    public string 본문요약 { get; set; } = string.Empty;

    public string 작성자표시명 { get; set; } = string.Empty;

    public int 추천수 { get; set; }

    public int 댓글수 { get; set; }

    public DateTime 작성시각Utc { get; set; }
}

public sealed class 마트공개상품구매후기작성요청
{
    public string 작성자표시명 { get; set; } = string.Empty;

    public string 글비밀번호 { get; set; } = string.Empty;

    public string 제목 { get; set; } = string.Empty;

    public string 본문 { get; set; } = string.Empty;
}
