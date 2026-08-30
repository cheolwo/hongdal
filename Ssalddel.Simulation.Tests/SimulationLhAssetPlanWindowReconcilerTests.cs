using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Q338 LH Window 역할·캐시 용량·결정적 해제를 회귀 검증한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata
        .SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    Boundary = "Headless 시험은 실제 이동·Scene·Game View 증거가 아니다.")]
public sealed class SimulationLhAssetPlanWindowReconcilerTests
{
    [Fact]
    public void Window역할은_Active_Prepared_Cached_Released로결정적으로전이된다()
    {
        var lifecycle = new SimulationLhAssetPlanLifecycleService();
        var detail = Prepared(lifecycle, "cell:detail");
        var prefetch = Prepared(lifecycle, "cell:prefetch");
        var outsideA = Active(lifecycle, "cell:outside:a");
        var outsideB = Active(lifecycle, "cell:outside:b");
        var preview = Preview(
            Cell("cell:detail", SimulationLhWorldCodes.Detail),
            Cell("cell:prefetch", SimulationLhWorldCodes.Prefetch));
        var reconciler = new SimulationLhAssetPlanWindowReconciler(lifecycle);

        var result = reconciler.Reconcile(
            new[] { outsideB, outsideA },
            new[] { prefetch, detail }, preview, cachedCellCapacity: 1);

        Assert.Equal(SimulationLhAssetPlanLifecycleCodes.Active,
            Find(result, "cell:detail").LifecycleStateCode);
        Assert.Equal(SimulationLhAssetPlanLifecycleCodes.Prepared,
            Find(result, "cell:prefetch").LifecycleStateCode);
        Assert.Equal(SimulationLhAssetPlanLifecycleCodes.Cached,
            Find(result, "cell:outside:a").LifecycleStateCode);
        Assert.Equal(SimulationLhAssetPlanLifecycleCodes.Released,
            Find(result, "cell:outside:b").LifecycleStateCode);
        Assert.Equal(64, result.ReconcileHashSha256.Length);
    }

    [Fact]
    public void 입력순서가달라도_같은전이Hash를만든다()
    {
        var lifecycle = new SimulationLhAssetPlanLifecycleService();
        var one = Prepared(lifecycle, "cell:one");
        var two = Prepared(lifecycle, "cell:two");
        var preview = Preview(
            Cell("cell:one", SimulationLhWorldCodes.Active),
            Cell("cell:two", SimulationLhWorldCodes.Prefetch));
        var reconciler = new SimulationLhAssetPlanWindowReconciler(lifecycle);

        var first = reconciler.Reconcile(Array.Empty<
                SimulationLhAssetPlanLifecycleSnapshot>(),
            new[] { one, two }, preview, 2);
        preview.Cells = preview.Cells.Reverse().ToArray();
        var second = reconciler.Reconcile(Array.Empty<
                SimulationLhAssetPlanLifecycleSnapshot>(),
            new[] { two, one }, preview, 2);

        Assert.Equal(first.ReconcileHashSha256,
            second.ReconcileHashSha256);
        Assert.Equal(first.Cells.Select(value => value.CellStableId),
            second.Cells.Select(value => value.CellStableId));
    }

    [Fact]
    public void 같은Cell의계획Identity가바뀌면_조용히교체하지않는다()
    {
        var lifecycle = new SimulationLhAssetPlanLifecycleService();
        var current = Prepared(lifecycle, "cell:stable");
        var incoming = Prepared(lifecycle, "cell:stable", "changed");

        var error = Assert.Throws<InvalidOperationException>(() =>
            new SimulationLhAssetPlanWindowReconciler(lifecycle).Reconcile(
                new[] { current }, new[] { incoming },
                Preview(Cell("cell:stable", SimulationLhWorldCodes.Detail)),
                1));

        Assert.Equal("SimulationLhAssetPlanIdentityChanged", error.Message);
    }

    private static SimulationLhAssetPlanLifecycleSnapshot Active(
        SimulationLhAssetPlanLifecycleService service, string cell)
    {
        var prepared = Prepared(service, cell);
        return service.Transition(prepared,
            new SimulationLhAssetPlanLifecycleTransitionRequest
            {
                ExpectedLifecycleRevision = prepared.LifecycleRevision,
                TargetStateCode = SimulationLhAssetPlanLifecycleCodes.Active,
            });
    }

    private static SimulationLhAssetPlanLifecycleSnapshot Prepared(
        SimulationLhAssetPlanLifecycleService service, string cell,
        string variant = "base")
    {
        var combinedHash = Simulation세계자산CanonicalHash.Hash(
            cell + ":" + variant + ":combined");
        return service.Prepare(new Simulation분리세계자산배치Result
        {
            CompatibilityPlan = new Simulation세계자산배치Plan
            {
                CellStableId = cell,
                SourceWorldRevision = 12,
                AssetPlacementPlanHashSha256 = combinedHash,
            },
            ExteriorPlan = new Simulation실외자산배치Plan
            {
                CellStableId = cell,
                SourceWorldRevision = 12,
                SourceCombinedPlanHashSha256 = combinedHash,
                ExteriorPlacementPlanHashSha256 =
                    Simulation세계자산CanonicalHash.Hash(
                        cell + ":" + variant + ":exterior"),
            },
            InteriorPlan = new Simulation실내자산배치Plan
            {
                CellStableId = cell,
                SourceWorldRevision = 12,
                SourceCombinedPlanHashSha256 = combinedHash,
                InteriorPlacementPlanHashSha256 =
                    Simulation세계자산CanonicalHash.Hash(
                        cell + ":" + variant + ":interior"),
            },
        });
    }

    private static SimulationLhCellPreviewResponse Preview(
        params SimulationLhCellPlanResponse[] cells)
        => new()
        {
            RequestEpoch = "epoch:lh-window:test",
            WorldRevision = 12,
            Cells = cells,
        };

    private static SimulationLhCellPlanResponse Cell(
        string cell, string role)
        => new()
        {
            CellKey = cell,
            WindowRoleCode = role,
        };

    private static SimulationLhAssetPlanLifecycleSnapshot Find(
        SimulationLhAssetPlanWindowReconcileResult result, string cell)
        => result.Cells.Single(value => value.CellStableId == cell);
}
