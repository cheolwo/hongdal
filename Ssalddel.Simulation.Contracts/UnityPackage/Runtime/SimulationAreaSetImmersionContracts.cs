using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationAreaSetImmersionCodes
    {
        public const string SchemaVersion = "simulation-area-set-immersion-readiness.v1";
        public const string SpatialE5Qualified = "E5Qualified";
        public const string NotInspected = "NotInspected";
        public const string StructureInspected = "StructureInspected";
        public const string ContextEvidenceBound = "ContextEvidenceBound";
        public const string ImmersionQualified = "ImmersionQualified";
        public const string Current = "Current";
        public const string Stale = "Stale";
        public const string NotApplied = "NotApplied";
        public const string RequiredBeforeE7 = "RequiredBeforeE7";
        public const string Open = "Open";
        public const string Closed = "Closed";
    }

    /// <summary>
    /// 한 H3를 장소·행위·상태 변화·현실 문맥 질문으로 정밀 조사한 결과다.
    /// 공공자료는 설명 근거이며 좌표·생산량·Simulation 규칙의 권위가 아니다.
    /// </summary>
    public sealed class SimulationH3ImmersionAuditResponse
    {
        public string H3StableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public int GraphRevision { get; set; }
        public string GraphHashSha256 { get; set; } = string.Empty;
        public string ImmersionMaturityCode { get; set; } = string.Empty;
        public string FreshnessStateCode { get; set; } = string.Empty;
        public string[] H2StableIds { get; set; } = Array.Empty<string>();
        public string[] H1StableIds { get; set; } = Array.Empty<string>();
        public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
        public SimulationImmersionQuestionResultResponse[] Questions { get; set; } =
            Array.Empty<SimulationImmersionQuestionResultResponse>();
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationImmersionQuestionResultResponse
    {
        public string QuestionStableId { get; set; } = string.Empty;
        public string QuestionTypeCode { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public bool RequiredForQualification { get; set; }
        public string QualificationResultCode { get; set; } = string.Empty;
        public string AnswerSummary { get; set; } = string.Empty;
        public string[] EvidenceSnapshotIds { get; set; } = Array.Empty<string>();
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationAreaSetCrossH3ClosureResponse
    {
        public string ClosureStableId { get; set; } = string.Empty;
        public string FromH3StableId { get; set; } = string.Empty;
        public string ToH3StableId { get; set; } = string.Empty;
        public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
        public string InputSemanticCode { get; set; } = string.Empty;
        public string OutputSemanticCode { get; set; } = string.Empty;
        public string QualificationResultCode { get; set; } = string.Empty;
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationImmersionEvidenceSnapshotResponse
    {
        public string EvidenceSnapshotStableId { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string SourceHashSha256 { get; set; } = string.Empty;
        public string LinkStatusCode { get; set; } = string.Empty;
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationAreaSetImmersionReadinessResponse
    {
        public string SchemaVersion { get; set; } = SimulationAreaSetImmersionCodes.SchemaVersion;
        public string AreaSetStableId { get; set; } = string.Empty;
        public int AreaSetRevision { get; set; }
        public string AreaSetHashSha256 { get; set; } = string.Empty;
        public string SpatialMaturityCode { get; set; } = string.Empty;
        public string ImmersionMaturityCode { get; set; } = string.Empty;
        public string FreshnessStateCode { get; set; } = string.Empty;
        public string GroundingStatusCode { get; set; } = string.Empty;
        public string E7GatePolicyCode { get; set; } = string.Empty;
        public string E7GateStateCode { get; set; } = string.Empty;
        public string ImmersionPolicyRevision { get; set; } = string.Empty;
        public string QuestionMatrixRevision { get; set; } = string.Empty;
        public string GeneratorVersion { get; set; } = string.Empty;
        public string InputHashSha256 { get; set; } = string.Empty;
        public string QualificationHashSha256 { get; set; } = string.Empty;
        public SimulationH3ImmersionAuditResponse[] H3Audits { get; set; } =
            Array.Empty<SimulationH3ImmersionAuditResponse>();
        public SimulationAreaSetCrossH3ClosureResponse[] CrossH3Closures { get; set; } =
            Array.Empty<SimulationAreaSetCrossH3ClosureResponse>();
        public SimulationImmersionEvidenceSnapshotResponse[] EvidenceSnapshots { get; set; } =
            Array.Empty<SimulationImmersionEvidenceSnapshotResponse>();
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool PublicDataChangesSimulationRules { get; set; }
        public bool PublicDataMovesSpatialDefinitions { get; set; }
        public bool RuntimeValidated { get; set; }
    }

    /// <summary>
    /// E7 실제 플레이 검증을 시작할 수 있는지 E6 관문을 통과한 뒤 만든 세션이다.
    /// 이 응답 자체가 Play Mode·Game View 또는 E7 완료 증거는 아니다.
    /// </summary>
    public sealed class SimulationE7LaunchResponse
    {
        public string TargetEvidenceStageCode { get; set; } = "E7";
        public bool RuntimeValidationCompleted { get; set; }
        public SimulationAreaSetImmersionReadinessResponse ImmersionReadiness { get; set; } = new();
        public SimulationActualE5SessionCreateResponse SessionCreation { get; set; } = new();
    }
}
