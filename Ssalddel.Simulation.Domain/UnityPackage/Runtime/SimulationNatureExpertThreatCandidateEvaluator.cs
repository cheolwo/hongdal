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
        "숙련자 위협 강화 세 축과 기존 집중 Profile 결속의 구현 준비도를 판정한다.",
        StepKey = "domain.nature-expert-threat-candidate-readiness",
        DependsOnStepKeys = new[]
        {
            "contract.nature-expert-threat-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 23,
        Boundary = "준비도만 판정하고 Spawn·전투·보상·집중 자원·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q005 숙련자 위협 강화와 기존 집중 체계 결속 후보의 공백을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-11", "WI-NATURE-14" },
        Boundary = "Candidate 준비도이며 실제 전투 난이도나 집중 판정 결과를 적용하지 않는다.")]
    public sealed class SimulationNatureExpertThreatCandidateEvaluator
    {
        public SimulationNatureExpertThreatCandidateSnapshot Evaluate(
            IEnumerable<SimulationNatureThreatIntensityDimensionRevision>
                intensityDimensionRevisions,
            string focusRequirementRevision)
        {
            var required = SimulationNatureExpertThreatCandidateCodes
                .RequiredIntensityDimensionCodes();
            var dimensions = (intensityDimensionRevisions
                    ?? Array.Empty<SimulationNatureThreatIntensityDimensionRevision>())
                .Where(value => value != null
                                && required.Contains(value.DimensionCode,
                                    StringComparer.Ordinal)
                                && !string.IsNullOrWhiteSpace(
                                    value.RuleRevision))
                .GroupBy(value => value.DimensionCode,
                    StringComparer.Ordinal)
                .Select(group => new
                    SimulationNatureThreatIntensityDimensionRevision
                    {
                        DimensionCode = group.Key,
                        RuleRevision = group
                            .Select(value => value.RuleRevision.Trim())
                            .OrderBy(value => value, StringComparer.Ordinal)
                            .First(),
                    })
                .OrderBy(value => value.DimensionCode,
                    StringComparer.Ordinal)
                .ToArray();
            var missing = required
                .Except(dimensions.Select(value => value.DimensionCode),
                    StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
            var focusRevision = (focusRequirementRevision ?? string.Empty)
                .Trim();
            if (focusRevision.Length == 0)
            {
                missing.Add(SimulationNatureExpertThreatCandidateCodes
                    .FocusRequirement);
            }

            return new SimulationNatureExpertThreatCandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? SimulationNatureExpertThreatCandidateCodes.Ready
                    : SimulationNatureExpertThreatCandidateCodes.Gap,
                IntensityDimensionRevisions = dimensions,
                MissingRequirementCodes = missing.ToArray(),
                FocusProfileCatalogRevision =
                    Simulation집중판정Codes.FocusProfileCatalogRevision,
                FocusRequirementRevision = focusRevision,
                ReusesExistingMeditationSystem = true,
                ChangesBaseWorldInteractionOutcome = false,
                UnresolvedDecisionCodes =
                    SimulationNatureExpertThreatCandidateCodes
                        .UnresolvedDecisionCodes(),
            };
        }
    }
}
