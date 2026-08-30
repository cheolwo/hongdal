using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Q338 LH 수명주기의 Unity 표현 명령 변환과 StableId 거부 경계를 회귀 검증한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata
        .SsalddelEvidenceSubmoduleKeys.E3Unity소비자회귀,
    Boundary = "순수 C# 시험은 실제 GameObject·Play Mode·Game View 증거가 아니다.")]
public sealed class SimulationLhAssetPlanPresentationReconcilerTests
{
    [Fact]
    public void LH상태는_Unity의_PrepareActivateCacheRelease명령으로변환된다()
    {
        var prepared = Snapshot("cell:one",
            SimulationLhAssetPlanLifecycleCodes.Prepared, 0, 'a');
        var active = Snapshot("cell:one",
            SimulationLhAssetPlanLifecycleCodes.Active, 1, 'b');
        var reconciler = new SimulationLhAssetPlanPresentationReconciler();

        var first = reconciler.Reconcile(Array.Empty<
            SimulationLhAssetPlanPresentationState>(), new[] { prepared });
        var second = reconciler.Reconcile(first.States, new[] { active });

        Assert.Equal(SimulationLhAssetPlanPresentationCommandCodes.Prepare,
            Assert.Single(first.Commands).CommandCode);
        Assert.Equal(SimulationLhAssetPlanPresentationCommandCodes.Activate,
            Assert.Single(second.Commands).CommandCode);
        Assert.Equal(prepared.ExteriorPlacementPlanHashSha256,
            second.States.Single().ExteriorPlacementPlanHashSha256);
        Assert.Equal(prepared.InteriorPlacementPlanHashSha256,
            second.States.Single().InteriorPlacementPlanHashSha256);
    }

    [Fact]
    public void 같은LifecycleHash는_명령을다시만들지않는다()
    {
        var prepared = Snapshot("cell:same",
            SimulationLhAssetPlanLifecycleCodes.Prepared, 0, 'c');
        var reconciler = new SimulationLhAssetPlanPresentationReconciler();
        var first = reconciler.Reconcile(Array.Empty<
            SimulationLhAssetPlanPresentationState>(), new[] { prepared });

        var repeated = reconciler.Reconcile(first.States,
            new[] { prepared });

        Assert.Empty(repeated.Commands);
        Assert.Equal("cell:same",
            Assert.Single(repeated.UnchangedCellStableIds));
    }

    [Fact]
    public void 낮은LifecycleRevision과_계획Identity교체를거부한다()
    {
        var prepared = Snapshot("cell:stable",
            SimulationLhAssetPlanLifecycleCodes.Prepared, 0, 'd');
        var active = Snapshot("cell:stable",
            SimulationLhAssetPlanLifecycleCodes.Active, 1, 'e');
        var reconciler = new SimulationLhAssetPlanPresentationReconciler();
        var current = reconciler.Reconcile(Array.Empty<
            SimulationLhAssetPlanPresentationState>(), new[] { active }).States;

        var lower = Assert.Throws<InvalidOperationException>(() =>
            reconciler.Reconcile(current, new[] { prepared }));
        Assert.StartsWith("SimulationLhPresentationReconcileLowerDataRevision",
            lower.Message);

        var changed = Snapshot("cell:stable",
            SimulationLhAssetPlanLifecycleCodes.Prepared, 2, 'f', '9');
        var identity = Assert.Throws<InvalidOperationException>(() =>
            reconciler.Reconcile(current, new[] { changed }));
        Assert.Equal("SimulationLhPresentationPlanIdentityChanged:cell:stable",
            identity.Message);
    }

    [Fact]
    public void 사라진Cell은_Release명령을만든다()
    {
        var prepared = Snapshot("cell:gone",
            SimulationLhAssetPlanLifecycleCodes.Prepared, 0, '1');
        var reconciler = new SimulationLhAssetPlanPresentationReconciler();
        var current = reconciler.Reconcile(Array.Empty<
            SimulationLhAssetPlanPresentationState>(), new[] { prepared }).States;

        var removed = reconciler.Reconcile(current, Array.Empty<
            SimulationLhAssetPlanLifecycleSnapshot>());

        Assert.Equal(SimulationLhAssetPlanPresentationCommandCodes.Release,
            Assert.Single(removed.Commands).CommandCode);
        Assert.Empty(removed.States);
    }

    [Fact]
    public void 중복Cell은_계획Identity검사전에_일관된오류로거부한다()
    {
        var prepared = Snapshot("cell:duplicate",
            SimulationLhAssetPlanLifecycleCodes.Prepared, 0, '2');
        var reconciler = new SimulationLhAssetPlanPresentationReconciler();

        var incomingDuplicate = Assert.Throws<InvalidOperationException>(() =>
            reconciler.Reconcile(Array.Empty<
                SimulationLhAssetPlanPresentationState>(),
                new[] { prepared, prepared }));

        Assert.Equal(
            "SimulationLhPresentationReconcileDuplicateStableId:cell:duplicate",
            incomingDuplicate.Message);

        var current = reconciler.Reconcile(Array.Empty<
            SimulationLhAssetPlanPresentationState>(),
            new[] { prepared }).States.Single();
        var currentDuplicate = Assert.Throws<InvalidOperationException>(() =>
            reconciler.Reconcile(new[] { current, current },
                new[] { prepared }));

        Assert.Equal(
            "SimulationLhPresentationReconcileDuplicateStableId:cell:duplicate",
            currentDuplicate.Message);
    }

    private static SimulationLhAssetPlanLifecycleSnapshot Snapshot(
        string cell, string state, long revision, char lifecycleHash,
        char planHash = '7')
        => new()
        {
            CellStableId = cell,
            SourceWorldRevision = 31,
            SourceCombinedPlanHashSha256 = new string(planHash, 64),
            ExteriorPlacementPlanHashSha256 = new string('8', 64),
            InteriorPlacementPlanHashSha256 = new string('6', 64),
            LifecycleStateCode = state,
            LifecycleRevision = revision,
            PlacementContentFrozen = true,
            PresentationOnly = true,
            LifecycleHashSha256 = new string(lifecycleHash, 64),
        };
}
