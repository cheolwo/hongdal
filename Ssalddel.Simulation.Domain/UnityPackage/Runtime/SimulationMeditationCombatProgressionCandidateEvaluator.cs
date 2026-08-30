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
        "명상 숙련의 전투 보상 성장 후보가 승인된 전투 Effect에 인계될 준비가 되었는지 판정한다.",
        StepKey = "domain.meditation-combat-progression-candidate-readiness",
        DependsOnStepKeys = new[]
        {
            "contract.meditation-combat-progression-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 25,
        Boundary = "성장 revision 준비도만 판정하고 현재 전투 피해·크리티컬·관찰·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q007 명상 전투 성장 순서와 승인되지 않은 전투 Effect 경계를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-11" },
        Boundary = "Candidate 준비도이며 실제 LocalCombat 공격 계산을 실행하지 않는다.")]
    public sealed class SimulationMeditationCombatProgressionCandidateEvaluator
    {
        public Simulation명상전투성장CandidateSnapshot Evaluate(
            IEnumerable<Simulation명상전투성장StageRevision>
                stageRevisions)
        {
            var ordered = Simulation명상전투성장CandidateCodes
                .OrderedStageCodes();
            var stages = (stageRevisions
                    ?? Array.Empty<Simulation명상전투성장StageRevision>())
                .Where(value => value != null
                                && ordered.Contains(value.StageCode,
                                    StringComparer.Ordinal)
                                && !string.IsNullOrWhiteSpace(
                                    value.RuleRevision))
                .GroupBy(value => value.StageCode, StringComparer.Ordinal)
                .Select(group =>
                {
                    var order = Array.IndexOf(ordered, group.Key) + 1;
                    return new Simulation명상전투성장StageRevision
                    {
                        StageCode = group.Key,
                        StageOrder = order,
                        RuleRevision = group.Select(value =>
                                value.RuleRevision.Trim())
                            .OrderBy(value => value, StringComparer.Ordinal)
                            .First(),
                    };
                })
                .OrderBy(value => value.StageOrder)
                .ToArray();
            var missing = ordered.Except(stages.Select(value =>
                        value.StageCode), StringComparer.Ordinal)
                .ToArray();

            return new Simulation명상전투성장CandidateSnapshot
            {
                ReadinessCode = missing.Length == 0
                    ? Simulation명상전투성장CandidateCodes.Ready
                    : Simulation명상전투성장CandidateCodes.Gap,
                StageRevisions = stages,
                MissingStageCodes = missing,
                MutatesCurrentCombatRule = false,
                GuaranteesCriticalAtAnyStage = false,
            };
        }
    }
}
