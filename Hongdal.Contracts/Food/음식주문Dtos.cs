using Hongdal.Contracts.Common.Participants;

namespace Hongdal.Contracts.Food;

public sealed class 음식주문상품Dto
{
    public string 상품명 { get; set; } = string.Empty;
    public int 수량 { get; set; }
    public decimal 단가 { get; set; }
}

public sealed class 음식주문등록요청
{
    public long 음식점Id { get; set; }
    public string 주문자UserId { get; set; } = string.Empty;
    public 음식주문수령인정보Dto 수령인정보 { get; set; } = new();
    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; set; } = [];
    public string? 결제수단 { get; set; }
}

public sealed class 음식주문응답
{
    public string 주문번호 { get; set; } = string.Empty;
    public long 음식점Id { get; set; }
    public string 주문자UserId { get; set; } = string.Empty;
    public 음식주문수령인정보Dto 수령인정보 { get; set; } = new();
    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; set; } = [];
    public decimal 총주문금액 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string? 결제수단 { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class 음식주문목록응답
{
    public IReadOnlyList<음식주문응답> Items { get; set; } = [];
}
