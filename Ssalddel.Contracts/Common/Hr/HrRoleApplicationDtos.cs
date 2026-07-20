namespace Ssalddel.Contracts.Common.Hr;

public static class HrRoleApplicationStatusCodes
{
    public const string Submitted = "Submitted";
    public const string Withdrawn = "Withdrawn";

    public static bool IsKnown(string? value)
        => string.Equals(value, Submitted, StringComparison.Ordinal)
           || string.Equals(value, Withdrawn, StringComparison.Ordinal);

    public static string GetDisplayName(string? value)
        => value switch
        {
            Submitted => "검토 대기",
            Withdrawn => "철회됨",
            _ => "알 수 없는 상태"
        };
}

public static class HrRoleApplicationConsent
{
    public const string CurrentVersion = "2026-07-20";

    public static IReadOnlyList<string> NoticeLines { get; } =
    [
        "지원 검토를 위해 계정 식별정보, 선택한 역할, 동의 버전과 제출·철회 시각을 저장합니다.",
        "전화번호, 주소, 계좌, 주민등록번호나 자유 서술형 민감정보는 이 단계에서 수집하지 않습니다.",
        "지원은 자발적 관심 표시이며 역할 배정, 채용, 고용·업무 계약이나 보수를 보장하지 않습니다.",
        "지원은 언제든 철회할 수 있습니다. 철회하면 활성 검토에서 제외되며 최소 처리 이력은 중복 방지와 운영 감사 목적으로 보관될 수 있습니다."
    ];

    public static bool IsValid(HrRoleApplicationSubmitRequest? request)
        => request is not null
           && request.ConfirmedVoluntaryApplication
           && request.ConfirmedNoRoleOrEmploymentGuarantee
           && request.ConfirmedReviewDataUse
           && string.Equals(request.ConsentVersion, CurrentVersion, StringComparison.Ordinal);
}

public sealed record HrRoleApplicationOptionResponse(
    string RoleCode,
    string RoleName,
    string ParticipantCategory,
    string ParticipantCategoryName,
    string ScopeType,
    string ScopeId,
    string Summary);

public static class HrRoleApplicationCatalog
{
    public static IReadOnlyList<HrRoleApplicationOptionResponse> Items { get; } =
    [
        Option(HrDetailedRoleCodes.WarehouseInboundOperator, "창고 입고 지원", "입고 확인과 현장 정리를 돕는 역할에 관심을 표시합니다."),
        Option(HrDetailedRoleCodes.WarehouseInventoryOperator, "창고 재고 지원", "재고 확인과 정리 업무를 돕는 역할에 관심을 표시합니다."),
        Option(HrDetailedRoleCodes.WarehouseDispatchOperator, "창고 출고 지원", "피킹·포장·출고 준비를 돕는 역할에 관심을 표시합니다."),
        Option(HrDetailedRoleCodes.ShippingAgencyOperator, "배송대행 운영 지원", "배송대행 운영과 정보 정리를 돕는 역할에 관심을 표시합니다."),
        Option(HrDetailedRoleCodes.OrdererGroupSortingWorker, "공동주문 분류 지원", "공동주문 물품 분류를 돕는 역할에 관심을 표시합니다.", HrParticipantCategoryCodes.CommunityPartTimeWorker),
        Option(HrDetailedRoleCodes.OrdererGroupDistributionWorker, "공동주문 배부 지원", "공동주문 물품 배부를 돕는 역할에 관심을 표시합니다.", HrParticipantCategoryCodes.CommunityPartTimeWorker),
        Option(HrDetailedRoleCodes.OrdererGroupParcelAggregationWorker, "택배 취합 지원", "공동 택배 취합과 정리를 돕는 역할에 관심을 표시합니다.", HrParticipantCategoryCodes.CommunityPartTimeWorker),
        Option(HrDetailedRoleCodes.OrdererGroupCommunityFacilityWorker, "공동시설 운영 지원", "공동시설의 일상 운영을 돕는 역할에 관심을 표시합니다.", HrParticipantCategoryCodes.CommunityPartTimeWorker)
    ];

    public static HrRoleApplicationOptionResponse? Find(string? roleCode)
        => Items.FirstOrDefault(item => string.Equals(item.RoleCode, roleCode?.Trim(), StringComparison.Ordinal));

    private static HrRoleApplicationOptionResponse Option(
        string roleCode,
        string roleName,
        string summary,
        string participantCategory = HrParticipantCategoryCodes.InternalProjectOperator)
        => new(
            roleCode,
            roleName,
            participantCategory,
            HrParticipantCategoryCodes.GetDisplayName(participantCategory),
            HrScopeTypes.Platform,
            HrScopeIds.Global,
            summary);
}

public sealed class HrRoleApplicationSubmitRequest
{
    public Guid SubmissionRequestId { get; set; }

    public string RoleCode { get; set; } = string.Empty;

    public bool ConfirmedVoluntaryApplication { get; set; }

    public bool ConfirmedNoRoleOrEmploymentGuarantee { get; set; }

    public bool ConfirmedReviewDataUse { get; set; }

    public string ConsentVersion { get; set; } = string.Empty;
}

public sealed class HrRoleApplicationResponse
{
    public Guid ApplicationId { get; set; }

    public string RoleCode { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string ParticipantCategory { get; set; } = string.Empty;

    public string ParticipantCategoryName { get; set; } = string.Empty;

    public string ScopeType { get; set; } = string.Empty;

    public string ScopeId { get; set; } = string.Empty;

    public string StatusCode { get; set; } = HrRoleApplicationStatusCodes.Submitted;

    public string StatusName { get; set; } = HrRoleApplicationStatusCodes.GetDisplayName(HrRoleApplicationStatusCodes.Submitted);

    public string ConsentVersion { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public DateTime? WithdrawnAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public bool CanWithdraw { get; set; }
}

public sealed class HrRoleApplicationPageResponse
{
    public IReadOnlyList<HrRoleApplicationOptionResponse> Options { get; set; } = [];

    public IReadOnlyList<HrRoleApplicationResponse> Applications { get; set; } = [];
}
