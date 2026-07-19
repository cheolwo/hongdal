using Ssalddel.Contracts.Common.Exploration;

namespace Ssalddel.Contracts.Shipper.Exploration;

public class 탐색문의목록항목응답
{
    public long 탐색캠페인Id { get; set; }
    public string 탐색명 { get; set; } = string.Empty;
    public string 개시자UserId { get; set; } = string.Empty;
    public string 개시자명 { get; set; } = string.Empty;
    public string 개시자역할 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public string 대상상태 { get; set; } = 탐색캠페인대상상태값.발송됨;
    public DateTime 발송일시 { get; set; }
}

public sealed class 탐색문의상세응답 : 탐색문의목록항목응답
{
    public string? 발송메시지 { get; set; }
    public string? 메모 { get; set; }
    public decimal? 최대적재중량Kg { get; set; }
    public decimal? 최대적재부피Cbm { get; set; }
    public 운행문의응답유형? 기존응답유형 { get; set; }
    public DateTime? 기존응답일시 { get; set; }
}

public sealed class 탐색문의응답요청
{
    public 운행문의응답유형 응답유형 { get; set; }
    public DateTime? 희망상차일시 { get; set; }
    public string? 출발지요약 { get; set; }
    public string? 도착지요약 { get; set; }
    public decimal? 예상중량Kg { get; set; }
    public decimal? 예상부피Cbm { get; set; }
    public int? 예상팔레트개수 { get; set; }
    public string? 메모 { get; set; }
}
