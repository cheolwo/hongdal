using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationIndividualOrderTests
{
    [Fact]
    public void WI_ORDER_01부터_07까지_확정_예약_피킹_포장_수령_소비를분리한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        var orderRequest = OrderRequest();
        orderRequest.FulfillmentDurationTicks = 3;
        var confirmed = service.ConfirmIndividualOrder(session.SessionStableId,
            ConfirmRequest(session.Revision, orderRequest,
                "command:wi-order:confirm"));
        var order = Assert.Single(confirmed.IndividualOrders);
        Assert.Equal(SimulationIndividualOrderStateCodes.StockReserved, order.StateCode);
        Assert.Equal(order.ConfirmedTick, order.StockReservedTick);
        Assert.Equal(SimulationStockReservationStateCodes.Reserved,
            Assert.Single(confirmed.StockReservations).StateCode);

        var picked = service.Advance(session.SessionStableId, Tick(
            "command:wi-order:picking", confirmed.Revision));
        order = Assert.Single(picked.IndividualOrders);
        Assert.Equal(SimulationIndividualOrderStateCodes.Picking, order.StateCode);
        Assert.NotNull(order.PickedTick);
        var packed = service.Advance(session.SessionStableId, Tick(
            "command:wi-order:packing", picked.Revision));
        order = Assert.Single(packed.IndividualOrders);
        Assert.Equal(SimulationIndividualOrderStateCodes.Packed, order.StateCode);
        Assert.NotNull(order.PackedTick);
        var ready = service.Advance(session.SessionStableId, Tick(
            "command:wi-order:ready", packed.Revision));
        order = Assert.Single(ready.IndividualOrders);
        Assert.Equal(SimulationIndividualOrderStateCodes.ReadyForPickup, order.StateCode);

        var pickup = new SimulationIndividualOrderPickupPreviewRequest
        {
            OrderStableId = order.OrderStableId,
            OrderRevision = order.Revision,
            ActorStableId = order.ActorStableId,
            PreferredSpatialStableId = "spatial:sim:market-pickup",
            PickupDurationTicks = 1,
            SourceStableIds = [order.OrderStableId],
        };
        Assert.Empty(service.PreviewIndividualOrderPickup(
            session.SessionStableId, pickup).Decision.BlockReasonCodes);
        Assert.Equal(ready.Revision, service.Get(session.SessionStableId).Revision);
        var pickupScheduled = service.ConfirmIndividualOrderPickup(
            session.SessionStableId, new SimulationIndividualOrderPickupConfirmRequest
            {
                CommandId = "command:wi-order:pickup",
                ExpectedRevision = ready.Revision,
                Pickup = pickup,
            });
        var fulfilled = service.Advance(session.SessionStableId, Tick(
            "command:wi-order:pickup-tick", pickupScheduled.Revision));
        order = Assert.Single(fulfilled.IndividualOrders);
        Assert.Equal(SimulationIndividualOrderStateCodes.Fulfilled, order.StateCode);

        var consumption = new Simulation시장소비PreviewRequest
        {
            ConsumptionStableId = "market-consumption:wi-order:potato-20kg",
            OrderStableId = order.OrderStableId,
            OrderRevision = order.Revision,
            ActorStableId = order.ActorStableId,
            ConsumptionDurationTicks = 1,
            SourceStableIds = [order.OrderStableId],
        };
        Assert.Empty(service.PreviewMarketConsumption(session.SessionStableId,
            consumption).BlockReasonCodes);
        var consumptionScheduled = service.ConfirmMarketConsumption(
            session.SessionStableId, new Simulation시장소비ConfirmRequest
            {
                CommandId = "command:wi-order:consume",
                ExpectedRevision = fulfilled.Revision,
                Consumption = consumption,
            });
        var consumed = service.Advance(session.SessionStableId, Tick(
            "command:wi-order:consume-tick", consumptionScheduled.Revision));
        Assert.Equal(SimulationIndividualOrderStateCodes.Consumed,
            Assert.Single(consumed.IndividualOrders).StateCode);
        Assert.Equal(Simulation시장소비StateCodes.Consumed,
            Assert.Single(consumed.MarketConsumptions).StateCode);
        Assert.Equal(280m,
            Assert.Single(consumed.Settlement!.MarketSupplyByProduct).Quantity);
        var saved = service.Save(session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:wi-order:full-lifecycle",
                ExpectedRevision = consumed.Revision,
            });
        var restored = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore).Restore(
                new SimulationSessionRestoreRequest
                {
                    SaveStableId = saved.SaveStableId,
                });
        Assert.Equal(saved.ReplayHash, restored.ReplayHash);
        Assert.Equal(SimulationIndividualOrderStateCodes.Consumed,
            Assert.Single(restored.Session.IndividualOrders).StateCode);
    }
    [Fact]
    public void Preview는_감자재고와노동후보를계산하지만_session을변경하지않는다()
    {
        var service = Service();
        var session = CreateSession(service);

        var preview = service.PreviewIndividualOrder(session.SessionStableId, OrderRequest());
        var unchanged = service.Get(session.SessionStableId);

        Assert.Equal(300m, preview.AvailableBeforeReservation);
        Assert.Equal(280m, preview.AvailableAfterReservation);
        Assert.Equal(40_000m, preview.TotalPrice);
        Assert.Equal(10m, preview.LaborAvailableBeforeReservation);
        Assert.Equal(8m, preview.LaborAvailableAfterReservation);
        Assert.Empty(preview.BlockReasonCodes);
        Assert.Equal(0, unchanged.Revision);
        Assert.Empty(unchanged.IndividualOrders);
        Assert.Empty(unchanged.StockReservations);
        Assert.Equal(300m, Assert.Single(unchanged.Settlement!.MarketSupplyByProduct).Quantity);
    }

    [Fact]
    public void Confirm은_재고를예약하고_완료Tick은_수령준비상태와잔여재고를확정한다()
    {
        var service = Service();
        var session = CreateSession(service);

        var confirmed = service.ConfirmIndividualOrder(
            session.SessionStableId,
            ConfirmRequest(session.Revision));

        var order = Assert.Single(confirmed.IndividualOrders);
        var reservation = Assert.Single(confirmed.StockReservations);
        Assert.Equal(SimulationIndividualOrderStateCodes.StockReserved, order.StateCode);
        Assert.Equal(20m, order.OrderedQuantity);
        Assert.Equal(0m, order.FulfilledQuantity);
        Assert.Equal(SimulationStockReservationStateCodes.Reserved, reservation.StateCode);
        Assert.Equal(20m, reservation.Quantity);
        Assert.Equal(2m, confirmed.Settlement!.LaborReserved);
        Assert.Equal(300m, Assert.Single(confirmed.Settlement.MarketSupplyByProduct).Quantity);
        Assert.Single(confirmed.Decisions);
        Assert.Single(confirmed.Tasks);
        Assert.Single(confirmed.Effects);

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.order.potato-20kg",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });

        order = Assert.Single(completed.IndividualOrders);
        reservation = Assert.Single(completed.StockReservations);
        Assert.Equal(SimulationIndividualOrderStateCodes.ReadyForPickup, order.StateCode);
        Assert.Equal(20m, order.FulfilledQuantity);
        Assert.Equal(1, order.ReadyForPickupTick);
        Assert.Equal(SimulationStockReservationStateCodes.Consumed, reservation.StateCode);
        Assert.Equal(1, reservation.ConsumedTick);
        Assert.Equal(0m, completed.Settlement!.LaborReserved);
        Assert.Equal(280m, Assert.Single(completed.Settlement.MarketSupplyByProduct).Quantity);
        Assert.Equal(SimulationTaskStateCodes.Completed, Assert.Single(completed.Tasks).StateCode);
        Assert.Equal(SimulationEffectStateCodes.Applied, Assert.Single(completed.Effects).StateCode);
    }

    [Fact]
    public void 예약된수량을뺀_가용재고가부족하면_두번째Confirm을차단한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var firstOrder = OrderRequest(quantity: 290m, orderId: "order:sim.potato-large-1");
        var first = service.ConfirmIndividualOrder(
            session.SessionStableId,
            ConfirmRequest(session.Revision, firstOrder, "command:order.confirm.large-1"));
        var secondOrder = OrderRequest(quantity: 20m, orderId: "order:sim.potato-second-1");

        var preview = service.PreviewIndividualOrder(session.SessionStableId, secondOrder);
        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmIndividualOrder(
                session.SessionStableId,
                ConfirmRequest(first.Revision, secondOrder, "command:order.confirm.second-1")));

        Assert.Equal(10m, preview.AvailableBeforeReservation);
        Assert.Equal("SimulationMarketSupplyInsufficient", Assert.Single(preview.BlockReasonCodes));
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Single(service.Get(session.SessionStableId).IndividualOrders);
    }

    [Fact]
    public void Confirm재시도와_SaveReplay는_같은주문과재고결과를보존한다()
    {
        var sessionStore = new InMemory경영SimulationSessionStore();
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(sessionStore, saveStore);
        var session = CreateSession(service);
        var command = ConfirmRequest(session.Revision);

        var first = service.ConfirmIndividualOrder(session.SessionStableId, command);
        var retry = service.ConfirmIndividualOrder(session.SessionStableId, command);
        Assert.Equal(first.Revision, retry.Revision);
        Assert.Single(retry.IndividualOrders);
        Assert.Single(retry.StockReservations);

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.order.replay-1",
                ExpectedRevision = retry.Revision,
                TickCount = 1,
            });
        var package = service.Save(session.SessionStableId, new SimulationSessionSaveRequest
        {
            SaveStableId = "save:sim.order-core-1",
            ExpectedRevision = completed.Revision,
        });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        Assert.Equal(completed.Revision, restored.Session.Revision);
        Assert.Equal(
            SimulationIndividualOrderStateCodes.ReadyForPickup,
            Assert.Single(restored.Session.IndividualOrders).StateCode);
        Assert.Equal(
            SimulationStockReservationStateCodes.Consumed,
            Assert.Single(restored.Session.StockReservations).StateCode);
        Assert.Equal(280m, Assert.Single(restored.Session.Settlement!.MarketSupplyByProduct).Quantity);
    }

    [Fact]
    public void 수령준비전취소는_포장작업을취소하고_재고와노동예약을반환한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var orderRequest = OrderRequest();
        orderRequest.FulfillmentDurationTicks = 2;
        var ordered = service.ConfirmIndividualOrder(
            session.SessionStableId,
            ConfirmRequest(session.Revision, orderRequest));
        var cancel = new SimulationIndividualOrderCancelRequest
        {
            CommandId = "command:order.cancel.potato-20kg-1",
            ExpectedRevision = ordered.Revision,
            OrderStableId = orderRequest.OrderStableId,
            ActorStableId = orderRequest.ActorStableId,
            ReasonCode = "ChangedMind",
            SourceStableIds = ["scenario:sim.order-core-1"],
        };

        var preview = service.PreviewIndividualOrderCancellation(session.SessionStableId, cancel);
        var cancellationScheduled = service.ConfirmIndividualOrderCancellation(
            session.SessionStableId,
            cancel);
        Assert.Empty(preview.Decision.BlockReasonCodes);
        Assert.Equal(
            SimulationIndividualOrderStateCodes.CancellationScheduled,
            Assert.Single(cancellationScheduled.IndividualOrders).StateCode);

        var cancelled = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.order.cancel-1",
                ExpectedRevision = cancellationScheduled.Revision,
                TickCount = 1,
            });

        Assert.Equal(
            SimulationIndividualOrderStateCodes.Cancelled,
            Assert.Single(cancelled.IndividualOrders).StateCode);
        Assert.Equal(
            SimulationStockReservationStateCodes.Released,
            Assert.Single(cancelled.StockReservations).StateCode);
        Assert.Equal(300m, Assert.Single(cancelled.Settlement!.MarketSupplyByProduct).Quantity);
        Assert.Equal(0m, cancelled.Settlement.LaborReserved);
        Assert.Contains(cancelled.Tasks, x => x.StateCode == SimulationTaskStateCodes.Cancelled);
        Assert.Contains(cancelled.Tasks, x => x.StateCode == SimulationTaskStateCodes.Completed);
        Assert.Contains(cancelled.Effects, x => x.StateCode == SimulationEffectStateCodes.Cancelled);
        Assert.Contains(cancelled.Effects, x => x.StateCode == SimulationEffectStateCodes.Applied);
    }

    [Fact]
    public void 수령준비완료뒤에는_주문취소를차단한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var ordered = service.ConfirmIndividualOrder(
            session.SessionStableId,
            ConfirmRequest(session.Revision));
        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.order.ready-before-cancel",
                ExpectedRevision = ordered.Revision,
                TickCount = 1,
            });
        var cancel = new SimulationIndividualOrderCancelRequest
        {
            CommandId = "command:order.cancel.after-ready",
            ExpectedRevision = completed.Revision,
            OrderStableId = "order:sim.potato-20kg-1",
            ActorStableId = "resident:sim.household-1",
            ReasonCode = "TooLate",
            SourceStableIds = ["scenario:sim.order-core-1"],
        };

        var preview = service.PreviewIndividualOrderCancellation(session.SessionStableId, cancel);
        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmIndividualOrderCancellation(session.SessionStableId, cancel));

        Assert.Equal(
            "SimulationIndividualOrderCancellationNotAllowed",
            Assert.Single(preview.Decision.BlockReasonCodes));
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Equal(280m, Assert.Single(service.Get(session.SessionStableId).Settlement!.MarketSupplyByProduct).Quantity);
    }

    private static 경영SimulationSessionService Service()
        => new(
            new InMemory경영SimulationSessionStore(),
            new InMemorySimulationSessionSaveStore());

    private static 경영SimulationSessionSnapshot CreateSession(
        경영SimulationSessionService service)
        => service.Create(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.order-core-1",
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
                        SourceStableIds = ["scenario:sim.order-core-1"],
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.storage-1",
                        DistrictTypeCode = "StorageDistrict",
                        SourceStableIds = ["scenario:sim.order-core-1"],
                    },
                ],
                Facilities =
                [
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.market-1",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:sim.market-1",
                        SourceStableIds = ["scenario:sim.order-core-1"],
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.storage-1",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:sim.storage-1",
                        SourceStableIds = ["scenario:sim.order-core-1"],
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
                SourceStableIds = ["scenario:sim.order-core-1"],
            },
            SpatialWorld = new Simulation공간세계InitialStateRequest
            {
                Definitions =
                [
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = "spatial:sim:market-pickup",
                        FacilityStableId = "facility:sim.market-1",
                        AreaStableId = "area:sim:market-1",
                        AreaSetStableId = "area-set:sim:order-core",
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes =
                        [
                            Simulation공간능력Codes.CustomerAccessible,
                            Simulation공간능력Codes.PickupArea,
                        ],
                        BaseCapacities =
                        [
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.WorkArea,
                                Quantity = 1m,
                                UnitCode = "slot",
                            },
                        ],
                        DefinitionRevision = "scenario-order-pickup.v1",
                        DefinitionHashSha256 = new string('e', 64),
                        SourceStableIds = ["scenario:sim.order-core-1"],
                    },
                ],
            },
        });

    private static 경영SimulationTick진행Request Tick(
        string commandId,
        long expectedRevision)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = expectedRevision,
            TickCount = 1,
        };

    private static SimulationIndividualOrderPreviewRequest OrderRequest(
        decimal quantity = 20m,
        string orderId = "order:sim.potato-20kg-1")
        => new()
        {
            OrderStableId = orderId,
            ActorStableId = "resident:sim.household-1",
            ProductStableId = "product:potato",
            MarketFacilityStableId = "facility:sim.market-1",
            Quantity = quantity,
            UnitCode = "kg",
            UnitPrice = 2_000m,
            CurrencyCode = "KRW",
            RequiredLabor = 2m,
            FulfillmentDurationTicks = 1,
            SourceStableIds = ["scenario:sim.order-core-1", "market-stock:sim.potato-300kg"],
        };

    private static SimulationIndividualOrderConfirmRequest ConfirmRequest(
        long expectedRevision,
        SimulationIndividualOrderPreviewRequest? order = null,
        string commandId = "command:order.confirm.potato-20kg-1")
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = expectedRevision,
            Order = order ?? OrderRequest(),
        };
}
