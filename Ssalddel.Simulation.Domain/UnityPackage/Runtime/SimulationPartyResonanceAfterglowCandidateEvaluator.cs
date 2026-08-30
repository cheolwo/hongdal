using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "Q011 회복 공명 후보와 판본화된 시간 정책을 읽어 권위 Tick 기반 잔향 준비도를 판정한다.",
        StepKey = "domain.party-resonance-afterglow-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.party-resonance-recovery-candidate",
            "domain.party-resonance-recovery-candidate",
            "contract.party-resonance-afterglow-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "잔향 준비도만 판정하며 WorldTick·Save·NatureMind·Unity 프레임 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q012 공명 잔향의 권위 시간·지속·감쇠 판본 준비도를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 감쇠 계산·저장·Replay hash·Runtime 증거가 아니다.")]
    public sealed class SimulationPartyResonanceAfterglowCandidateEvaluator
    {
        public Simulation파티공명잔향CandidateSnapshot Evaluate(
            Simulation파티공명잔향CandidateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var recovery = request.RecoveryCandidate
                ?? new Simulation파티공명회복CandidateSnapshot();
            var missing = new List<string>();
            if (!string.Equals(recovery.EligibilityCode,
                    Simulation파티공명회복CandidateCodes.Eligible,
                    StringComparison.Ordinal))
                missing.Add(Simulation파티공명잔향CandidateCodes
                    .RecoveryCandidateRequired);
            if (string.IsNullOrWhiteSpace(request.DurationPolicyRevision))
                missing.Add(Simulation파티공명잔향CandidateCodes
                    .DurationPolicyRevisionRequired);
            if (string.IsNullOrWhiteSpace(request.DecayCurveRevision))
                missing.Add(Simulation파티공명잔향CandidateCodes
                    .DecayCurveRevisionRequired);
            if (string.IsNullOrWhiteSpace(request.AuthorityTimeRevision))
                missing.Add(Simulation파티공명잔향CandidateCodes
                    .AuthorityTimeRevisionRequired);

            return new Simulation파티공명잔향CandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? Simulation파티공명잔향CandidateCodes.Ready
                    : Simulation파티공명잔향CandidateCodes.Gap,
                ProviderPlayerStableId = recovery.ProviderPlayerStableId,
                TargetPlayerStableId = recovery.TargetPlayerStableId,
                DurationPolicyRevision = request.DurationPolicyRevision?.Trim()
                    ?? string.Empty,
                DecayCurveRevision = request.DecayCurveRevision?.Trim()
                    ?? string.Empty,
                AuthorityTimeRevision = request.AuthorityTimeRevision?.Trim()
                    ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                LeavesAfterglowOnProximityExit = true,
                RemovesEffectImmediatelyOnExit = false,
                UsesAuthorityWorldTick = true,
                UsesUnityDeltaTime = false,
                RequiresRemainingMagnitudeInSave = true,
                RequiresReferenceTickInSave = true,
                AppliesAfterglowState = false,
                ChangesWorldState = false,
            };
        }
    }
}
