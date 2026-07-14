using Hongdal.Contracts.Common.TraditionalMarkets;
using Hongdal.Domain.TraditionalMarkets;

namespace Hongdal.Tests.Domain.TraditionalMarkets;

public sealed class TraditionalMarketLogisticsHubPolicyTests
{
    [Fact]
    public void 시범운영은_검토상태에서만_진입할수있다()
    {
        Assert.True(TraditionalMarketLogisticsHubPolicy.CanTransition(
            TraditionalMarketLogisticsHubStatuses.UnderReview,
            TraditionalMarketLogisticsHubStatuses.Pilot));
        Assert.False(TraditionalMarketLogisticsHubPolicy.CanTransition(
            TraditionalMarketLogisticsHubStatuses.Candidate,
            TraditionalMarketLogisticsHubStatuses.Pilot));
        Assert.False(TraditionalMarketLogisticsHubPolicy.CanTransition(
            TraditionalMarketLogisticsHubStatuses.UnderReview,
            TraditionalMarketLogisticsHubStatuses.Active));
    }

    [Fact]
    public void 운영준비는_동의_현장확인_입고_분류_인도방식_용량_생활권반경을_요구한다()
    {
        var hub = ReadyHub();

        Assert.Null(TraditionalMarketLogisticsHubPolicy.GetReadinessError(hub));

        hub.HasOperatorConsent = false;
        Assert.Contains("동의", TraditionalMarketLogisticsHubPolicy.GetReadinessError(hub));

        hub = ReadyHub();
        hub.SupportsResidentPickup = false;
        hub.SupportsLastMileDelivery = false;
        Assert.Contains("주민 수령", TraditionalMarketLogisticsHubPolicy.GetReadinessError(hub));
    }

    [Fact]
    public void 활성거점은_바로종료하지않고_먼저중단해야한다()
    {
        Assert.True(TraditionalMarketLogisticsHubPolicy.CanTransition(
            TraditionalMarketLogisticsHubStatuses.Active,
            TraditionalMarketLogisticsHubStatuses.Paused));
        Assert.False(TraditionalMarketLogisticsHubPolicy.CanTransition(
            TraditionalMarketLogisticsHubStatuses.Active,
            TraditionalMarketLogisticsHubStatuses.Closed));
    }

    private static TraditionalMarketLogisticsHub ReadyHub()
        => new()
        {
            OperatorOrganizationName = "테스트시장 상인회",
            ServiceRadiusKm = 3,
            DailyGroupPurchaseCapacity = 100,
            SupportsBulkReceiving = true,
            SupportsSorting = true,
            SupportsResidentPickup = true,
            HasOperatorConsent = true,
            SiteVerifiedAtUtc = DateTime.UtcNow
        };
}
