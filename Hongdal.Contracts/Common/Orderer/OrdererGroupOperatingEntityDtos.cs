using Hongdal.Contracts.Common.Hr;

namespace Hongdal.Contracts.Common.Orderer;

public static class OrdererGroupOperatingEntityTypeCode
{
    public const string InformalGroup = "InformalGroup";
    public const string IndividualBusiness = "IndividualBusiness";
    public const string Corporation = "Corporation";
    public const string Cooperative = "Cooperative";
    public const string ManagementOfficeEntrusted = "ManagementOfficeEntrusted";
    public const string PlatformEntrusted = "PlatformEntrusted";
}

public static class OrdererGroupBusinessVerificationStatusCode
{
    public const string NotRequired = "NotRequired";
    public const string Required = "Required";
    public const string Pending = "Pending";
    public const string Verified = "Verified";
    public const string Rejected = "Rejected";
}

public static class OrdererGroupEmploymentReadinessStatusCode
{
    public const string NotReady = "NotReady";
    public const string NeedsBusinessEntity = "NeedsBusinessEntity";
    public const string ReadyForDraftContract = "ReadyForDraftContract";
    public const string ContractingInProgress = "ContractingInProgress";
    public const string ReadyToOperate = "ReadyToOperate";
}

public static class OrdererGroupWorkerSourcePreferenceCode
{
    public const string InternalResidentPreferred = "InternalResidentPreferred";
    public const string InternalResidentOnly = "InternalResidentOnly";
    public const string ExternalAllowed = "ExternalAllowed";
}

public sealed class OrdererGroupOperatingEntityQuery
{
    public string? OrdererGroupScopeKey { get; set; }
    public string? EntityType { get; set; }
    public string? BusinessVerificationStatus { get; set; }
    public bool? CanActAsImporterOfRecord { get; set; }
    public bool? CanEmployWorkers { get; set; }
}

public sealed class OrdererGroupOperatingEntityDto
{
    public string EntityId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string EmploymentEmployerScopeType { get; set; } = HrScopeTypes.OrdererGroup;
    public string EmploymentEmployerScopeId { get; set; } = string.Empty;
    public string EntityType { get; set; } = OrdererGroupOperatingEntityTypeCode.InformalGroup;
    public string RepresentativeUserId { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string LegalEntityName { get; set; } = string.Empty;
    public string BusinessRegistrationNumber { get; set; } = string.Empty;
    public string MaskedBusinessRegistrationNumber { get; set; } = string.Empty;
    public string BusinessVerificationStatus { get; set; } = OrdererGroupBusinessVerificationStatusCode.Required;
    public bool CanActAsImporterOfRecord { get; set; }
    public bool CanEmployWorkers { get; set; }
    public bool CanIssuePayroll { get; set; }
    public string EmploymentReadinessStatus { get; set; } = OrdererGroupEmploymentReadinessStatusCode.NotReady;
    public string ImportCustomsReadinessStatus { get; set; } = string.Empty;
    public string PayrollSettlementMethod { get; set; } = HrPaymentMethods.PlatformSettlement;
    public IReadOnlyList<OrdererGroupEmploymentRolePolicyDto> EmploymentRolePolicies { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string AdminMemo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class OrdererGroupOperatingEntityPublicDto
{
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string EmploymentEmployerScopeType { get; set; } = HrScopeTypes.OrdererGroup;
    public string EmploymentEmployerScopeId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string LegalEntityName { get; set; } = string.Empty;
    public string MaskedBusinessRegistrationNumber { get; set; } = string.Empty;
    public string BusinessVerificationStatus { get; set; } = string.Empty;
    public bool CanActAsImporterOfRecord { get; set; }
    public bool CanEmployWorkers { get; set; }
    public bool CanIssuePayroll { get; set; }
    public string EmploymentReadinessStatus { get; set; } = string.Empty;
    public string ImportCustomsReadinessStatus { get; set; } = string.Empty;
    public IReadOnlyList<OrdererGroupEmploymentRolePolicyDto> EmploymentRolePolicies { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class OrdererGroupEmploymentRolePolicyDto
{
    public string RoleCode { get; set; } = HrDetailedRoleCodes.OrdererGroupSortingWorker;
    public string RoleName { get; set; } = string.Empty;
    public string ParticipantCategory { get; set; } = HrParticipantCategoryCodes.CommunityPartTimeWorker;
    public string WorkerSourcePreference { get; set; } = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred;
    public bool InternalResidentPreferred { get; set; } = true;
    public bool ExternalWorkerAllowed { get; set; }
    public string ContractType { get; set; } = HrEmploymentContractTypes.PartTime;
    public string WageType { get; set; } = HrWageTypes.Hourly;
    public string PaymentCycle { get; set; } = HrPaymentCycles.Monthly;
    public string WorkDescriptionTemplate { get; set; } = string.Empty;
    public bool RequiresSignedContractBeforeWork { get; set; } = true;
}

public sealed class OrdererGroupOperatingEntityUpsertRequest
{
    public string? EntityId { get; set; }
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string EntityType { get; set; } = OrdererGroupOperatingEntityTypeCode.InformalGroup;
    public string RepresentativeUserId { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string LegalEntityName { get; set; } = string.Empty;
    public string BusinessRegistrationNumber { get; set; } = string.Empty;
    public string BusinessVerificationStatus { get; set; } = OrdererGroupBusinessVerificationStatusCode.Required;
    public bool? CanActAsImporterOfRecord { get; set; }
    public bool? CanEmployWorkers { get; set; }
    public bool? CanIssuePayroll { get; set; }
    public string ImportCustomsReadinessStatus { get; set; } = string.Empty;
    public string PayrollSettlementMethod { get; set; } = HrPaymentMethods.PlatformSettlement;
    public IReadOnlyList<OrdererGroupEmploymentRolePolicyDto> EmploymentRolePolicies { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string AdminMemo { get; set; } = string.Empty;
}
