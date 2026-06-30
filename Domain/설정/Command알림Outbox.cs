using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("Command_알림_Outbox")]
public class Command알림Outbox
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("command_name")]
    [MaxLength(200)]
    public string CommandName { get; set; } = string.Empty;

    [Column("event_name")]
    [MaxLength(200)]
    public string EventName { get; set; } = string.Empty;

    [Column("feature_name")]
    [MaxLength(100)]
    public string FeatureName { get; set; } = string.Empty;

    [Column("target")]
    [MaxLength(100)]
    public string Target { get; set; } = string.Empty;

    [Column("payload_json")]
    public string PayloadJson { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [Column("retry_count")]
    public int RetryCount { get; set; }

    [Column("trace_id")]
    [MaxLength(64)]
    public string TraceId { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
