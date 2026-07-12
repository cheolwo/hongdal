namespace 홍달.도메인.설정;

public class 배차추천알림Outbox
{
    public long Id { get; set; }

    public long 배차대기Id { get; set; }

    public string 의뢰Id { get; set; } = string.Empty;

    public string 기사Id { get; set; } = string.Empty;

    public int 추천라운드 { get; set; }

    public string 제목 { get; set; } = string.Empty;

    public string 본문 { get; set; } = string.Empty;

    public string DataJson { get; set; } = string.Empty;

    public string 발송상태 { get; set; } = "Pending";

    public int 시도횟수 { get; set; }

    public DateTime? 마지막시도시각 { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
