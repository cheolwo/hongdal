namespace ShipperApp.Models.Shipper;

public sealed class ShipperRequestItem
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = string.Empty;
    public string 차량종류 { get; set; } = string.Empty;
    public string 결제수단 { get; set; } = string.Empty;
    public int? 결제예정금액 { get; set; }
    public DateTime 생성일시 { get; set; }
    public string? 픽업지 { get; set; }
    public string? 하차지 { get; set; }

    public bool CanPay => string.Equals(배차상태, "상차완료", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(결제상태, "결제완료", StringComparison.OrdinalIgnoreCase);
}
