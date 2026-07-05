using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("사용자_행위_로그")]
public class 사용자행위로그
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("app_key")]
    [MaxLength(100)]
    public string AppKey { get; set; } = string.Empty;

    [Column("user_id")]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Column("user_name")]
    [MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Column("role_name")]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [Column("email_masked")]
    [MaxLength(256)]
    public string EmailMasked { get; set; } = string.Empty;

    [Column("phone_last4")]
    [MaxLength(4)]
    public string PhoneLast4 { get; set; } = string.Empty;

    [Column("action_type")]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [Column("action_name")]
    [MaxLength(200)]
    public string ActionName { get; set; } = string.Empty;

    [Column("route")]
    [MaxLength(300)]
    public string Route { get; set; } = string.Empty;

    [Column("trace_id")]
    [MaxLength(100)]
    public string TraceId { get; set; } = string.Empty;

    [Column("is_success")]
    public bool IsSuccess { get; set; }

    [Column("error_code")]
    [MaxLength(100)]
    public string ErrorCode { get; set; } = string.Empty;

    [Column("error_message")]
    [MaxLength(2000)]
    public string ErrorMessage { get; set; } = string.Empty;

    [Column("client_ip")]
    [MaxLength(100)]
    public string ClientIp { get; set; } = string.Empty;

    [Column("user_agent")]
    [MaxLength(1000)]
    public string UserAgent { get; set; } = string.Empty;

    [Column("metadata_json", TypeName = "longtext")]
    public string MetadataJson { get; set; } = string.Empty;

    [Column("occurred_at_utc")]
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
