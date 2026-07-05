namespace Hongdal.Contracts.Driver.Transport;

public class 기사운송요약응답
{
    public long Id { get; set; }
    public string 운송번호 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 도착지 { get; set; } = string.Empty;
    public string 기사_운송자 { get; set; } = string.Empty;
    public DateTime? 출발_픽업 { get; set; }
    public DateTime? 도착 { get; set; }
    public decimal? 운임 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 기사운송상세응답 : 기사운송요약응답
{
    public string 첨부Json { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
}

public sealed class 기사운송상태변경응답
{
    public long Id { get; set; }
    public string 운송번호 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public sealed class 기사운송문제신고요청
{
    public string 사유 { get; set; } = string.Empty;
    public string? 메모 { get; set; }
}
