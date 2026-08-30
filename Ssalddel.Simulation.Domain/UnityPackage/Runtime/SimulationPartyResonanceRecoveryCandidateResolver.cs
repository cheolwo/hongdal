using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "Q010의 근접 공명 후보를 개인 Recovery 축 후보로만 해석하고 분야별 직접 버프를 만들지 않는다.",
        StepKey = "domain.party-resonance-recovery-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.party-proximity-resonance-candidate",
            "domain.party-proximity-resonance-candidate",
            "contract.party-resonance-recovery-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 29,
        Boundary = "효과 축 후보만 반환하며 NatureMind Effect를 추가하거나 기간·행위 능력치·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q011 승인된 근접 공명의 결과를 개인 Recovery 축 후보로 읽기 전용 해석한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 해석이며 실제 회복량·잔향·중첩·MindImpact 적용·Runtime 증거가 아니다.")]
    public sealed class SimulationPartyResonanceRecoveryCandidateResolver
    {
        public Simulation파티공명회복CandidateSnapshot Resolve(
            Simulation파티공명회복CandidateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var proximity = request.ProximityCandidate
                ?? new Simulation파티근접공명CandidateSnapshot();
            var reasons = new List<string>();
            if (!proximity.PassiveEffectCandidateCreated
                || !string.Equals(proximity.EligibilityCode,
                    Simulation파티근접공명CandidateCodes.Eligible,
                    StringComparison.Ordinal))
                reasons.Add(Simulation파티공명회복CandidateCodes
                    .PartyProximityCandidateRequired);
            if (string.IsNullOrWhiteSpace(request.EffectPolicyRevision))
                reasons.Add(Simulation파티공명회복CandidateCodes
                    .EffectPolicyRevisionRequired);

            return new Simulation파티공명회복CandidateSnapshot
            {
                EligibilityCode = reasons.Count == 0
                    ? Simulation파티공명회복CandidateCodes.Eligible
                    : Simulation파티공명회복CandidateCodes.Ineligible,
                ProviderPlayerStableId = proximity.ProviderPlayerStableId,
                TargetPlayerStableId = proximity.TargetPlayerStableId,
                PartyStableId = proximity.PartyStableId,
                EffectPolicyRevision = request.EffectPolicyRevision?.Trim()
                    ?? string.Empty,
                ReasonCodes = reasons.ToArray(),
                CreatesDirectCombatModifier = false,
                CreatesDirectCraftModifier = false,
                CreatesDirectGatheringModifier = false,
                ChangesRegionalThreat = false,
                AppliesMindImpactEffect = false,
                ChangesWorldState = false,
            };
        }
    }
}
