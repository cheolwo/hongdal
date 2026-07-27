using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Contracts.Common.Orderer;

public static class 같이수입준비Os상태코드
{
    public const string 자료수집중 = "EvidenceCollecting";
    public const string 근거재확인필요 = "EvidenceRefreshRequired";
    public const string 전문검토인계준비 = "ReadyForQualifiedReview";
    public const string 전문검토진행중 = "QualifiedReviewInProgress";
    public const string 다음단계인계후보 = "ReadyForNextStageHandoff";
}

public static class 같이수입준비Os작업코드
{
    public const string 전체준비점검 = "AllReadinessChecks";
    public const string 공유공공데이터점검 = "SharedPublicDataAvailability";
    public const string 재료묶음운송검토 = "MaterialBundleAndTransportReview";
    public const string 공급자근거점검 = "SupplierEvidenceFreshness";
    public const string 견적원가점검 = "QuoteAndLandedCostFreshness";
    public const string 품목규제점검 = "ClassificationAndComplianceReview";
    public const string 책임초안점검 = "ResponsibilityDraftReview";
    public const string 전문검토인계 = "QualifiedReviewHandoff";

    public static IReadOnlySet<string> 수동실행지원목록 { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        전체준비점검,
        공유공공데이터점검,
        재료묶음운송검토,
        공급자근거점검,
        견적원가점검,
        품목규제점검,
        책임초안점검
    };
}

public static class 같이수입준비Os작업상태코드
{
    public const string 대기 = "Pending";
    public const string 준비 = "Ready";
    public const string 차단 = "Blocked";
    public const string 사람검토대기 = "AwaitingHumanReview";
    public const string 진행중 = "InProgress";
    public const string 완료 = "Completed";
    public const string 설정비활성 = "DisabledByConfiguration";
    public const string 실패 = "Failed";
}

public static class 같이수입준비Os트리거코드
{
    public const string 원장조회 = "LedgerRead";
    public const string 수동점검 = "ManualReview";
    public const string 수동재시도 = "ManualRetry";
    public const string 정기점검 = "ScheduledReview";
    public const string 전문검토인계 = "QualifiedReviewHandoff";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Contract,
    "기존 같이 수입 원장의 1.5 준비 블록을 읽어 내부 점검, 공유 공공데이터 배치, 포워더 인계·회신과 사람 전문검토 handoff를 조율합니다.",
    FlowOrder = 14,
    Effects = SsalddelCodeEffect.None,
    Boundary = "OS는 수요 집계·정보 최소화·동의·사람 인계와 회신 기록만 조율하며 업체 자동 선정, 외부 자동 전송, 계약, 결제, 신고, 운송·창고 실행 권한을 만들지 않습니다.")]
public sealed class 같이수입준비Os상태응답
{
    public string 운영체제Id { get; set; } = OperatingSystemIds.GroupPurchaseImport;
    public string 정책버전 { get; set; } = "1.5";
    public string 자동집단Id { get; set; } = string.Empty;
    public string 원장Id { get; set; } = string.Empty;
    public long 원장Revision { get; set; }
    public bool 기능활성여부 { get; set; }
    public bool OsWorker활성여부 { get; set; }
    public string 실행모드 { get; set; } = "Simulation";
    public bool 시뮬레이션여부 { get; set; } = true;
    public string 상태코드 { get; set; } = 같이수입준비Os상태코드.자료수집중;
    public string 마지막트리거코드 { get; set; } = 같이수입준비Os트리거코드.원장조회;
    public string 마지막조율자표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 마지막조율시각Utc { get; set; }
    public DateTimeOffset 다음점검시각Utc { get; set; }
    public bool 포워더인계준비가능 { get; set; }
    public bool 포워더인계기록완료 { get; set; }
    public bool 포워더회신기록완료 { get; set; }
    public bool 전문검토인계가능 { get; set; }
    public bool 전문검토완료여부 { get; set; }
    public bool 다음단계인계후보여부 { get; set; }
    public bool 이미처리됨 { get; set; }
    public 같이수입준비Os전문검토인계기록? 전문검토인계기록 { get; set; }
    public IReadOnlyList<같이수입준비Os작업응답> 작업목록 { get; set; } = [];
    public IReadOnlyList<공동구매수요모집Os배치작업응답> 공유배치목록 { get; set; } = [];
    public IReadOnlyList<string> 차단사유목록 { get; set; } = [];
    public IReadOnlyList<string> 경고목록 { get; set; } = [];
    public bool 계약서명가능 { get; set; }
    public bool 결제가능 { get; set; }
    public bool 신고실행가능 { get; set; }
    public bool 운송지시가능 { get; set; }
    public bool 포워더자동선정가능 { get; set; }
    public bool 외부자동전송가능 { get; set; }
}

public sealed class 같이수입준비Os작업응답
{
    public string 작업코드 { get; set; } = string.Empty;
    public string 작업명 { get; set; } = string.Empty;
    public string 작업유형 { get; set; } = string.Empty;
    public string 상태코드 { get; set; } = 같이수입준비Os작업상태코드.대기;
    public string 상태안내 { get; set; } = string.Empty;
    public string 데이터출처 { get; set; } = string.Empty;
    public string 실행방식 { get; set; } = "LedgerInspection";
    public string 스케줄 { get; set; } = string.Empty;
    public bool 차단작업여부 { get; set; } = true;
    public bool 수동실행가능여부 { get; set; } = true;
    public bool 재시도가능여부 { get; set; }
    public int 시도횟수 { get; set; }
    public DateTimeOffset? 마지막실행시각Utc { get; set; }
    public string 마지막오류 { get; set; } = string.Empty;
    public IReadOnlyList<string> 선행작업코드목록 { get; set; } = [];
    public IReadOnlyList<string> 차단사유목록 { get; set; } = [];
    public IReadOnlyList<string> 경고목록 { get; set; } = [];
    public string 실행경계 { get; set; } = string.Empty;
}

public sealed class 같이수입준비Os작업실행요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public long? 기대Revision { get; set; }
    public string 작업코드 { get; set; } = 같이수입준비Os작업코드.전체준비점검;
    public bool 재시도여부 { get; set; }
}

public sealed class 같이수입준비Os전문검토인계요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public long? 기대Revision { get; set; }
    public string 검토수신자표시명 { get; set; } = string.Empty;
    public string 검토범위 { get; set; } = string.Empty;
    public string 인계메모 { get; set; } = string.Empty;
}

public sealed class 같이수입준비Os전문검토인계기록
{
    public string 검토수신자표시명 { get; set; } = string.Empty;
    public string 검토범위 { get; set; } = string.Empty;
    public string 인계메모 { get; set; } = string.Empty;
    public string 인계자UserId { get; set; } = string.Empty;
    public string 인계자표시명 { get; set; } = string.Empty;
    public DateTimeOffset 인계시각Utc { get; set; }
}

public sealed class 같이수입준비Os정기점검응답
{
    public DateTimeOffset 기준시각Utc { get; set; }
    public int 조회건수 { get; set; }
    public int 조율건수 { get; set; }
    public int 건너뜀건수 { get; set; }
    public int 실패건수 { get; set; }
}
