namespace Hongdal.Contracts.Common.Hr;

public sealed class SocialInsuranceEligibilityAssessmentRequest
{
    public Guid? EmploymentContractId { get; set; }
    public string WorkerUserId { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string EmployerScopeType { get; set; } = HrScopeTypes.Platform;
    public string EmployerScopeId { get; set; } = HrScopeIds.Global;
    public string EmployerName { get; set; } = string.Empty;
    public string ContractType { get; set; } = HrEmploymentContractTypes.PartTime;
    public DateOnly ContractStartDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public decimal? ExpectedWeeklyWorkHours { get; set; }
    public decimal? ExpectedMonthlyWorkHours { get; set; }
    public int? ExpectedMonthlyWorkDays { get; set; }
    public decimal? ExpectedMonthlyWage { get; set; }
    public int? ExpectedEmploymentMonths { get; set; }
    public bool IsDailyWorker { get; set; }
    public bool EmployerCanEmployWorkers { get; set; }
    public bool EmployerHasBusinessRegistration { get; set; }
    public bool MultipleWorkplacesTotalMonthlyHoursAtLeast60 { get; set; }
    public bool WorkerWantsNationalPensionWhenShortTime { get; set; }
    public bool PreferEdi { get; set; } = true;
    public string Memo { get; set; } = string.Empty;
}

public sealed class SocialInsuranceEligibilityAssessmentResponse
{
    public Guid? EmploymentContractId { get; set; }
    public string WorkerUserId { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string EmployerScopeType { get; set; } = HrScopeTypes.Platform;
    public string EmployerScopeId { get; set; } = HrScopeIds.Global;
    public string EmployerName { get; set; } = string.Empty;
    public string OverallStatus { get; set; } = SocialInsuranceFilingStatusCodes.ManualReviewRequired;
    public IReadOnlyList<SocialInsuranceEligibilityItem> Items { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public DateTimeOffset AssessedAtUtc { get; set; }
}

public sealed class SocialInsuranceEligibilityItem
{
    public string InsuranceType { get; set; } = SocialInsuranceTypeCodes.HealthInsurance;
    public string Decision { get; set; } = SocialInsuranceEligibilityDecisionCodes.ManualReviewRequired;
    public string RecommendedFilingChannel { get; set; } = SocialInsuranceFilingChannelCodes.Edi;
    public IReadOnlyList<string> ReasonCodes { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string Note { get; set; } = string.Empty;
}

public sealed class SocialInsuranceFilingPlanCreateRequest
{
    public SocialInsuranceEligibilityAssessmentRequest Assessment { get; set; } = new();
    public IReadOnlyList<string> SelectedInsuranceTypes { get; set; } = [];
    public DateOnly? DueDate { get; set; }
    public string PreparedByUserId { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
}

public sealed class SocialInsuranceFilingPlanResponse
{
    public Guid Id { get; set; }
    public Guid? EmploymentContractId { get; set; }
    public string WorkerUserId { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string EmployerScopeType { get; set; } = HrScopeTypes.Platform;
    public string EmployerScopeId { get; set; } = HrScopeIds.Global;
    public string EmployerName { get; set; } = string.Empty;
    public string FilingChannel { get; set; } = SocialInsuranceFilingChannelCodes.Edi;
    public string FilingStatus { get; set; } = SocialInsuranceFilingStatusCodes.EdiPreparationReady;
    public DateOnly? DueDate { get; set; }
    public IReadOnlyList<SocialInsuranceEligibilityItem> Items { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string PreparedByUserId { get; set; } = string.Empty;
    public DateTimeOffset PreparedAtUtc { get; set; }
    public string? SubmittedByUserId { get; set; }
    public DateTimeOffset? SubmittedAtUtc { get; set; }
    public string SubmissionReferenceNumber { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class SocialInsuranceFilingPlanListResponse
{
    public IReadOnlyList<SocialInsuranceFilingPlanResponse> Items { get; set; } = [];
}

public sealed class SocialInsuranceFilingStatusUpdateRequest
{
    public string FilingStatus { get; set; } = SocialInsuranceFilingStatusCodes.SubmittedByEdi;
    public string SubmittedByUserId { get; set; } = string.Empty;
    public string SubmissionReferenceNumber { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
}

public static class SocialInsuranceTypeCodes
{
    public const string HealthInsurance = "HealthInsurance";
    public const string NationalPension = "NationalPension";
    public const string EmploymentInsurance = "EmploymentInsurance";
    public const string IndustrialAccidentInsurance = "IndustrialAccidentInsurance";
}

public static class SocialInsuranceEligibilityDecisionCodes
{
    public const string Required = "Required";
    public const string NotRequired = "NotRequired";
    public const string ManualReviewRequired = "ManualReviewRequired";
}

public static class SocialInsuranceFilingChannelCodes
{
    public const string Edi = "Edi";
    public const string Manual = "Manual";
}

public static class SocialInsuranceFilingStatusCodes
{
    public const string EdiPreparationReady = "EdiPreparationReady";
    public const string ManualPreparationReady = "ManualPreparationReady";
    public const string SubmittedByEdi = "SubmittedByEdi";
    public const string SubmittedManually = "SubmittedManually";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string ManualReviewRequired = "ManualReviewRequired";
    public const string Cancelled = "Cancelled";
}

public static class SocialInsuranceFilingRequiredActionCodes
{
    public const string ConfirmEmployerEntity = "ConfirmEmployerEntity";
    public const string ConfirmBusinessRegistration = "ConfirmBusinessRegistration";
    public const string ConfirmWorkerIdentity = "ConfirmWorkerIdentity";
    public const string ConfirmWorkPattern = "ConfirmWorkPattern";
    public const string PrepareEdiSubmission = "PrepareEdiSubmission";
    public const string PrepareManualSubmission = "PrepareManualSubmission";
    public const string ReviewLaborRules = "ReviewLaborRules";
    public const string UpdateSubmissionResult = "UpdateSubmissionResult";
}
