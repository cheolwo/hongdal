namespace Ssalddel.FoodApi.Domain.Orders;

public sealed class 음식주문상품
{
    public string 상품명 { get; set; } = string.Empty;
    public int 수량 { get; set; }
    public decimal 단가 { get; set; }
}
