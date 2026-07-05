namespace Hongdal.Contracts.Common.Participants;

public static class 업무도메인구분
{
    public const string 음식배달 = "FoodDelivery";
    public const string 화주물류 = "Logistics";
}

public sealed class 음식주문수령인정보Dto
{
    public string 수령인명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string? 상세주소 { get; set; }
    public string? 요청사항 { get; set; }
    public bool 주문자본인수령여부 { get; set; }
}

public sealed class 물류전달받는자정보Dto
{
    public string 전달받는자명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string? 상세주소 { get; set; }
    public string? 업체명 { get; set; }
    public string? 요청사항 { get; set; }
}
