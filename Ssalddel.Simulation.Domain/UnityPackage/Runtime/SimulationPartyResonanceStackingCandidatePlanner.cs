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
        "여러 공명 제공자를 강도 내림차순·고유 식별자 오름차순으로 정렬해 최강 전체·후속 감쇠 후보를 계획한다.",
        StepKey = "domain.party-resonance-stacking-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.party-resonance-afterglow-candidate",
            "domain.party-resonance-afterglow-candidate",
            "contract.party-resonance-stacking-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 31,
        Boundary = "결정적 순위와 감쇠 필요 여부만 반환하며 최종 합산량·상한·NatureMind·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q013 다중 공명의 결정적 순위와 최강 전체·후속 감쇠 후보를 읽기 전용으로 계획한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 계획이며 실제 감쇠 계수·최대 기여 인원·회복 적용·Runtime 증거가 아니다.")]
    public sealed class SimulationPartyResonanceStackingCandidatePlanner
    {
        public Simulation파티공명중첩CandidateSnapshot Plan(
            IEnumerable<Simulation파티공명기여CandidateInput> inputs,
            string attenuationPolicyRevision)
        {
            var rejected = new List<string>();
            var valid = (inputs
                    ?? Array.Empty<Simulation파티공명기여CandidateInput>())
                .Where(value => value != null)
                .Where(value =>
                {
                    var id = (value.ProviderPlayerStableId
                              ?? string.Empty).Trim();
                    if (id.Length > 0 && value.BaseMagnitude > 0m)
                        return true;
                    rejected.Add(id);
                    return false;
                })
                .GroupBy(value => value.ProviderPlayerStableId.Trim(),
                    StringComparer.Ordinal)
                .Select(group => group
                    .OrderByDescending(value => value.BaseMagnitude)
                    .First())
                .OrderByDescending(value => value.BaseMagnitude)
                .ThenBy(value => value.ProviderPlayerStableId,
                    StringComparer.Ordinal)
                .ToArray();
            var policyReady = !string.IsNullOrWhiteSpace(
                attenuationPolicyRevision);
            var ranked = valid.Select((value, index) =>
                new Simulation파티공명기여RankSnapshot
                {
                    Rank = index + 1,
                    ProviderPlayerStableId =
                        value.ProviderPlayerStableId.Trim(),
                    BaseMagnitude = value.BaseMagnitude,
                    ContributionPolicyCode = index == 0
                        ? Simulation파티공명중첩CandidateCodes
                            .StrongestFullContribution
                        : Simulation파티공명중첩CandidateCodes
                            .RankedAttenuatedContribution,
                    UsesFullContribution = index == 0,
                    RequiresAttenuation = index > 0,
                }).ToArray();

            return new Simulation파티공명중첩CandidateSnapshot
            {
                ReadinessCode = policyReady && ranked.Length > 0
                    ? Simulation파티공명중첩CandidateCodes.Ready
                    : Simulation파티공명중첩CandidateCodes.Gap,
                AttenuationPolicyRevision =
                    attenuationPolicyRevision?.Trim() ?? string.Empty,
                RankedContributions = ranked,
                RejectedProviderStableIds = rejected
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                OrderingIgnoresInputOrder = true,
                AllowsUnlimitedLinearGrowth = false,
                AppliesStackedRecovery = false,
                ChangesWorldState = false,
            };
        }
    }
}
