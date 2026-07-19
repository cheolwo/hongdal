using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.정산;

[Table("platform_profit_return_schedules")]
public sealed class PlatformProfitReturnScheduleRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("policy_id")]
    public Guid PolicyId { get; set; }

    [Column("participant_user_id")]
    [MaxLength(450)]
    public string ParticipantUserId { get; set; } = string.Empty;

    [Column("participant_name")]
    [MaxLength(200)]
    public string ParticipantName { get; set; } = string.Empty;

    [Column("participant_category")]
    [MaxLength(100)]
    public string ParticipantCategory { get; set; } = string.Empty;

    [Column("period_start_date")]
    public DateOnly PeriodStartDate { get; set; }

    [Column("period_end_date")]
    public DateOnly PeriodEndDate { get; set; }

    [Column("scheduled_payment_date")]
    public DateOnly ScheduledPaymentDate { get; set; }

    [Column("total_platform_revenue_amount", TypeName = "decimal(18,2)")]
    public decimal TotalPlatformRevenueAmount { get; set; }

    [Column("operating_cost_amount", TypeName = "decimal(18,2)")]
    public decimal OperatingCostAmount { get; set; }

    [Column("estimated_profit_amount", TypeName = "decimal(18,2)")]
    public decimal EstimatedProfitAmount { get; set; }

    [Column("return_pool_amount", TypeName = "decimal(18,2)")]
    public decimal ReturnPoolAmount { get; set; }

    [Column("participant_weight", TypeName = "decimal(18,4)")]
    public decimal ParticipantWeight { get; set; }

    [Column("planned_return_amount", TypeName = "decimal(18,2)")]
    public decimal PlannedReturnAmount { get; set; }

    [Column("status")]
    [MaxLength(100)]
    public string Status { get; set; } = string.Empty;

    [Column("memo")]
    [MaxLength(1000)]
    public string Memo { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public PlatformProfitReturnPolicyRecord? Policy { get; set; }
}
