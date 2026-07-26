using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Contracts.Restaurants;

public sealed class 음식점탐색권역응답
{
    public string 배달권키 { get; set; } = string.Empty;

    public string 시도명 { get; set; } = string.Empty;

    public string? 시군구명 { get; set; }

    public string 표시명 { get; set; } = string.Empty;

    public string 기준지명 { get; set; } = string.Empty;

    public string 거리기준안내 { get; set; } = string.Empty;
}

public sealed class 음식점공개목록조회요청
{
    public string 배달권키 { get; set; } = string.Empty;

    public decimal 반경Km { get; set; } = RestaurantSearchPolicyDefaults.DefaultRadiusKm;

    public string? 검색어 { get; set; }

    public bool 주문가능만 { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}

public sealed class 음식점공개목록응답
{
    public IReadOnlyList<음식점공개요약응답> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;

    public string 배달권키 { get; set; } = string.Empty;

    public decimal 적용반경Km { get; set; }

    public string 거리기준안내 { get; set; } = string.Empty;
}

public sealed class 음식점공개요약응답
{
    public long Id { get; set; }

    public string 상호명 { get; set; } = string.Empty;

    public string 카테고리 { get; set; } = string.Empty;

    public string 소개 { get; set; } = string.Empty;

    public string 공개주소 { get; set; } = string.Empty;

    public string? 대표이미지Url { get; set; }

    public decimal? 거리Km { get; set; }

    public decimal 최소주문금액 { get; set; }

    public int 예상조리분 { get; set; }

    public bool 주문가능여부 { get; set; }

    public int 공개메뉴수 { get; set; }

    public DateTime 수정일시Utc { get; set; }
}

public sealed class 음식점공개상세응답
{
    public 음식점공개요약응답 음식점 { get; set; } = new();

    public IReadOnlyList<음식점메뉴공개응답> 메뉴목록 { get; set; } = [];
}

public sealed class 음식점메뉴공개응답
{
    public long Id { get; set; }

    public string 메뉴명 { get; set; } = string.Empty;

    public string 설명 { get; set; } = string.Empty;

    public decimal 판매가 { get; set; }

    public string? 대표이미지Url { get; set; }

    public bool 품절여부 { get; set; }

    public int 표시순서 { get; set; }
}
