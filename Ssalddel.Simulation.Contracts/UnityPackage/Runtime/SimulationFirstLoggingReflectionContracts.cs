using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation첫벌목성찰Codes
    {
        public const string SchemaVersion = "first-logging-reflection-seed.v1";
        public const string RuleRevision = "first-logging-reflection-seed.r1";
        public const string PlanningRevision = "dual-possession-alchemist-succession.r63:first-logging-reflection-p0";
        public const string WorldInteractionId = "WI-NATURE-06";
        public const string HarvestCompleted = "HarvestCompleted";
        public const string HansHouseSafeRest = "safe-rest:hans-house:first-night";

        public const string NotReady = "NotReady";
        public const string Ready = "Ready";
        public const string Interrupted = "Interrupted";
        public const string Completed = "Completed";

        public const string Observation = "Observation";
        public const string Cause = "Cause";
        public const string Improvement = "Improvement";

        public const string ActionHistoryIncomplete = "ActionHistoryIncomplete";
        public const string FirstPlayerLoggingRequired = "FirstPlayerLoggingRequired";
        public const string HansHouseSafeRestRequired = "HansHouseSafeRestRequired";
        public const string SafeRestMustFollowLogging = "SafeRestMustFollowLogging";
        public const string ApprovedFocusEvidenceRequired =
            "ApprovedFocusEvidenceRequired";
        public const string RewardReady = "RewardReady";
        public const string RewardAlreadyApplied = "RewardAlreadyApplied";

        public static string[] OrderedFragmentCodes()
            => new[] { Observation, Cause, Improvement };
    }

    public sealed class Simulation안전휴식근거Snapshot
    {
        public string EvidenceStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string PlaceStableId { get; set; } = string.Empty;
        public bool SafeRestConfirmed { get; set; }
        public long AppliedWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation첫벌목성찰ProgressSnapshot
    {
        public string SeedStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string[] ConnectedFragmentCodes { get; set; } = Array.Empty<string>();
        public string StateCode { get; set; } = Simulation첫벌목성찰Codes.Ready;
    }

    public sealed class Simulation첫벌목성찰SeedRequest
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public bool ActionHistoryComplete { get; set; }
        public Simulation행위발현Record[] ActionRecords { get; set; }
            = Array.Empty<Simulation행위발현Record>();
        public Simulation안전휴식근거Snapshot? SafeRestEvidence { get; set; }
        public Simulation첫벌목성찰ProgressSnapshot? PreviousProgress { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "실제 첫 벌목 행위 기록과 한스 집 안전 휴식 근거에서 관찰·원인·개선 성찰 씨앗을 준비한다.",
        StepKey = "contract.first-logging-reflection-seed",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 24,
        Boundary = "ActionRecord와 안전 휴식 근거를 읽을 뿐 벌목·휴식·보상·편린을 새로 만들지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "한스 집 첫 벌목 명상의 성찰 씨앗·중단·재개·한 번 보상 준비 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-06" },
        Boundary = "Logic 준비 계약이며 한스 집 실제 휴식 연결·Unity 화면·이데아 편린 획득 증거가 아니다.")]
    public sealed class Simulation첫벌목성찰SeedSnapshot
    {
        public string SchemaVersion { get; set; }
            = Simulation첫벌목성찰Codes.SchemaVersion;
        public string RuleRevision { get; set; }
            = Simulation첫벌목성찰Codes.RuleRevision;
        public string PlanningRevision { get; set; }
            = Simulation첫벌목성찰Codes.PlanningRevision;
        public string StatusCode { get; set; } = Simulation첫벌목성찰Codes.NotReady;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SeedStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string SourceActionRecordHashSha256 { get; set; } = string.Empty;
        public long SourceAfterWorldRevision { get; set; }
        public string SafeRestEvidenceStableId { get; set; } = string.Empty;
        public string[] OrderedFragmentCodes { get; set; }
            = Simulation첫벌목성찰Codes.OrderedFragmentCodes();
        public string[] FragmentContentCodes { get; set; } = new[]
        {
            "LoggingAxeAngleAndGrainObserved",
            "LoggingForceTransferCause",
            "LoggingRecoveryMotionImprovement",
        };
        public Simulation첫벌목성찰ProgressSnapshot Progress { get; set; }
            = new Simulation첫벌목성찰ProgressSnapshot();
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public bool CreatesActionRecord { get; set; }
        public bool AppliesReward { get; set; }
        public bool CreatesIdeaFragment { get; set; }
        public bool ChangesWorldState { get; set; }
    }

    public sealed class Simulation첫벌목성찰RewardPreparationRequest
    {
        public Simulation첫벌목성찰SeedSnapshot Seed { get; set; }
            = new Simulation첫벌목성찰SeedSnapshot();
        public string[] ConnectedFragmentCodes { get; set; } = Array.Empty<string>();
        public Simulation행위발현Record SourceActionRecord { get; set; }
            = new Simulation행위발현Record();
        public Simulation집중판정ResultSnapshot? ApprovedFocusResult { get; set; }
        public Simulation플레이어분야ProfileSnapshot PlayerDomainProfile { get; set; }
            = new Simulation플레이어분야ProfileSnapshot();
    }

    public sealed class Simulation첫벌목성찰RewardPreparationSnapshot
    {
        public string SeedStableId { get; set; } = string.Empty;
        public string StateCode { get; set; }
            = Simulation첫벌목성찰Codes.Completed;
        public string RewardStatusCode { get; set; }
            = Simulation첫벌목성찰Codes.ApprovedFocusEvidenceRequired;
        public string ExistingContributionStableId { get; set; } = string.Empty;
        public Simulation명상숙련기여Request? MeditationProgressionRequest
            { get; set; }
        public bool CreatesNewRewardAmount { get; set; }
        public bool AppliesRewardDirectly { get; set; }
        public bool ChangesWorldState { get; set; }
    }
}
