using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Q338 통합 계획 분리의 결정성·identity 보존·입력 거부를 회귀 검증한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata
        .SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    Boundary = "단위 시험은 실제 World 배치나 Runtime 표현 증거가 아니다.")]
public sealed class SimulationWorldAssetPlacementPlanPartitioningTests
{
    [Fact]
    public void 입력배열순서가달라도_실외실내인계Hash와정렬은같다()
    {
        var first = Plan(reverse: false);
        var reversed = Plan(reverse: true);
        var service = new Simulation결정적세계자산배치Plan분리Service();

        var left = service.Partition(first);
        var right = service.Partition(reversed);

        Assert.Equal(left.ExteriorPlan.ExteriorPlacementPlanHashSha256,
            right.ExteriorPlan.ExteriorPlacementPlanHashSha256);
        Assert.Equal(left.InteriorPlan.InteriorPlacementPlanHashSha256,
            right.InteriorPlan.InteriorPlacementPlanHashSha256);
        Assert.Equal(new[] { "placement:building", "placement:tree" },
            left.ExteriorPlan.Placements.Select(value =>
                value.PlacementStableId));
        Assert.Equal(new[] { "placement:interior:a", "placement:interior:b" },
            left.InteriorPlan.OverlayPlacements.Select(value =>
                value.PlacementStableId));
        Assert.Same(first, left.CompatibilityPlan);
    }

    [Fact]
    public void 분리Service는_봉인되지않은통합계획을거부한다()
    {
        var source = Plan(reverse: false);
        source.AssetPlacementPlanHashSha256 = "not-a-hash";

        var error = Assert.Throws<ArgumentException>(() =>
            new Simulation결정적세계자산배치Plan분리Service()
                .Partition(source));

        Assert.Equal("source", error.ParamName);
        Assert.Contains("SimulationWorldAssetPlacementPlanPartitionInputInvalid",
            error.Message, StringComparison.Ordinal);
    }

    private static Simulation세계자산배치Plan Plan(bool reverse)
    {
        var placements = new[]
        {
            Placement("placement:tree",
                Simulation세계자산배치Codes.Environment),
            Placement("placement:interior:b",
                Simulation세계자산배치Codes.InteriorOverlay),
            Placement("placement:building",
                Simulation세계자산배치Codes.Building),
            Placement("placement:interior:a",
                Simulation세계자산배치Codes.InteriorOverlay),
        };
        if (reverse) Array.Reverse(placements);
        return new Simulation세계자산배치Plan
        {
            CellStableId = "kr5186:l3:2801:4581",
            SourceWorldRevision = 19,
            AssetPlacementPlanHashSha256 =
                Simulation세계자산CanonicalHash.Hash("combined-plan"),
            Placements = placements,
            InteriorPlanHandles = new[]
            {
                new SimulationInteriorPlanHandleSnapshot
                {
                    BuildingPlacementStableId = "placement:building",
                    InteriorPlacementPlanHashSha256 =
                        Simulation세계자산CanonicalHash.Hash("interior-plan"),
                },
            },
            InteriorPlanBodies = new[]
            {
                new SimulationInteriorPlacementPlanBodySnapshot
                {
                    BuildingPlacementStableId = "placement:building",
                    BodyHashSha256 =
                        Simulation세계자산CanonicalHash.Hash("interior-body"),
                },
            },
        };
    }

    private static Simulation세계자산PlacementSnapshot Placement(
        string stableId, string kind)
        => new()
        {
            PlacementStableId = stableId,
            OwnerCellStableId = "kr5186:l3:2801:4581",
            PlacementKindCode = kind,
            CompositionKey = "test:" + stableId,
            UniformScale = 1d,
        };
}
