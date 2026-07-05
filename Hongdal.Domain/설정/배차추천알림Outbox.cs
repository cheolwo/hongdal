using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("배차추천_알림_Outbox")]
public class 배차추천알림Outbox
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("dispatch_waiting_id")]
    public long 배차대기Id { get; set; }

    [Column("request_id")]
    [MaxLength(64)]
    public string 의뢰Id { get; set; } = string.Empty;

    [Column("driver_id")]
    [MaxLength(64)]
    public string 기사Id { get; set; } = string.Empty;

    [Column("recommendation_round")]
    public int 추천라운드 { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string 제목 { get; set; } = string.Empty;

    [Column("body")]
    [MaxLength(500)]
    public string 본문 { get; set; } = string.Empty;

    [Column("data_json")]
    public string DataJson { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(50)]
    public string 발송상태 { get; set; } = "Pending";

    [Column("retry_count")]
    public int 시도횟수 { get; set; }

    [Column("last_attempted_at")]
    public DateTime? 마지막시도시각 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
