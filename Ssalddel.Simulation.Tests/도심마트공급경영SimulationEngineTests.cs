using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 도심마트공급경영SimulationEngineTests
{
    [Fact]
    public void 감자Fixture는_28개Tick과HardDemand보존식을만든다()
    {
        var result = 도심마트감자공급경영SimulationEngineFixture.Run();

        Assert.Equal(28, result.Ticks.Length);
        Assert.Equal(2105m, result.HardDemandQuantity);
        Assert.Equal(
            result.HardDemandQuantity,
            result.FulfilledQuantity + result.UnfulfilledQuantity);
        Assert.Equal(3, result.SourceLineage.Length);
    }

    [Fact]
    public void 집단확정은주문이되지만_집단의향은주문이되지않는다()
    {
        var result = 도심마트감자공급경영SimulationEngineFixture.Run();
        var groupOrder = Assert.Single(result.Orders, order =>
            order.DemandSourceTypeCode == SimulationDemandSourceTypeCodes.GroupConfirmedDemand);

        Assert.Equal(385m, groupOrder.RequestedQuantity);
        Assert.Equal(7, groupOrder.CreatedTick);
        Assert.Equal(27, groupOrder.FulfillmentDueTick);
        Assert.DoesNotContain(result.Orders, order =>
            order.DemandSourceTypeCode == SimulationDemandSourceTypeCodes.GroupIntentDemand);
    }

    [Fact]
    public void 재고는_초기재고와검수입고에서충족폐기잔량을뺀값이다()
    {
        var rule = 도심마트감자공급경영SimulationEngineFixture.Rule();
        var result = Run(rule);

        Assert.Equal(
            rule.InitialInventoryQuantity + result.DeliveredQuantity,
            result.FulfilledQuantity + result.WasteQuantity + result.EndingInventoryQuantity);
        Assert.All(result.Ticks, tick => Assert.InRange(
            tick.ClosingInventory, 0m, rule.StorageCapacity));
    }

    [Fact]
    public void 입고는작업Capacity를넘지않고_초과분은거부수량으로남는다()
    {
        var rule = 도심마트감자공급경영SimulationEngineFixture.Rule();
        rule.ReceivingWorkCapacityPerTick = 20m;
        var result = Run(rule);

        Assert.All(result.Ticks, tick => Assert.InRange(
            tick.ReceivingWorkload, 0m, rule.ReceivingWorkCapacityPerTick));
        Assert.True(result.RejectedDeliveryQuantity > 0m);
        Assert.Equal(
            result.Deliveries.Sum(delivery => delivery.PlannedQuantity),
            result.DeliveredQuantity + result.RejectedDeliveryQuantity);
    }

    [Fact]
    public void 현금은음수가되지않고_미지급금은별도로남는다()
    {
        var rule = 도심마트감자공급경영SimulationEngineFixture.Rule();
        rule.InitialCash = 0m;
        var result = Run(rule);

        Assert.Equal(0m, result.EndingCash);
        Assert.Equal(result.PurchaseCost, result.OutstandingPaymentAmount);
        Assert.All(result.Ticks, tick => Assert.True(tick.ClosingCash >= 0m));
    }

    [Fact]
    public void 공급처별수량비중과운송비포함비용을별도원장으로남긴다()
    {
        var rule = 도심마트감자공급경영SimulationEngineFixture.Rule();
        var result = Run(rule);

        Assert.Equal(3, result.SupplierResults.Length);
        Assert.Equal(result.DeliveredQuantity,
            result.SupplierResults.Sum(supplier => supplier.AcceptedQuantity));
        Assert.Equal(result.PurchaseCost,
            result.SupplierResults.Sum(supplier => supplier.PurchaseCost));
        Assert.InRange(
            result.SupplierResults.Sum(supplier => supplier.AcceptedSupplyShareRate),
            0.999999m,
            1.000001m);
        Assert.True(result.PurchaseCost
            > result.Deliveries.Sum(delivery => delivery.AcceptedQuantity *
                도심마트감자공급SimulationFixture.CreateSupplySnapshot().ContractDrafts
                    .Single(draft => draft.ContractDraftStableId == delivery.ContractDraftStableId)
                    .UnitPrice));
    }

    [Fact]
    public void 같은입력과Rule은_같은결과를만든다()
    {
        var first = 도심마트감자공급경영SimulationEngineFixture.Run();
        var second = 도심마트감자공급경영SimulationEngineFixture.Run();

        Assert.Equal(first.SimulationRevision, second.SimulationRevision);
        Assert.Equal(ResultKey(first), ResultKey(second));
        Assert.Equal(
            first.Deliveries.Select(DeliveryKey),
            second.Deliveries.Select(DeliveryKey));
    }

    [Fact]
    public void 계약안허용량을넘는공급계획은거부한다()
    {
        var rule = 도심마트감자공급경영SimulationEngineFixture.Rule();
        rule.SelectedSupplyPlans[0].WeeklyQuantity = 101m;

        AssertError("SupplyEngineWeeklyQuantityInvalid", () => Run(rule));
    }

    [Fact]
    public void 다른Session이나품질판매기한누락은거부한다()
    {
        var supply = 도심마트감자공급SimulationFixture.CreateSupplySnapshot();
        supply.SessionStableId = "simulation-session:other";
        AssertError("SupplyEngineSessionMismatch", () => new 도심마트공급경영SimulationEngine().Run(
            supply,
            도심마트감자기본방문주문SimulationFixture.Create(),
            도심마트감자수요CompositionSimulationFixture.Create(),
            도심마트감자공급경영SimulationEngineFixture.Rule()));

        var rule = 도심마트감자공급경영SimulationEngineFixture.Rule();
        rule.QualityShelfLives = rule.QualityShelfLives.Take(2).ToArray();
        AssertError("SupplyEngineShelfLifeRuleInvalid", () => Run(rule));
    }

    private static 도심마트공급경영SimulationWorldState Run(
        도심마트공급경영SimulationEngineRule rule)
        => new 도심마트공급경영SimulationEngine().Run(
            도심마트감자공급SimulationFixture.CreateSupplySnapshot(),
            도심마트감자기본방문주문SimulationFixture.Create(),
            도심마트감자수요CompositionSimulationFixture.Create(),
            rule);

    private static string ResultKey(도심마트공급경영SimulationWorldState result)
        => result.HardDemandQuantity + "|" + result.FulfilledQuantity + "|"
            + result.UnfulfilledQuantity + "|" + result.DeliveredQuantity + "|"
            + result.RejectedDeliveryQuantity + "|" + result.WasteQuantity + "|"
            + result.EndingInventoryQuantity + "|" + result.PurchaseCost + "|"
            + result.EndingCash + "|" + result.OutstandingPaymentAmount;

    private static string DeliveryKey(도심마트납품SimulationResult delivery)
        => delivery.DeliveryStableId + "|" + delivery.PlannedTick + "|"
            + delivery.ArrivalTick + "|" + delivery.PlannedQuantity + "|"
            + delivery.AcceptedQuantity + "|" + delivery.RejectedQuantity + "|"
            + delivery.PaymentDueTick + "|" + delivery.PaymentAmount;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
