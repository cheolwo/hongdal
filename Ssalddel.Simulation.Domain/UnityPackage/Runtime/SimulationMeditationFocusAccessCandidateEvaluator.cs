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
        "명상 숙련의 집중 접근 확대 후보가 기존 집중 판정과 결속될 준비가 되었는지 판정한다.",
        StepKey = "domain.meditation-focus-access-candidate-readiness",
        DependsOnStepKeys = new[]
        {
            "contract.meditation-focus-access-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 24,
        Boundary = "revision 준비도만 판정하고 숙련·집중·공격 피해·크리티컬·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q006 명상 숙련·순간 집중·기본 공격 집중 접근 후보의 구현 공백을 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-11" },
        Boundary = "Candidate 준비도이며 실제 집중 Challenge나 전투 Effect를 실행하지 않는다.")]
    public sealed class SimulationMeditationFocusAccessCandidateEvaluator
    {
        public Simulation명상집중접근CandidateSnapshot Evaluate(
            IEnumerable<Simulation명상집중접근RevisionBinding>
                revisionBindings)
        {
            var required = Simulation명상집중접근CandidateCodes
                .RequiredRevisionCodes();
            var bindings = (revisionBindings
                    ?? Array.Empty<Simulation명상집중접근RevisionBinding>())
                .Where(value => value != null
                                && required.Contains(
                                    value.ResponsibilityCode,
                                    StringComparer.Ordinal)
                                && !string.IsNullOrWhiteSpace(
                                    value.RuleRevision))
                .GroupBy(value => value.ResponsibilityCode,
                    StringComparer.Ordinal)
                .Select(group => new Simulation명상집중접근RevisionBinding
                {
                    ResponsibilityCode = group.Key,
                    RuleRevision = group.Select(value =>
                            value.RuleRevision.Trim())
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .First(),
                })
                .OrderBy(value => value.ResponsibilityCode,
                    StringComparer.Ordinal)
                .ToArray();
            var missing = required.Except(bindings.Select(value =>
                        value.ResponsibilityCode), StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            return new Simulation명상집중접근CandidateSnapshot
            {
                ReadinessCode = missing.Length == 0
                    ? Simulation명상집중접근CandidateCodes.Ready
                    : Simulation명상집중접근CandidateCodes.Gap,
                RevisionBindings = bindings,
                MissingResponsibilityCodes = missing,
                ReusesExistingFocusPolicy = true,
                GuaranteesCriticalOutcome = false,
            };
        }
    }
}
