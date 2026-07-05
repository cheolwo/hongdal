using Hongdal.Contracts.Common.Participants;

namespace Hongdal.Contracts.Logistics;

public sealed class 용달기사요약Dto
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
}

public sealed class 화주요약Dto
{
    public string 화주Id { get; set; } = string.Empty;
    public string 화주명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
}

public sealed class 물류운송요청Dto
{
    public string 운송의뢰번호 { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public string 주문자UserId { get; set; } = string.Empty;
    public 물류전달받는자정보Dto 전달받는자정보 { get; set; } = new();
    public string 출발지주소 { get; set; } = string.Empty;
    public string 도착지주소 { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
}
