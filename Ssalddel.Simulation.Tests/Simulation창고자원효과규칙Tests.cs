using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class Simulation창고자원효과규칙Tests
{
    private readonly Simulation창고자원효과규칙 rule = new();
    private readonly Simulation자원효과적용기 applicator = new();

    [Fact]
    public void 입고대기는_인수Cargo를검수대기Cargo로옮긴다()
    {
        var result = rule.CreateIntake(IntakeRequest());

        Assert.Equal(Simulation업무규칙영역Codes.Warehouse,
            result.PendingEffectBundle.RuleDomainCode);
        Assert.Equal(-297m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "ReceivedCargoHandoff").Delta);
        Assert.Equal(297m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseInspectionPending").Delta);
    }

    [Fact]
    public void 검수는_합격290kg과거부7kg을분리하고총량을보존한다()
    {
        var result = rule.CreateInspection(InspectionRequest());

        Assert.Equal(290m, result.OutputQuantity);
        Assert.Equal(7m, result.LossQuantity);
        Assert.Equal(0m, result.PendingEffectBundle.Lines.Sum(value => value.ConservationQuantity));
    }

    [Fact]
    public void 적치는_합격재고를창고재고로옮기고저장용량을점유한다()
    {
        var result = rule.CreatePutaway(PutawayRequest());

        Assert.Equal(290m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseStock").Delta);
        Assert.Equal(-290m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseStorageCapacity"
                && value.RoleCode == Simulation자원효과역할Codes.Available).Delta);
        Assert.Equal(290m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseStorageCapacity"
                && value.RoleCode == Simulation자원효과역할Codes.Reserved).Delta);
    }

    [Fact]
    public void 보관감모는_재고와점유용량을함께줄이고손실을남긴다()
    {
        var result = rule.CreateStorageLoss(StorageLossRequest());

        Assert.Equal(10m, result.LossQuantity);
        Assert.Equal(-10m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseStock").Delta);
        Assert.Equal(10m, Assert.Single(result.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseStorageLoss").Delta);
    }

    [Fact]
    public void 피킹과출고는_재고예약과용량해제를분리한다()
    {
        var picking = rule.CreatePicking(PickingRequest());
        var outbound = rule.CreateOutbound(OutboundRequest());

        Assert.Equal("Picking", picking.StageCode);
        Assert.Equal("Outbound", outbound.StageCode);
        Assert.Equal(100m, Assert.Single(outbound.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseOutboundHandoff").Delta);
        Assert.Equal(100m, Assert.Single(outbound.PendingEffectBundle.Lines,
            value => value.ResourceTypeCode == "WarehouseStorageCapacity"
                && value.RoleCode == Simulation자원효과역할Codes.Available).Delta);
    }

    [Fact]
    public void 입고부터출고까지적용하면_재고180kg_손실10kg_출고100kg_점유180kg이된다()
    {
        var state = InitialState();
        state = applicator.Apply(state, rule.CreateIntake(IntakeRequest()).PendingEffectBundle, 15).State;
        state = applicator.Apply(state,
            rule.CreateInspection(InspectionRequest()).PendingEffectBundle, 16).State;
        state = applicator.Apply(state, rule.CreatePutaway(PutawayRequest()).PendingEffectBundle, 17).State;
        state = applicator.Apply(state,
            rule.CreateStorageLoss(StorageLossRequest()).PendingEffectBundle, 20).State;
        state = applicator.Apply(state, rule.CreatePicking(PickingRequest()).PendingEffectBundle, 21).State;
        state = applicator.Apply(state, rule.CreateOutbound(OutboundRequest()).PendingEffectBundle, 22).State;

        Assert.Equal(180m, Ledger(state, "warehouse-stock:potato").Value);
        Assert.Equal(10m, Ledger(state, "warehouse-loss:potato").Value);
        Assert.Equal(100m, Ledger(state, "warehouse-outbound:potato").Value);
        Assert.Equal(820m, Ledger(state, "warehouse-capacity:available").Value);
        Assert.Equal(180m, Ledger(state, "warehouse-capacity:occupied").Value);
    }

    [Fact]
    public void 검수합격과거부합이인수수량과다르면차단한다()
    {
        var request = InspectionRequest();
        request.Work.AcceptedQuantity = 289m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateInspection(request));

        Assert.Equal("SimulationWarehouseInspectionQuantityInvalid", error.ErrorCode);
    }

    [Fact]
    public void 저장가용용량보다많은적치는차단한다()
    {
        var request = PutawayRequest();
        request.StorageAvailableBefore = 200m;
        request.StorageOccupiedBefore = 800m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreatePutaway(request));

        Assert.Equal("SimulationWarehouseStorageCapacityExceeded", error.ErrorCode);
    }

    [Fact]
    public void 인수완료만으로창고입고를확정하지않고완료된창고Task를요구한다()
    {
        var request = IntakeRequest();
        request.Work.TaskStateCode = SimulationTaskStateCodes.InProgress;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreateIntake(request));

        Assert.Equal("SimulationWarehouseWorkStateInvalid", error.ErrorCode);
    }

    private static Simulation창고자원효과Request IntakeRequest()
    {
        var request = Request("intake", Work(Simulation창고작업상태Codes.ReceivedAtDock));
        request.ReceivedCargoBefore = 297m;
        request.InspectionPendingBefore = 0m;
        return request;
    }

    private static Simulation창고자원효과Request InspectionRequest()
    {
        var request = Request("inspection", Work(Simulation창고작업상태Codes.InspectionCompleted));
        request.InspectionPendingBefore = 297m;
        request.InspectionAcceptedBefore = 0m;
        request.InspectionRejectedBefore = 0m;
        return request;
    }

    private static Simulation창고자원효과Request PutawayRequest()
    {
        var request = Request("putaway", Work(Simulation창고작업상태Codes.Stored));
        request.InspectionAcceptedBefore = 290m;
        request.WarehouseStockBefore = 0m;
        request.StorageAvailableBefore = 1000m;
        request.StorageOccupiedBefore = 0m;
        return request;
    }

    private static Simulation창고자원효과Request StorageLossRequest()
    {
        var request = Request("storage-loss", Work(Simulation창고작업상태Codes.Stored));
        request.WarehouseStockBefore = 290m;
        request.WarehouseLossBefore = 0m;
        request.StorageAvailableBefore = 710m;
        request.StorageOccupiedBefore = 290m;
        request.StorageLossQuantity = 10m;
        return request;
    }

    private static Simulation창고자원효과Request PickingRequest()
    {
        var request = Request("picking", Work(Simulation창고작업상태Codes.Picked));
        request.WarehouseStockBefore = 280m;
        request.OutboundReservedBefore = 0m;
        return request;
    }

    private static Simulation창고자원효과Request OutboundRequest()
    {
        var request = Request("outbound", Work(Simulation창고작업상태Codes.OutboundCompleted));
        request.OutboundReservedBefore = 100m;
        request.OutboundHandoffBefore = 0m;
        request.StorageAvailableBefore = 720m;
        request.StorageOccupiedBefore = 280m;
        return request;
    }

    private static Simulation창고자원효과Request Request(
        string stage,
        Simulation창고작업Snapshot work)
        => new()
        {
            EffectBundleStableId = "effect-bundle:warehouse." + stage + ".potato-1",
            EffectLineStableIdPrefix = "effect-line:warehouse." + stage + ".potato-1",
            Work = work,
            ReceivedCargoLedgerStableId = "cargo:potato.received",
            InspectionPendingLedgerStableId = "warehouse-inspection:potato.pending",
            InspectionAcceptedLedgerStableId = "warehouse-inspection:potato.accepted",
            InspectionRejectedLedgerStableId = "warehouse-inspection:potato.rejected",
            WarehouseStockLedgerStableId = "warehouse-stock:potato",
            WarehouseLossLedgerStableId = "warehouse-loss:potato",
            OutboundReservedLedgerStableId = "warehouse-outbound-reserved:potato",
            OutboundHandoffLedgerStableId = "warehouse-outbound:potato",
            StorageAvailableLedgerStableId = "warehouse-capacity:available",
            StorageOccupiedLedgerStableId = "warehouse-capacity:occupied",
            StorageCapacity = 1000m,
            SourceStableIds = new[] { "source:warehouse-resource.fixture" },
        };

    private static Simulation창고작업Snapshot Work(string state)
        => new()
        {
            WarehouseWorkStableId = "warehouse-work:potato-1." + state,
            Revision = 1,
            StateCode = state,
            CargoStableId = "cargo:sim.potato-1",
            ProductStableId = "product:potato",
            WarehouseFacilityStableId = "facility:hub-warehouse-1",
            ReceivedQuantity = 297m,
            AcceptedQuantity = 290m,
            RejectedQuantity = 7m,
            PickedQuantity = 100m,
            UnitCode = "kg",
            DecisionStableId = "decision:warehouse.potato-1." + state,
            DecisionStateCode = SimulationDecisionStateCodes.Confirmed,
            TaskStableId = "task:warehouse.potato-1." + state,
            TaskStateCode = SimulationTaskStateCodes.Completed,
            CompletedTick = state switch
            {
                Simulation창고작업상태Codes.ReceivedAtDock => 15,
                Simulation창고작업상태Codes.InspectionCompleted => 16,
                Simulation창고작업상태Codes.Stored => 17,
                Simulation창고작업상태Codes.Picked => 21,
                _ => 22,
            },
            SourceStableIds = new[] { "source:warehouse-work.snapshot" },
        };

    private static Simulation자원원장상태Snapshot InitialState()
        => new()
        {
            Revision = 1,
            WorldTick = 14,
            Ledgers = new[]
            {
                Ledger("cargo:potato.received", "ReceivedCargoHandoff", 297m, true),
                Ledger("warehouse-capacity:available", "WarehouseStorageCapacity", 1000m, false),
                Ledger("warehouse-capacity:occupied", "WarehouseStorageCapacity", 0m, false),
            },
            SourceStableIds = new[] { "session:warehouse.fixture" },
        };

    private static Simulation자원원장항목Snapshot Ledger(
        string id, string type, decimal value, bool cargo)
        => new()
        {
            LedgerStableId = id,
            ResourceTypeCode = type,
            ProductStableId = cargo ? "product:potato" : null,
            LotStableId = cargo ? "cargo:sim.potato-1" : null,
            Value = value,
            UnitCode = "kg",
            SourceStableIds = new[] { "source:warehouse-ledger.fixture" },
        };

    private static Simulation자원원장항목Snapshot Ledger(
        Simulation자원원장상태Snapshot state,
        string id)
        => Assert.Single(state.Ledgers, value => value.LedgerStableId == id);
}
