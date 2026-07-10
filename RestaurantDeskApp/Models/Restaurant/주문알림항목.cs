namespace RestaurantDeskApp.Models.Restaurant;

public sealed class 주문알림항목
{
    public long Id { get; set; }
    public string 주문번호 { get; set; } = string.Empty;
    public long 음식점Id { get; set; }
    public string 고객명 { get; set; } = string.Empty;
    public string 메뉴요약 { get; set; } = string.Empty;
    public decimal 주문금액 { get; set; }
    public DateTime 접수시각 { get; set; }
    public bool 미확인 { get; set; }
}
