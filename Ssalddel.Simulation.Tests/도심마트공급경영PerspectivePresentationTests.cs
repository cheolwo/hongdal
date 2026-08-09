using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class 도심마트공급경영PerspectivePresentationTests
{
    [Fact]
    public void 위험은단일점수가아닌Reason별근거로분리된다()
    {
        var perspective = 도심마트감자공급RiskSimulationFixture.Perspective();

        Assert.Contains(perspective.RiskItems, item =>
            item.ReasonCode == 도심마트공급RiskReasonCodes.SupplyCoverageGap
            && item.ObservedValue > 0m);
        Assert.Contains(perspective.RiskItems, item =>
            item.ReasonCode == 도심마트공급RiskReasonCodes.OrderFulfillmentGap);
        Assert.All(perspective.RiskItems, item => Assert.NotEmpty(item.EvidenceStableId));
    }

    [Fact]
    public void 위험Perspective는자동발주나계약확정Intent를제공하지않는다()
    {
        var intents = 도심마트감자공급RiskSimulationFixture.Perspective().IntentCodes;

        Assert.Contains("ReviewDemandAndOrders", intents);
        Assert.Contains("ReviewOrderFulfillmentRisk", intents);
        Assert.DoesNotContain(intents, value => value.Contains("Confirm", StringComparison.Ordinal));
        Assert.DoesNotContain(intents, value => value.Contains("OrderSupply", StringComparison.Ordinal));
    }

    [Fact]
    public void 주문브리핑은현재재고와예정입고를분리한다()
    {
        var surface = 도심마트감자공급RiskSimulationFixture.Presentation().DemandAndOrders;

        Assert.Equal(7, surface.AsOfTick);
        Assert.True(surface.TodayOrderCount >= 3);
        Assert.True(surface.TodayRequestedQuantity > 385m);
        Assert.True(surface.PendingOrderQuantity > 0m);
        Assert.True(surface.TodayScheduledInbound >= 0m);
        Assert.Equal(surface.PendingOrderQuantity,
            surface.ImmediatelyFulfillableQuantity
            + surface.InboundAfterProcessingPotentialQuantity
            + surface.CannotCoverQuantity);
    }

    [Fact]
    public void 브리핑은Simulation표시와한계를보존한다()
    {
        var surface = 도심마트감자공급RiskSimulationFixture.Presentation().DemandAndOrders;

        Assert.Equal("Simulation", surface.SimulationLabel);
        Assert.Contains("자동", surface.LimitationText);
        Assert.NotEmpty(surface.ReasonCodes);
    }

    [Fact]
    public void 관리Preview는비용현금폐기작업을별도Metric으로보존한다()
    {
        var preview = 도심마트감자공급RiskSimulationFixture.Presentation().ManagementPreview;

        Assert.Equal(2105m, preview.HardDemandQuantity);
        Assert.Equal(preview.HardDemandQuantity,
            preview.FulfilledQuantity + preview.UnfulfilledQuantity);
        Assert.True(preview.PurchaseCost > 0m);
        Assert.True(preview.EndingCash >= 0m);
        Assert.True(preview.ReceivingWorkload > 0m);
    }

    [Fact]
    public void 공급포트폴리오는공급처별비중과비용을보존한다()
    {
        var surface = 도심마트감자공급RiskSimulationFixture.Presentation().SupplyPortfolio;

        Assert.Equal(3, surface.Length);
        Assert.InRange(surface.Sum(value => value.AcceptedSupplyShareRate), 0.999999m, 1.000001m);
        Assert.All(surface, value => Assert.True(value.PurchaseCost >= 0m));
    }

    [Fact]
    public void 현금과납품은독립Surface배열로투영된다()
    {
        var presentation = 도심마트감자공급RiskSimulationFixture.Presentation();

        Assert.Equal(28, presentation.CashSchedule.Length);
        Assert.NotEmpty(presentation.DeliveryCommitments);
        Assert.All(presentation.DeliveryCommitments, delivery =>
            Assert.True(delivery.PlannedQuantity
                == delivery.AcceptedQuantity + delivery.RejectedQuantity));
    }

    [Fact]
    public void 같은입력과AsOfTick은같은PresentationRevision을만든다()
    {
        var first = 도심마트감자공급RiskSimulationFixture.Presentation(7);
        var second = 도심마트감자공급RiskSimulationFixture.Presentation(7);

        Assert.Equal(first.PresentationRevision, second.PresentationRevision);
        Assert.Equal(first.DemandAndOrders.PendingOrderQuantity,
            second.DemandAndOrders.PendingOrderQuantity);
        Assert.Equal(3, first.SourceLineage.Length);
    }
}
