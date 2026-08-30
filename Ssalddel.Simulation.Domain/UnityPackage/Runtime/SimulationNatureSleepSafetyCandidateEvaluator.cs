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
        "주변 상황별 수면 안전 기획 후보의 보호 수단과 질병 증분 범위 준비도를 판정한다.",
        StepKey = "domain.nature-sleep-safety-candidate-readiness",
        DependsOnStepKeys = new[]
        {
            "contract.nature-sleep-safety-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 20,
        Boundary = "후보 준비도만 읽으며 위험 수면 허용·차단, 불 연료 소비, 질병 발병·회복을 실행하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q002 단계적 수면 안전 후보의 보호 요구와 미결정 경계를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "Candidate 판정이며 수면 Confirm이나 권위 Revision을 변경하지 않는다.")]
    public sealed class SimulationNatureSleepSafetyCandidateEvaluator
    {
        public SimulationNatureSleepSafetyCandidateSnapshot Evaluate(
            string situationCode,
            IEnumerable<string> availableProtectionCodes,
            int? diseaseRiskIncrementMinimum = null,
            int? diseaseRiskIncrementMaximum = null)
        {
            var requirements = RequirementsFor(situationCode);
            var available = (availableProtectionCodes
                    ?? Array.Empty<string>())
                .Where(IsKnownProtection)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var missing = requirements
                .Where(requirement => !requirement.AlternativeProtectionCodes
                    .Any(candidate => available.Contains(candidate,
                        StringComparer.Ordinal)))
                .Select(requirement => requirement.RequirementStableId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
            var boundsDefined = diseaseRiskIncrementMinimum.HasValue
                                && diseaseRiskIncrementMaximum.HasValue
                                && diseaseRiskIncrementMinimum.Value >= 0
                                && diseaseRiskIncrementMaximum.Value >=
                                diseaseRiskIncrementMinimum.Value;
            if (!boundsDefined)
            {
                missing.Add(SimulationNatureSleepSafetyCandidateCodes
                    .DiseaseIncrementBoundsRequired);
            }

            return new SimulationNatureSleepSafetyCandidateSnapshot
            {
                SituationCode = situationCode ?? string.Empty,
                ReadinessCode = missing.Count == 0
                    ? SimulationNatureSleepSafetyCandidateCodes.Ready
                    : SimulationNatureSleepSafetyCandidateCodes.Gap,
                ProtectionRequirements = requirements,
                AvailableProtectionCodes = available,
                MissingRequirementStableIds = missing.ToArray(),
                DiseaseIncrementBoundsDefined = boundsDefined,
                DiseaseRiskIncrementMinimum = diseaseRiskIncrementMinimum ?? 0,
                DiseaseRiskIncrementMaximum = diseaseRiskIncrementMaximum ?? 0,
                UnresolvedDecisionCodes =
                    SimulationNatureSleepSafetyCandidateCodes
                        .UnresolvedDecisionCodes(),
            };
        }

        private static SimulationNatureSleepProtectionRequirement[]
            RequirementsFor(string situationCode)
        {
            var requirements = new List<SimulationNatureSleepProtectionRequirement>
            {
                Requirement("shelter", SimulationNatureSleepSafetyCandidateCodes.Cabin),
            };
            if (string.Equals(situationCode,
                    SimulationNatureSleepSafetyCandidateCodes.AnimalThreat,
                    StringComparison.Ordinal))
            {
                requirements.Add(Requirement("animal-deterrence",
                    SimulationNatureSleepSafetyCandidateCodes.Fire));
            }
            else if (string.Equals(situationCode,
                         SimulationNatureSleepSafetyCandidateCodes.MonsterThreat,
                         StringComparison.Ordinal))
            {
                requirements.Add(Requirement("perimeter-defense",
                    SimulationNatureSleepSafetyCandidateCodes.Fence,
                    SimulationNatureSleepSafetyCandidateCodes.MagicCircle));
            }

            return requirements.ToArray();
        }

        private static SimulationNatureSleepProtectionRequirement Requirement(
            string stableId, params string[] alternatives)
            => new SimulationNatureSleepProtectionRequirement
            {
                RequirementStableId = stableId,
                AlternativeProtectionCodes = alternatives,
            };

        private static bool IsKnownProtection(string value)
            => string.Equals(value,
                   SimulationNatureSleepSafetyCandidateCodes.Cabin,
                   StringComparison.Ordinal)
               || string.Equals(value,
                   SimulationNatureSleepSafetyCandidateCodes.Fire,
                   StringComparison.Ordinal)
               || string.Equals(value,
                   SimulationNatureSleepSafetyCandidateCodes.Fence,
                   StringComparison.Ordinal)
               || string.Equals(value,
                   SimulationNatureSleepSafetyCandidateCodes.MagicCircle,
                   StringComparison.Ordinal);
    }
}
