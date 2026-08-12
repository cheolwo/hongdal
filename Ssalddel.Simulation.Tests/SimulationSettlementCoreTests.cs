using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationSettlementCoreTests
{
    [Fact]
    public void 명시적초기상태를_독립경제지표와FoodSecurityDays로투영한다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());

        var settlement = Assert.IsType<SimulationSettlementEconomySnapshot>(session.Settlement);
        Assert.Equal("settlement:sim.farm-town-1", settlement.SettlementStableId);
        Assert.Equal(1_000_000m, settlement.TreasuryBalance);
        Assert.Equal(100m, settlement.LaborCapacityTotal);
        Assert.Equal(25m, settlement.LaborReserved);
        Assert.Equal(75m, settlement.LaborAvailable);
        Assert.Equal(2000m, settlement.StorageCapacity);
        Assert.Equal(1200m, settlement.StorageOccupied);
        Assert.Equal(800m, settlement.StorageAvailable);
        Assert.Equal(1200m, settlement.FoodReserveEquivalent);
        Assert.Equal(120m, settlement.FoodDemandPerTick);
        Assert.Equal(10m, settlement.FoodSecurityDays);
        Assert.Equal("food-equivalent:fixture-r1", settlement.FoodEquivalentRuleRevision);
        Assert.Equal(
            SimulationFoodSecurityFormulaCodes.AvailableFoodEquivalentDividedByDemand,
            settlement.FoodSecurityFormulaCode);
    }

    [Fact]
    public void graph와상품Lot은_stableId결정순서와source를보존한다()
    {
        var service = Service();
        var settlement = Assert.IsType<SimulationSettlementEconomySnapshot>(
            service.Create(CreateRequest()).Settlement);

        Assert.Equal(
            new[] { "district:sim.central", "district:sim.farm" },
            settlement.Districts.Select(value => value.DistrictStableId));
        Assert.Equal(
            new[] { "facility:sim.market", "facility:sim.storage" },
            settlement.Facilities.Select(value => value.FacilityStableId));
        Assert.Equal(
            new[] { "product:cabbage", "product:potato" },
            settlement.MarketSupplyByProduct.Select(value => value.ProductStableId));
        Assert.Equal(
            new[] { "stock-lot:sim.grain-1", "stock-lot:sim.potato-1" },
            settlement.ReserveStockLots.Select(value => value.StockLotStableId));
        Assert.Equal(800m, settlement.ReserveStockLots[1].AvailableQuantity);
        Assert.Equal(960m, settlement.ReserveStockLots[1].AvailableFoodEquivalentQuantity);
        Assert.Contains("source:scenario-settlement-r1", settlement.SourceStableIds);
    }

    [Fact]
    public void 정착지초기상태가없는기존session도호환된다()
    {
        var service = Service();
        var request = CreateRequest();
        request.Settlement = null;

        var session = service.Create(request);

        Assert.Null(session.Settlement);
        Assert.Equal(0, session.Revision);
    }

    [Theory]
    [InlineData("Labor", "SimulationSettlementLaborCapacityInvalid")]
    [InlineData("StorageCapacity", "SimulationSettlementStorageCapacityInvalid")]
    [InlineData("ReserveStorage", "SimulationSettlementReserveExceedsStorageOccupied")]
    [InlineData("PopulationDemand", "SimulationSettlementPopulationDemandInvalid")]
    [InlineData("GarrisonDemand", "SimulationSettlementGarrisonDemandInvalid")]
    public void capacity와수요보존식위반을거부한다(string mutation, string expectedErrorCode)
    {
        var service = Service();
        var request = CreateRequest();
        var settlement = request.Settlement!;
        switch (mutation)
        {
            case "Labor":
                settlement.LaborReserved = 101m;
                break;
            case "StorageCapacity":
                settlement.StorageOccupied = 2001m;
                break;
            case "ReserveStorage":
                settlement.StorageOccupied = 1199m;
                break;
            case "PopulationDemand":
                settlement.PopulationFoodDemandPerTick = 0m;
                break;
            case "GarrisonDemand":
                settlement.GarrisonCount = 0;
                break;
        }

        var error = Assert.Throws<SimulationContractException>(() => service.Create(request));

        Assert.Equal(expectedErrorCode, error.ErrorCode);
    }

    [Fact]
    public void Facility의danglingDistrict를거부한다()
    {
        var service = Service();
        var request = CreateRequest();
        request.Settlement!.Facilities[0].DistrictStableId = "district:sim.missing";

        var error = Assert.Throws<SimulationContractException>(() => service.Create(request));

        Assert.Equal("SimulationSettlementFacilityDistrictNotFound", error.ErrorCode);
    }

    [Fact]
    public void 비축Lot은_Storage시설만참조할수있다()
    {
        var service = Service();
        var request = CreateRequest();
        request.Settlement!.ReserveStockLots[0].StorageFacilityStableId = "facility:sim.market";

        var error = Assert.Throws<SimulationContractException>(() => service.Create(request));

        Assert.Equal("SimulationSettlementStockStorageFacilityInvalid", error.ErrorCode);
    }

    [Fact]
    public void 상품별시장공급과LotStableId의중복을거부한다()
    {
        var service = Service();
        var duplicateProduct = CreateRequest();
        duplicateProduct.Settlement!.MarketSupplyByProduct[0].ProductStableId = "product:cabbage";
        var productError = Assert.Throws<SimulationContractException>(() =>
            service.Create(duplicateProduct));

        var duplicateLot = CreateRequest(Guid.NewGuid());
        duplicateLot.Settlement!.ReserveStockLots[0].StockLotStableId = "stock-lot:sim.grain-1";
        var lotError = Assert.Throws<SimulationContractException>(() =>
            service.Create(duplicateLot));

        Assert.Equal("SimulationSettlementMarketProductDuplicate", productError.ErrorCode);
        Assert.Equal("SimulationSettlementStockLotStableIdDuplicate", lotError.ErrorCode);
    }

    [Fact]
    public void 같은생성요청Id의다른정착지초기값은_충돌로거부한다()
    {
        var service = Service();
        var request = CreateRequest();
        service.Create(request);
        var changed = CreateRequest(request.ClientRequestId);
        changed.Settlement!.TreasuryBalance += 1m;

        var error = Assert.Throws<SimulationConflictException>(() => service.Create(changed));

        Assert.Equal("SimulationCreateRequestPayloadConflict", error.ErrorCode);
    }

    [Fact]
    public void Confirm된미완료Task만_ActiveTaskStableIds에표시한다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());
        var confirmed = service.ConfirmDecision(
            session.SessionStableId,
            ConfirmRequest(expectedRevision: 0));

        Assert.Equal(
            new[] { "task:sim.settlement-core-1" },
            confirmed.Settlement!.ActiveTaskStableIds);
        Assert.Equal(1, confirmed.Settlement.Revision);

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:settlement-core.tick-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });
        Assert.Empty(completed.Settlement!.ActiveTaskStableIds);
        Assert.Equal(1, completed.Settlement.WorldTick);
        Assert.Equal(2, completed.Settlement.Revision);
        Assert.Equal(1_000_000m, completed.Settlement.TreasuryBalance);
    }

    [Fact]
    public void saveReplay는_정착지graph와계산값을같은hash로복원한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var sourceService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore);
        var source = sourceService.Create(CreateRequest());
        var saved = sourceService.Save(
            source.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.settlement-core-1",
                ExpectedRevision = source.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });
        var resaved = restoredService.Save(
            restored.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.settlement-core-check-1",
                ExpectedRevision = restored.Session.Revision,
            });

        Assert.Equal(saved.ReplayHash, resaved.ReplayHash);
        Assert.Equal(10m, restored.Session.Settlement!.FoodSecurityDays);
        Assert.Equal(2, restored.Session.Settlement.Districts.Length);
        Assert.Equal("facility:sim.storage", restored.Session.Settlement.ReserveStockLots[0].StorageFacilityStableId);
    }

    [Fact]
    public void 반환snapshot변경은_aggregate정착지상태를오염시키지않는다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());
        session.Settlement!.TreasuryBalance = -1m;
        session.Settlement.ReserveStockLots[0].Quantity = -1m;

        var stored = service.Get(session.SessionStableId);

        Assert.Equal(1_000_000m, stored.Settlement!.TreasuryBalance);
        Assert.Equal(200m, stored.Settlement.ReserveStockLots[0].Quantity);
    }

    private static 경영SimulationSessionService Service()
        => new(
            new InMemory경영SimulationSessionStore(),
            new InMemorySimulationSessionSaveStore());

    private static 경영SimulationSession생성Request CreateRequest(Guid? clientRequestId = null)
        => new()
        {
            ClientRequestId = clientRequestId ?? Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.settlement-core-1",
            ScenarioDataRevision = "scenario-data:r1",
            ScenarioSeed = 20260810,
            RuleRevision = "rule:settlement-core-r1",
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
                LaborCapacityTotal = 100m,
                LaborReserved = 25m,
                StorageCapacity = 2000m,
                StorageOccupied = 1200m,
                StorageUnitCode = "KGM",
                PopulationCount = 100,
                PopulationFoodDemandPerTick = 100m,
                GarrisonCount = 20,
                GarrisonFoodDemandPerTick = 20m,
                FoodEquivalentUnitCode = "FoodEquivalentUnit",
                FoodEquivalentRuleRevision = "food-equivalent:fixture-r1",
                Districts = new[]
                {
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.farm",
                        DistrictTypeCode = "FarmDistrict",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.central",
                        DistrictTypeCode = "CentralDistrict",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                },
                Facilities = new[]
                {
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.storage",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:sim.farm",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.market",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:sim.central",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                },
                MarketSupplyByProduct = new[]
                {
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = "product:potato",
                        Quantity = 300m,
                        UnitCode = "KGM",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = "product:cabbage",
                        Quantity = 100m,
                        UnitCode = "KGM",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                },
                ReserveStockLots = new[]
                {
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = "stock-lot:sim.potato-1",
                        ProductStableId = "product:potato",
                        StorageFacilityStableId = "facility:sim.storage",
                        Quantity = 1000m,
                        OutboundReservedQuantity = 200m,
                        UnitCode = "KGM",
                        FoodEquivalentQuantity = 1200m,
                        OutboundReservedFoodEquivalentQuantity = 240m,
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = "stock-lot:sim.grain-1",
                        ProductStableId = "product:grain",
                        StorageFacilityStableId = "facility:sim.storage",
                        Quantity = 200m,
                        OutboundReservedQuantity = 0m,
                        UnitCode = "KGM",
                        FoodEquivalentQuantity = 240m,
                        OutboundReservedFoodEquivalentQuantity = 0m,
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                },
                SourceStableIds = new[] { "source:scenario-settlement-r1" },
            },
        };

    private static SimulationDecisionConfirmRequest ConfirmRequest(long expectedRevision)
        => new()
        {
            CommandId = "command:settlement-core.confirm-1",
            ExpectedRevision = expectedRevision,
            Preview = new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:sim.settlement-core-1",
                DecisionTypeCode = "SettlementWork",
                ActorStableId = "actor:sim.manager-1",
                TargetStableIds = new[] { "settlement:sim.farm-town-1" },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "TreasuryProjection",
                        TargetLedgerStableId = "ledger:sim.treasury-1",
                        BeforeValue = 1_000_000m,
                        Delta = 0m,
                        AfterValue = 1_000_000m,
                        UnitCode = "KRW",
                        SourceStableIds = new[] { "source:scenario-settlement-r1" },
                    },
                },
                SourceStableIds = new[] { "source:scenario-settlement-r1" },
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:sim.settlement-core-1",
                    TaskTypeCode = "SettlementWork",
                    FacilityStableId = "facility:sim.storage",
                    AssignedCapacity = 10m,
                    AssignedCapacityUnitCode = "LaborHour",
                    DurationTicks = 1,
                    InputLotStableIds = new[] { "stock-lot:sim.potato-1" },
                    SourceStableIds = new[] { "source:scenario-settlement-r1" },
                },
            },
        };
}
