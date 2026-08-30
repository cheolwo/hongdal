using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "개인 회복 감쇠 후보와 Save 기준 Tick을 읽어 오프라인 현실 시간 정지·권위 게임 시간 재개 정책의 준비도를 판정한다.",
        StepKey = "domain.personal-recovery-offline-time-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.personal-recovery-decay-candidate",
            "domain.personal-recovery-decay-candidate",
            "contract.personal-recovery-offline-time-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 36,
        Boundary = "오프라인 정책 준비도만 판정하며 벽시계·Recovery·Save·WorldTick 상태를 읽거나 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q018 오프라인 현실 시간 감쇠 금지와 권위 게임 시간 재개 조건을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 Save/Load·감쇠 재개·Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationPersonalRecoveryOfflineTimeCandidateEvaluator
    {
        public Simulation개인회복오프라인시간CandidateSnapshot Evaluate(
            Simulation개인회복오프라인시간CandidateRequest request)
        {
            var missing = new List<string>();
            var decay = request?.RecoveryDecayCandidate;
            if (decay == null || decay.ReadinessCode !=
                Simulation개인회복감쇠CandidateCodes.Ready)
            {
                missing.Add(Simulation개인회복오프라인시간CandidateCodes
                    .RecoveryDecayCandidateRequired);
            }

            if (string.IsNullOrWhiteSpace(request?.OfflinePolicyRevision))
                missing.Add(Simulation개인회복오프라인시간CandidateCodes
                    .OfflinePolicyRevisionRequired);
            if (string.IsNullOrWhiteSpace(request?.SaveStateRevision))
                missing.Add(Simulation개인회복오프라인시간CandidateCodes
                    .SaveStateRevisionRequired);
            if (request?.SaveReferenceTickAvailable != true)
                missing.Add(Simulation개인회복오프라인시간CandidateCodes
                    .SaveReferenceTickRequired);

            return new Simulation개인회복오프라인시간CandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? Simulation개인회복오프라인시간CandidateCodes.Ready
                    : Simulation개인회복오프라인시간CandidateCodes.Gap,
                OfflinePolicyRevision =
                    request?.OfflinePolicyRevision?.Trim() ?? string.Empty,
                SaveStateRevision =
                    request?.SaveStateRevision?.Trim() ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                PausesDuringOfflineRealTime = true,
                AppliesOfflineRealTimeDecay = false,
                UsesWallClockElapsedTime = false,
                ResumesOnAuthorityGameTime = true,
                RequiresSaveReferenceTick = true,
                SaveReferenceTickAvailable =
                    request?.SaveReferenceTickAvailable == true,
                AppliesRecoveryDecay = false,
                ChangesWorldState = false,
            };
        }
    }
}
