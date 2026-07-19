using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.정산;

[Table("platform_profit_return_policies")]
public sealed class PlatformProfitReturnPolicyRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("policy_name")]
    [MaxLength(200)]
    public string PolicyName { get; set; } = string.Empty;

    [Column("target_participant_category")]
    [MaxLength(100)]
    public string TargetParticipantCategory { get; set; } = string.Empty;

    [Column("return_rate_percent", TypeName = "decimal(9,4)")]
    public decimal ReturnRatePercent { get; set; }

    [Column("company_reserve_amount", TypeName = "decimal(18,2)")]
    public decimal CompanyReserveAmount { get; set; }

    [Column("minimum_profit_threshold", TypeName = "decimal(18,2)")]
    public decimal MinimumProfitThreshold { get; set; }

    [Column("effective_start_date")]
    public DateOnly EffectiveStartDate { get; set; }

    [Column("effective_end_date")]
    public DateOnly? EffectiveEndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("memo")]
    [MaxLength(1000)]
    public string Memo { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
