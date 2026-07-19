namespace 살뜰.도메인.창고;

public class 출고묶음
{
    public long Id { get; set; }

    public string 출고묶음번호 { get; set; } = string.Empty;

    public string 주문참조번호 { get; set; } = string.Empty;

    public long 출고창고Id { get; set; }

    public string 판매자UserId { get; set; } = string.Empty;

    public string 주문자UserId { get; set; } = string.Empty;

    public string 상태 { get; set; } = 출고상태.예정;

    public DateTime? 피킹시작일시 { get; set; }

    public DateTime? 피킹완료일시 { get; set; }

    public DateTime? 포장완료일시 { get; set; }

    public DateTime? 출고완료일시 { get; set; }

    public string? 운송의뢰Id { get; set; }

    public string? 커뮤니티원장Id { get; set; }

    public string? 커뮤니티원장템플릿Key { get; set; }

    public string? 커뮤니티원장상태 { get; set; }

    public DateTime? 커뮤니티원장동기화시각Utc { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
