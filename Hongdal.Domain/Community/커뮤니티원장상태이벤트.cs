namespace Hongdal.Domain.Community;

public class 커뮤니티원장상태이벤트
{
    public long Id { get; set; }

    public string EventId { get; set; } = Guid.NewGuid().ToString("N");

    public string 커뮤니티원장Id { get; set; } = string.Empty;

    public string 커뮤니티Id { get; set; } = string.Empty;

    public string 원장템플릿Key { get; set; } = string.Empty;

    public string EventType { get; set; } = 커뮤니티원장상태이벤트유형.상태변경;

    public string? 이전상태 { get; set; }

    public string 상태 { get; set; } = string.Empty;

    public string? 현재단계Key { get; set; }

    public string? 변경사유 { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }

    public string SnapshotJson { get; set; } = "{}";

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class 커뮤니티원장상태이벤트유형
{
    public const string 저장 = "저장";
    public const string 상태변경 = "상태변경";
}
