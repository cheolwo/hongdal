namespace 살뜰.도메인.설정;

using 살뜰.도메인.기사;

/// <summary>
/// 기사 지급 승인 뒤 외부 지급 경계를 처리하기 위한 재시도 가능한 Outbox입니다.
/// 계좌번호·예금주명 같은 개인정보는 payload와 감사 필드에 저장하지 않습니다.
/// </summary>
public sealed class 기사지급Outbox
{
    public long Id { get; set; }

    public long 기사지급요청Id { get; set; }

    public 기사운송대금지급요청? 기사지급요청 { get; set; }

    public string 멱등키 { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public string 처리상태 { get; set; } = 기사지급Outbox상태코드.대기;

    public int 시도횟수 { get; set; }

    public DateTime? 다음시도시각Utc { get; set; }

    public DateTime? 마지막시도시각Utc { get; set; }

    public string 마지막결과코드 { get; set; } = string.Empty;

    public string 마지막오류메시지 { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class 기사지급Outbox상태코드
{
    public const string 대기 = "Pending";
    public const string 재시도대기 = "RetryScheduled";
    public const string Simulation검증완료 = "SimulationVerified";
    public const string 운영Provider미구성 = "OperationalProviderNotConfigured";
}
