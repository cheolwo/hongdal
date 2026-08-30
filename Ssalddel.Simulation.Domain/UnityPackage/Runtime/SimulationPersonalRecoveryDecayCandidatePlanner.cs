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
        "권위 시간과 위협·피로·집중 실패 상태를 읽어 개인 회복 감쇠 원인의 결정적 계산 순서를 계획한다.",
        StepKey = "domain.personal-recovery-decay-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.gwangbok-resonance-maintenance-candidate",
            "domain.gwangbok-resonance-maintenance-candidate",
            "contract.personal-recovery-decay-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 35,
        Boundary = "원인 순서만 계획하며 감쇠 계수·Recovery·기간·WorldTick 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q017 개인 회복 감쇠의 기본·추가 원인과 계산 순서를 읽기 전용으로 계획한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 계획이며 실제 수치 계산·오프라인 시간·Runtime 증거가 아니다.")]
    public sealed class SimulationPersonalRecoveryDecayCandidatePlanner
    {
        public Simulation개인회복감쇠CandidateSnapshot Plan(
            string authorityTimePolicyRevision,
            string decayProfileRevision,
            bool threatExposure,
            bool fatigueAccumulation,
            bool focusFailure)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(authorityTimePolicyRevision))
                missing.Add(Simulation개인회복감쇠CandidateCodes
                    .AuthorityTimePolicyRevisionRequired);
            if (string.IsNullOrWhiteSpace(decayProfileRevision))
                missing.Add(Simulation개인회복감쇠CandidateCodes
                    .DecayProfileRevisionRequired);
            var active = new HashSet<string>(StringComparer.Ordinal)
            {
                Simulation개인회복감쇠CandidateCodes
                    .AuthorityGameTimeBaseDecay,
            };
            if (threatExposure)
                active.Add(Simulation개인회복감쇠CandidateCodes
                    .ThreatExposureAdditionalDecay);
            if (fatigueAccumulation)
                active.Add(Simulation개인회복감쇠CandidateCodes
                    .FatigueAccumulationAdditionalDecay);
            if (focusFailure)
                active.Add(Simulation개인회복감쇠CandidateCodes
                    .FocusFailureAdditionalDecay);
            var causes = Simulation개인회복감쇠CandidateCodes
                .OrderedCauseCodes().Select((code, index) =>
                    new Simulation개인회복감쇠CauseSnapshot
                    {
                        Order = index + 1,
                        CauseCode = code,
                        Active = active.Contains(code),
                    }).ToArray();

            return new Simulation개인회복감쇠CandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? Simulation개인회복감쇠CandidateCodes.Ready
                    : Simulation개인회복감쇠CandidateCodes.Gap,
                AuthorityTimePolicyRevision =
                    authorityTimePolicyRevision?.Trim() ?? string.Empty,
                DecayProfileRevision = decayProfileRevision?.Trim()
                    ?? string.Empty,
                OrderedCauses = causes,
                MissingRequirementCodes = missing.ToArray(),
                UsesAuthorityGameTime = true,
                UsesUnityDeltaTime = false,
                AppliesRecoveryDecay = false,
                ChangesWorldState = false,
            };
        }
    }
}
