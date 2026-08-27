using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation거점성찰SchemaCodes
    {
        public const string YouTube학습원문관측 = "youtube-learning-source-observation.v1";
        public const string 학습해석후보 = "learning-interpretation-candidate.v1";
        public const string 승인학습자료Publication = "approved-learning-material.v1";
        public const string 기존학습카드Publication = "hongik-unity-learning-card-publication.v1";
        public const string 파생원장 = "simulation-approved-learning-ledger.v1";
        public const string 거점성찰상태 = "simulation-base-reflection.v1";
    }

    public static class Simulation학습자료상태Codes
    {
        public const string Candidate = "Candidate";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Unavailable = "Unavailable";
    }

    public static class Simulation학습분류Codes
    {
        public const string 상황인식 = "SituationAwareness";
        public const string 통합실천 = "IntegratedPractice";
    }

    public static class Simulation거점성찰선택Codes
    {
        public const string 그냥휴식 = "RestOnly";
        public const string 오늘행동성찰 = "ReflectOnToday";
        public const string 원문열기 = "OpenOptionalSource";
    }

    public static class Simulation내면능력치Codes
    {
        public const string 알아차림 = "Awareness";
        public const string 결의 = "Resolve";
    }

    public static class Simulation내면효과Codes
    {
        public const string 초심 = "BeginnerMind";
        public const string 통합진전 = "IntegratedProgress";
    }

    public static class Simulation거점성찰결과Codes
    {
        public const string 휴식함 = "Rested";
        public const string 원문확인가능 = "OptionalSourceAvailable";
        public const string 다음활동적용대기 = "InnerLearningPending";
        public const string 내면학습적용 = "InnerLearningApplied";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Contract,
        "YouTube 원문 관측에서 사람 승인 학습자료까지의 3계층 계약을 정의한다.",
        StepKey = "contract.base-reflection-learning-material",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "시청 시간·재생 상태·API key·원문 전체는 Simulation 보상 계약에 포함하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "WI-REFLECT-01의 승인 학습자료와 거점 성찰 상태 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-REFLECT-01" },
        WorkOrderIds = new[] { "E9-WO-NATURE-BASE-REFLECTION" },
        Boundary = "계약은 자료 승인, 실행 결과 또는 E7 플레이 증거를 대신하지 않는다.")]
    public sealed class SimulationYouTube학습원문관측Snapshot
    {
        public string SchemaCode { get; set; }
            = Simulation거점성찰SchemaCodes.YouTube학습원문관측;
        public string 관측StableId { get; set; } = string.Empty;
        public string VideoStableId { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string 제목 { get; set; } = string.Empty;
        public string 채널명 { get; set; } = string.Empty;
        public DateTimeOffset 조회시각 { get; set; }
        public DateTimeOffset? 게시시각 { get; set; }
        public string 원문MetadataHashSha256 { get; set; } = string.Empty;
        public string 수집AdapterCode { get; set; } = string.Empty;
        public string 이용한계 { get; set; } = string.Empty;
        public Simulation학습근거구간Snapshot[] 근거구간들 { get; set; }
            = Array.Empty<Simulation학습근거구간Snapshot>();
    }

    public sealed class Simulation학습근거구간Snapshot
    {
        public int 시작Millisecond { get; set; }
        public int 종료Millisecond { get; set; }
        public string 근거요약 { get; set; } = string.Empty;
        public string 구간HashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation학습해석후보Snapshot
    {
        public string SchemaCode { get; set; }
            = Simulation거점성찰SchemaCodes.학습해석후보;
        public string 후보StableId { get; set; } = string.Empty;
        public string 원문관측StableId { get; set; } = string.Empty;
        public string 원문관측HashSha256 { get; set; } = string.Empty;
        public string 분류Code { get; set; } = string.Empty;
        public string 요약 { get; set; } = string.Empty;
        public string[] 성찰질문들 { get; set; } = Array.Empty<string>();
        public string 제안내면능력치Code { get; set; } = string.Empty;
        public string 제안내면효과Code { get; set; } = string.Empty;
        public string 해석RuleRevision { get; set; } = string.Empty;
        public string InputHashSha256 { get; set; } = string.Empty;
        public string 상태Code { get; set; } = Simulation학습자료상태Codes.Candidate;
    }

    public sealed class Simulation승인학습자료Publication
    {
        public string SchemaCode { get; set; }
            = Simulation거점성찰SchemaCodes.승인학습자료Publication;
        public string PublicationStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string 제목 { get; set; } = string.Empty;
        public string 분류Code { get; set; } = string.Empty;
        public string 요약 { get; set; } = string.Empty;
        public string[] 성찰질문들 { get; set; } = Array.Empty<string>();
        public string 내면능력치Code { get; set; } = string.Empty;
        public int 능력치증가량 { get; set; } = 1;
        public string 내면효과Code { get; set; } = string.Empty;
        public string 원문관측StableId { get; set; } = string.Empty;
        public string 원문관측HashSha256 { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string 승인자StableId { get; set; } = string.Empty;
        public DateTimeOffset 승인시각 { get; set; }
        public string 상태Code { get; set; } = Simulation학습자료상태Codes.Approved;
        public string InputHashSha256 { get; set; } = string.Empty;
        public string PublicationHashSha256 { get; set; } = string.Empty;
        public string 이용한계 { get; set; } = string.Empty;
    }

    public sealed class Simulation승인학습자료동기화Bundle
    {
        public string SchemaCode { get; set; }
            = Simulation거점성찰SchemaCodes.파생원장;
        public string LedgerRevision { get; set; } = string.Empty;
        public DateTimeOffset 수집시각 { get; set; }
        public string InputHashSha256 { get; set; } = string.Empty;
        public Simulation승인학습자료Publication[] Publications { get; set; }
            = Array.Empty<Simulation승인학습자료Publication>();
    }

    public sealed class Simulation거점성찰InitialStateRequest
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public int 시작일차 { get; set; } = 1;
        public Simulation내면상태Snapshot 내면상태 { get; set; }
            = new Simulation내면상태Snapshot();
        public Simulation승인학습자료동기화Bundle 승인자료묶음 { get; set; }
            = new Simulation승인학습자료동기화Bundle();
    }

    public sealed class Simulation내면상태Snapshot
    {
        public int 알아차림 { get; set; }
        public int 결의 { get; set; }
        public string[] 획득내면효과Codes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation거점성찰PreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public int 일차 { get; set; }
        public string 선택StableId { get; set; } = string.Empty;
        public string PublicationStableId { get; set; } = string.Empty;
        public string PublicationRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation거점성찰Preview
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public int 일차 { get; set; }
        public string 선택StableId { get; set; } = string.Empty;
        public string PublicationStableId { get; set; } = string.Empty;
        public string PublicationRevision { get; set; } = string.Empty;
        public string PublicationHashSha256 { get; set; } = string.Empty;
        public bool 보상적용가능 { get; set; }
        public string 내면능력치Code { get; set; } = string.Empty;
        public int 능력치증가량 { get; set; }
        public string 내면효과Code { get; set; } = string.Empty;
        public string 결과Code { get; set; } = string.Empty;
        public string[] 설명Codes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation거점성찰ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation거점성찰Preview Preview { get; set; }
            = new Simulation거점성찰Preview();
    }

    public sealed class Simulation거점성찰GrantSnapshot
    {
        public string GrantStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public int 선택일차 { get; set; }
        public string PublicationStableId { get; set; } = string.Empty;
        public string PublicationRevision { get; set; } = string.Empty;
        public string PublicationHashSha256 { get; set; } = string.Empty;
        public string 내면능력치Code { get; set; } = string.Empty;
        public int 능력치증가량 { get; set; }
        public string 내면효과Code { get; set; } = string.Empty;
        public string 상태Code { get; set; } = string.Empty;
    }

    public sealed class Simulation거점성찰StateSnapshot
    {
        public string SchemaCode { get; set; }
            = Simulation거점성찰SchemaCodes.거점성찰상태;
        public string PlayerStableId { get; set; } = string.Empty;
        public int 현재일차 { get; set; } = 1;
        public long Revision { get; set; }
        public string FrozenLedgerRevision { get; set; } = string.Empty;
        public string FrozenLedgerInputHashSha256 { get; set; } = string.Empty;
        public Simulation승인학습자료Publication[] FrozenPublications { get; set; }
            = Array.Empty<Simulation승인학습자료Publication>();
        public Simulation내면상태Snapshot 내면상태 { get; set; }
            = new Simulation내면상태Snapshot();
        public int[] 성찰완료일차들 { get; set; } = Array.Empty<int>();
        public Simulation거점성찰GrantSnapshot[] Grants { get; set; }
            = Array.Empty<Simulation거점성찰GrantSnapshot>();
        public string[] 적용CommandIds { get; set; } = Array.Empty<string>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }
}
