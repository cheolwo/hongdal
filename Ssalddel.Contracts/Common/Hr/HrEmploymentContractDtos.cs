using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Contracts.Common.Hr;

public sealed class HrEmploymentContractResponse
{
    public Guid Id { get; set; }
    public string WorkerUserId { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "근로계약 당사자 식별",
        IsContractData = true,
        ProtectionNote = "근로계약 목록에서는 필요한 범위의 표시명만 노출")]
    public string WorkerName { get; set; } = string.Empty;
    public string EmployerScopeType { get; set; } = HrScopeTypes.Platform;
    public string EmployerScopeId { get; set; } = HrScopeIds.Global;
    public string EmployerName { get; set; } = string.Empty;
    public string ContractType { get; set; } = HrEmploymentContractTypes.PartTime;
    public string ContractStatus { get; set; } = HrEmploymentContractStatuses.Draft;
    public DateOnly ContractStartDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public string WorkDescription { get; set; } = string.Empty;
    public string WageType { get; set; } = HrWageTypes.Hourly;
    public decimal WageAmount { get; set; }
    public decimal? MinimumWageAmount { get; set; }
    public bool MinimumWageCheckPassed { get; set; }
    public string MinimumWageCheckMessage { get; set; } = string.Empty;
    public string PaymentCycle { get; set; } = HrPaymentCycles.Monthly;
    public int PaymentDayOfMonth { get; set; }
    [IsmsPProtectedData(
        PersonalDataFieldKey.PaymentMethod,
        "급여 지급 방식 확인",
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem)]
    public string PaymentMethod { get; set; } = HrPaymentMethods.BankTransfer;
    public string BankName { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.BankAccountNumber,
        "급여 지급 계좌 표시",
        IsContractData = true,
        ProtectionNote = "응답 DTO는 마스킹된 계좌번호만 포함")]
    public string MaskedAccountNumber { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "급여 계좌 예금주 확인",
        IsContractData = true,
        ProtectionNote = "예금주명은 급여 지급 담당자와 계약 당사자 범위로 제한")]
    public string AccountHolderName { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public IReadOnlyList<HrPayrollScheduleResponse> PayrollSchedules { get; set; } = [];
}

public sealed class HrEmploymentContractListResponse
{
    public IReadOnlyList<HrEmploymentContractResponse> Items { get; set; } = [];
}

public sealed class HrEmploymentContractDraftRequest
{
    public string WorkerUserId { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "근로계약 초안의 근로자 식별",
        IsContractData = true)]
    public string WorkerName { get; set; } = string.Empty;
    public string EmployerScopeType { get; set; } = HrScopeTypes.Platform;
    public string EmployerScopeId { get; set; } = HrScopeIds.Global;
    public string EmployerName { get; set; } = string.Empty;
    public string ContractType { get; set; } = HrEmploymentContractTypes.PartTime;
    public DateOnly ContractStartDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public string WorkDescription { get; set; } = string.Empty;
    public string WageType { get; set; } = HrWageTypes.Hourly;
    public decimal WageAmount { get; set; }
    public decimal? MinimumWageAmount { get; set; }
    public string PaymentCycle { get; set; } = HrPaymentCycles.Monthly;
    public int PaymentDayOfMonth { get; set; } = 10;
    [IsmsPProtectedData(
        PersonalDataFieldKey.PaymentMethod,
        "급여 지급 방식 등록",
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem)]
    public string PaymentMethod { get; set; } = HrPaymentMethods.BankTransfer;
    public string BankName { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.BankAccountNumber,
        "급여 지급 계좌 등록",
        IsContractData = true,
        ProtectionNote = "요청 DTO의 원본 계좌번호는 저장/전송 보호와 접근 로그가 필요")]
    public string AccountNumber { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "급여 계좌 예금주 등록",
        IsContractData = true)]
    public string AccountHolderName { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
}

public sealed class HrEmploymentContractSignRequest
{
    public string SignedByUserId { get; set; } = string.Empty;
}

public sealed class HrPayrollScheduleCreateRequest
{
    public DateOnly ScheduleStartDate { get; set; }
    public DateOnly ScheduleEndDate { get; set; }
}

public sealed class HrPayrollScheduleResponse
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string WorkerUserId { get; set; } = string.Empty;
    public string EmployerScopeType { get; set; } = HrScopeTypes.Platform;
    public string EmployerScopeId { get; set; } = HrScopeIds.Global;
    public DateOnly WorkPeriodStartDate { get; set; }
    public DateOnly WorkPeriodEndDate { get; set; }
    public DateOnly ScheduledPaymentDate { get; set; }
    public decimal PlannedAmount { get; set; }
    public string CurrencyCode { get; set; } = "KRW";
    [IsmsPProtectedData(
        PersonalDataFieldKey.PaymentMethod,
        "급여 지급 예정 방식 표시",
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem)]
    public string PaymentMethod { get; set; } = HrPaymentMethods.BankTransfer;
    public string Status { get; set; } = HrPayrollScheduleStatuses.Planned;
    public string Memo { get; set; } = string.Empty;
}

public sealed class HrPayrollScheduleListResponse
{
    public IReadOnlyList<HrPayrollScheduleResponse> Items { get; set; } = [];
}

public static class HrEmploymentContractTypes
{
    public const string PartTime = "PartTime";
    public const string FixedTerm = "FixedTerm";
    public const string Regular = "Regular";
    public const string Contractor = "Contractor";
}

public static class HrEmploymentContractStatuses
{
    public const string Draft = "Draft";
    public const string Signed = "Signed";
    public const string Active = "Active";
    public const string Ended = "Ended";
    public const string Cancelled = "Cancelled";
}

public static class HrWageTypes
{
    public const string Hourly = "Hourly";
    public const string Daily = "Daily";
    public const string Monthly = "Monthly";
    public const string PerTask = "PerTask";
}

public static class HrPaymentCycles
{
    public const string Monthly = "Monthly";
    public const string Weekly = "Weekly";
    public const string Biweekly = "Biweekly";
}

public static class HrPaymentMethods
{
    public const string BankTransfer = "BankTransfer";
    public const string Cash = "Cash";
    public const string PlatformSettlement = "PlatformSettlement";
}

public static class HrPayrollScheduleStatuses
{
    public const string Planned = "Planned";
    public const string Approved = "Approved";
    public const string PaymentRequested = "PaymentRequested";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
}
