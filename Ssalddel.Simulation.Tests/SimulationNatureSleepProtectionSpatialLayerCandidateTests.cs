using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q025 수면 보호 다섯 공간층과 배치·형상·경계 Graph 근거 요구를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14" },
    Boundary = "후보 판정 시험이며 실제 H 조립·배치·Collider·Bounds·Play Mode 증거가 아니다.")]
public sealed class SimulationNatureSleepProtectionSpatialLayerCandidateTests
{
    [Fact]
    public void 다섯보호역할을_중첩가능한공간층과근거로분리한다()
    {
        var result = new SimulationNatureSleepProtectionSpatialLayerCandidateEvaluator()
            .Evaluate(ReadyRequest(AllLayers()));

        Assert.Equal(SimulationNatureSleepProtectionSpatialLayerCandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Empty(result.MissingLayerRoleCodes);
        Assert.True(result.SupportsOverlappingLayers);
        Assert.True(result.UsesPlacementAndGeometryEvidence);
        Assert.True(result.UsesBoundaryGraphEvidence);
        Assert.False(result.AppliesSpatialProtection);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 경계Graph나필수공간층이없으면_준비완료로보지않는다()
    {
        var layers = AllLayers().Where(value => value.LayerRoleCode !=
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .HeatInfluenceLayer).ToArray();
        layers.Single(value => value.LayerRoleCode ==
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .PhysicalPerimeterLayer).BoundaryGraphRevision = string.Empty;

        var result = new SimulationNatureSleepProtectionSpatialLayerCandidateEvaluator()
            .Evaluate(ReadyRequest(layers));

        Assert.Equal(SimulationNatureSleepProtectionSpatialLayerCandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
            .HeatInfluenceLayer, result.MissingLayerRoleCodes);
        Assert.Contains(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
            .BoundaryGraphRevisionRequired, result.MissingRequirementCodes);
    }

    private static SimulationNatureSleepProtectionSpatialLayerCandidateRequest
        ReadyRequest(SimulationNatureSleepProtectionSpatialLayerDefinition[] layers)
    {
        return new SimulationNatureSleepProtectionSpatialLayerCandidateRequest
        {
            WeatherProfileCandidate =
                new SimulationNatureWeatherProfileFreezeCandidateSnapshot
                {
                    ReadinessCode =
                        SimulationNatureWeatherProfileFreezeCandidateCodes.Ready,
                },
            SpatialPolicyRevision = "sleep-protection-spatial.r1",
            Layers = layers,
        };
    }

    private static SimulationNatureSleepProtectionSpatialLayerDefinition[]
        AllLayers() => SimulationNatureSleepProtectionSpatialLayerCandidateCodes
            .RequiredLayerRoleCodes().Select((role, index) =>
                new SimulationNatureSleepProtectionSpatialLayerDefinition
                {
                    LayerStableId = $"layer:{index + 1}",
                    LayerRoleCode = role,
                    PlacementStableId = $"placement:{index + 1}",
                    GeometryRevision = "geometry.r1",
                    BoundaryGraphRevision = role ==
                            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                                .PhysicalPerimeterLayer || role ==
                            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                                .HigherThreatBoundaryLayer
                        ? "boundary-graph.r1"
                        : string.Empty,
                }).ToArray();
}
