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
        "환경·전투·사회 성장 낌새의 단계적 관찰 Projection 후보 준비도를 판정한다.",
        StepKey = "domain.deep-observation-progression-candidate-readiness",
        DependsOnStepKeys = new[]
        {
            "contract.deep-observation-progression-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 26,
        Boundary = "Projection revision 준비도만 판정하고 권한을 만들거나 비공개 원본·정확한 성장 수치를 노출하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q008 심층 관찰 세 계층과 개인정보 보호 경계의 구현 공백을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-01", "WI-NATURE-11" },
        Boundary = "Candidate 준비도이며 실제 관찰 Preview·전투 카드·온라인 Projection을 실행하지 않는다.")]
    public sealed class SimulationDeepObservationCandidateEvaluator
    {
        public Simulation심층관찰CandidateSnapshot Evaluate(
            IEnumerable<Simulation심층관찰LayerRevision> layerRevisions)
        {
            var ordered = Simulation심층관찰CandidateCodes
                .OrderedLayerCodes();
            var layers = (layerRevisions
                    ?? Array.Empty<Simulation심층관찰LayerRevision>())
                .Where(value => value != null
                                && ordered.Contains(value.LayerCode,
                                    StringComparer.Ordinal)
                                && !string.IsNullOrWhiteSpace(
                                    value.ProjectionRevision))
                .GroupBy(value => value.LayerCode, StringComparer.Ordinal)
                .Select(group => new Simulation심층관찰LayerRevision
                {
                    LayerCode = group.Key,
                    LayerOrder = Array.IndexOf(ordered, group.Key) + 1,
                    ProjectionRevision = group.Select(value =>
                            value.ProjectionRevision.Trim())
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .First(),
                })
                .OrderBy(value => value.LayerOrder)
                .ToArray();
            var missing = ordered.Except(layers.Select(value =>
                        value.LayerCode), StringComparer.Ordinal)
                .ToArray();

            return new Simulation심층관찰CandidateSnapshot
            {
                ReadinessCode = missing.Length == 0
                    ? Simulation심층관찰CandidateCodes.Ready
                    : Simulation심층관찰CandidateCodes.Gap,
                LayerRevisions = layers,
                MissingLayerCodes = missing,
                SocialLayerRequiresAuthorization = true,
                ExposesRawActionLog = false,
                ExposesPrivateInventory = false,
                ChangesWorldState = false,
            };
        }
    }
}
