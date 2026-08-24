using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class Simulation소비자원효과규칙Tests
{
    private readonly Simulation소비자원효과규칙 rule = new();
    private readonly Simulation자원효과적용기 applicator = new();

    [Fact]
    public void 주문예약은_시장가용20kg을예약원장으로옮기고총량을보존한다()
    {
        var result = rule.CreateReservation(ReservationRequest());
        var lines = result.PendingEffectBundle.Lines;

        Assert.Equal(Simulation업무규칙영역Codes.Market,
            result.PendingEffectBundle.RuleDomainCode);
        Assert.Equal(-20m, Assert.Single(lines, value =>
            value.RoleCode == Simulation자원효과역할Codes.Available).Delta);
        Assert.Equal(20m, Assert.Single(lines, value =>
            value.RoleCode == Simulation자원효과역할Codes.Reserved).Delta);
        Assert.Equal(0m, lines.Sum(value => value.ConservationQuantity));
    }

    [Fact]
    public void 주문이행은_예약20kg을주민수령량으로옮기고시장재고를한번만차감한다()
    {
        var result = rule.CreateFulfillment(FulfillmentRequest());
        var lines = result.PendingEffectBundle.Lines;

        Assert.Equal(Simulation업무규칙영역Codes.Market,
            result.PendingEffectBundle.RuleDomainCode);
        Assert.Equal(-20m, Assert.Single(lines, value =>
            value.ResourceTypeCode == "MarketReservedStock").Delta);
        Assert.Equal(20m, Assert.Single(lines, value =>
            value.ResourceTypeCode == "ResidentReceivedStock").Delta);
        Assert.DoesNotContain(lines, value => value.ResourceTypeCode == "MarketAvailableStock");
    }

    [Fact]
    public void 실제소비는_주민수령량과소비누계만바꾸고시장원장을건드리지않는다()
    {
        var result = rule.CreateConsumption(ConsumptionRequest());
        var lines = result.PendingEffectBundle.Lines;

        Assert.Equal(Simulation업무규칙영역Codes.Consumption,
            result.PendingEffectBundle.RuleDomainCode);
        Assert.Equal(-20m, Assert.Single(lines, value =>
            value.ResourceTypeCode == "ResidentReceivedStock").Delta);
        Assert.Equal(20m, Assert.Single(lines, value =>
            value.ResourceTypeCode == "ResidentConsumptionCumulative").Delta);
        Assert.DoesNotContain(lines, value => value.ResourceTypeCode.StartsWith("Market"));
    }

    [Fact]
    public void 예약_이행_소비를순서대로적용하면_시장가용280kg_예약0kg_주민소비20kg이된다()
    {
        var state = new Simulation자원원장상태Snapshot
        {
            Revision = 1,
            WorldTick = 10,
            Ledgers = new[]
            {
                new Simulation자원원장항목Snapshot
                {
                    LedgerStableId = "market-stock:potato.available",
                    ResourceTypeCode = "MarketAvailableStock",
                    ProductStableId = "product:potato",
                    Value = 300m,
                    UnitCode = "kg",
                    SourceStableIds = new[] { "source:market-stock.fixture" },
                },
            },
            SourceStableIds = new[] { "session:consumption.fixture" },
        };

        state = applicator.Apply(state,
            rule.CreateReservation(ReservationRequest()).PendingEffectBundle, 10).State;
        state = applicator.Apply(state,
            rule.CreateFulfillment(FulfillmentRequest()).PendingEffectBundle, 11).State;
        state = applicator.Apply(state,
            rule.CreateConsumption(ConsumptionRequest()).PendingEffectBundle, 12).State;

        Assert.Equal(280m, Ledger(state, "market-stock:potato.available").Value);
        Assert.Equal(0m, Ledger(state, "market-stock:potato.reserved.order-1").Value);
        Assert.Equal(0m, Ledger(state, "resident-stock:resident-1.potato.received").Value);
        Assert.Equal(20m, Ledger(state, "resident-consumption:resident-1.potato").Value);
    }

    [Fact]
    public void 소비단계에서시장재고추가차감표시가있으면차단한다()
    {
        var request = ConsumptionRequest();
        request.Consumption.AdditionalMarketSupplyDeductionApplied = true;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateConsumption(request));

        Assert.Equal("SimulationConsumptionCompletionStateInvalid", error.ErrorCode);
    }

    [Fact]
    public void 소비시점시장잔여가주문이행후값과다르면차단한다()
    {
        var request = ConsumptionRequest();
        request.Consumption.MarketSupplyObservedAtConsumption = 260m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateConsumption(request));

        Assert.Equal("SimulationConsumptionCompletionStateInvalid", error.ErrorCode);
    }

    [Fact]
    public void 주문과예약수량이불일치하면효과를만들지않는다()
    {
        var request = ReservationRequest();
        request.Reservation.Quantity = 19m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateReservation(request));

        Assert.Equal("SimulationConsumptionOrderReservationMismatch", error.ErrorCode);
    }

    [Fact]
    public void 수령준비전주문은이행효과를만들지않는다()
    {
        var request = FulfillmentRequest();
        request.Order.StateCode = SimulationIndividualOrderStateCodes.StockReserved;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateFulfillment(request));

        Assert.Equal("SimulationConsumptionFulfillmentStateInvalid", error.ErrorCode);
    }

    [Fact]
    public void 소비완료전상태는소비효과를만들지않는다()
    {
        var request = ConsumptionRequest();
        request.Consumption.StateCode = Simulation시장소비StateCodes.Scheduled;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateConsumption(request));

        Assert.Equal("SimulationConsumptionCompletionStateInvalid", error.ErrorCode);
    }

    private static Simulation주문예약자원효과Request ReservationRequest()
        => new()
        {
            EffectBundleStableId = "effect-bundle:order-reservation.order-1",
            AvailableEffectLineStableId = "effect-line:order-reservation.available.order-1",
            ReservedEffectLineStableId = "effect-line:order-reservation.reserved.order-1",
            AvailableLedgerStableId = "market-stock:potato.available",
            ReservedLedgerStableId = "market-stock:potato.reserved.order-1",
            AvailableBeforeReservation = 300m,
            Order = Order(SimulationIndividualOrderStateCodes.StockReserved),
            Reservation = Reservation(SimulationStockReservationStateCodes.Reserved),
            SourceStableIds = new[] { "source:consumption-rule.fixture" },
        };

    private static Simulation주문이행자원효과Request FulfillmentRequest()
        => new()
        {
            EffectBundleStableId = "effect-bundle:order-fulfillment.order-1",
            ReservedEffectLineStableId = "effect-line:order-fulfillment.reserved.order-1",
            ResidentReceivedEffectLineStableId = "effect-line:order-fulfillment.received.order-1",
            ReservedLedgerStableId = "market-stock:potato.reserved.order-1",
            ResidentReceivedLedgerStableId = "resident-stock:resident-1.potato.received",
            ResidentReceivedBeforeFulfillment = 0m,
            Order = Order(SimulationIndividualOrderStateCodes.ReadyForPickup),
            Reservation = Reservation(SimulationStockReservationStateCodes.Consumed),
            SourceStableIds = new[] { "source:consumption-rule.fixture" },
        };

    private static Simulation주민소비자원효과Request ConsumptionRequest()
        => new()
        {
            EffectBundleStableId = "effect-bundle:resident-consumption.order-1",
            ResidentReceivedEffectLineStableId = "effect-line:resident-consumption.input.order-1",
            ConsumptionRecordEffectLineStableId = "effect-line:resident-consumption.record.order-1",
            ResidentReceivedLedgerStableId = "resident-stock:resident-1.potato.received",
            ConsumptionRecordLedgerStableId = "resident-consumption:resident-1.potato",
            ResidentReceivedBeforeConsumption = 20m,
            ConsumptionRecordBefore = 0m,
            Order = Order(SimulationIndividualOrderStateCodes.Consumed),
            Consumption = Consumption(),
            SourceStableIds = new[] { "source:consumption-rule.fixture" },
        };

    private static SimulationIndividualOrderSnapshot Order(string state)
        => new()
        {
            OrderStableId = "order:sim.potato-20kg-1",
            StateCode = state,
            Revision = state == SimulationIndividualOrderStateCodes.StockReserved ? 1 : 3,
            ActorStableId = "resident:sim-1",
            ProductStableId = "product:potato",
            MarketFacilityStableId = "facility:market-1",
            OrderedQuantity = 20m,
            FulfilledQuantity = state == SimulationIndividualOrderStateCodes.StockReserved ? 0m : 20m,
            UnitCode = "kg",
            DecisionStableId = "decision:individual-order.order-1",
            TaskStableId = "task:individual-order.order-1",
            ReservedTick = 10,
            ReadyForPickupTick = state == SimulationIndividualOrderStateCodes.StockReserved ? null : 11,
            ConsumptionDecisionStableId = state == SimulationIndividualOrderStateCodes.Consumed
                ? "decision:market-consumption.order-1" : null,
            ConsumptionTaskStableId = state == SimulationIndividualOrderStateCodes.Consumed
                ? "task:market-consumption.order-1" : null,
            ConsumedTick = state == SimulationIndividualOrderStateCodes.Consumed ? 12 : null,
            SourceStableIds = new[] { "source:individual-order.fixture" },
        };

    private static SimulationStockReservationSnapshot Reservation(string state)
        => new()
        {
            ReservationStableId = "reservation:order:sim.potato-20kg-1",
            OrderStableId = "order:sim.potato-20kg-1",
            MarketFacilityStableId = "facility:market-1",
            ProductStableId = "product:potato",
            Quantity = 20m,
            UnitCode = "kg",
            StateCode = state,
            ReservedTick = 10,
            ConsumedTick = state == SimulationStockReservationStateCodes.Consumed ? 11 : null,
            SourceStableIds = new[] { "source:stock-reservation.fixture" },
        };

    private static Simulation시장소비Snapshot Consumption()
        => new()
        {
            ConsumptionStableId = "market-consumption:sim.potato-20kg-1",
            OrderStableId = "order:sim.potato-20kg-1",
            ReservationStableId = "reservation:order:sim.potato-20kg-1",
            ActorStableId = "resident:sim-1",
            ProductStableId = "product:potato",
            MarketFacilityStableId = "facility:market-1",
            Quantity = 20m,
            UnitCode = "kg",
            StateCode = Simulation시장소비StateCodes.Consumed,
            Revision = 2,
            DecisionStableId = "decision:market-consumption.order-1",
            TaskStableId = "task:market-consumption.order-1",
            ScheduledTick = 11,
            ConsumedTick = 12,
            MarketSupplyAfterOrderFulfillment = 280m,
            MarketSupplyObservedAtConsumption = 280m,
            AdditionalMarketSupplyDeductionApplied = false,
            SourceStableIds = new[] { "source:market-consumption.fixture" },
        };

    private static Simulation자원원장항목Snapshot Ledger(
        Simulation자원원장상태Snapshot state,
        string stableId)
        => Assert.Single(state.Ledgers, value => value.LedgerStableId == stableId);
}
