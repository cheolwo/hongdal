using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.사용자;

[Table("hr_employment_contracts")]
public sealed class HrEmploymentContractRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("worker_user_id")]
    [MaxLength(450)]
    public string WorkerUserId { get; set; } = string.Empty;

    [Column("worker_name")]
    [MaxLength(200)]
    public string WorkerName { get; set; } = string.Empty;

    [Column("employer_scope_type")]
    [MaxLength(100)]
    public string EmployerScopeType { get; set; } = string.Empty;

    [Column("employer_scope_id")]
    [MaxLength(200)]
    public string EmployerScopeId { get; set; } = string.Empty;

    [Column("employer_name")]
    [MaxLength(200)]
    public string EmployerName { get; set; } = string.Empty;

    [Column("contract_type")]
    [MaxLength(100)]
    public string ContractType { get; set; } = string.Empty;

    [Column("contract_status")]
    [MaxLength(100)]
    public string ContractStatus { get; set; } = string.Empty;

    [Column("contract_start_date")]
    public DateOnly ContractStartDate { get; set; }

    [Column("contract_end_date")]
    public DateOnly? ContractEndDate { get; set; }

    [Column("work_description")]
    [MaxLength(1000)]
    public string WorkDescription { get; set; } = string.Empty;

    [Column("wage_type")]
    [MaxLength(100)]
    public string WageType { get; set; } = string.Empty;

    [Column("wage_amount", TypeName = "decimal(18,2)")]
    public decimal WageAmount { get; set; }

    [Column("minimum_wage_amount", TypeName = "decimal(18,2)")]
    public decimal? MinimumWageAmount { get; set; }

    [Column("minimum_wage_check_passed")]
    public bool MinimumWageCheckPassed { get; set; }

    [Column("minimum_wage_check_message")]
    [MaxLength(1000)]
    public string MinimumWageCheckMessage { get; set; } = string.Empty;

    [Column("payment_cycle")]
    [MaxLength(100)]
    public string PaymentCycle { get; set; } = string.Empty;

    [Column("payment_day_of_month")]
    public int PaymentDayOfMonth { get; set; }

    [Column("payment_method")]
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Column("bank_name")]
    [MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    [Column("account_number")]
    [MaxLength(200)]
    public string AccountNumber { get; set; } = string.Empty;

    [Column("account_holder_name")]
    [MaxLength(100)]
    public string AccountHolderName { get; set; } = string.Empty;

    [Column("signed_at_utc")]
    public DateTime? SignedAtUtc { get; set; }

    [Column("signed_by_user_id")]
    [MaxLength(450)]
    public string SignedByUserId { get; set; } = string.Empty;

    [Column("memo")]
    [MaxLength(2000)]
    public string Memo { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<HrPayrollScheduleRecord> PayrollSchedules { get; set; } = [];
}
