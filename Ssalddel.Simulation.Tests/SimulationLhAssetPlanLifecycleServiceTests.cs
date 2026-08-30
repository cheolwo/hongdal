using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Q338 LH 배치 계획 수명주기와 계획 identity 고정을 회귀 검증한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata
        .SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "단위 시험은 실제 Unity 활성화나 Runtime 증거가 아니다.")]
public sealed class SimulationLhAssetPlanLifecycleServiceTests
{
    [Fact]
    public void 동결계획은_Prepare_Active_Cached_Released를거쳐도내용Hash가같다()
    {
        var service = new SimulationLhAssetPlanLifecycleService();
        var prepared = service.Prepare(Plans());
        var active = Move(service, prepared,
            SimulationLhAssetPlanLifecycleCodes.Active);
        var cached = Move(service, active,
            SimulationLhAssetPlanLifecycleCodes.Cached);
        var reactivated = Move(service, cached,
            SimulationLhAssetPlanLifecycleCodes.Active);
        var released = Move(service, reactivated,
            SimulationLhAssetPlanLifecycleCodes.Released);

        Assert.Equal(SimulationLhAssetPlanLifecycleCodes.Prepared,
            prepared.LifecycleStateCode);
        Assert.Equal(SimulationLhAssetPlanLifecycleCodes.Released,
            released.LifecycleStateCode);
        Assert.Equal(4, released.LifecycleRevision);
        Assert.All(new[] { active, cached, reactivated, released }, value =>
        {
            Assert.Equal(prepared.SourceWorldRevision,
                value.SourceWorldRevision);
            Assert.Equal(prepared.SourceCombinedPlanHashSha256,
                value.SourceCombinedPlanHashSha256);
            Assert.Equal(prepared.ExteriorPlacementPlanHashSha256,
                value.ExteriorPlacementPlanHashSha256);
            Assert.Equal(prepared.InteriorPlacementPlanHashSha256,
                value.InteriorPlacementPlanHashSha256);
            Assert.True(value.PlacementContentFrozen);
            Assert.True(value.PresentationOnly);
            Assert.Equal(64, value.LifecycleHashSha256.Length);
        });
    }

    [Fact]
    public void 같은상태요청은_멱등이고_수명주기Revision을늘리지않는다()
    {
        var service = new SimulationLhAssetPlanLifecycleService();
        var prepared = service.Prepare(Plans());

        var repeated = Move(service, prepared,
            SimulationLhAssetPlanLifecycleCodes.Prepared);

        Assert.Equal(prepared.LifecycleRevision,
            repeated.LifecycleRevision);
        Assert.Equal(prepared.LifecycleHashSha256,
            repeated.LifecycleHashSha256);
        Assert.NotSame(prepared, repeated);
    }

    [Fact]
    public void 오래된Revision과_허용되지않은전이는거부된다()
    {
        var service = new SimulationLhAssetPlanLifecycleService();
        var prepared = service.Prepare(Plans());
        var active = Move(service, prepared,
            SimulationLhAssetPlanLifecycleCodes.Active);

        var stale = Assert.Throws<InvalidOperationException>(() =>
            service.Transition(active,
                new SimulationLhAssetPlanLifecycleTransitionRequest
                {
                    ExpectedLifecycleRevision = 0,
                    TargetStateCode =
                        SimulationLhAssetPlanLifecycleCodes.Cached,
                }));
        Assert.Equal("SimulationLhLifecycleRevisionMismatch", stale.Message);

        var invalid = Assert.Throws<InvalidOperationException>(() =>
            service.Transition(active,
                new SimulationLhAssetPlanLifecycleTransitionRequest
                {
                    ExpectedLifecycleRevision = active.LifecycleRevision,
                    TargetStateCode =
                        SimulationLhAssetPlanLifecycleCodes.Prepared,
                }));
        Assert.Equal("SimulationLhLifecycleTransitionInvalid", invalid.Message);
    }

    [Fact]
    public void 계획Hash가서로다르면_Prepare를거부한다()
    {
        var plans = Plans();
        plans.InteriorPlan.SourceCombinedPlanHashSha256 =
            Simulation세계자산CanonicalHash.Hash("another-plan");

        var error = Assert.Throws<ArgumentException>(() =>
            new SimulationLhAssetPlanLifecycleService().Prepare(plans));

        Assert.Equal("SimulationLhAssetPlanSetInvalid", error.Message);
    }

    private static SimulationLhAssetPlanLifecycleSnapshot Move(
        SimulationLhAssetPlanLifecycleService service,
        SimulationLhAssetPlanLifecycleSnapshot current,
        string target)
        => service.Transition(current,
            new SimulationLhAssetPlanLifecycleTransitionRequest
            {
                ExpectedLifecycleRevision = current.LifecycleRevision,
                TargetStateCode = target,
            });

    private static Simulation분리세계자산배치Result Plans()
    {
        var combinedHash = Simulation세계자산CanonicalHash.Hash("combined");
        return new Simulation분리세계자산배치Result
        {
            CompatibilityPlan = new Simulation세계자산배치Plan
            {
                CellStableId = "kr5186:l3:2801:4581",
                SourceWorldRevision = 21,
                AssetPlacementPlanHashSha256 = combinedHash,
            },
            ExteriorPlan = new Simulation실외자산배치Plan
            {
                CellStableId = "kr5186:l3:2801:4581",
                SourceWorldRevision = 21,
                SourceCombinedPlanHashSha256 = combinedHash,
                ExteriorPlacementPlanHashSha256 =
                    Simulation세계자산CanonicalHash.Hash("exterior"),
            },
            InteriorPlan = new Simulation실내자산배치Plan
            {
                CellStableId = "kr5186:l3:2801:4581",
                SourceWorldRevision = 21,
                SourceCombinedPlanHashSha256 = combinedHash,
                InteriorPlacementPlanHashSha256 =
                    Simulation세계자산CanonicalHash.Hash("interior"),
            },
        };
    }
}
