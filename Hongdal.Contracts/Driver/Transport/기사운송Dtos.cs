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
    public string 결제방식 { get; set; } = string.Empty;
    public bool 인수증필요 { get; set; }
    public bool 인수증서명필수 { get; set; }
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

public sealed class 기사운송상차완료요청
{
    public string? 상차사진ObjectName { get; set; }
    public string? 상차사진Url { get; set; }
    public string? 인수증증빙방식 { get; set; }
    public string? 인수자명 { get; set; }
    public string? 인수자소속 { get; set; }
    public string? 인수자서명 { get; set; }
    public string? 기사서명 { get; set; }
    public bool 인수증확인완료 { get; set; }
    public bool 인수증서명생략확인 { get; set; }
    public string? 인수증서명생략사유 { get; set; }
}

public sealed class 기사운송문제신고요청
{
    public string 사유 { get; set; } = string.Empty;
    public string? 메모 { get; set; }
}
