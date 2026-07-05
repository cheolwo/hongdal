namespace Hongdal.Contracts.Common.Exploration;

public static class 탐색캠페인개시자역할값
{
    public const string 기사 = "기사";
    public const string 화주 = "화주";
}

public static class 탐색캠페인대상역할값
{
    public const string 기사 = "기사";
    public const string 화주 = "화주";
}

public static class 탐색캠페인유형값
{
    public const string 운행가능문의 = "운행가능문의";
    public const string 물량문의 = "물량문의";
}

public sealed class 탐색캠페인생성요청
{
    public string 탐색명 { get; set; } = string.Empty;
    public string 개시자역할 { get; set; } = string.Empty;
    public string 대상역할 { get; set; } = string.Empty;
    public string 탐색유형 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string? 희망복귀지주소 { get; set; }
    public decimal? 희망복귀지위도 { get; set; }
    public decimal? 희망복귀지경도 { get; set; }
    public string? 복귀지출처 { get; set; }
    public string? 경유권역Json { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public decimal? 최대적재중량Kg { get; set; }
    public decimal? 최대적재부피Cbm { get; set; }
    public int 모집대상수 { get; set; }
    public string? 메모 { get; set; }
}

public class 탐색캠페인응답
{
    public long Id { get; set; }
    public string 개시자UserId { get; set; } = string.Empty;
    public string 개시자역할 { get; set; } = string.Empty;
    public string 대상역할 { get; set; } = string.Empty;
    public string 탐색유형 { get; set; } = string.Empty;
    public string 탐색명 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string? 희망복귀지주소 { get; set; }
    public decimal? 희망복귀지위도 { get; set; }
    public decimal? 희망복귀지경도 { get; set; }
    public string? 복귀지출처 { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public decimal? 최대적재중량Kg { get; set; }
    public decimal? 최대적재부피Cbm { get; set; }
    public int 모집대상수 { get; set; }
    public string 탐색상태 { get; set; } = 탐색캠페인상태값.초안;
    public string? 메모 { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class 탐색캠페인목록항목응답
{
    public long Id { get; set; }
    public string 개시자역할 { get; set; } = string.Empty;
    public string 대상역할 { get; set; } = string.Empty;
    public string 탐색유형 { get; set; } = string.Empty;
    public string 탐색명 { get; set; } = string.Empty;
    public DateTime 운행예정일 { get; set; }
    public string 출발권역 { get; set; } = string.Empty;
    public string? 희망도착권역 { get; set; }
    public string? 희망복귀지주소 { get; set; }
    public string? 복귀지출처 { get; set; }
    public string 차량종류 { get; set; } = string.Empty;
    public string 탐색상태 { get; set; } = string.Empty;
    public int 모집대상수 { get; set; }
    public int 응답수 { get; set; }
    public int 있음응답수 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 탐색캠페인상세응답 : 탐색캠페인응답
{
    public string? 실행판단사유 { get; set; }
    public int 응답수 { get; set; }
    public int 있음응답수 { get; set; }
    public decimal? 예상총중량Kg { get; set; }
    public decimal? 예상총부피Cbm { get; set; }
    public IReadOnlyList<탐색캠페인대상자응답> 대상자목록 { get; set; } = Array.Empty<탐색캠페인대상자응답>();
}

public sealed class 탐색캠페인추천대상응답
{
    public string 대상UserId { get; set; } = string.Empty;
    public string 대상역할 { get; set; } = string.Empty;
    public string 대상명 { get; set; } = string.Empty;
    public string? 연락처마스킹 { get; set; }
    public decimal 관계점수 { get; set; }
    public decimal 반응가능성점수 { get; set; }
    public decimal 최종추천점수 { get; set; }
    public string 선정사유 { get; set; } = string.Empty;
    public string? 선호출발권역 { get; set; }
    public string? 선호도착권역 { get; set; }
}

public sealed class 탐색캠페인대상자응답
{
    public string 대상UserId { get; set; } = string.Empty;
    public string 대상역할 { get; set; } = string.Empty;
    public string 대상명 { get; set; } = string.Empty;
    public decimal 관계점수Snapshot { get; set; }
    public string 대상상태 { get; set; } = string.Empty;
    public string 선정사유 { get; set; } = string.Empty;
    public DateTime? 마지막응답일시 { get; set; }
    public string? 응답유형 { get; set; }
    public decimal? 예상중량Kg { get; set; }
    public decimal? 예상부피Cbm { get; set; }
}

public sealed class 탐색캠페인발송요청
{
    public IReadOnlyList<string> 대상UserIds { get; set; } = Array.Empty<string>();
    public string? 발송메시지 { get; set; }
}

public sealed class 탐색캠페인실행판단요청
{
    public string? 실행판단사유 { get; set; }
    public bool 강제실행검토전환 { get; set; }
}

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
    public DateTime? 발송일시 { get; set; }
}

public sealed class 탐색문의상세응답 : 탐색문의목록항목응답
{
    public string? 발송메시지 { get; set; }
    public string? 메모 { get; set; }
    public string? 희망복귀지주소 { get; set; }
    public string? 복귀지출처 { get; set; }
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
