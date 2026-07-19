using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.사용자;

[Table("hr_payroll_schedules")]
public sealed class HrPayrollScheduleRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("contract_id")]
    public Guid ContractId { get; set; }

    [Column("worker_user_id")]
    [MaxLength(450)]
    public string WorkerUserId { get; set; } = string.Empty;

    [Column("employer_scope_type")]
    [MaxLength(100)]
    public string EmployerScopeType { get; set; } = string.Empty;

    [Column("employer_scope_id")]
    [MaxLength(200)]
    public string EmployerScopeId { get; set; } = string.Empty;

    [Column("work_period_start_date")]
    public DateOnly WorkPeriodStartDate { get; set; }

    [Column("work_period_end_date")]
    public DateOnly WorkPeriodEndDate { get; set; }

    [Column("scheduled_payment_date")]
    public DateOnly ScheduledPaymentDate { get; set; }

    [Column("planned_amount", TypeName = "decimal(18,2)")]
    public decimal PlannedAmount { get; set; }

    [Column("currency_code")]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = "KRW";

    [Column("payment_method")]
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty;

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

    public HrEmploymentContractRecord? Contract { get; set; }
}
