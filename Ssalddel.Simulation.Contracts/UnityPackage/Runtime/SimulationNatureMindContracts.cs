using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation암흑기정신차림EffectStrengthCandidateCodes
    {
        public const string SchemaVersion =
            "dark-age-mindfulness-effect-strength-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string CurrentRecoveryShareAccessSource =
            "CurrentRecoveryShareAccessSource";
        public const string LongTermMeditationProficiencyStrengthSource =
            "LongTermMeditationProficiencyStrengthSource";
        public const string AllowedEffectScopeCandidateRequired =
            "AllowedEffectScopeCandidateRequired";
        public const string PositiveCurrentRecoveryShareRequired =
            "PositiveCurrentRecoveryShareRequired";
        public const string LongTermMeditationProficiencyRequired =
            "LongTermMeditationProficiencyRequired";
        public const string StrengthProfileRevisionRequired =
            "StrengthProfileRevisionRequired";
        public const string StrengthCurveUnresolved =
            "StrengthCurveUnresolved";
        public const string DurationAndCostUnresolved =
            "DurationAndCostUnresolved";
    }

    public sealed class Simulation암흑기정신차림EffectStrengthCandidateRequest
    {
        public Simulation암흑기정신차림EffectScopeCandidateSnapshot
            EffectScopeCandidate { get; set; } =
                new Simulation암흑기정신차림EffectScopeCandidateSnapshot();
        public decimal CurrentRecoveryShare { get; set; }
        public decimal LongTermMeditationProficiency { get; set; }
        public string StrengthProfileRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "암흑기 정신 차림 효과의 접근은 현재 회복 비중으로, 접근 후 강도는 장기 명상 숙련도로 판정하도록 입력 책임을 분리한다.",
        StepKey = "contract.dark-age-mindfulness-effect-strength-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "접근·강도 입력 소유자만 정의하며 정확한 숙련 곡선·효과량·지속·비용이나 실제 Effect를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q022 암흑기 개인 정신 차림 효과의 현재 회복 기반 접근과 장기 명상 숙련도 기반 강도를 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 강도 계산·Effect 적용·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation암흑기정신차림EffectStrengthCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation암흑기정신차림EffectStrengthCandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation암흑기정신차림EffectStrengthCandidateCodes
                .PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation암흑기정신차림EffectStrengthCandidateCodes.Gap;
        public string PlayerStableId { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public string AccessSourceCode { get; set; } =
            Simulation암흑기정신차림EffectStrengthCandidateCodes
                .CurrentRecoveryShareAccessSource;
        public string StrengthSourceCode { get; set; } =
            Simulation암흑기정신차림EffectStrengthCandidateCodes
                .LongTermMeditationProficiencyStrengthSource;
        public string StrengthProfileRevision { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool AccessAvailableFromCurrentRecovery { get; set; }
        public bool StrengthCandidateFromLongTermProficiency { get; set; }
        public bool UsesCurrentRecoveryShareForStrength { get; set; }
        public bool AppliesEffectStrength { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation암흑기정신차림EffectStrengthCandidateCodes
                .StrengthCurveUnresolved,
            Simulation암흑기정신차림EffectStrengthCandidateCodes
                .DurationAndCostUnresolved,
        };
    }

    public static class Simulation암흑기정신차림EffectScopeCandidateCodes
    {
        public const string SchemaVersion =
            "dark-age-mindfulness-effect-scope-candidate.v1";
        public const string ProfileRevision =
            "dark-age-mindfulness-effect-scope.r1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Allowed = "Allowed";
        public const string Denied = "Denied";
        public const string PersonalCombatFocus = "PersonalCombatFocus";
        public const string PersonalDeepObservation =
            "PersonalDeepObservation";
        public const string PersonalPrecisionCrafting =
            "PersonalPrecisionCrafting";
        public const string RegionalRestoration = "RegionalRestoration";
        public const string SpatialExpansion = "SpatialExpansion";
        public const string CommunityProduction = "CommunityProduction";
        public const string PersonalMindfulnessConsumer =
            "PersonalMindfulnessConsumer";
        public const string WorldOrCommunityConsumer =
            "WorldOrCommunityConsumer";
        public const string DarkAgeAccessCandidateRequired =
            "DarkAgeAccessCandidateRequired";
        public const string ProfileRevisionRequired =
            "ProfileRevisionRequired";
        public const string EffectCodeRequired = "EffectCodeRequired";
        public const string EffectProfileMissing = "EffectProfileMissing";
        public const string PersonalEffectStrengthOwnedByQ022 =
            "PersonalEffectStrengthOwnedByQ022";
    }

    public sealed class Simulation암흑기정신차림EffectScopeProfileDefinition
    {
        public string EffectCode { get; set; } = string.Empty;
        public string ConsumerScopeCode { get; set; } = string.Empty;
        public bool AllowedInDarkAge { get; set; }
    }

    public sealed class Simulation암흑기정신차림EffectScopeCandidateRequest
    {
        public Simulation암흑기정신차림접근CandidateSnapshot AccessCandidate
            { get; set; } = new Simulation암흑기정신차림접근CandidateSnapshot();
        public string EffectCode { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public Simulation암흑기정신차림EffectScopeProfileDefinition[] Profiles
            { get; set; } =
                Array.Empty<Simulation암흑기정신차림EffectScopeProfileDefinition>();
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "암흑기 안의 제한 접근을 개인 전투 집중·심층 관찰·정밀 제작 효과로 한정하고 세계·공동체 효과를 차단하는 Profile을 전달한다.",
        StepKey = "contract.dark-age-mindfulness-effect-scope-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 29,
        Boundary = "효과 범주와 소비자 허용 여부만 정의하며 강도·지속·실제 전투·관찰·제작 수치를 적용하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q021 암흑기 제한 접근을 개인 정신 차림 효과에만 허용하고 지역·공간·공동체 효과를 차단한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 Effect 적용·분야별 수치·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation암흑기정신차림EffectScopeCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation암흑기정신차림EffectScopeCandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation암흑기정신차림EffectScopeCandidateCodes
                .PlanningCandidate;
        public string AccessDecisionCode { get; set; } =
            Simulation암흑기정신차림EffectScopeCandidateCodes.Denied;
        public string PlayerStableId { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public string ConsumerScopeCode { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public bool PersonalMindfulnessEffect { get; set; }
        public bool WorldOrCommunityEffect { get; set; }
        public bool AppliesEffect { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation암흑기정신차림EffectScopeCandidateCodes
                .PersonalEffectStrengthOwnedByQ022,
        };
    }

    public static class Simulation암흑기정신차림접근CandidateCodes
    {
        public const string SchemaVersion =
            "dark-age-mindfulness-access-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string DarkAgeRemainsDominant =
            "DarkAgeRemainsDominant";
        public const string LimitedGwangbokEffectAccessCandidate =
            "LimitedGwangbokEffectAccessCandidate";
        public const string RecoveryThreatOffsetCandidateRequired =
            "RecoveryThreatOffsetCandidateRequired";
        public const string DarkAgePeriodRequired = "DarkAgePeriodRequired";
        public const string PositiveRecoveryShareRequired =
            "PositiveRecoveryShareRequired";
        public const string ThreatDominanceRequired =
            "ThreatDominanceRequired";
        public const string ExtremeMeditationProficiencyRequired =
            "ExtremeMeditationProficiencyRequired";
        public const string ConflictPolicyRevisionRequired =
            "ConflictPolicyRevisionRequired";
        public const string AllowedEffectScopeOwnedByQ021 =
            "AllowedEffectScopeOwnedByQ021";
        public const string EffectStrengthOwnedByQ022 =
            "EffectStrengthOwnedByQ022";
    }

    public sealed class Simulation암흑기정신차림접근CandidateRequest
    {
        public Simulation개인회복위협상쇄CandidateSnapshot
            RecoveryThreatOffsetCandidate { get; set; } =
                new Simulation개인회복위협상쇄CandidateSnapshot();
        public SimulationNaturePeriodStateSnapshot Period { get; set; } =
            new SimulationNaturePeriodStateSnapshot();
        public decimal RecoveryShare { get; set; }
        public decimal ThreatShare { get; set; }
        public bool ExtremeMeditationProficiencyReached { get; set; }
        public string ConflictPolicyRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "극한 위협에서는 암흑기를 지배 기간으로 유지하면서 극한 명상 숙련자의 제한적 광복기 계열 효과 접근 후보를 별도 상태로 전달한다.",
        StepKey = "contract.dark-age-mindfulness-access-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 28,
        Boundary = "단일 PeriodStateCode를 보존하며 허용 효과 범위·강도·유지 비용이나 실제 기간 전이를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q020 암흑기 우세 유지와 극한 명상 숙련자의 제한적 광복기 효과 접근 후보를 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 Effect 권한·기간 전이·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation암흑기정신차림접근CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation암흑기정신차림접근CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation암흑기정신차림접근CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation암흑기정신차림접근CandidateCodes.Gap;
        public string PlayerStableId { get; set; } = string.Empty;
        public string DominantPeriodStateCode { get; set; } = string.Empty;
        public string ConflictPolicyRevision { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool PreservesSingleDominantPeriodState { get; set; } = true;
        public bool DarkAgeRemainsDominant { get; set; }
        public bool LimitedGwangbokEffectAccessCandidate { get; set; }
        public bool ReplacesPeriodStateCode { get; set; }
        public bool AppliesEffectAccess { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation암흑기정신차림접근CandidateCodes
                .AllowedEffectScopeOwnedByQ021,
            Simulation암흑기정신차림접근CandidateCodes
                .EffectStrengthOwnedByQ022,
        };
    }

    public static class Simulation개인회복위협상쇄CandidateCodes
    {
        public const string SchemaVersion =
            "personal-recovery-threat-offset-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string RecoveryThreatOffset = "RecoveryThreatOffset";
        public const string SelfRecoveryAcceleration =
            "SelfRecoveryAcceleration";
        public const string ProficiencyThresholdRelaxation =
            "ProficiencyThresholdRelaxation";
        public const string OfflineTimeCandidateRequired =
            "OfflineTimeCandidateRequired";
        public const string PlayerStableIdRequired =
            "PlayerStableIdRequired";
        public const string OffsetPolicyRevisionRequired =
            "OffsetPolicyRevisionRequired";
        public const string PeriodThresholdProfileRevisionRequired =
            "PeriodThresholdProfileRevisionRequired";
        public const string OffsetRatioUnresolved = "OffsetRatioUnresolved";
        public const string ProficiencyThresholdCurveUnresolved =
            "ProficiencyThresholdCurveUnresolved";
        public const string PeriodConflictOwnedByQ020 =
            "PeriodConflictOwnedByQ020";
    }

    public sealed class Simulation개인회복위협상쇄CandidateRequest
    {
        public Simulation개인회복오프라인시간CandidateSnapshot
            OfflineTimeCandidate { get; set; } =
                new Simulation개인회복오프라인시간CandidateSnapshot();
        public string PlayerStableId { get; set; } = string.Empty;
        public decimal RecoveryOutput { get; set; }
        public decimal ThreatOutput { get; set; }
        public bool EligibleSelfRecoveryActionPresent { get; set; }
        public decimal MeditationProficiency { get; set; }
        public string OffsetPolicyRevision { get; set; } = string.Empty;
        public string PeriodThresholdProfileRevision { get; set; } =
            string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "개인 회복이 같은 플레이어의 위협을 낮추고 명상·정신 차림 성공과 숙련도가 상쇄·기간 문턱 후보에 기여하는 경계를 전달한다.",
        StepKey = "contract.personal-recovery-threat-offset-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 27,
        Boundary = "상쇄와 숙련도 문턱 완화 후보만 정의하며 비율·곡선·기간 충돌·실제 Mind Effect를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q019 개인 Recovery의 Threat 상쇄와 명상 성공 가속·숙련도별 광복기 문턱 완화 후보 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 Threat 변경·기간 전이·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation개인회복위협상쇄CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation개인회복위협상쇄CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation개인회복위협상쇄CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation개인회복위협상쇄CandidateCodes.Gap;
        public string PlayerStableId { get; set; } = string.Empty;
        public string OffsetPolicyRevision { get; set; } = string.Empty;
        public string PeriodThresholdProfileRevision { get; set; } =
            string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool ThreatOffsetCandidate { get; set; }
        public bool AcceleratedByEligibleSelfRecoveryAction { get; set; }
        public bool ProficiencyAdjustedGwangbokThresholdCandidate
            { get; set; }
        public bool AppliesThreatOffset { get; set; }
        public bool AppliesPeriodTransition { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation개인회복위협상쇄CandidateCodes.OffsetRatioUnresolved,
            Simulation개인회복위협상쇄CandidateCodes
                .ProficiencyThresholdCurveUnresolved,
            Simulation개인회복위협상쇄CandidateCodes
                .PeriodConflictOwnedByQ020,
        };
    }

    public static class Simulation개인회복오프라인시간CandidateCodes
    {
        public const string SchemaVersion =
            "personal-recovery-offline-time-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string PauseDuringOfflineRealTime =
            "PauseDuringOfflineRealTime";
        public const string ResumeOnAuthorityGameTime =
            "ResumeOnAuthorityGameTime";
        public const string RecoveryDecayCandidateRequired =
            "RecoveryDecayCandidateRequired";
        public const string OfflinePolicyRevisionRequired =
            "OfflinePolicyRevisionRequired";
        public const string SaveStateRevisionRequired =
            "SaveStateRevisionRequired";
        public const string SaveReferenceTickRequired =
            "SaveReferenceTickRequired";
        public const string SaveReplayBindingRequired =
            "SaveReplayBindingRequired";
        public const string AuthorityGameTimeResumeApplicationRequired =
            "AuthorityGameTimeResumeApplicationRequired";
        public const string ThreatOffsetOwnedByQ019 =
            "ThreatOffsetOwnedByQ019";
    }

    public sealed class Simulation개인회복오프라인시간CandidateRequest
    {
        public Simulation개인회복감쇠CandidateSnapshot RecoveryDecayCandidate
            { get; set; } = new Simulation개인회복감쇠CandidateSnapshot();
        public string OfflinePolicyRevision { get; set; } = string.Empty;
        public string SaveStateRevision { get; set; } = string.Empty;
        public bool SaveReferenceTickAvailable { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "게임 종료 중 현실 시간에는 개인 회복 감쇠를 멈추고 Save 복원 뒤 권위 게임 시간이 재개될 때만 이어가는 후보를 전달한다.",
        StepKey = "contract.personal-recovery-offline-time-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 26,
        Boundary = "벽시계 경과시간을 입력으로 사용하지 않으며 실제 Save 판본·복원·감쇠 적용이나 위협 상쇄를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q018 오프라인 현실 시간 감쇠 금지와 권위 게임 시간 재개 뒤 감쇠 재개 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 Save/Load·WorldTick 감쇠·Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation개인회복오프라인시간CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation개인회복오프라인시간CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation개인회복오프라인시간CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation개인회복오프라인시간CandidateCodes.Gap;
        public string OfflinePolicyRevision { get; set; } = string.Empty;
        public string SaveStateRevision { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool PausesDuringOfflineRealTime { get; set; } = true;
        public bool AppliesOfflineRealTimeDecay { get; set; }
        public bool UsesWallClockElapsedTime { get; set; }
        public bool ResumesOnAuthorityGameTime { get; set; } = true;
        public bool RequiresSaveReferenceTick { get; set; } = true;
        public bool SaveReferenceTickAvailable { get; set; }
        public bool AppliesRecoveryDecay { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation개인회복오프라인시간CandidateCodes
                .SaveReplayBindingRequired,
            Simulation개인회복오프라인시간CandidateCodes
                .AuthorityGameTimeResumeApplicationRequired,
            Simulation개인회복오프라인시간CandidateCodes
                .ThreatOffsetOwnedByQ019,
        };
    }

    public static class Simulation개인회복감쇠CandidateCodes
    {
        public const string SchemaVersion =
            "personal-recovery-decay-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string AuthorityGameTimeBaseDecay =
            "AuthorityGameTimeBaseDecay";
        public const string ThreatExposureAdditionalDecay =
            "ThreatExposureAdditionalDecay";
        public const string FatigueAccumulationAdditionalDecay =
            "FatigueAccumulationAdditionalDecay";
        public const string FocusFailureAdditionalDecay =
            "FocusFailureAdditionalDecay";
        public const string AuthorityTimePolicyRevisionRequired =
            "AuthorityTimePolicyRevisionRequired";
        public const string DecayProfileRevisionRequired =
            "DecayProfileRevisionRequired";
        public const string NumericCoefficientsUnresolved =
            "NumericCoefficientsUnresolved";
        public const string OfflineTimeOwnedByQ018 =
            "OfflineTimeOwnedByQ018";

        public static string[] OrderedCauseCodes() => new[]
        {
            AuthorityGameTimeBaseDecay,
            ThreatExposureAdditionalDecay,
            FatigueAccumulationAdditionalDecay,
            FocusFailureAdditionalDecay,
        };
    }

    public sealed class Simulation개인회복감쇠CauseSnapshot
    {
        public int Order { get; set; }
        public string CauseCode { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "개인 회복 감쇠를 권위 게임 시간 기본 감쇠와 위협·피로·집중 실패 추가 감쇠로 분리해 전달한다.",
        StepKey = "contract.personal-recovery-decay-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 25,
        Boundary = "감쇠 원인과 순서만 정의하며 계수·오프라인 시간·Recovery 상태·기간 이탈을 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q017 개인 Recovery의 권위 시간 기본 감쇠와 위협·피로·집중 실패 추가 원인 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 감쇠 수치·WorldTick 적용·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation개인회복감쇠CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation개인회복감쇠CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation개인회복감쇠CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation개인회복감쇠CandidateCodes.Gap;
        public string AuthorityTimePolicyRevision { get; set; } = string.Empty;
        public string DecayProfileRevision { get; set; } = string.Empty;
        public Simulation개인회복감쇠CauseSnapshot[] OrderedCauses
            { get; set; } = Array.Empty<Simulation개인회복감쇠CauseSnapshot>();
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool UsesAuthorityGameTime { get; set; } = true;
        public bool UsesUnityDeltaTime { get; set; }
        public bool AppliesRecoveryDecay { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation개인회복감쇠CandidateCodes
                .NumericCoefficientsUnresolved,
            Simulation개인회복감쇠CandidateCodes.OfflineTimeOwnedByQ018,
        };
    }

    public static class Simulation광복기공명유지CandidateCodes
    {
        public const string SchemaVersion =
            "gwangbok-resonance-maintenance-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string GwangbokPeriodRequired =
            "GwangbokPeriodRequired";
        public const string SelfEntryActionRequired =
            "SelfEntryActionRequired";
        public const string MaintenancePolicyRevisionRequired =
            "MaintenancePolicyRevisionRequired";
        public const string AuthorityTimeRevisionRequired =
            "AuthorityTimeRevisionRequired";
        public const string SelfRecoveryRefreshRequired =
            "SelfRecoveryRefreshRequired";
        public const string DecayProfileOwnedByQ017 =
            "DecayProfileOwnedByQ017";
    }

    public sealed class Simulation광복기공명유지CandidateRequest
    {
        public SimulationNaturePeriodStateSnapshot Period { get; set; } =
            new SimulationNaturePeriodStateSnapshot();
        public Simulation광복기자기회복행위CandidateSnapshot EntryAction
            { get; set; } = new Simulation광복기자기회복행위CandidateSnapshot();
        public bool ActiveResonancePresent { get; set; }
        public bool AfterglowPresent { get; set; }
        public bool PeriodicSelfRecoveryRefreshPresent { get; set; }
        public string MaintenancePolicyRevision { get; set; } = string.Empty;
        public string AuthorityTimeRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "자기 행위로 진입한 광복기에서 공명·잔향은 감쇠를 늦추지만 영구 유지하지 못하고 주기적 자기 회복 행위를 요구한다.",
        StepKey = "contract.gwangbok-resonance-maintenance-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 24,
        Boundary = "유지 책임만 정의하며 실제 감쇠율·자기 행동 주기·기간 이탈·WorldTick 상태를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q016 공명·잔향의 광복기 유지 보조와 영구 고정 금지·주기적 자기 회복 필요 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 Recovery 감쇠·기간 유지·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class Simulation광복기공명유지CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation광복기공명유지CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation광복기공명유지CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation광복기공명유지CandidateCodes.Gap;
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public string MaintenancePolicyRevision { get; set; } = string.Empty;
        public string AuthorityTimeRevision { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool ResonanceMaySlowRecoveryDecay { get; set; }
        public bool AfterglowMaySlowRecoveryDecay { get; set; }
        public bool AllowsResonanceOnlyPermanentMaintenance { get; set; }
        public bool RequiresPeriodicSelfRecoveryAction { get; set; } = true;
        public bool PeriodicSelfRecoveryRefreshPresent { get; set; }
        public bool AppliesRecoveryDecay { get; set; }
        public bool AppliesPeriodTransition { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation광복기공명유지CandidateCodes.DecayProfileOwnedByQ017,
        };
    }

    public static class Simulation광복기공명상한CandidateCodes
    {
        public const string SchemaVersion =
            "gwangbok-resonance-entry-cap-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string EntryCandidate = "EntryCandidate";
        public const string CappedBeforeEntry = "CappedBeforeEntry";
        public const string StackingCandidateRequired =
            "StackingCandidateRequired";
        public const string PeriodEntryPolicyRevisionRequired =
            "PeriodEntryPolicyRevisionRequired";
        public const string TargetOwnRecoveryContributionRequired =
            "TargetOwnRecoveryContributionRequired";
        public const string EligibleSelfActionOwnedByQ015 =
            "EligibleSelfActionOwnedByQ015";
    }

    public sealed class Simulation광복기공명상한CandidateRequest
    {
        public Simulation파티공명중첩CandidateSnapshot StackingCandidate
            { get; set; } = new Simulation파티공명중첩CandidateSnapshot();
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string PeriodEntryPolicyRevision { get; set; } = string.Empty;
        public bool TargetOwnRecoveryContributionPresent { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "파티 공명은 광복기 진입 직전까지만 돕고 대상 플레이어 자신의 회복 기여가 마지막 문턱을 넘게 하는 후보를 전달한다.",
        StepKey = "contract.gwangbok-resonance-entry-cap-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 22,
        Boundary = "공명 단독 진입 금지와 자기 행위 필요만 정의하며 인정 WI·정확한 여유 폭·기간 전이를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q014 공명 단독 광복기 진입을 막고 대상 플레이어 자기 회복 기여를 마지막 문턱으로 요구한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 Recovery cap 계산·기간 전이·ActionRecord 결속·Runtime 증거가 아니다.")]
    public sealed class Simulation광복기공명상한CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation광복기공명상한CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation광복기공명상한CandidateCodes.PlanningCandidate;
        public string EntryDecisionCode { get; set; } =
            Simulation광복기공명상한CandidateCodes.CappedBeforeEntry;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string PeriodEntryPolicyRevision { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public bool ResonanceOnlyEntryAllowed { get; set; }
        public bool TargetOwnRecoveryContributionRequired { get; set; } = true;
        public bool EntryThresholdCrossingCandidate { get; set; }
        public bool AppliesPeriodTransition { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation광복기공명상한CandidateCodes
                .EligibleSelfActionOwnedByQ015,
        };
    }

    public static class Simulation파티공명중첩CandidateCodes
    {
        public const string SchemaVersion =
            "party-resonance-stacking-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string StrongestFullContribution =
            "StrongestFullContribution";
        public const string RankedAttenuatedContribution =
            "RankedAttenuatedContribution";
        public const string ProviderStableIdRequired =
            "ProviderStableIdRequired";
        public const string PositiveMagnitudeRequired =
            "PositiveMagnitudeRequired";
        public const string AttenuationPolicyRevisionRequired =
            "AttenuationPolicyRevisionRequired";
        public const string AttenuationCoefficientsUnresolved =
            "AttenuationCoefficientsUnresolved";
        public const string MaximumContributorCountUnresolved =
            "MaximumContributorCountUnresolved";
        public const string EntryCapOwnedByQ014 =
            "EntryCapOwnedByQ014";
    }

    public sealed class Simulation파티공명기여CandidateInput
    {
        public string ProviderPlayerStableId { get; set; } = string.Empty;
        public decimal BaseMagnitude { get; set; }
    }

    public sealed class Simulation파티공명기여RankSnapshot
    {
        public int Rank { get; set; }
        public string ProviderPlayerStableId { get; set; } = string.Empty;
        public decimal BaseMagnitude { get; set; }
        public string ContributionPolicyCode { get; set; } = string.Empty;
        public bool UsesFullContribution { get; set; }
        public bool RequiresAttenuation { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "여러 파티 공명을 강도와 고유 식별자로 결정적 정렬하고 최강 기여는 온전히, 후속 기여는 순위 감쇠 대상으로 전달한다.",
        StepKey = "contract.party-resonance-stacking-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 21,
        Boundary = "중첩 순서와 정책 종류만 정의하며 감쇠 계수·최대 인원·광복기 진입 상한이나 NatureMind 상태를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q013 최강 공명 전체 기여·후속 순위 감쇠·결정적 동률 정렬 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 합산 회복량·상한·MindImpact 적용·Runtime 증거가 아니다.")]
    public sealed class Simulation파티공명중첩CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation파티공명중첩CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation파티공명중첩CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation파티공명중첩CandidateCodes.Gap;
        public string AttenuationPolicyRevision { get; set; } = string.Empty;
        public Simulation파티공명기여RankSnapshot[] RankedContributions
            { get; set; } = Array.Empty<Simulation파티공명기여RankSnapshot>();
        public string[] RejectedProviderStableIds { get; set; } =
            Array.Empty<string>();
        public bool OrderingIgnoresInputOrder { get; set; } = true;
        public bool AllowsUnlimitedLinearGrowth { get; set; }
        public bool AppliesStackedRecovery { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation파티공명중첩CandidateCodes
                .AttenuationCoefficientsUnresolved,
            Simulation파티공명중첩CandidateCodes
                .MaximumContributorCountUnresolved,
            Simulation파티공명중첩CandidateCodes.EntryCapOwnedByQ014,
        };
    }

    public static class Simulation파티공명잔향CandidateCodes
    {
        public const string SchemaVersion =
            "party-resonance-afterglow-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string RecoveryCandidateRequired =
            "RecoveryCandidateRequired";
        public const string DurationPolicyRevisionRequired =
            "DurationPolicyRevisionRequired";
        public const string DecayCurveRevisionRequired =
            "DecayCurveRevisionRequired";
        public const string AuthorityTimeRevisionRequired =
            "AuthorityTimeRevisionRequired";
        public const string DurationAndCoefficientsUnresolved =
            "DurationAndCoefficientsUnresolved";
        public const string SaveReplayBindingRequired =
            "SaveReplayBindingRequired";
        public const string StackingOwnedByQ013 =
            "StackingOwnedByQ013";
    }

    public sealed class Simulation파티공명잔향CandidateRequest
    {
        public Simulation파티공명회복CandidateSnapshot RecoveryCandidate
            { get; set; } = new Simulation파티공명회복CandidateSnapshot();
        public string DurationPolicyRevision { get; set; } = string.Empty;
        public string DecayCurveRevision { get; set; } = string.Empty;
        public string AuthorityTimeRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "근접 파티 공명이 끝난 뒤 회복 효과가 즉시 사라지지 않고 권위 시간 기반 잔향으로 감쇠하는 후보를 전달한다.",
        StepKey = "contract.party-resonance-afterglow-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 20,
        Boundary = "잔향의 시간 책임만 정의하며 지속시간·감쇠 계수·중첩·Save 상태나 Unity deltaTime 계산을 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q012 근접 종료 뒤 잔향 유지와 권위 Tick 기반 점진 감쇠·Save/Replay 필요 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 잔향 Effect·WorldTick 갱신·저장·Unity 표현·Runtime 증거가 아니다.")]
    public sealed class Simulation파티공명잔향CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation파티공명잔향CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation파티공명잔향CandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            Simulation파티공명잔향CandidateCodes.Gap;
        public string ProviderPlayerStableId { get; set; } = string.Empty;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string DurationPolicyRevision { get; set; } = string.Empty;
        public string DecayCurveRevision { get; set; } = string.Empty;
        public string AuthorityTimeRevision { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool LeavesAfterglowOnProximityExit { get; set; } = true;
        public bool RemovesEffectImmediatelyOnExit { get; set; }
        public bool UsesAuthorityWorldTick { get; set; } = true;
        public bool UsesUnityDeltaTime { get; set; }
        public bool RequiresRemainingMagnitudeInSave { get; set; } = true;
        public bool RequiresReferenceTickInSave { get; set; } = true;
        public bool AppliesAfterglowState { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation파티공명잔향CandidateCodes
                .DurationAndCoefficientsUnresolved,
            Simulation파티공명잔향CandidateCodes
                .SaveReplayBindingRequired,
            Simulation파티공명잔향CandidateCodes.StackingOwnedByQ013,
        };
    }

    public static class Simulation파티공명회복CandidateCodes
    {
        public const string SchemaVersion =
            "party-resonance-recovery-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Eligible = "Eligible";
        public const string Ineligible = "Ineligible";
        public const string PartyProximityCandidateRequired =
            "PartyProximityCandidateRequired";
        public const string EffectPolicyRevisionRequired =
            "EffectPolicyRevisionRequired";
        public const string PartyResonance = "PartyResonance";
        public const string EffectMagnitudeUnresolved =
            "EffectMagnitudeUnresolved";
        public const string PersistenceOwnedByQ012 =
            "PersistenceOwnedByQ012";
    }

    public sealed class Simulation파티공명회복CandidateRequest
    {
        public Simulation파티근접공명CandidateSnapshot ProximityCandidate
            { get; set; } = new Simulation파티근접공명CandidateSnapshot();
        public string EffectPolicyRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "파티 근접 공명의 첫 결과를 분야별 직접 버프가 아닌 대상 플레이어의 개인 회복 축 후보로 전달한다.",
        StepKey = "contract.party-resonance-recovery-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 19,
        Boundary = "회복 축 선택만 정의하며 크기·지속·중첩·기간 진입이나 전투·제작·채집 능력치를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q011 파티 공명을 개인 Recovery 축 하나로 결속하고 분야별 직접 강화와 세계 위협 변경을 금지한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        Boundary = "기획 후보이며 실제 MindImpact Effect·기간 전이·Unity 표현·Runtime 증거가 아니다.")]
    public sealed class Simulation파티공명회복CandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            Simulation파티공명회복CandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            Simulation파티공명회복CandidateCodes.PlanningCandidate;
        public string EligibilityCode { get; set; } =
            Simulation파티공명회복CandidateCodes.Ineligible;
        public string ProviderPlayerStableId { get; set; } = string.Empty;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string PartyStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } =
            Simulation파티공명회복CandidateCodes.PartyResonance;
        public string TargetAxisCode { get; set; } =
            SimulationNatureMindCodes.RecoveryAxis;
        public string EffectPolicyRevision { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public bool CreatesDirectCombatModifier { get; set; }
        public bool CreatesDirectCraftModifier { get; set; }
        public bool CreatesDirectGatheringModifier { get; set; }
        public bool ChangesRegionalThreat { get; set; }
        public bool AppliesMindImpactEffect { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            Simulation파티공명회복CandidateCodes
                .EffectMagnitudeUnresolved,
            Simulation파티공명회복CandidateCodes
                .PersistenceOwnedByQ012,
        };
    }

    public static class SimulationNatureMindCodes
    {
        public const string RuleRevision = "nature-mind-balance.r1";
        public const string DefaultPlayerStableId = "player:session:owner";
        public const string RecoveryAxis = "Recovery";
        public const string ThreatAxis = "Threat";
        public const string MixedBand = "Mixed";
        public const string RecoveryDominantBand = "RecoveryDominant";
        public const string ThreatDominantBand = "ThreatDominant";
        public const string FarmHarvestDispositionCompleted =
            "FarmHarvestDispositionCompleted";
        public const string FarmStorageFact = "fact:farm-storage-utilization";
        public const string FocusTimingCompleted = "FocusTimingCompleted";
    }

    public static class SimulationNaturePeriodCodes
    {
        public const string RuleRevision = "nature-period-state.r1";
        public const string ExitThresholdPolicyRevision =
            "nature-period-exit-threshold.r1";
        public const string OrdinaryPeriod = "OrdinaryPeriod";
        public const string GwangbokPeriod = "GwangbokPeriod";
        public const string DarkAgePeriod = "DarkAgePeriod";
        public const string EnteredEffect = "NaturePeriodEnteredEffect";
        public const string ExitedEffect = "NaturePeriodExitedEffect";
        public const string GwangbokEntryReason = "RecoveryShareAtLeast80Percent";
        public const string DarkAgeEntryReason = "ThreatShareAtLeast80Percent";
        public const string OrdinaryFallbackReason = "SpecialPeriodThresholdNotMet";
        public const string GwangbokExitReason = "RecoveryShareBelow75Percent";
        public const string DarkAgeExitReason = "ThreatShareBelow75Percent";
        public const string GwangbokRevelationCandidate =
            "revelation:nature.farm-storage-opportunity";
        public const string DarkAgeRecoveryWorldInteraction = "WI-NATURE-04";
    }

    public sealed class SimulationNatureMindInitialStateRequest
    {
        public string RuleRevision { get; set; } = SimulationNatureMindCodes.RuleRevision;
        public SimulationNatureMindPlayerInitialStateRequest[] Players { get; set; }
            = Array.Empty<SimulationNatureMindPlayerInitialStateRequest>();
    }

    public sealed class SimulationNatureMindPlayerInitialStateRequest
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public decimal RecoveryBaseOutput { get; set; }
        public decimal ThreatBaseOutput { get; set; }
    }

    public sealed class SimulationMindImpactEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string AxisCode { get; set; } = string.Empty;
        public decimal Magnitude { get; set; }
        public int AppliedWorldTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureMindContributorSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public decimal Magnitude { get; set; }
    }

    public sealed class SimulationNatureMindBalanceSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public decimal RecoveryOutput { get; set; }
        public decimal ThreatOutput { get; set; }
        public decimal RecoveryShare { get; set; }
        public decimal ThreatShare { get; set; }
        public decimal InterpretationStrength { get; set; }
        public string InterpretationBandCode { get; set; } = string.Empty;
        public SimulationNatureMindContributorSnapshot[] TopRecoveryContributors
            { get; set; } = Array.Empty<SimulationNatureMindContributorSnapshot>();
        public SimulationNatureMindContributorSnapshot[] TopThreatContributors
            { get; set; } = Array.Empty<SimulationNatureMindContributorSnapshot>();
        public long Revision { get; set; }
        public string BalanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureMindStateSnapshot
    {
        public string RuleRevision { get; set; } = string.Empty;
        public SimulationNatureMindBalanceSnapshot[] Balances { get; set; }
            = Array.Empty<SimulationNatureMindBalanceSnapshot>();
        public SimulationMindImpactEffectSnapshot[] Effects { get; set; }
            = Array.Empty<SimulationMindImpactEffectSnapshot>();
        public SimulationNaturePeriodStateSnapshot[] Periods { get; set; }
            = Array.Empty<SimulationNaturePeriodStateSnapshot>();
        public SimulationNaturePeriodHistorySnapshot[] PeriodHistory { get; set; }
            = Array.Empty<SimulationNaturePeriodHistorySnapshot>();
        public SimulationNaturePeriodTransitionEffectSnapshot[] PeriodTransitionEffects
            { get; set; } = Array.Empty<SimulationNaturePeriodTransitionEffectSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNaturePeriodStateSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodStateCode { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public long SourceBalanceRevision { get; set; }
        public string SourceBalanceHashSha256 { get; set; } = string.Empty;
        public int EnteredAtWorldTick { get; set; }
        public string EnterReasonCode { get; set; } = string.Empty;
        public string ExitThresholdPolicyRevision { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string PeriodStateHashSha256 { get; set; } = string.Empty;
        public int BaseRecoveryWorkDurationTicks { get; set; }
        public int EffectiveRecoveryWorkDurationTicks { get; set; }
        public int WorkDurationModifierTicks { get; set; }
        public string[] CandidateStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNaturePeriodHistorySnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int EnterTick { get; set; }
        public int? ExitTick { get; set; }
        public string[] MajorOutcomeRefs { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNaturePeriodTransitionEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string EffectTypeCode { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public string SourceBalanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureFarmInterpretationSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string FactStableId { get; set; } = string.Empty;
        public decimal FactValue { get; set; }
        public string FactUnitCode { get; set; } = string.Empty;
        public string FactStateHashSha256 { get; set; } = string.Empty;
        public string InferenceCode { get; set; } = string.Empty;
        public string InferenceText { get; set; } = string.Empty;
        public string MoodProjectionCode { get; set; } = string.Empty;
        public string[] PrioritizedCardStableIds { get; set; } = Array.Empty<string>();
        public SimulationNatureMindBalanceSnapshot Balance { get; set; }
            = new SimulationNatureMindBalanceSnapshot();
        public SimulationNaturePeriodStateSnapshot Period { get; set; }
            = new SimulationNaturePeriodStateSnapshot();
        public bool ChangesSharedFact { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
