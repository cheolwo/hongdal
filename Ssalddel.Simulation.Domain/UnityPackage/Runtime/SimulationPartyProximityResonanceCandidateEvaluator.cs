using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "같은 파티·근접·승인된 명상 자격을 읽어 역할 배정 없는 수동 공명 후보를 판정한다.",
        StepKey = "domain.party-proximity-resonance-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.party-proximity-resonance-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 28,
        Boundary = "입력된 승인 판정과 파티 문맥만 읽으며 명상 Profile·NatureMind·역할·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q010 수동 공명의 최소 파티·근접·명상 자격 조건을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 거리 계산·온라인 신원 검증·회복 Effect 적용·Runtime 증거가 아니다.")]
    public sealed class SimulationPartyProximityResonanceCandidateEvaluator
    {
        public Simulation파티근접공명CandidateSnapshot Evaluate(
            Simulation파티근접공명CandidateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var reasons = new List<string>();
            var provider = (request.ProviderPlayerStableId ?? string.Empty)
                .Trim();
            var target = (request.TargetPlayerStableId ?? string.Empty).Trim();
            var providerParty = (request.ProviderPartyStableId
                                 ?? string.Empty).Trim();
            var targetParty = (request.TargetPartyStableId
                               ?? string.Empty).Trim();

            if (provider.Length == 0 || target.Length == 0)
                reasons.Add(Simulation파티근접공명CandidateCodes
                    .ParticipantIdentityInvalid);
            else if (string.Equals(provider, target,
                         StringComparison.Ordinal))
                reasons.Add(Simulation파티근접공명CandidateCodes.SamePlayer);
            if (providerParty.Length == 0 || targetParty.Length == 0
                || !string.Equals(providerParty, targetParty,
                    StringComparison.Ordinal))
                reasons.Add(Simulation파티근접공명CandidateCodes
                    .SamePartyRequired);
            if (string.IsNullOrWhiteSpace(
                    request.MeditationEligibilityPolicyRevision))
                reasons.Add(Simulation파티근접공명CandidateCodes
                    .MeditationPolicyRevisionRequired);
            if (string.IsNullOrWhiteSpace(request.ProximityPolicyRevision))
                reasons.Add(Simulation파티근접공명CandidateCodes
                    .ProximityPolicyRevisionRequired);
            if (!request.ProviderEligibleByMeditationPolicy)
                reasons.Add(Simulation파티근접공명CandidateCodes
                    .ProviderMeditationEligibilityRequired);
            if (!request.IsWithinApprovedProximity)
                reasons.Add(Simulation파티근접공명CandidateCodes
                    .ApprovedProximityRequired);

            var eligible = reasons.Count == 0;
            return new Simulation파티근접공명CandidateSnapshot
            {
                EligibilityCode = eligible
                    ? Simulation파티근접공명CandidateCodes.Eligible
                    : Simulation파티근접공명CandidateCodes.Ineligible,
                ProviderPlayerStableId = provider,
                TargetPlayerStableId = target,
                PartyStableId = providerParty,
                MeditationEligibilityPolicyRevision =
                    request.MeditationEligibilityPolicyRevision?.Trim()
                    ?? string.Empty,
                ProximityPolicyRevision =
                    request.ProximityPolicyRevision?.Trim() ?? string.Empty,
                ReasonCodes = reasons.ToArray(),
                PassiveEffectCandidateCreated = eligible,
                RequiresRoleProposal = false,
                RequiresRoleAcceptance = false,
                AssignsRole = false,
                ReadsPrivateGrowthProfile = false,
                ChangesNatureMindState = false,
                ChangesWorldState = false,
            };
        }
    }
}
