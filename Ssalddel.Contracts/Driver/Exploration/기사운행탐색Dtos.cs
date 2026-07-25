using Ssalddel.Contracts.Common.Exploration;

namespace Ssalddel.Contracts.Driver.Exploration;

[Obsolete("탐색캠페인생성요청 사용")]
public sealed class 운행탐색생성요청
{
    public string 탐색명 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string? 경유권역Json { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public decimal? 최대적재중량Kg { get; set; }
    public decimal? 최대적재부피Cbm { get; set; }
    public int 모집대상수 { get; set; }
    public string? 메모 { get; set; }
}

[Obsolete("탐색캠페인응답 사용")]
public class 운행탐색생성응답
{
    public long Id { get; set; }
    public string 기사Id { get; set; } = string.Empty;
    public string 탐색명 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public decimal? 최대적재중량Kg { get; set; }
    public decimal? 최대적재부피Cbm { get; set; }
    public int 모집대상수 { get; set; }
    public string 탐색상태 { get; set; } = 운행탐색상태값.초안;
    public string? 메모 { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

[Obsolete("탐색캠페인목록항목응답 사용")]
public class 운행탐색목록항목응답
{
    public long Id { get; set; }
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

[Obsolete("탐색캠페인상세응답 사용")]
public sealed class 운행탐색상세응답 : 운행탐색생성응답
{
    public string? 실행판단사유 { get; set; }
    public int 응답수 { get; set; }
    public int 있음응답수 { get; set; }
    public decimal? 예상총중량Kg { get; set; }
    public decimal? 예상총부피Cbm { get; set; }
    public IReadOnlyList<운행탐색대상화주응답> 대상화주목록 { get; set; } = Array.Empty<운행탐색대상화주응답>();
}

[Obsolete("탐색캠페인추천대상응답 사용")]
public sealed class 운행탐색추천화주응답
{
    public string 화주UserId { get; set; } = string.Empty;
    public string 화주명 { get; set; } = string.Empty;
    public string? 연락처마스킹 { get; set; }
    public decimal 친구관계점수 { get; set; }
    public decimal 반응가능성점수 { get; set; }
    public decimal 최종추천점수 { get; set; }
    public string 선정사유 { get; set; } = string.Empty;
    public string? 선호출발권역 { get; set; }
    public string? 선호도착권역 { get; set; }
}

[Obsolete("탐색캠페인대상자응답 사용")]
public sealed class 운행탐색대상화주응답
{
    public string 화주UserId { get; set; } = string.Empty;
    public string 화주명 { get; set; } = string.Empty;
    public decimal 친구관계점수Snapshot { get; set; }
    public string 대상상태 { get; set; } = string.Empty;
    public string 선정사유 { get; set; } = string.Empty;
    public DateTime? 마지막응답일시 { get; set; }
    public string? 응답유형 { get; set; }
    public decimal? 예상중량Kg { get; set; }
    public decimal? 예상부피Cbm { get; set; }
}

[Obsolete("탐색캠페인발송요청 사용")]
public sealed class 운행탐색제안발송요청
{
    public IReadOnlyList<string> 화주UserIds { get; set; } = Array.Empty<string>();
    public string? 발송메시지 { get; set; }
}

[Obsolete("탐색캠페인실행판단요청 사용")]
public sealed class 운행탐색실행판단요청
{
    public string? 실행판단사유 { get; set; }
    public bool 강제실행검토전환 { get; set; }
}
