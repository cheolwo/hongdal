namespace Ssalddel.Contracts.Admin.Exploration;

[Obsolete("기사화주관계집계응답 사용")]
public sealed class 기사화주인연집계응답
{
    public long Id { get; set; }
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 화주UserId { get; set; } = string.Empty;
    public string 화주명 { get; set; } = string.Empty;
    public DateTime? 최근거래일시 { get; set; }
    public int 누적운송건수 { get; set; }
    public decimal 최근응답률 { get; set; }
    public int 최근30일접점수 { get; set; }
    public decimal 취소율 { get; set; }
    public decimal 인연점수 { get; set; }
    public DateTime? 최근연락일시 { get; set; }
}

[Obsolete("탐색캠페인관리목록응답 사용")]
public sealed class 운행탐색관리목록응답
{
    public long Id { get; set; }
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 탐색명 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public string 탐색상태 { get; set; } = string.Empty;
    public int 모집대상수 { get; set; }
    public int 응답수 { get; set; }
    public int 있음응답수 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

[Obsolete("탐색캠페인응답통계응답 사용")]
public sealed class 운행탐색응답통계응답
{
    public int 총탐색수 { get; set; }
    public int 총발송대상수 { get; set; }
    public int 총응답수 { get; set; }
    public int 있음응답수 { get; set; }
    public decimal 전체응답률 { get; set; }
    public decimal 있음응답률 { get; set; }
}
