namespace Ssalddel.Contracts.Restaurants;

public sealed class 음식점가까운조회요청
{
    public decimal 위도 { get; set; }
    public decimal 경도 { get; set; }
    public decimal 반경Km { get; set; } = 5m;
    public int 최대건수 { get; set; } = 20;
}

public sealed class 음식점인기조회요청
{
    public int 최대건수 { get; set; } = 20;
    public int 최소리뷰수 { get; set; } = 3;
}

public sealed class 음식점목록응답
{
    public IReadOnlyList<음식점요약응답> Items { get; set; } = [];
}

public sealed class 음식점요약응답
{
    public long Id { get; set; }
    public string 상호명 { get; set; } = string.Empty;
    public string 카테고리 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string? 대표이미지Url { get; set; }
    public decimal 위도 { get; set; }
    public decimal 경도 { get; set; }
    public decimal? 거리Km { get; set; }
    public decimal 평균평점 { get; set; }
    public int 리뷰수 { get; set; }
    public bool 주문가능여부 { get; set; }
    public bool 저평점주의필요 { get; set; }
}

public sealed class 음식점리뷰등록요청
{
    public string 주문자UserId { get; set; } = string.Empty;
    public string? 주문번호 { get; set; }
    public int 별점 { get; set; }
    public string 내용 { get; set; } = string.Empty;
    public IReadOnlyList<string> 사진Urls { get; set; } = [];
}

public sealed class 음식점리뷰노출수정요청
{
    public bool 사장노출허용여부 { get; set; }
    public string 수정자UserId { get; set; } = string.Empty;
}

public sealed class 음식점리뷰목록응답
{
    public long 음식점Id { get; set; }
    public IReadOnlyList<음식점리뷰요약응답> Items { get; set; } = [];
}

public sealed class 음식점리뷰요약응답
{
    public long Id { get; set; }
    public long 음식점Id { get; set; }
    public string 주문자UserId { get; set; } = string.Empty;
    public string? 주문번호 { get; set; }
    public int 별점 { get; set; }
    public string 내용 { get; set; } = string.Empty;
    public IReadOnlyList<string> 사진Urls { get; set; } = [];
    public bool 사진포함여부 { get; set; }
    public bool 사장노출허용여부 { get; set; }
    public bool 관리자검토필요여부 { get; set; }
    public bool 관리자게시강제여부 { get; set; }
    public bool 현재노출여부 { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? 게시종료일시Utc { get; set; }
}
