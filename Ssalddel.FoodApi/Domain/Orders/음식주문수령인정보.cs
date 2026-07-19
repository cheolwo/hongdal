namespace Ssalddel.FoodApi.Domain.Orders;

public sealed class 음식주문수령인정보
{
    public string 수령인명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string? 상세주소 { get; set; }
    public string? 요청사항 { get; set; }
    public bool 주문자본인수령여부 { get; set; }
}
