namespace 살뜰.도메인.기사;

/// <summary>
/// 기사 본인이 등록한 정산 입금 계좌입니다.
/// 계좌번호와 예금주명은 persistence 경계의 value converter로 암호화합니다.
/// </summary>
public sealed class 기사정산계좌
{
    public long Id { get; set; }

    public string 기사Id { get; set; } = string.Empty;

    public string 국가코드 { get; set; } = "KR";

    public string 은행명 { get; set; } = string.Empty;

    public string 예금주명 { get; set; } = string.Empty;

    public string 계좌번호 { get; set; } = string.Empty;

    public string 확인상태 { get; set; } = 기사정산계좌확인상태.미확인;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class 기사정산계좌확인상태
{
    public const string 미확인 = "Unverified";
    public const string 확인중 = "Pending";
    public const string 확인완료 = "Verified";
}
