namespace ShipperApp.Models.Shipper;

public sealed class 배차주소저장요청
{
    public string 상차지우편번호 { get; set; } = string.Empty;
    public string 상차지기본주소 { get; set; } = string.Empty;
    public string 상차지상세주소 { get; set; } = string.Empty;

    public string 하차지우편번호 { get; set; } = string.Empty;
    public string 하차지기본주소 { get; set; } = string.Empty;
    public string 하차지상세주소 { get; set; } = string.Empty;

    public string 사업자등록번호 { get; set; } = string.Empty;
}
