using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Contracts.Common.Orderer;

public static class 공동구매수요모집Os트리거코드
{
    public const string 수요변경 = "DemandChanged";
    public const string 수요철회 = "DemandWithdrawn";
    public const string 모집마감점검 = "RecruitmentDeadlineReached";
    public const string 수동재조율 = "ManualReconcile";
    public const string 인계승인 = "HandoffApproved";
    public const string 후속원장연결 = "DownstreamLedgerLinked";
}

public static class 공동구매수요모집Os정책코드
{
    public const string 수요집단화묶음 = "DemandClusterBatching";
    public const string 모집마감우선 = "RecruitmentDeadlineEdf";
    public const string 장기모집정체보정 = "DemandRecruitmentAging";
}

public static class 공동구매수요모집Os큐코드
{
    public const string 모집중 = "Recruiting";
    public const string 확정검토 = "ConfirmationReview";
    public const string 모집종료 = "RecruitmentClosed";
    public const string 인계준비 = "HandoffReady";
}

public static class 공동구매수요모집인계상태코드
{
    public const string 미요청 = "NotRequested";
    public const string 승인대기 = "AwaitingApproval";
    public const string 승인후속대기 = "ApprovedAwaitingGroupPurchaseImport";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Contract,
    "공동구매 수요·모집 OS의 큐, 정책, 조율 결과와 사람 승인 인계 계약을 정의합니다.",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "Confirmed는 주문·결제 확정이 아니라 1.5 준비 단계로의 사람 승인 인계입니다.")]
public sealed class 공동구매수요모집Os상태응답
{
    public string 운영체제Id { get; set; } = OperatingSystemIds.GroupPurchaseDemand;
    public string 정책버전 { get; set; } = "1.0";
    public string 자동집단Id { get; set; } = string.Empty;
    public string 집단상태 { get; set; } = 공동구매자동집단상태코드.수요수집중;
    public string 현재큐 { get; set; } = 공동구매수요모집Os큐코드.모집중;
    public string 마지막트리거 { get; set; } = string.Empty;
    public IReadOnlyList<string> 적용정책코드목록 { get; set; } = [];
    public DateTime? 마지막조율시각Utc { get; set; }
    public DateTime? 다음운영점검시각Utc { get; set; }
    public string 인계상태 { get; set; } = 공동구매수요모집인계상태코드.미요청;
    public string 인계요청Id { get; set; } = string.Empty;
    public string 대상운영체제Id { get; set; } = OperatingSystemIds.GroupPurchaseImport;
    public string 대상워크플로우코드 { get; set; } = "GroupPurchaseImport";
    public string 대상원장Id { get; set; } = string.Empty;
    public string 승인자키 { get; set; } = string.Empty;
    public DateTime? 승인시각Utc { get; set; }
    public string 승인사유 { get; set; } = string.Empty;
    public string 실행모드 { get; set; } = "Simulation";
    public bool 시뮬레이션여부 { get; set; } = true;
    public bool 후속워크플로우활성여부 { get; set; }
}

public sealed class 공동구매수요모집Os조율응답
{
    public 공동구매자동집단응답 집단 { get; set; } = new();
    public 공동구매수요모집Os상태응답 운영상태 { get; set; } = new();
    public bool 집단상태변경여부 { get; set; }
    public bool 운영큐변경여부 { get; set; }
}

public sealed class 공동구매수요모집인계승인요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public string 승인사유 { get; set; } = string.Empty;
}

public sealed class 공동구매수요모집인계승인응답
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public bool 이미처리됨 { get; set; }
    public 공동구매자동집단응답 집단 { get; set; } = new();
    public 공동구매수요모집Os상태응답 운영상태 { get; set; } = new();
    public string 안내 { get; set; } = string.Empty;
}

public sealed class 공동구매수요모집마감스캔응답
{
    public DateTime 기준시각Utc { get; set; }
    public int 조회건수 { get; set; }
    public int 조율건수 { get; set; }
    public int 확정검토건수 { get; set; }
    public int 모집종료건수 { get; set; }
    public int 실패건수 { get; set; }
}

public static class 공동구매수요모집Os배치작업코드
{
    public const string 모집마감장기정체점검 = "DemandDeadlineAndAgingReview";
    public const string Kamis일별가격수집 = "KamisDailyPriceCollection";
    public const string Kamis월별가격이력수집 = "KamisMonthlyPriceCollection";
    public const string UsdaNass월별가격수집 = "UsdaMonthlyPriceCollection";
    public const string 공식재료기업근거수집 = "OfficialFoodIngredientCompanyResearch";
}

public static class 공동구매수요모집Os배치실행방식코드
{
    public const string HostedWorker = nameof(HostedWorker);
    public const string Quartz = nameof(Quartz);
}

public static class 공동구매수요모집Os배치상태코드
{
    public const string Os활성 = "ActiveInOS";
    public const string 등록됨Os비활성 = "RegisteredOSInactive";
    public const string 설정비활성 = "DisabledByConfiguration";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Contract,
    "1.0 공동구매 판단을 돕는 모집 점검, 공공가격과 검토용 근거 수집 작업의 OS 등록 상태를 정의합니다.",
    FlowOrder = 12,
    Effects = SsalddelCodeEffect.None,
    Boundary = "설정·스케줄·출처와 실행 경계만 공개하며 API key, 원문 개인정보와 외부 실행 자격은 노출하지 않습니다.")]
public sealed class 공동구매수요모집Os배치작업응답
{
    public string 작업코드 { get; set; } = string.Empty;
    public string 작업명 { get; set; } = string.Empty;
    public string 목적 { get; set; } = string.Empty;
    public string 작업유형 { get; set; } = string.Empty;
    public string 실행방식 { get; set; } = string.Empty;
    public string 스케줄 { get; set; } = string.Empty;
    public string 시간대 { get; set; } = string.Empty;
    public bool 등록여부 { get; set; }
    public bool Os사용활성여부 { get; set; }
    public bool 공유인프라여부 { get; set; }
    public bool 게시글작성여부 { get; set; }
    public string 상태코드 { get; set; } = 공동구매수요모집Os배치상태코드.설정비활성;
    public string 데이터출처 { get; set; } = string.Empty;
    public IReadOnlyList<string> 선행작업코드목록 { get; set; } = [];
    public IReadOnlyList<string> 필요설정목록 { get; set; } = [];
    public string 상태안내 { get; set; } = string.Empty;
    public string 실행경계 { get; set; } = string.Empty;
}

public sealed class 공동구매수요모집Os배치Catalog응답
{
    public string 운영체제Id { get; set; } = OperatingSystemIds.GroupPurchaseDemand;
    public string 정책버전 { get; set; } = "1.0";
    public bool 기능활성여부 { get; set; }
    public bool OsWorker활성여부 { get; set; }
    public string 실행모드 { get; set; } = "Simulation";
    public bool 시뮬레이션여부 { get; set; } = true;
    public IReadOnlyList<공동구매수요모집Os배치작업응답> 작업목록 { get; set; } = [];
}
