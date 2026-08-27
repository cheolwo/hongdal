using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation집중판정Codes
    {
        public const string RuleRevision = "focus-timing.r1";
        public const string MeditationRuleRevision = "meditation-progress.r2";
        public const string FocusProfileCatalogRevision =
            "focus-profile-catalog.r1";
        public const long MilliPerPoint = 1_000L;
        public const long PerfectRewardMilli = 250L;
        public const long GoodRewardMilli = 100L;
        public const string FocusTiming = "FocusTiming";
        public const string TriangleOnce = "TriangleOnce";
        public const string Standard = "Standard";
        public const string Assisted = "Assisted";
        public const string NeutralSkip = "NeutralSkip";
        public const string Offered = "Offered";
        public const string AttemptSubmitted = "AttemptSubmitted";
        public const string AttemptEvaluated = "AttemptEvaluated";
        public const string Manifested = "Manifested";
        public const string Voided = "Voided";
        public const string Perfect = "Perfect";
        public const string Good = "Good";
        public const string Miss = "Miss";
        public const string NoInput = "NoInput";
        public const string AssistedNeutral = "AssistedNeutral";
        public const string SubmitFocusTiming = "SubmitFocusTiming";
        public const string SourceCode = "FocusTiming";
        public const string GatheringAndResources = "GatheringAndResources";
        public const string Logging = "logging";
        public const string Applied = "Applied";
        public const string Reused = "Reused";
        public const string NotApplicable = "NotApplicable";
        public const string ProfileApplied = "Applied";
        public const string ProfilePending = "PendingProfile";
        public const string ProfileNpcOnly = "NpcOnly";
        public const string ProfileAutomatic = "Automatic";
        public const string ProfileExcluded = "Excluded";
    }

    public sealed class Simulation집중판정PolicySnapshot
    {
        public string ChallengeKindCode { get; set; }
            = Simulation집중판정Codes.FocusTiming;
        public string CyclePolicyCode { get; set; }
            = Simulation집중판정Codes.TriangleOnce;
        public string AccessibilityModeCode { get; set; }
            = Simulation집중판정Codes.Standard;
        public int ChallengeStartOffsetMillis { get; set; } = 2_000;
        public int DurationMillis { get; set; } = 2_000;
        public int TargetPositionMicro { get; set; } = 500_000;
        public int PerfectDistanceMicro { get; set; } = 60_000;
        public int GoodDistanceMicro { get; set; } = 180_000;
        public string RuleRevision { get; set; }
            = Simulation집중판정Codes.RuleRevision;
    }

    public sealed class Simulation집중ProfileDefinition
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string 적용상태Code { get; set; } = string.Empty;
        public string ChallengeKindCode { get; set; } = string.Empty;
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public string 사유Code { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation집중ProfileCatalogSnapshot
    {
        public string CatalogRevision { get; set; }
            = Simulation집중판정Codes.FocusProfileCatalogRevision;
        public Simulation집중ProfileDefinition[] Profiles { get; set; }
            = Array.Empty<Simulation집중ProfileDefinition>();
    }

    public sealed class Simulation집중판정ChallengeSnapshot
    {
        public string ChallengeStableId { get; set; } = string.Empty;
        public long ChallengeRevision { get; set; }
        public string StateCode { get; set; } = Simulation집중판정Codes.Offered;
        public string PlayerStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string OriginCommandId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public Simulation집중판정PolicySnapshot Policy { get; set; }
            = new Simulation집중판정PolicySnapshot();
        public int? InputOffsetMillis { get; set; }
        public string CandidateResultCode { get; set; } = string.Empty;
        public int CandidatePositionMicro { get; set; }
        public int CandidateDistanceMicro { get; set; }
    }

    public sealed class Simulation집중판정AttemptRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public string ChallengeStableId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public long ExpectedChallengeRevision { get; set; }
        public int InputOffsetMillis { get; set; }
    }

    public sealed class Simulation집중판정ResultSnapshot
    {
        public string ChallengeStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public int PositionMicro { get; set; }
        public int DistanceMicro { get; set; }
        public long 명상경험증가Milli { get; set; }
        public long 회복증가Milli { get; set; }
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public long AppliedWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation명상숙련기여Snapshot
    {
        public string ContributionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string ChallengeStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public long 명상경험증가Milli { get; set; }
        public long AppliedWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation명상분야기여요약Snapshot
    {
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public int ContributionCount { get; set; }
        public long 명상경험Milli { get; set; }
    }

    public sealed class Simulation명상성장적용Snapshot
    {
        public string 상태Code { get; set; }
            = Simulation집중판정Codes.NotApplicable;
        public string 사유Code { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string ContributionStableId { get; set; } = string.Empty;
        public long 명상경험증가Milli { get; set; }
        public long 회복증가Milli { get; set; }
        public long BeforeProfileRevision { get; set; }
        public long AfterProfileRevision { get; set; }
    }

    public sealed class Simulation명상숙련기여Request
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public Simulation행위발현Record 행위기록 { get; set; }
            = new Simulation행위발현Record();
        public Simulation집중판정ResultSnapshot 집중판정결과 { get; set; }
            = new Simulation집중판정ResultSnapshot();
    }
}
