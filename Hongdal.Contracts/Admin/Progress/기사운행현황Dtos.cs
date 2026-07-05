namespace Hongdal.Contracts.Admin.Progress;

public sealed class 현재운행기사조회요청
{
    public string? 운행상태 { get; set; }
    public string? 기사명검색어 { get; set; }
    public string? 활동지역검색어 { get; set; }
}

public sealed class 현재운행기사응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public DateTime? 최근근무시작시각 { get; set; }
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
}