using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class Simulation운송자원효과규칙Tests
{
    private readonly Simulation운송자원효과규칙 rule = new();
    private readonly Simulation자원효과적용기 applicator = new();

    [Fact]
    public void 상차는_원산지Cargo300kg을차량Cargo로옮긴다()
    {
        var result = rule.CreateLoading(LoadingRequest());

        Assert.Equal(Simulation업무규칙영역Codes.Transport,
            result.PendingEffectBundle.RuleDomainCode);
        Assert.Equal(-300m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "OriginCargoStock").Delta);
        Assert.Equal(300m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "VehicleCargoStock").Delta);
    }

    [Fact]
    public void 운송은_연료와노동을소비하고Cargo손실을별도기록한다()
    {
        var result = rule.CreateTravel(TravelRequest());

        Assert.Equal(3m, result.LostQuantity);
        Assert.Equal(297m, result.DeliveredQuantity);
        Assert.Equal(-5m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "TransportFuel").Delta);
        Assert.Equal(-2m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "TransportLabor").Delta);
        Assert.Equal(3m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "TransportCargoLoss").Delta);
    }

    [Fact]
    public void 하차와인수확인은_도착과인수완료를분리한다()
    {
        var unloading = rule.CreateUnloading(UnloadingRequest());
        var receipt = rule.CreateReceipt(ReceiptRequest());

        Assert.Equal("Unloading", unloading.StageCode);
        Assert.Equal("Receipt", receipt.StageCode);
        Assert.Equal("task:freight-receipt:potato-1",
            receipt.PendingEffectBundle.CausedByTaskStableId);
        Assert.Equal("decision:freight-receipt:potato-1",
            receipt.PendingEffectBundle.CausedByDecisionStableId);
    }

    [Fact]
    public void 상차_운송_하차_인수를순서대로적용하면_297kg만인수되고3kg손실이남는다()
    {
        var state = InitialState();
        state = applicator.Apply(state,
            rule.CreateLoading(LoadingRequest()).PendingEffectBundle, 10).State;
        state = applicator.Apply(state,
            rule.CreateTravel(TravelRequest()).PendingEffectBundle, 13).State;
        state = applicator.Apply(state,
            rule.CreateUnloading(UnloadingRequest()).PendingEffectBundle, 13).State;
        state = applicator.Apply(state,
            rule.CreateReceipt(ReceiptRequest()).PendingEffectBundle, 14).State;

        Assert.Equal(0m, Ledger(state, "cargo:potato.origin").Value);
        Assert.Equal(0m, Ledger(state, "cargo:potato.vehicle").Value);
        Assert.Equal(0m, Ledger(state, "cargo:potato.destination-staging").Value);
        Assert.Equal(297m, Ledger(state, "cargo:potato.received").Value);
        Assert.Equal(3m, Ledger(state, "cargo-loss:potato.transport").Value);
        Assert.Equal(45m, Ledger(state, "fuel:vehicle.truck-1").Value);
        Assert.Equal(8m, Ledger(state, "labor:driver.truck-1").Value);
    }

    [Fact]
    public void 차량용량보다큰Cargo는운송효과를만들지않는다()
    {
        var request = LoadingRequest();
        request.Freight.VehicleCapacity = 299m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateLoading(request));

        Assert.Equal("SimulationTransportBindingInvalid", error.ErrorCode);
    }

    [Fact]
    public void 목적지도착전에는하차효과를만들지않는다()
    {
        var request = UnloadingRequest();
        request.Movement.StateCode = SimulationLogisticsMovementStateCodes.InTransit;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateUnloading(request));

        Assert.Equal("SimulationTransportUnloadingStateInvalid", error.ErrorCode);
    }

    [Fact]
    public void 인수Confirm완료전에는인수효과를만들지않는다()
    {
        var request = ReceiptRequest();
        request.Freight.StateCode = 화물운송상태코드.하차지도착;
        request.Freight.ReceivedTick = null;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateReceipt(request));

        Assert.Equal("SimulationTransportReceiptStateInvalid", error.ErrorCode);
    }

    [Fact]
    public void NavMesh와차량Transform은운송효과입력이아니다()
    {
        var request = TravelRequest();

        var result = rule.CreateTravel(request);

        Assert.DoesNotContain(result.PendingEffectBundle.SourceStableIds,
            value => value.Contains("Transform", StringComparison.Ordinal)
                || value.Contains("NavMesh", StringComparison.Ordinal));
        Assert.Contains("source:logistics-movement.snapshot", result.PendingEffectBundle.SourceStableIds);
    }

    [Fact]
    public void 운송손실은Cargo전체량보다작아야한다()
    {
        var request = TravelRequest();
        request.CargoLossQuantity = 300m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateTravel(request));

        Assert.Equal("SimulationTransportTravelResourceInvalid", error.ErrorCode);
    }

    private static Simulation운송자원효과Request LoadingRequest()
    {
        var request = Request("loading", Movement(SimulationLogisticsMovementStateCodes.InTransit),
            Freight(화물운송상태코드.운송중));
        request.OriginCargoBefore = 300m;
        request.VehicleCargoBefore = 0m;
        request.CargoLossQuantity = 0m;
        return request;
    }

    private static Simulation운송자원효과Request TravelRequest()
    {
        var request = Request("travel",
            Movement(SimulationLogisticsMovementStateCodes.ArrivedAtDestination),
            Freight(화물운송상태코드.하차지도착));
        request.VehicleCargoBefore = 300m;
        request.TransportLossBefore = 0m;
        request.FuelBefore = 50m;
        request.FuelConsumption = 5m;
        request.LaborBefore = 10m;
        request.LaborConsumption = 2m;
        request.CargoLossQuantity = 3m;
        return request;
    }

    private static Simulation운송자원효과Request UnloadingRequest()
    {
        var request = Request("unloading",
            Movement(SimulationLogisticsMovementStateCodes.ArrivedAtDestination),
            Freight(화물운송상태코드.하차지도착));
        request.VehicleCargoBefore = 297m;
        request.DestinationStagingBefore = 0m;
        request.CargoLossQuantity = 3m;
        return request;
    }

    private static Simulation운송자원효과Request ReceiptRequest()
    {
        var request = Request("receipt",
            Movement(SimulationLogisticsMovementStateCodes.ArrivedAtDestination),
            Freight(화물운송상태코드.인수완료));
        request.DestinationStagingBefore = 297m;
        request.ReceivedCargoBefore = 0m;
        request.CargoLossQuantity = 3m;
        return request;
    }

    private static Simulation운송자원효과Request Request(
        string stage,
        SimulationLogisticsMovementSnapshot movement,
        SimulationFreightTransportSnapshot freight)
        => new()
        {
            EffectBundleStableId = "effect-bundle:transport." + stage + ".potato-1",
            EffectLineStableIdPrefix = "effect-line:transport." + stage + ".potato-1",
            Movement = movement,
            Freight = freight,
            OriginCargoLedgerStableId = "cargo:potato.origin",
            VehicleCargoLedgerStableId = "cargo:potato.vehicle",
            DestinationStagingLedgerStableId = "cargo:potato.destination-staging",
            ReceivedCargoLedgerStableId = "cargo:potato.received",
            TransportLossLedgerStableId = "cargo-loss:potato.transport",
            FuelLedgerStableId = "fuel:vehicle.truck-1",
            LaborLedgerStableId = "labor:driver.truck-1",
            FuelUnitCode = "L",
            LaborUnitCode = "hour",
            SourceStableIds = new[] { "source:transport-resource.fixture" },
        };

    private static SimulationLogisticsMovementSnapshot Movement(string state)
        => new()
        {
            CargoStableId = "cargo:sim.potato-1",
            CargoRevision = 2,
            StateCode = state,
            Revision = 3,
            SourceAllocationStableId = "allocation:potato-1",
            HarvestLotStableId = "harvest-lot:potato-1",
            PackageLotStableId = "package-lot:potato-1",
            ProductStableId = "product:potato",
            Quantity = 300m,
            ReservedQuantity = 300m,
            UnitCode = "kg",
            RouteStableId = "route:farm-to-hub",
            OriginFacilityStableId = "facility:farm-1",
            DestinationFacilityStableId = "facility:hub-1",
            DecisionStableId = "decision:logistics-movement:potato-1",
            TaskStableId = "task:logistics-movement:potato-1",
            RequiredRouteTicks = 3,
            CompletedRouteTicks = state == SimulationLogisticsMovementStateCodes.InTransit ? 1 : 3,
            ReservedTick = 9,
            DepartedTick = 10,
            ArrivedTick = state == SimulationLogisticsMovementStateCodes.ArrivedAtDestination ? 13 : null,
            DestinationStockCandidateStableId = "stock-candidate:hub.potato-1",
            SourceStableIds = new[] { "source:logistics-movement.snapshot" },
        };

    private static SimulationFreightTransportSnapshot Freight(string state)
        => new()
        {
            TransportRequestStableId = "freight-transport:potato-1",
            DispatchOfferStableId = "dispatch-offer:potato-1",
            RequestStateCode = 화물운송상태코드.배차대기,
            DispatchStateCode = 화물운송상태코드.배차확정,
            StateCode = state,
            Revision = state == 화물운송상태코드.인수완료 ? 6 : 4,
            CargoStableId = "cargo:sim.potato-1",
            CarrierCandidateStableId = "carrier:sim-1",
            VehicleStableId = "vehicle:sim.truck-1",
            VehicleCapacity = 400m,
            VehicleCapacityUnitCode = "kg",
            Quantity = 300m,
            UnitCode = "kg",
            LogisticsTaskStableId = "task:logistics-movement:potato-1",
            ReceiptDecisionStableId = state == 화물운송상태코드.인수완료
                ? "decision:freight-receipt:potato-1" : null,
            ReceiptTaskStableId = state == 화물운송상태코드.인수완료
                ? "task:freight-receipt:potato-1" : null,
            RequestedTick = 9,
            DispatchedTick = 9,
            PickedUpTick = 10,
            ArrivedAtDropoffTick = state == 화물운송상태코드.운송중 ? null : 13,
            ReceivedTick = state == 화물운송상태코드.인수완료 ? 14 : null,
            RuleRevision = "freight-simulation.v1",
            SourceStableIds = new[] { "source:freight-transport.snapshot" },
            StateHistory = new[]
            {
                new SimulationFreightTransportTransitionSnapshot
                {
                    FromStateCode = 화물운송상태코드.상차지도착,
                    ToStateCode = 화물운송상태코드.상차완료,
                    WorldTick = 10,
                    CauseStableId = "task:logistics-movement:potato-1",
                    RuleRevision = "freight-simulation.v1",
                },
            },
        };

    private static Simulation자원원장상태Snapshot InitialState()
        => new()
        {
            Revision = 1,
            WorldTick = 9,
            Ledgers = new[]
            {
                Ledger("cargo:potato.origin", "OriginCargoStock", 300m, "kg", "cargo:sim.potato-1"),
                Ledger("fuel:vehicle.truck-1", "TransportFuel", 50m, "L", null),
                Ledger("labor:driver.truck-1", "TransportLabor", 10m, "hour", null),
            },
            SourceStableIds = new[] { "session:transport.fixture" },
        };

    private static Simulation자원원장항목Snapshot Ledger(
        string id, string type, decimal value, string unit, string? lot)
        => new()
        {
            LedgerStableId = id,
            ResourceTypeCode = type,
            ProductStableId = lot == null ? null : "product:potato",
            LotStableId = lot,
            Value = value,
            UnitCode = unit,
            SourceStableIds = new[] { "source:transport-ledger.fixture" },
        };

    private static Simulation자원원장항목Snapshot Ledger(
        Simulation자원원장상태Snapshot state,
        string stableId)
        => Assert.Single(state.Ledgers, value => value.LedgerStableId == stableId);
}
