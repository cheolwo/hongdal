using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "FB01 지정 1회 수확의 순수 정책 코드다.",
        Boundary = "권위 명령·실제 수확·운반·저장 완료를 뜻하지 않는다.",
        WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05" })]
    public static class Simulation농사수확위임Codes
    {
        public const string Revision = "farm-harvest-delegation-policy.r1";
        public const string Kilograms = "KGM";
        public const string Safe = "Safe";
        public const string InputInvalid = "FarmHarvestDelegationInputInvalid";
        public const string PreviewRejected = "FarmHarvestDelegationPreviewRejected";
        public const string PreviewInvalid = "FarmHarvestDelegationPreviewInvalid";
        public const string ScopeMismatch = "FarmHarvestDelegationScopeMismatch";
        public const string AuthorityDenied = "FarmHarvestDelegationAuthorityDenied";
        public const string AlreadyExecuted = "FarmHarvestDelegationAlreadyExecuted";
        public const string UnsafeOrUnknown = "FarmHarvestDelegationUnsafeOrUnknown";
        public const string CapacityInvalid = "FarmHarvestDelegationCapacityInvalid";
        public const string UnitMismatch = "FarmHarvestDelegationUnitMismatch";
        public const string CompletionUnitInvalid = "FarmHarvestDelegationCompletionUnitInvalid";
        public const string NoCompleteUnit = "FarmHarvestDelegationNoCompleteUnit";
        public const string ReviewBlockers = "ReviewBlockers";
        public const string AwaitAuthorityCommand = "AwaitAuthorityCommand";
    }

    /// <summary>신뢰된 권위 계층이 구성할 읽기 입력이다. 클라이언트 권한 주장이나 명령이 아니다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "기존 Farm Preview·공간 용량과 사전 승인 범위를 결속한다.",
        Boundary = "권한 확인 결과는 호출자가 제공하며 이 입력만으로 Session 권한을 인증하지 않는다.",
        WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05" })]
    public sealed class Simulation농사수확위임Input
    {
        public SimulationFarmWorkPreviewSnapshot 수확Preview { get; set; } = new SimulationFarmWorkPreviewSnapshot();
        public string 승인ActorStableId { get; set; } = string.Empty;
        public string 승인재배단위StableId { get; set; } = string.Empty;
        public string 승인보관처StableId { get; set; } = string.Empty;
        public string 대상보관처StableId { get; set; } = string.Empty;
        public decimal 승인최대수량Kgm { get; set; }
        public bool 기존위임자격확인 { get; set; }
        public bool 보관처재고사용권한확인 { get; set; }
        public int 이미실행횟수 { get; set; }
        public string 안전상태Code { get; set; } = string.Empty;
        public Simulation공간용량Snapshot 보관용량 { get; set; } = new Simulation공간용량Snapshot();
        public Simulation공간용량Snapshot 점유용량 { get; set; } = new Simulation공간용량Snapshot();
        public Simulation공간용량Snapshot 예약용량 { get; set; } = new Simulation공간용량Snapshot();
        public decimal 운반여유수량 { get; set; }
        public string 운반여유단위Code { get; set; } = string.Empty;
        /// <summary>양수인 명시 입력. 비교 Fixture 값이며 실제 게임 완료단위 승인과 별개다.</summary>
        public decimal 완료단위Kgm { get; set; }
        public string 완료단위기준Revision { get; set; } = string.Empty;
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "수용 가능한 후보와 작물 잔량·차단 사유만 반환한다.",
        Boundary = "후보량은 실제 수확량이나 완료 기록이 아니며 입력 상태를 변경하지 않는다.",
        WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05" })]
    public sealed class Simulation농사수확위임Result
    {
        public string RuleRevision { get; set; } = Simulation농사수확위임Codes.Revision;
        public string 완료단위기준Revision { get; set; } = string.Empty;
        public bool 후보허용 { get; set; }
        public decimal 수용가능후보수량Kgm { get; set; }
        public decimal 작물잔량Kgm { get; set; }
        public string[] 차단사유Codes { get; set; } = Array.Empty<string>();
        public string 다음행동Code { get; set; } = Simulation농사수확위임Codes.ReviewBlockers;
        public bool StateChanged => false;
        public bool SimulationOnly => true;
        public bool IsOperationalState => false;
    }
}
