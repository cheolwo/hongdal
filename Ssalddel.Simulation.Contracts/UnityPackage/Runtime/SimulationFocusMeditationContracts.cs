using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation광복기자기회복행위CandidateCodes
    {
        public const string SchemaVersion =
            "gwangbok-self-recovery-action-candidate.v1";
        public const string ProfileRevision =
            "gwangbok-self-recovery-action-profile.r1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Eligible = "Eligible";
        public const string Ineligible = "Ineligible";
        public const string MindfulnessAction = "MindfulnessAction";
        public const string SuccessfulFocus = "SuccessfulFocus";
        public const string CompleteSleepExcluded =
            "CompleteSleepExcluded";
        public const string ProfileMissing = "ProfileMissing";
        public const string ActionRecordRequired = "ActionRecordRequired";
        public const string SuccessfulOutcomeRequired =
            "SuccessfulOutcomeRequired";
        public const string RecoveryChangeRequired =
            "RecoveryChangeRequired";
        public const string FocusSuccessRequired = "FocusSuccessRequired";
        public const string EntryApplicationOwnedByPeriodRule =
            "EntryApplicationOwnedByPeriodRule";
    }

    public sealed class Simulation광복기자기회복행위ProfileDefinition
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ActionKindCode { get; set; } = string.Empty;
        public bool EligibleForEntryTrigger { get; set; }
        public bool RequiresFocusSuccess { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
    }

    public sealed class Simulation광복기자기회복행위CandidateRequest
    {
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public Simulation광복기자기회복행위ProfileDefinition[] Profiles
            { get; set; } = Array.Empty<Simulation광복기자기회복행위ProfileDefinition>();
        public Simulation행위발현Record ActionRecord { get; set; } =
            new Simulation행위발현Record();
        public Simulation집중판정ResultSnapshot? FocusResult { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "명상(정신 차림) 또는 집중 성공으로 실제 개인 회복 기여를 남긴 WI만 광복기 마지막 자기 행위 후보로 전달한다.",
        StepKey = "contract.gwangbok-self-recovery-action-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 23,
        Boundary = "ActionRecord·회복 변화·집중 성공 자격만 정의하며 완전한 수면은 제외하고 실제 기간 전이를 적용하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q015 명상(정신 차림)·집중 성공 WI의 자기 회복 기여와 완전한 수면 제외 Profile 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[]
        {
            "WI-REFLECT-01", "WI-NATURE-06", "WI-NATURE-14",
        },
        Boundary = "기획 후보이며 실제 기간 진입·ActionRecord 생성·Unity 피드백·Runtime 증거가 아니다.")]
    public sealed class Simulation광복기자기회복행위CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation광복기자기회복행위CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation광복기자기회복행위CandidateCodes.PlanningCandidate;
        public string EligibilityCode { get; set; } =
            Simulation광복기자기회복행위CandidateCodes.Ineligible;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ActionRecordStableId { get; set; } = string.Empty;
        public string ActionKindCode { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public bool HasSuccessfulActionRecord { get; set; }
        public bool HasRecoveryContribution { get; set; }
        public bool HasRequiredFocusSuccess { get; set; }
        public bool EligibleForGwangbokEntryTrigger { get; set; }
        public bool AppliesPeriodTransition { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation광복기자기회복행위CandidateCodes
                .EntryApplicationOwnedByPeriodRule,
        };
    }

    public static class Simulation파티근접공명CandidateCodes
    {
        public const string SchemaVersion =
            "party-proximity-resonance-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Eligible = "Eligible";
        public const string Ineligible = "Ineligible";
        public const string ParticipantIdentityInvalid =
            "ParticipantIdentityInvalid";
        public const string SamePlayer = "SamePlayer";
        public const string SamePartyRequired = "SamePartyRequired";
        public const string MeditationPolicyRevisionRequired =
            "MeditationPolicyRevisionRequired";
        public const string ProximityPolicyRevisionRequired =
            "ProximityPolicyRevisionRequired";
        public const string ProviderMeditationEligibilityRequired =
            "ProviderMeditationEligibilityRequired";
        public const string ApprovedProximityRequired =
            "ApprovedProximityRequired";
        public const string EffectOutcomeOwnedByQ011 =
            "EffectOutcomeOwnedByQ011";
    }

    public sealed class Simulation파티근접공명CandidateRequest
    {
        public string ProviderPlayerStableId { get; set; } = string.Empty;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string ProviderPartyStableId { get; set; } = string.Empty;
        public string TargetPartyStableId { get; set; } = string.Empty;
        public bool ProviderEligibleByMeditationPolicy { get; set; }
        public bool IsWithinApprovedProximity { get; set; }
        public string MeditationEligibilityPolicyRevision { get; set; } =
            string.Empty;
        public string ProximityPolicyRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "같은 파티의 승인된 명상 숙련자가 가까이 있을 때 역할 배정 없는 수동 공명 후보를 전달한다.",
        StepKey = "contract.party-proximity-resonance-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 18,
        Boundary = "공명 발생 조건만 정의하며 역할을 제안·수락·배정하거나 회복·위협·행위 능력치를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q010 같은 파티·근접·명상 자격에 따른 수동 공명 후보와 자동 역할 배정 금지 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 온라인 파티 위치 판정·NatureMind Effect·Unity 표현·Runtime 증거가 아니다.")]
    public sealed class Simulation파티근접공명CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation파티근접공명CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation파티근접공명CandidateCodes.PlanningCandidate;
        public string EligibilityCode { get; set; } =
            Simulation파티근접공명CandidateCodes.Ineligible;
        public string ProviderPlayerStableId { get; set; } = string.Empty;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string PartyStableId { get; set; } = string.Empty;
        public string MeditationEligibilityPolicyRevision { get; set; } =
            string.Empty;
        public string ProximityPolicyRevision { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public bool PassiveEffectCandidateCreated { get; set; }
        public bool RequiresRoleProposal { get; set; }
        public bool RequiresRoleAcceptance { get; set; }
        public bool AssignsRole { get; set; }
        public bool ReadsPrivateGrowthProfile { get; set; }
        public bool ChangesNatureMindState { get; set; }
        public bool ChangesWorldState { get; set; }
        public string PendingEffectOutcomeCode { get; set; } =
            Simulation파티근접공명CandidateCodes
                .EffectOutcomeOwnedByQ011;
    }

    public static class Simulation심층관찰CandidateCodes
    {
        public const string SchemaVersion =
            "deep-observation-progression-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string EnvironmentSignals = "EnvironmentSignals";
        public const string CombatIntentAndWeakness =
            "CombatIntentAndWeakness";
        public const string AuthorizedSocialGrowthHint =
            "AuthorizedSocialGrowthHint";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string ObservationThresholdsUnresolved =
            "ObservationThresholdsUnresolved";
        public const string PresentationGrammarUnapproved =
            "PresentationGrammarUnapproved";
        public const string SocialHintResolutionOwnedByQ009 =
            "SocialHintResolutionOwnedByQ009";

        public static string[] OrderedLayerCodes()
            => new[]
            {
                EnvironmentSignals,
                CombatIntentAndWeakness,
                AuthorizedSocialGrowthHint,
            };
    }

    public sealed class Simulation심층관찰LayerRevision
    {
        public string LayerCode { get; set; } = string.Empty;
        public int LayerOrder { get; set; }
        public string ProjectionRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "명상 성장에 따른 환경·전투·사회 성장 낌새의 단계적 관찰 후보를 전달한다.",
        StepKey = "contract.deep-observation-progression-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 16,
        Boundary = "관찰 계층을 정의하지만 권한 없는 원본 행동 기록·인벤토리·정확한 성장 수치를 노출하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q008 환경 징후→전투 의도·약점→허용된 사회 성장 낌새의 관찰 단계 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-01", "WI-NATURE-11" },
        Boundary = "기획 후보이며 실제 정보 Projection·권한 판정·Unity 표현·Runtime 증거가 아니다.")]
    public sealed class Simulation심층관찰CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation심층관찰CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation심층관찰CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation심층관찰CandidateCodes.Gap;
        public Simulation심층관찰LayerRevision[] LayerRevisions
        {
            get;
            set;
        } = Array.Empty<Simulation심층관찰LayerRevision>();
        public string[] MissingLayerCodes { get; set; } =
            Simulation심층관찰CandidateCodes.OrderedLayerCodes();
        public string ExistingNatureObservationWorldInteractionId
        {
            get;
            set;
        } = "WI-NATURE-01";
        public string ExistingCombatObservationCardDefinition
        {
            get;
            set;
        } = SimulationLocalCombatCodes.WeaknessObservationCardDefinition;
        public bool SocialLayerRequiresAuthorization { get; set; } = true;
        public bool ExposesRawActionLog { get; set; }
        public bool ExposesPrivateInventory { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation심층관찰CandidateCodes
                .ObservationThresholdsUnresolved,
            Simulation심층관찰CandidateCodes
                .PresentationGrammarUnapproved,
            Simulation심층관찰CandidateCodes
                .SocialHintResolutionOwnedByQ009,
        };
    }

    public static class Simulation명상전투성장CandidateCodes
    {
        public const string SchemaVersion =
            "meditation-combat-progression-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string CriticalChanceIncrease =
            "CriticalChanceIncrease";
        public const string BasicDamageStabilization =
            "BasicDamageStabilization";
        public const string DeepObservationHandover =
            "DeepObservationHandover";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string NumericCurveUnresolved =
            "NumericCurveUnresolved";
        public const string CombatEffectUnapproved =
            "CombatEffectUnapproved";
        public const string ObservationScopeOwnedByQ008 =
            "ObservationScopeOwnedByQ008";

        public static string[] OrderedStageCodes()
            => new[]
            {
                CriticalChanceIncrease,
                BasicDamageStabilization,
                DeepObservationHandover,
            };
    }

    public sealed class Simulation명상전투성장StageRevision
    {
        public string StageCode { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "명상 숙련의 크리티컬 확률·기본 피해 안정화·심층 관찰 인계 성장 후보를 전달한다.",
        StepKey = "contract.meditation-combat-progression-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 15,
        Boundary = "성장 순서를 정의하지만 승인 전 기본 공격 피해·크리티컬 결과·관찰 정보를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q007 명상 숙련의 전투 보상 성장 순서와 전투 권위 인계 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-11" },
        Boundary = "기획 후보이며 실제 피해 계산·Critical Event·관찰 Projection·Runtime 증거가 아니다.")]
    public sealed class Simulation명상전투성장CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation명상전투성장CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation명상전투성장CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation명상전투성장CandidateCodes.Gap;
        public string MeditationRuleRevision { get; set; } =
            Simulation집중판정Codes.MeditationRuleRevision;
        public string CurrentCombatRuleRevision { get; set; } =
            SimulationLocalCombatCodes.RuleRevision;
        public Simulation명상전투성장StageRevision[] StageRevisions
        {
            get;
            set;
        } = Array.Empty<Simulation명상전투성장StageRevision>();
        public string[] MissingStageCodes { get; set; } =
            Simulation명상전투성장CandidateCodes.OrderedStageCodes();
        public bool MutatesCurrentCombatRule { get; set; }
        public bool GuaranteesCriticalAtAnyStage { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation명상전투성장CandidateCodes.NumericCurveUnresolved,
            Simulation명상전투성장CandidateCodes.CombatEffectUnapproved,
            Simulation명상전투성장CandidateCodes
                .ObservationScopeOwnedByQ008,
        };
    }

    public static class Simulation명상집중접근CandidateCodes
    {
        public const string SchemaVersion =
            "meditation-focus-access-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string EverydayActionAccess = "EverydayActionAccess";
        public const string FocusThresholdCurve = "FocusThresholdCurve";
        public const string BasicAttackEligibility =
            "BasicAttackEligibility";
        public const string CurrentFocusRole = "CurrentFocusRole";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string CriticalOutcomeUnresolved =
            "CriticalOutcomeUnresolved";
        public const string CombatEffectApprovalRequired =
            "CombatEffectApprovalRequired";

        public static string[] RequiredRevisionCodes()
            => new[]
            {
                EverydayActionAccess,
                FocusThresholdCurve,
                BasicAttackEligibility,
                CurrentFocusRole,
            };
    }

    public sealed class Simulation명상집중접근RevisionBinding
    {
        public string ResponsibilityCode { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "명상 숙련에 따른 일상 행동·기본 공격의 집중 접근 확대 후보와 기존 집중 판정 경계를 전달한다.",
        StepKey = "contract.meditation-focus-access-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 14,
        Boundary = "숙련 접근 후보를 정의하지만 기본 공격 피해·크리티컬·순간 집중 역할을 임의로 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q006 장기 명상 숙련의 집중 접근 확대와 순간 집중 역할의 분리 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-11" },
        Boundary = "기획 후보이며 실제 전투 Effect·집중 판정·Runtime 증거가 아니다.")]
    public sealed class Simulation명상집중접근CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation명상집중접근CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation명상집중접근CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation명상집중접근CandidateCodes.Gap;
        public string MeditationRuleRevision { get; set; } =
            Simulation집중판정Codes.MeditationRuleRevision;
        public string FocusProfileCatalogRevision { get; set; } =
            Simulation집중판정Codes.FocusProfileCatalogRevision;
        public Simulation명상집중접근RevisionBinding[] RevisionBindings
        {
            get;
            set;
        } = Array.Empty<Simulation명상집중접근RevisionBinding>();
        public string[] MissingResponsibilityCodes { get; set; } =
            Simulation명상집중접근CandidateCodes.RequiredRevisionCodes();
        public bool ReusesExistingFocusPolicy { get; set; } = true;
        public bool GuaranteesCriticalOutcome { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation명상집중접근CandidateCodes.CriticalOutcomeUnresolved,
            Simulation명상집중접근CandidateCodes
                .CombatEffectApprovalRequired,
        };
    }

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
