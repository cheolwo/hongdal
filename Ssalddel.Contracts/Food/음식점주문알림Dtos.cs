namespace Ssalddel.Contracts.Food;

public sealed class 음식점주문수신알림
{
    public string 주문번호 { get; set; } = string.Empty;

    public long 음식점Id { get; set; }

    public string 고객명 { get; set; } = string.Empty;

    public string 메뉴요약 { get; set; } = string.Empty;

    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; set; } = [];

    public decimal 주문금액 { get; set; }

    public string 상태 { get; set; } = string.Empty;

    public DateTimeOffset 수신시각 { get; set; } = DateTimeOffset.UtcNow;

    public string 제목 { get; set; } = "신규 음식 주문";

    public string 본문 { get; set; } = string.Empty;
}
