using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Contracts.Common.Orderer;

public static class 주문자집단운영주체유형코드
{
    public const string 비사업자모임 = "InformalGroup";
    public const string 개인사업자 = "IndividualBusiness";
    public const string 법인 = "Corporation";
    public const string 협동조합 = "Cooperative";
    public const string 관리사무소위임 = "ManagementOfficeEntrusted";
    public const string 플랫폼위임 = "PlatformEntrusted";
}

public static class 주문자집단사업자검증상태코드
{
    public const string 불필요 = "NotRequired";
    public const string 필요 = "Required";
    public const string 대기 = "Pending";
    public const string 검증완료 = "Verified";
    public const string 반려 = "Rejected";
}

public static class 주문자집단고용준비상태코드
{
    public const string 미준비 = "NotReady";
    public const string 사업자주체필요 = "NeedsBusinessEntity";
    public const string 계약초안가능 = "ReadyForDraftContract";
    public const string 계약진행중 = "ContractingInProgress";
    public const string 운영가능 = "ReadyToOperate";
}

public static class 주문자집단근로자출처선호코드
{
    public const string 입주민우선 = "InternalResidentPreferred";
    public const string 입주민만 = "InternalResidentOnly";
    public const string 외부허용 = "ExternalAllowed";
}

public sealed class 주문자집단운영주체조회조건
{
    public string? 주문자집단배송권키 { get; set; }
    public string? 운영주체유형 { get; set; }
    public string? 사업자검증상태 { get; set; }
    public bool? 수입자역할가능 { get; set; }
    public bool? 고용가능 { get; set; }
}

public sealed class 주문자집단운영주체Dto
{
    public string 운영주체Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 고용주체범위유형 { get; set; } = HrScopeTypes.OrdererGroup;
    public string 고용주체범위Id { get; set; } = string.Empty;
    public string 운영주체유형 { get; set; } = 주문자집단운영주체유형코드.비사업자모임;
    public string 대표UserId { get; set; } = string.Empty;
    public string 대표자명 { get; set; } = string.Empty;
    public string 법적주체명 { get; set; } = string.Empty;
    public string 사업자등록번호 { get; set; } = string.Empty;
    public string 마스킹사업자등록번호 { get; set; } = string.Empty;
    public string 사업자검증상태 { get; set; } = 주문자집단사업자검증상태코드.필요;
    public bool 수입자역할가능 { get; set; }
    public bool 고용가능 { get; set; }
    public bool 급여지급가능 { get; set; }
    public string 고용준비상태 { get; set; } = 주문자집단고용준비상태코드.미준비;
    public string 수입통관준비상태 { get; set; } = string.Empty;
    public string 급여정산방식 { get; set; } = HrPaymentMethods.PlatformSettlement;
    public IReadOnlyList<주문자집단고용역할정책Dto> 고용역할정책목록 { get; set; } = [];
    public IReadOnlyList<string> 필요조치코드목록 { get; set; } = [];
    public string 관리자메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 주문자집단운영주체공개Dto
{
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 고용주체범위유형 { get; set; } = HrScopeTypes.OrdererGroup;
    public string 고용주체범위Id { get; set; } = string.Empty;
    public string 운영주체유형 { get; set; } = string.Empty;
    public string 법적주체명 { get; set; } = string.Empty;
    public string 마스킹사업자등록번호 { get; set; } = string.Empty;
    public string 사업자검증상태 { get; set; } = string.Empty;
    public bool 수입자역할가능 { get; set; }
    public bool 고용가능 { get; set; }
    public bool 급여지급가능 { get; set; }
    public string 고용준비상태 { get; set; } = string.Empty;
    public string 수입통관준비상태 { get; set; } = string.Empty;
    public IReadOnlyList<주문자집단고용역할정책Dto> 고용역할정책목록 { get; set; } = [];
    public IReadOnlyList<string> 필요조치코드목록 { get; set; } = [];
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 주문자집단고용역할정책Dto
{
    public string 역할코드 { get; set; } = HrDetailedRoleCodes.OrdererGroupSortingWorker;
    public string 역할명 { get; set; } = string.Empty;
    public string 참여자분류 { get; set; } = HrParticipantCategoryCodes.CommunityPartTimeWorker;
    public string 근로자출처선호 { get; set; } = 주문자집단근로자출처선호코드.입주민우선;
    public bool 입주민우선 { get; set; } = true;
    public bool 외부근로자허용 { get; set; }
    public string 계약유형 { get; set; } = HrEmploymentContractTypes.PartTime;
    public string 임금유형 { get; set; } = HrWageTypes.Hourly;
    public string 지급주기 { get; set; } = HrPaymentCycles.Monthly;
    public string 업무설명템플릿 { get; set; } = string.Empty;
    public bool 근로전서명계약필요 { get; set; } = true;
}

public sealed class 주문자집단운영주체저장요청
{
    public string? 운영주체Id { get; set; }
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 운영주체유형 { get; set; } = 주문자집단운영주체유형코드.비사업자모임;
    public string 대표UserId { get; set; } = string.Empty;
    public string 대표자명 { get; set; } = string.Empty;
    public string 법적주체명 { get; set; } = string.Empty;
    public string 사업자등록번호 { get; set; } = string.Empty;
    public string 사업자검증상태 { get; set; } = 주문자집단사업자검증상태코드.필요;
    public bool? 수입자역할가능 { get; set; }
    public bool? 고용가능 { get; set; }
    public bool? 급여지급가능 { get; set; }
    public string 수입통관준비상태 { get; set; } = string.Empty;
    public string 급여정산방식 { get; set; } = HrPaymentMethods.PlatformSettlement;
    public IReadOnlyList<주문자집단고용역할정책Dto> 고용역할정책목록 { get; set; } = [];
    public IReadOnlyList<string> 필요조치코드목록 { get; set; } = [];
    public string 관리자메모 { get; set; } = string.Empty;
}
