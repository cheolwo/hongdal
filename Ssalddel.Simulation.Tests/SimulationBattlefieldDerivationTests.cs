using System.Linq;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationBattlefieldDerivationTests
{
    private static SimulationBattlefieldDerivationService CreateService() => new(
        new FileSimulationWorldLayoutCatalogReader(
            "eng/world-seedbeds/generated/h5-world-layout.v1.json"),
        new FileSimulationWorldActualE5SpatialCatalogReader(
            "eng/world-seedbeds/generated/actual-e5-spatial.v1.json"));

    [Fact]
    public void Farm_지역_사실에서_결정적인_독립_전장을_파생한다()
    {
        var service = CreateService();

        var first = service.Derive("session:battlefield-test",
            "encounter:farm:gate-1", "area:sim:pyeongchang:farm", 17, false);
        var second = service.Derive("session:battlefield-test",
            "encounter:farm:gate-1", "area:sim:pyeongchang:farm", 17, false);

        Assert.True(first.CanConfirm, string.Join(",", first.BlockingReasonCodes));
        Assert.Equal(SimulationBattlefieldDerivationCodes.FarmPerimeter500,
            first.BattlefieldPlan.ProfileCode);
        Assert.Equal(SimulationBattlefieldDerivationCodes.BattleLocalMeters,
            first.BattlefieldPlan.CoordinateSpaceCode);
        Assert.Equal(500d, first.BattlefieldPlan.WidthMeters);
        Assert.Equal(500d, first.BattlefieldPlan.DepthMeters);
        Assert.Equal(15_625, first.BattlefieldPlan.TerrainCells.Length);
        Assert.Equal(first.WorldContext.ContextHashSha256,
            second.WorldContext.ContextHashSha256);
        Assert.Equal(first.WorldContext.AnchorSetHashSha256,
            second.WorldContext.AnchorSetHashSha256);
        Assert.Equal(first.BattlefieldDerivationInputHashSha256,
            second.BattlefieldDerivationInputHashSha256);
        Assert.Equal(first.BattlefieldPlan.BattlefieldPlanHashSha256,
            second.BattlefieldPlan.BattlefieldPlanHashSha256);
    }

    [Fact]
    public void 전역_WorldRevision_변경은_같은_지역_전장을_무효화하지_않는다()
    {
        var service = CreateService();

        var before = service.Derive("session:battlefield-revision",
            "encounter:farm:gate-2", "area:sim:pyeongchang:farm", 40, false);
        var after = service.Derive("session:battlefield-revision",
            "encounter:farm:gate-2", "area:sim:pyeongchang:farm", 41, false);

        Assert.Equal(40, before.SpatialOrigin.CapturedWorldRevision);
        Assert.Equal(41, after.SpatialOrigin.CapturedWorldRevision);
        Assert.Equal(before.WorldContext.ContextHashSha256,
            after.WorldContext.ContextHashSha256);
        Assert.Equal(before.BattlefieldDerivationInputHashSha256,
            after.BattlefieldDerivationInputHashSha256);
        Assert.Equal(before.BattlefieldPlan.BattlefieldPlanHashSha256,
            after.BattlefieldPlan.BattlefieldPlanHashSha256);
    }

    [Fact]
    public void Anchor_선정은_Seed_전에_닫히고_원본과_전투_ID를_분리한다()
    {
        var value = CreateService().Derive("session:battlefield-anchor",
            "encounter:farm:gate-3", "area:sim:pyeongchang:farm", 3, false);

        Assert.NotEmpty(value.WorldContext.Anchors);
        Assert.All(value.WorldContext.Anchors, anchor =>
            Assert.StartsWith("battlefield-anchor:", anchor.BattlefieldAnchorStableId));
        Assert.All(value.WorldContext.Anchors.Where(anchor =>
                anchor.PreservationPolicyCode == SimulationBattlefieldDerivationCodes.Required),
            anchor => Assert.Contains(value.BattlefieldPlan.AnchorPlacements,
                placement => placement.BattlefieldAnchorStableId ==
                             anchor.BattlefieldAnchorStableId));
        Assert.Contains(value.WorldContext.Anchors, anchor =>
            anchor.WorldEffectTargetStableId.Length > 0
            && anchor.WorldEffectTargetStableId != anchor.BattlefieldAnchorStableId);
    }

    [Fact]
    public void 문맥_경계를_통과하는_경로를_Portal로_보존한다()
    {
        var value = CreateService().Derive("session:battlefield-portal",
            "encounter:farm:gate-4", "area:sim:pyeongchang:farm", 5, false);

        Assert.NotEmpty(value.WorldContext.BoundaryPortals);
        Assert.All(value.WorldContext.BoundaryPortals, portal =>
        {
            Assert.StartsWith("battle-context-portal:", portal.PortalStableId);
            Assert.NotEmpty(portal.SourceRouteStableId);
            Assert.True(System.Math.Abs(portal.Pose.XMeters
                                        - value.WorldContext.CenterXMeters) >= 499d
                        || System.Math.Abs(portal.Pose.ZMeters
                                           - value.WorldContext.CenterZMeters) >= 499d);
        });
        Assert.Contains(value.WorldContext.RouteConstraints, route =>
            route.SourceRouteStableId ==
            value.WorldContext.BoundaryPortals[0].SourceRouteStableId);
    }

    [Fact]
    public void Nature_조우도_같은_파이프라인에서_별도_Profile로_파생한다()
    {
        var value = CreateService().Derive("session:battlefield-nature",
            "encounter:nature:route-1", "nature-route:trail", 9, true);

        Assert.True(value.CanConfirm, string.Join(",", value.BlockingReasonCodes));
        Assert.Equal(SimulationBattlefieldDerivationCodes.NatureField500,
            value.BattlefieldPlan.ProfileCode);
        Assert.NotEqual(value.SpatialOrigin.AttackerPose.XMeters,
            value.SpatialOrigin.DefenderPose.XMeters);
    }
}
