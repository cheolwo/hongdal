namespace Ssalddel.FoodApi.Domain.Orders;

public sealed class 음식주문
{
    public string 주문번호 { get; set; } = string.Empty;
    public long 음식점Id { get; set; }
    public string 주문자UserId { get; set; } = string.Empty;
    public 음식주문수령인정보 수령인정보 { get; set; } = new();
    public List<음식주문상품> 상품목록 { get; set; } = [];
    public decimal 총주문금액 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string? 결제수단 { get; set; }
    public DateTime CreatedAt { get; set; }
}
