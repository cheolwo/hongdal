namespace Ssalddel.Contracts.Admin.Food;

public static class 음식주문운영추적상태코드
{
    public const string 정상 = "정상";
    public const string 진행중 = "진행중";
    public const string 주의 = "주의";
    public const string 복구필요 = "복구필요";
    public const string 완료 = "완료";
    public const string 종료 = "종료";
    public const string 미시작 = "미시작";
    public const string 해당없음 = "해당없음";
}

public sealed class 음식주문운영추적응답
{
    public string 주문번호 { get; set; } = string.Empty;

    public string 음식점명 { get; set; } = string.Empty;

    public string 주문상태 { get; set; } = string.Empty;

    public string 배차상태 { get; set; } = string.Empty;

    public string 전체상태 { get; set; } = 음식주문운영추적상태코드.진행중;

    public long? 배차대기Id { get; set; }

    public string 운송번호 { get; set; } = string.Empty;

    public string 운송상태 { get; set; } = string.Empty;

    public string 원본의뢰유형 { get; set; } = string.Empty;

    public string 원본의뢰Id { get; set; } = string.Empty;

    public string 커뮤니티원장Id { get; set; } = string.Empty;

    public string 커뮤니티원장상태 { get; set; } = string.Empty;

    public string 추천상태 { get; set; } = 음식주문운영추적상태코드.미시작;

    public int 추천라운드 { get; set; }

    public DateTime? 추천만료시각Utc { get; set; }

    public bool 추천만료됨 { get; set; }

    public DateTime 생성시각Utc { get; set; }

    public DateTime 최근변경시각Utc { get; set; }

    public DateTime 조회시각Utc { get; set; }

    public IReadOnlyList<음식주문운영체크포인트응답> 체크포인트 { get; set; } = [];

    public IReadOnlyList<음식주문운영Outbox응답> Outbox목록 { get; set; } = [];

    public IReadOnlyList<음식주문운영이벤트응답> 운송이벤트목록 { get; set; } = [];

    public IReadOnlyList<string> 경고목록 { get; set; } = [];

    public IReadOnlyList<string> 복구안내목록 { get; set; } = [];
}

public sealed class 음식주문운영체크포인트응답
{
    public string 단계Key { get; set; } = string.Empty;

    public string 단계명 { get; set; } = string.Empty;

    public string 상태 { get; set; } = 음식주문운영추적상태코드.미시작;

    public string 설명 { get; set; } = string.Empty;

    public DateTime? 변경시각Utc { get; set; }
}

public sealed class 음식주문운영Outbox응답
{
    public string 종류 { get; set; } = string.Empty;

    public long OutboxId { get; set; }

    public string 상태 { get; set; } = string.Empty;

    public int 시도횟수 { get; set; }

    public DateTime? 마지막시도시각Utc { get; set; }

    public DateTime 갱신시각Utc { get; set; }

    public bool 재시도예정 { get; set; }

    public bool 운영자확인필요 { get; set; }

    public string 실패요약 { get; set; } = string.Empty;
}

public sealed class 음식주문운영이벤트응답
{
    public long 이벤트Id { get; set; }

    public string 이벤트유형 { get; set; } = string.Empty;

    public DateTime 이벤트시각Utc { get; set; }
}
