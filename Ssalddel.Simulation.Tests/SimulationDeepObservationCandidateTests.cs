using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q008 심층 관찰 계층 순서와 온라인 정보 보호 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-01", "WI-NATURE-11" },
    Boundary = "후보 계약 시험이며 실제 관찰 Projection·권한·Unity 표현·Play Mode 증거가 아니다.")]
public sealed class SimulationDeepObservationCandidateTests
{
    [Fact]
    public void 관찰후보는_환경_전투_허용된사회낌새순서를_유지한다()
    {
        var result = new SimulationDeepObservationCandidateEvaluator()
            .Evaluate(new[]
            {
                Layer(Simulation심층관찰CandidateCodes
                    .AuthorizedSocialGrowthHint, "social-hint.r1"),
                Layer(Simulation심층관찰CandidateCodes
                    .EnvironmentSignals, "environment-signals.r1"),
                Layer(Simulation심층관찰CandidateCodes
                    .CombatIntentAndWeakness, "combat-observation.r1"),
            });

        Assert.Equal(Simulation심층관찰CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Equal(new[]
        {
            Simulation심층관찰CandidateCodes.EnvironmentSignals,
            Simulation심층관찰CandidateCodes.CombatIntentAndWeakness,
            Simulation심층관찰CandidateCodes.AuthorizedSocialGrowthHint,
        }, result.LayerRevisions.Select(value => value.LayerCode));
        Assert.Equal("WI-NATURE-01",
            result.ExistingNatureObservationWorldInteractionId);
        Assert.Equal(SimulationLocalCombatCodes
                .WeaknessObservationCardDefinition,
            result.ExistingCombatObservationCardDefinition);
    }

    [Fact]
    public void 사회관찰후보는_권한을요구하고_원본기록과인벤토리를노출하지않는다()
    {
        var result = new SimulationDeepObservationCandidateEvaluator()
            .Evaluate(new[]
            {
                Layer(Simulation심층관찰CandidateCodes
                    .AuthorizedSocialGrowthHint, "social-hint.r1"),
            });

        Assert.True(result.SocialLayerRequiresAuthorization);
        Assert.False(result.ExposesRawActionLog);
        Assert.False(result.ExposesPrivateInventory);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation심층관찰CandidateCodes.EnvironmentSignals,
            result.MissingLayerCodes);
    }

    private static Simulation심층관찰LayerRevision Layer(
        string code, string revision)
        => new Simulation심층관찰LayerRevision
        {
            LayerCode = code,
            ProjectionRevision = revision,
        };
}
