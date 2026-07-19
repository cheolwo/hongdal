using Ssalddel.Services.LogisticsProcessing.Warehouse;

namespace Ssalddel.Tests.Services.LogisticsProcessing.Warehouse;

public sealed class WarehouseSelectionPolicyTests
{
    [Fact]
    public void ServiceAreaPolicy_matches_when_region_tokens_overlap()
    {
        var policy = new WarehouseServiceAreaPolicy();

        var result = policy.IsInServiceArea(
            "서울특별시 송파구 문정동 100",
            "서울특별시 송파구 가락동 200");

        Assert.True(result);
    }

    [Fact]
    public void ServiceAreaPolicy_does_not_match_when_region_tokens_differ()
    {
        var policy = new WarehouseServiceAreaPolicy();

        var result = policy.IsInServiceArea(
            "경기도 수원시 영통구",
            "서울특별시 송파구 문정동");

        Assert.False(result);
    }

    [Fact]
    public void DistanceCostEstimator_returns_distance_and_cost_when_coordinates_exist()
    {
        var estimator = new WarehouseDistanceCostEstimator();

        var estimate = estimator.Estimate(
            37.5665m,
            126.9780m,
            37.5013m,
            127.0396m);

        Assert.NotNull(estimate.DistanceKm);
        Assert.NotNull(estimate.EstimatedTransportCost);
        Assert.True(estimate.DistanceKm > 0);
        Assert.True(estimate.EstimatedTransportCost > 3000m);
    }

    [Fact]
    public void DistanceCostEstimator_returns_empty_estimate_without_coordinates()
    {
        var estimator = new WarehouseDistanceCostEstimator();

        var estimate = estimator.Estimate(null, null, 37.5013m, 127.0396m);

        Assert.Null(estimate.DistanceKm);
        Assert.Null(estimate.EstimatedTransportCost);
    }
}
