using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("결제승인완료_Outbox")]
public class 결제승인완료Outbox
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("payment_record_id")]
    public long 결제레코드Id { get; set; }

    [Column("payment_id")]
    [MaxLength(64)]
    public string 결제Id { get; set; } = string.Empty;

    [Column("target_type")]
    public int 결제대상유형 { get; set; }

    [Column("target_id")]
    [MaxLength(128)]
    public string 대상Id { get; set; } = string.Empty;

    [Column("provider_type")]
    public int 결제제공자 { get; set; }

    [Column("amount")]
    public int 결제금액 { get; set; }

    [Column("currency")]
    [MaxLength(10)]
    public string 통화 { get; set; } = "KRW";

    [Column("approved_at")]
    public DateTime 승인일시Utc { get; set; }

    [Column("payload_json")]
    public string PayloadJson { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(50)]
    public string 처리상태 { get; set; } = "Pending";

    [Column("retry_count")]
    public int 시도횟수 { get; set; }

    [Column("last_attempted_at")]
    public DateTime? 마지막시도시각 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
