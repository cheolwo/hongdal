using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "개인 회복·위협·명상 자기 행위·숙련도를 읽어 위협 상쇄와 광복기 문턱 완화 후보를 판정한다.",
        StepKey = "domain.personal-recovery-threat-offset-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.personal-recovery-offline-time-candidate",
            "domain.personal-recovery-offline-time-candidate",
            "contract.personal-recovery-threat-offset-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 37,
        Boundary = "후보 자격만 판정하며 상쇄량·기간 문턱·Recovery·Threat·WorldTick 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q019 회복 기반 개인 위협 상쇄와 자기 회복 행위 가속·숙련도 문턱 완화 후보를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 Mind Effect·기간 전이·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationPersonalRecoveryThreatOffsetCandidateEvaluator
    {
        public Simulation개인회복위협상쇄CandidateSnapshot Evaluate(
            Simulation개인회복위협상쇄CandidateRequest request)
        {
            var missing = new List<string>();
            if (request?.OfflineTimeCandidate?.ReadinessCode !=
                Simulation개인회복오프라인시간CandidateCodes.Ready)
                missing.Add(Simulation개인회복위협상쇄CandidateCodes
                    .OfflineTimeCandidateRequired);
            if (string.IsNullOrWhiteSpace(request?.PlayerStableId))
                missing.Add(Simulation개인회복위협상쇄CandidateCodes
                    .PlayerStableIdRequired);
            if (string.IsNullOrWhiteSpace(request?.OffsetPolicyRevision))
                missing.Add(Simulation개인회복위협상쇄CandidateCodes
                    .OffsetPolicyRevisionRequired);
            if (string.IsNullOrWhiteSpace(
                request?.PeriodThresholdProfileRevision))
                missing.Add(Simulation개인회복위협상쇄CandidateCodes
                    .PeriodThresholdProfileRevisionRequired);

            var recovery = request?.RecoveryOutput ?? 0m;
            var threat = request?.ThreatOutput ?? 0m;
            var proficiency = request?.MeditationProficiency ?? 0m;
            var offsetCandidate = recovery > 0m && threat > 0m;

            return new Simulation개인회복위협상쇄CandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? Simulation개인회복위협상쇄CandidateCodes.Ready
                    : Simulation개인회복위협상쇄CandidateCodes.Gap,
                PlayerStableId = request?.PlayerStableId?.Trim()
                    ?? string.Empty,
                OffsetPolicyRevision = request?.OffsetPolicyRevision?.Trim()
                    ?? string.Empty,
                PeriodThresholdProfileRevision = request?
                    .PeriodThresholdProfileRevision?.Trim() ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                ThreatOffsetCandidate = offsetCandidate,
                AcceleratedByEligibleSelfRecoveryAction = offsetCandidate &&
                    request?.EligibleSelfRecoveryActionPresent == true,
                ProficiencyAdjustedGwangbokThresholdCandidate =
                    proficiency > 0m,
                AppliesThreatOffset = false,
                AppliesPeriodTransition = false,
                ChangesWorldState = false,
            };
        }
    }
}
