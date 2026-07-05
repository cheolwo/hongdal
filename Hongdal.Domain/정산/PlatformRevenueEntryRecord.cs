using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.정산;

[Table("platform_revenue_entries")]
public sealed class PlatformRevenueEntryRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("revenue_source")]
    [MaxLength(100)]
    public string RevenueSource { get; set; } = string.Empty;

    [Column("source_reference_type")]
    [MaxLength(100)]
    public string SourceReferenceType { get; set; } = string.Empty;

    [Column("source_reference_id")]
    [MaxLength(200)]
    public string SourceReferenceId { get; set; } = string.Empty;

    [Column("payer_user_id")]
    [MaxLength(450)]
    public string PayerUserId { get; set; } = string.Empty;

    [Column("related_participant_user_id")]
    [MaxLength(450)]
    public string RelatedParticipantUserId { get; set; } = string.Empty;

    [Column("gross_amount", TypeName = "decimal(18,2)")]
    public decimal GrossAmount { get; set; }

    [Column("platform_revenue_amount", TypeName = "decimal(18,2)")]
    public decimal PlatformRevenueAmount { get; set; }

    [Column("currency_code")]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = "KRW";

    [Column("occurred_at_utc")]
    public DateTime OccurredAtUtc { get; set; }

    [Column("memo")]
    [MaxLength(1000)]
    public string Memo { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
