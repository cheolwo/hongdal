namespace Hongdal.Contracts.Admin.Progress;

public sealed class 운송진행응답
{
    public long Id { get; set; }
    public string 운송번호 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 출발_픽업 { get; set; }
    public DateTime? 도착 { get; set; }
    public string 기사_운송자 { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 도착지 { get; set; } = string.Empty;
    public decimal? 운임 { get; set; }
    public bool 예외신고됨 { get; set; }
    public string 최근예외단계 { get; set; } = string.Empty;
    public string 최근예외코드 { get; set; } = string.Empty;
    public string 최근예외메시지 { get; set; } = string.Empty;
    public bool 관리자확인필요 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 운송이벤트로그응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public string 이벤트타입 { get; set; } = string.Empty;
    public DateTime 이벤트시각 { get; set; }
    public string 메타데이터 { get; set; } = string.Empty;
}

public sealed class 운송원장이벤트응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public string 정산상태 { get; set; } = string.Empty;
    public DateTime? 의뢰UpdatedAt { get; set; }
    public long? 운송Id { get; set; }
    public string 운송상태 { get; set; } = string.Empty;
    public DateTime? 운송UpdatedAt { get; set; }
    public string Mongo원장Id { get; set; } = string.Empty;
    public bool Mongo원장존재 { get; set; }
    public string Mongo원장상태 { get; set; } = string.Empty;
    public string Mongo원장현재단계Key { get; set; } = string.Empty;
    public string Mongo원장대상OsCode { get; set; } = string.Empty;
    public DateTime? Mongo원장UpdatedAtUtc { get; set; }
    public int Mongo원장블록수 { get; set; }
    public bool Rdb운송실행투영존재 { get; set; }
    public string 원장동기화메시지 { get; set; } = string.Empty;
    public DateTime 마지막변경시각 { get; set; }
    public IReadOnlyList<운송원장이벤트항목응답> 이벤트목록 { get; set; } = [];
}

public sealed class 운송원장이벤트항목응답
{
    public long Id { get; set; }
    public string 이벤트타입 { get; set; } = string.Empty;
    public DateTime 이벤트시각 { get; set; }
    public string 메타데이터 { get; set; } = string.Empty;
}
