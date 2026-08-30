using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "허용된 개인 정신 차림 효과와 현재 회복·장기 명상 숙련도를 읽어 접근과 강도 입력 책임을 분리한다.",
        StepKey = "domain.dark-age-mindfulness-effect-strength-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.dark-age-mindfulness-effect-scope-candidate",
            "domain.dark-age-mindfulness-effect-scope-candidate",
            "contract.dark-age-mindfulness-effect-strength-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 40,
        Boundary = "입력 책임과 준비도만 판정하며 효과 강도·Recovery·숙련도·세계 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q022 현재 회복 기반 접근과 장기 명상 숙련도 기반 강도 후보를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 강도 계산·Effect 적용·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationDarkAgeMindfulnessEffectStrengthCandidateEvaluator
    {
        public Simulation암흑기정신차림EffectStrengthCandidateSnapshot Evaluate(
            Simulation암흑기정신차림EffectStrengthCandidateRequest request)
        {
            var missing = new List<string>();
            var scope = request?.EffectScopeCandidate;
            if (scope?.AccessDecisionCode !=
                Simulation암흑기정신차림EffectScopeCandidateCodes.Allowed)
                missing.Add(Simulation암흑기정신차림EffectStrengthCandidateCodes
                    .AllowedEffectScopeCandidateRequired);
            if ((request?.CurrentRecoveryShare ?? 0m) <= 0m)
                missing.Add(Simulation암흑기정신차림EffectStrengthCandidateCodes
                    .PositiveCurrentRecoveryShareRequired);
            if ((request?.LongTermMeditationProficiency ?? 0m) <= 0m)
                missing.Add(Simulation암흑기정신차림EffectStrengthCandidateCodes
                    .LongTermMeditationProficiencyRequired);
            if (string.IsNullOrWhiteSpace(request?.StrengthProfileRevision))
                missing.Add(Simulation암흑기정신차림EffectStrengthCandidateCodes
                    .StrengthProfileRevisionRequired);

            return new Simulation암흑기정신차림EffectStrengthCandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? Simulation암흑기정신차림EffectStrengthCandidateCodes.Ready
                    : Simulation암흑기정신차림EffectStrengthCandidateCodes.Gap,
                PlayerStableId = scope?.PlayerStableId ?? string.Empty,
                EffectCode = scope?.EffectCode ?? string.Empty,
                StrengthProfileRevision =
                    request?.StrengthProfileRevision?.Trim() ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                AccessAvailableFromCurrentRecovery =
                    scope?.AccessDecisionCode ==
                        Simulation암흑기정신차림EffectScopeCandidateCodes.Allowed &&
                    (request?.CurrentRecoveryShare ?? 0m) > 0m,
                StrengthCandidateFromLongTermProficiency =
                    (request?.LongTermMeditationProficiency ?? 0m) > 0m,
                UsesCurrentRecoveryShareForStrength = false,
                AppliesEffectStrength = false,
                ChangesWorldState = false,
            };
        }
    }
}
