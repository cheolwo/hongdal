using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation시장소비Tests
{
    [Fact]
    public void 수령준비전소비는_Preview에서차단된다()
    {
        var context = CreateContext();
        var ordered = ConfirmOrder(context);
        var order = Assert.Single(ordered.IndividualOrders);

        var preview = context.Service.PreviewMarketConsumption(
            context.Session.SessionStableId,
            Consumption(order.Revision));

        Assert.Contains("SimulationIndividualOrderNotReadyForConsumption", preview.BlockReasonCodes);
        Assert.Contains("SimulationStockReservationNotConsumed", preview.BlockReasonCodes);
        Assert.Empty(ordered.MarketConsumptions);
    }

    [Fact]
    public void Preview는_주문에서이미차감된시장잔여재고를_다시차감하지않는다()
    {
        var context = ReadyOrder();
        var order = Assert.Single(context.Ready.IndividualOrders);

        var preview = context.Service.PreviewMarketConsumption(
            context.Session.SessionStableId,
            Consumption(order.Revision));
        var unchanged = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal(20m, preview.ConsumptionQuantity);
        Assert.Equal(280m, preview.MarketSupplyAfterOrderFulfillment);
        Assert.Equal(280m, preview.MarketSupplyAfterConsumption);
        Assert.False(preview.AdditionalMarketSupplyDeductionRequired);
        Assert.Empty(preview.BlockReasonCodes);
        Assert.Empty(unchanged.MarketConsumptions);
        Assert.Equal(280m, Assert.Single(unchanged.Settlement!.MarketSupplyByProduct).Quantity);
    }

    [Fact]
    public void Confirm은_소비Task와주문계보를예약하지만_시장재고를변경하지않는다()
    {
        var context = ReadyOrder();
        var order = Assert.Single(context.Ready.IndividualOrders);

        var scheduled = ConfirmConsumption(context, order.Revision);
        var consumption = Assert.Single(scheduled.MarketConsumptions);
        var scheduledOrder = Assert.Single(scheduled.IndividualOrders);

        Assert.Equal(Simulation시장소비StateCodes.Scheduled, consumption.StateCode);
        Assert.Equal(order.OrderStableId, consumption.OrderStableId);
        Assert.Equal(Assert.Single(scheduled.StockReservations).ReservationStableId,
            consumption.ReservationStableId);
        Assert.Equal(SimulationIndividualOrderStateCodes.ConsumptionScheduled,
            scheduledOrder.StateCode);
        Assert.Equal(consumption.TaskStableId, scheduledOrder.ConsumptionTaskStableId);
        Assert.Equal(280m, Assert.Single(scheduled.Settlement!.MarketSupplyByProduct).Quantity);
        Assert.Empty(scheduled.Settlement.ResidentConsumptionByProduct);
    }

    [Fact]
    public void 완료Tick은_주민소비20kg과시장잔여280kg을같은경제Snapshot에수렴한다()
    {
        var context = ReadyOrder();
        var order = Assert.Single(context.Ready.IndividualOrders);
        var scheduled = ConfirmConsumption(context, order.Revision);

        var completed = Advance(context, scheduled, "command:tick.market-consumption-1");
        var consumption = Assert.Single(completed.MarketConsumptions);
        var summary = Assert.Single(completed.Settlement!.ResidentConsumptionByProduct);

        Assert.Equal(Simulation시장소비StateCodes.Consumed, consumption.StateCode);
        Assert.Equal(SimulationIndividualOrderStateCodes.Consumed,
            Assert.Single(completed.IndividualOrders).StateCode);
        Assert.Equal(20m, summary.Quantity);
        Assert.Equal(1, summary.ConsumptionCount);
        Assert.Equal("product:potato", summary.ProductStableId);
        Assert.Equal(280m, Assert.Single(completed.Settlement.MarketSupplyByProduct).Quantity);
        Assert.Equal(280m, consumption.MarketSupplyObservedAtConsumption);
        Assert.False(consumption.AdditionalMarketSupplyDeductionApplied);
    }

    [Fact]
    public void 다른주문자는소비를확정할수없고_같은주문을중복소비할수없다()
    {
        var context = ReadyOrder();
        var order = Assert.Single(context.Ready.IndividualOrders);
        var wrongActor = Consumption(order.Revision);
        wrongActor.ActorStableId = "resident:sim.household-2";

        var blocked = context.Service.PreviewMarketConsumption(
            context.Session.SessionStableId, wrongActor);
        Assert.Contains("SimulationIndividualOrderActorMismatch", blocked.BlockReasonCodes);

        var scheduled = ConfirmConsumption(context, order.Revision);
        var completed = Advance(context, scheduled, "command:tick.market-consumption-duplicate");
        var consumedOrder = Assert.Single(completed.IndividualOrders);
        var duplicate = Consumption(consumedOrder.Revision);
        duplicate.ConsumptionStableId = "market-consumption:sim.potato-duplicate";
        var duplicatePreview = context.Service.PreviewMarketConsumption(
            context.Session.SessionStableId, duplicate);

        Assert.Contains("SimulationIndividualOrderNotReadyForConsumption", duplicatePreview.BlockReasonCodes);
        Assert.Contains("SimulationIndividualOrderAlreadyConsumed", duplicatePreview.BlockReasonCodes);
        Assert.Single(completed.MarketConsumptions);
    }

    [Fact]
    public void SaveReplay는_주문예약시장재고주민소비계보를동일하게복원한다()
    {
        var context = ReadyOrder();
        var order = Assert.Single(context.Ready.IndividualOrders);
        var scheduled = ConfirmConsumption(context, order.Revision);
        var completed = Advance(context, scheduled, "command:tick.market-consumption-save");
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.market-consumption-1",
                ExpectedRevision = completed.Revision,
            });

        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        Assert.Equal(SimulationIndividualOrderStateCodes.Consumed,
            Assert.Single(restored.Session.IndividualOrders).StateCode);
        Assert.Equal(SimulationStockReservationStateCodes.Consumed,
            Assert.Single(restored.Session.StockReservations).StateCode);
        Assert.Equal(20m,
            Assert.Single(restored.Session.Settlement!.ResidentConsumptionByProduct).Quantity);
        Assert.Equal(280m,
            Assert.Single(restored.Session.Settlement.MarketSupplyByProduct).Quantity);
    }

    private static Context CreateContext()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore);
        var session = service.Create(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.market-consumption-1",
            ScenarioDataRevision = "scenario-data:potato-market-r1",
            ScenarioSeed = 20260811,
            RuleRevision = "workflow-rules.v1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim.farmers-1",
                TerritoryStableId = "territory:sim.farm-region-1",
                SettlementStableId = "settlement:sim.farm-town-1",
                GameDateStartsOn = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
            Settlement = new SimulationSettlementInitialStateRequest
            {
                TreasuryBalance = 1_000_000m,
                CurrencyCode = "KRW",
                LaborCapacityTotal = 10m,
                StorageCapacity = 1_000m,
                StorageUnitCode = "kg",
                PopulationCount = 100,
                PopulationFoodDemandPerTick = 100m,
                FoodEquivalentUnitCode = "food-kg",
                FoodEquivalentRuleRevision = "food-equivalent:r1",
                Districts =
                [
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.market-1",
                        DistrictTypeCode = "MarketDistrict",
                        SourceStableIds = ["scenario:sim.market-consumption-1"],
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.storage-1",
                        DistrictTypeCode = "StorageDistrict",
                        SourceStableIds = ["scenario:sim.market-consumption-1"],
                    },
                ],
                Facilities =
                [
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.market-1",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:sim.market-1",
                        SourceStableIds = ["scenario:sim.market-consumption-1"],
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.storage-1",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:sim.storage-1",
                        SourceStableIds = ["scenario:sim.market-consumption-1"],
                    },
                ],
                MarketSupplyByProduct =
                [
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = "product:potato",
                        Quantity = 300m,
                        UnitCode = "kg",
                        SourceStableIds = ["market-stock:sim.potato-300kg"],
                    },
                ],
                SourceStableIds = ["scenario:sim.market-consumption-1"],
            },
        });
        return new Context(service, saveStore, session, session);
    }

    private static Context ReadyOrder()
    {
        var context = CreateContext();
        var ordered = ConfirmOrder(context);
        var ready = Advance(context, ordered, "command:tick.market-order-ready");
        return context with { Ready = ready };
    }

    private static 경영SimulationSessionSnapshot ConfirmOrder(Context context)
        => context.Service.ConfirmIndividualOrder(context.Session.SessionStableId,
            new SimulationIndividualOrderConfirmRequest
            {
                CommandId = "command:market-order.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Order = new SimulationIndividualOrderPreviewRequest
                {
                    OrderStableId = "order:sim.potato-consumption-1",
                    ActorStableId = "resident:sim.household-1",
                    ProductStableId = "product:potato",
                    MarketFacilityStableId = "facility:sim.market-1",
                    Quantity = 20m,
                    UnitCode = "kg",
                    UnitPrice = 2_000m,
                    CurrencyCode = "KRW",
                    RequiredLabor = 2m,
                    FulfillmentDurationTicks = 1,
                    SourceStableIds = ["market-stock:sim.potato-300kg"],
                },
            });

    private static Simulation시장소비PreviewRequest Consumption(long orderRevision)
        => new()
        {
            ConsumptionStableId = "market-consumption:sim.potato-20kg-1",
            OrderStableId = "order:sim.potato-consumption-1",
            OrderRevision = orderRevision,
            ActorStableId = "resident:sim.household-1",
            ConsumptionDurationTicks = 1,
            SourceStableIds = ["source:fixture.market-consumption-1"],
        };

    private static 경영SimulationSessionSnapshot ConfirmConsumption(Context context, long orderRevision)
        => context.Service.ConfirmMarketConsumption(context.Session.SessionStableId,
            new Simulation시장소비ConfirmRequest
            {
                CommandId = "command:market-consumption.confirm-1",
                ExpectedRevision = context.Ready.Revision,
                Consumption = Consumption(orderRevision),
            });

    private static 경영SimulationSessionSnapshot Advance(
        Context context,
        경영SimulationSessionSnapshot current,
        string commandId)
        => context.Service.Advance(context.Session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = commandId,
                ExpectedRevision = current.Revision,
                TickCount = 1,
            });

    private sealed record Context(
        경영SimulationSessionService Service,
        InMemorySimulationSessionSaveStore SaveStore,
        경영SimulationSessionSnapshot Session,
        경영SimulationSessionSnapshot Ready);
}
