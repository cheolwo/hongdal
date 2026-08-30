using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "개인 기간·회복 비중·위협 비중·명상 숙련도를 읽어 암흑기 우세와 제한적 정신 차림 효과 접근 후보를 판정한다.",
        StepKey = "domain.dark-age-mindfulness-access-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.personal-recovery-threat-offset-candidate",
            "domain.personal-recovery-threat-offset-candidate",
            "contract.dark-age-mindfulness-access-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 38,
        Boundary = "접근 후보만 판정하며 PeriodStateCode·Effect 권한·Recovery·Threat 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q020 암흑기 우세를 유지한 채 극한 명상 숙련자의 제한적 효과 접근 후보를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 기간 전이·Effect 적용·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationDarkAgeMindfulnessAccessCandidateEvaluator
    {
        public Simulation암흑기정신차림접근CandidateSnapshot Evaluate(
            Simulation암흑기정신차림접근CandidateRequest request)
        {
            var missing = new List<string>();
            if (request?.RecoveryThreatOffsetCandidate?.ReadinessCode !=
                Simulation개인회복위협상쇄CandidateCodes.Ready)
                missing.Add(Simulation암흑기정신차림접근CandidateCodes
                    .RecoveryThreatOffsetCandidateRequired);
            var period = request?.Period;
            var isDarkAge = period?.PeriodStateCode ==
                SimulationNaturePeriodCodes.DarkAgePeriod;
            if (!isDarkAge)
                missing.Add(Simulation암흑기정신차림접근CandidateCodes
                    .DarkAgePeriodRequired);
            if ((request?.RecoveryShare ?? 0m) <= 0m)
                missing.Add(Simulation암흑기정신차림접근CandidateCodes
                    .PositiveRecoveryShareRequired);
            if ((request?.ThreatShare ?? 0m) <=
                (request?.RecoveryShare ?? 0m))
                missing.Add(Simulation암흑기정신차림접근CandidateCodes
                    .ThreatDominanceRequired);
            if (request?.ExtremeMeditationProficiencyReached != true)
                missing.Add(Simulation암흑기정신차림접근CandidateCodes
                    .ExtremeMeditationProficiencyRequired);
            if (string.IsNullOrWhiteSpace(request?.ConflictPolicyRevision))
                missing.Add(Simulation암흑기정신차림접근CandidateCodes
                    .ConflictPolicyRevisionRequired);

            var access = missing.Count == 0;
            return new Simulation암흑기정신차림접근CandidateSnapshot
            {
                ReadinessCode = access
                    ? Simulation암흑기정신차림접근CandidateCodes.Ready
                    : Simulation암흑기정신차림접근CandidateCodes.Gap,
                PlayerStableId = period?.PlayerStableId ?? string.Empty,
                DominantPeriodStateCode = period?.PeriodStateCode
                    ?? string.Empty,
                ConflictPolicyRevision =
                    request?.ConflictPolicyRevision?.Trim() ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                PreservesSingleDominantPeriodState = true,
                DarkAgeRemainsDominant = isDarkAge,
                LimitedGwangbokEffectAccessCandidate = access,
                ReplacesPeriodStateCode = false,
                AppliesEffectAccess = false,
                ChangesWorldState = false,
            };
        }
    }
}
