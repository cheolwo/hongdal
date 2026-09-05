using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation플레이어이데아맵Codes
    {
        public const string SchemaVersion = "simulation-player-idea-map-projection.v1";
        public const string ProjectionRevision = "player-idea-map.r1";

        public const string RecentExperience = "RecentExperience";
        public const string ReflectionSeed = "ReflectionSeed";
        public const string ObservedUnskilled = "ObservedUnskilled";
        public const string FragmentCandidate = "FragmentCandidate";
        public const string LearningNeed = "LearningNeed";
        public const string VerifiedKnowledgeSkill = "VerifiedKnowledgeSkill";

        public const string ObservedFrom = "ObservedFrom";
        public const string ReflectionOf = "ReflectionOf";
        public const string CauseHypothesis = "CauseHypothesis";
        public const string NeedsCorrection = "NeedsCorrection";
        public const string MentoredBy = "MentoredBy";
        public const string VerifiedBy = "VerifiedBy";
    }

    public sealed class Simulation이데아맵NodeSnapshot
    {
        public string NodeStableId { get; set; } = string.Empty;
        public string NodeKindCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public int 이해도 { get; set; }
        public int 현장숙련도 { get; set; }
        public int 운영숙련도 { get; set; }
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string SourceContributionStableId { get; set; } = string.Empty;
        public string SourceMentorActorStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation이데아맵EdgeSnapshot
    {
        public string EdgeStableId { get; set; } = string.Empty;
        public string EdgeKindCode { get; set; } = string.Empty;
        public string FromNodeStableId { get; set; } = string.Empty;
        public string ToNodeStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string SourceMentorActorStableId { get; set; } = string.Empty;
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "행위 기록·명상·분야 숙련·NPC 학습중점에서 플레이어 이데아 맵을 읽기 전용으로 파생한다.",
        Boundary = "별도 권위 상태를 만들지 않으며 UI·Unity 배치·실제 화면 증거가 아니다.")]
    public sealed class Simulation플레이어이데아맵ProjectionSnapshot
    {
        public string SchemaVersion { get; set; }
            = Simulation플레이어이데아맵Codes.SchemaVersion;
        public string ProjectionRevision { get; set; }
            = Simulation플레이어이데아맵Codes.ProjectionRevision;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public bool BasicViewAvailable { get; set; }
        public int MeditationProficiency { get; set; }
        public string MeditationStageCode { get; set; }
            = Simulation분야단계Codes.미경험;
        public Simulation이데아맵NodeSnapshot[] Nodes { get; set; }
            = Array.Empty<Simulation이데아맵NodeSnapshot>();
        public Simulation이데아맵EdgeSnapshot[] Edges { get; set; }
            = Array.Empty<Simulation이데아맵EdgeSnapshot>();
        public string SourceActionLedgerHashSha256 { get; set; } = string.Empty;
        public string SourceDomainProfileHashSha256 { get; set; } = string.Empty;
        public string SourceLearningFocusHashSha256 { get; set; } = string.Empty;
        public string StateHashSha256 { get; set; } = string.Empty;
        public bool ChangesWorldState { get; set; }
    }
}
