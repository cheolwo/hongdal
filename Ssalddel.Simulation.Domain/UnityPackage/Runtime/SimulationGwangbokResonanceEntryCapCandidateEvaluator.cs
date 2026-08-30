using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "공명 중첩 후보와 자기 회복 기여 여부를 읽어 광복기 마지막 문턱의 주도권 후보를 판정한다.",
        StepKey = "domain.gwangbok-resonance-entry-cap-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.party-resonance-stacking-candidate",
            "domain.party-resonance-stacking-candidate",
            "contract.gwangbok-resonance-entry-cap-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 32,
        Boundary = "진입 자격 후보만 판정하며 Recovery 수치·기간 상태·ActionRecord·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q014 공명 단독 진입 상한과 대상 플레이어 자기 회복 기여 관문을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 진입 문턱 계산·기간 전이·Runtime 증거가 아니다.")]
    public sealed class SimulationGwangbokResonanceEntryCapCandidateEvaluator
    {
        public Simulation광복기공명상한CandidateSnapshot Evaluate(
            Simulation광복기공명상한CandidateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var stacking = request.StackingCandidate
                ?? new Simulation파티공명중첩CandidateSnapshot();
            var reasons = new List<string>();
            if (!string.Equals(stacking.ReadinessCode,
                    Simulation파티공명중첩CandidateCodes.Ready,
                    StringComparison.Ordinal))
                reasons.Add(Simulation광복기공명상한CandidateCodes
                    .StackingCandidateRequired);
            if (string.IsNullOrWhiteSpace(request.PeriodEntryPolicyRevision))
                reasons.Add(Simulation광복기공명상한CandidateCodes
                    .PeriodEntryPolicyRevisionRequired);
            if (!request.TargetOwnRecoveryContributionPresent)
                reasons.Add(Simulation광복기공명상한CandidateCodes
                    .TargetOwnRecoveryContributionRequired);

            var canCross = reasons.Count == 0;
            return new Simulation광복기공명상한CandidateSnapshot
            {
                EntryDecisionCode = canCross
                    ? Simulation광복기공명상한CandidateCodes.EntryCandidate
                    : Simulation광복기공명상한CandidateCodes
                        .CappedBeforeEntry,
                TargetPlayerStableId =
                    request.TargetPlayerStableId?.Trim() ?? string.Empty,
                PeriodEntryPolicyRevision =
                    request.PeriodEntryPolicyRevision?.Trim() ?? string.Empty,
                ReasonCodes = reasons.ToArray(),
                ResonanceOnlyEntryAllowed = false,
                TargetOwnRecoveryContributionRequired = true,
                EntryThresholdCrossingCandidate = canCross,
                AppliesPeriodTransition = false,
                ChangesWorldState = false,
            };
        }
    }
}
