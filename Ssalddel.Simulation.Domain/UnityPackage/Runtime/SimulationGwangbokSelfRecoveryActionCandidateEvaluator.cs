using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "판본화된 WI Profile과 ActionRecord·회복 변화·집중 결과를 읽어 광복기 자기 회복 행위 후보를 판정한다.",
        StepKey = "domain.gwangbok-self-recovery-action-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.gwangbok-resonance-entry-cap-candidate",
            "domain.gwangbok-resonance-entry-cap-candidate",
            "contract.gwangbok-self-recovery-action-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 33,
        Boundary = "기존 기록을 읽어 자격만 판정하며 회복 Effect·기간·ActionRecord·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q015 자기 명상·집중 성공의 실제 회복 기여와 수면 제외 여부를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[]
        {
            "WI-REFLECT-01", "WI-NATURE-06", "WI-NATURE-14",
        },
        Boundary = "후보 판정이며 실제 광복기 진입·ActionRecord 생성·Runtime 증거가 아니다.")]
    public sealed class SimulationGwangbokSelfRecoveryActionCandidateEvaluator
    {
        public static Simulation광복기자기회복행위ProfileDefinition[]
            CreateDefaultCandidateProfiles() => new[]
            {
                Profile("WI-REFLECT-01",
                    Simulation광복기자기회복행위CandidateCodes
                        .MindfulnessAction, true, false, string.Empty),
                Profile("WI-NATURE-06",
                    Simulation광복기자기회복행위CandidateCodes
                        .SuccessfulFocus, true, true, string.Empty),
                Profile("WI-NATURE-14",
                    Simulation광복기자기회복행위CandidateCodes
                        .CompleteSleepExcluded, false, false,
                    Simulation광복기자기회복행위CandidateCodes
                        .CompleteSleepExcluded),
            };

        public Simulation광복기자기회복행위CandidateSnapshot Evaluate(
            Simulation광복기자기회복행위CandidateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var record = request.ActionRecord ?? new Simulation행위발현Record();
            var profile = (request.Profiles
                    ?? Array.Empty<Simulation광복기자기회복행위ProfileDefinition>())
                .FirstOrDefault(value => value != null && string.Equals(
                    value.WorldInteractionId, record.WorldInteractionId,
                    StringComparison.Ordinal));
            var reasons = new List<string>();
            if (!string.Equals(request.ProfileRevision,
                    Simulation광복기자기회복행위CandidateCodes.ProfileRevision,
                    StringComparison.Ordinal))
                reasons.Add(Simulation광복기자기회복행위CandidateCodes
                    .ProfileMissing);
            if (profile == null || !profile.EligibleForEntryTrigger)
                reasons.Add(profile?.ReasonCode.Length > 0
                    ? profile.ReasonCode
                    : Simulation광복기자기회복행위CandidateCodes.ProfileMissing);
            var hasRecord = !string.IsNullOrWhiteSpace(
                                record.행위기록StableId)
                            && string.Equals(record.결과분류Code,
                                Simulation행위결과분류Codes.성공,
                                StringComparison.Ordinal);
            if (!hasRecord)
                reasons.Add(Simulation광복기자기회복행위CandidateCodes
                    .ActionRecordRequired);
            var hasRecovery = record.변화의미Codes.Contains(
                Simulation행위변화의미Codes.플레이어회복변경,
                StringComparer.Ordinal);
            if (!hasRecovery)
                reasons.Add(Simulation광복기자기회복행위CandidateCodes
                    .RecoveryChangeRequired);
            var focusRequired = profile?.RequiresFocusSuccess == true;
            var focusSuccess = !focusRequired || IsFocusSuccess(
                request.FocusResult, record.행위기록StableId);
            if (!focusSuccess)
                reasons.Add(Simulation광복기자기회복행위CandidateCodes
                    .FocusSuccessRequired);
            var eligible = reasons.Count == 0;

            return new Simulation광복기자기회복행위CandidateSnapshot
            {
                EligibilityCode = eligible
                    ? Simulation광복기자기회복행위CandidateCodes.Eligible
                    : Simulation광복기자기회복행위CandidateCodes.Ineligible,
                TargetPlayerStableId =
                    request.TargetPlayerStableId?.Trim() ?? string.Empty,
                WorldInteractionId = record.WorldInteractionId,
                ActionRecordStableId = record.행위기록StableId,
                ActionKindCode = profile?.ActionKindCode ?? string.Empty,
                ProfileRevision = request.ProfileRevision ?? string.Empty,
                ReasonCodes = reasons.Distinct(StringComparer.Ordinal).ToArray(),
                HasSuccessfulActionRecord = hasRecord,
                HasRecoveryContribution = hasRecovery,
                HasRequiredFocusSuccess = focusSuccess,
                EligibleForGwangbokEntryTrigger = eligible,
                AppliesPeriodTransition = false,
                ChangesWorldState = false,
            };
        }

        private static bool IsFocusSuccess(
            Simulation집중판정ResultSnapshot? focus, string recordId)
            => focus != null
               && string.Equals(focus.SourceActionRecordStableId, recordId,
                   StringComparison.Ordinal)
               && focus.회복증가Milli > 0
               && (string.Equals(focus.ResultCode,
                       Simulation집중판정Codes.Perfect,
                       StringComparison.Ordinal)
                   || string.Equals(focus.ResultCode,
                       Simulation집중판정Codes.Good,
                       StringComparison.Ordinal));

        private static Simulation광복기자기회복행위ProfileDefinition Profile(
            string wi, string kind, bool eligible, bool focus, string reason)
            => new()
            {
                WorldInteractionId = wi,
                ActionKindCode = kind,
                EligibleForEntryTrigger = eligible,
                RequiresFocusSuccess = focus,
                ReasonCode = reason,
            };
    }
}
