using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation공간상호작용Tests
{
    [Fact]
    public void 입고검수_실행전검토는_시나리오공간을선택하고_상태를바꾸지않는다()
    {
        var context = CreateContext();
        var created = context.Service.Create(CreateRequest());

        var preview = context.Service.PreviewDecision(
            created.SessionStableId,
            Inbound("preview"));
        var after = context.Service.Get(created.SessionStableId);

        Assert.Equal(created.Revision, after.Revision);
        Assert.Equal(created.CurrentTick, after.CurrentTick);
        Assert.Empty(after.SpatialReservations);
        Assert.NotNull(preview.SpatialInteraction);
        Assert.Equal(Simulation공간근거종류Codes.Scenario,
            preview.SpatialInteraction!.EvidenceKindCode);
        Assert.Equal(PyeongchangSimulation공간StableIds.진부Hub검수공간,
            preview.TaskPlan.SelectedSpatialStableId);
        Assert.Empty(preview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 지정한공간이부적합하면_다른공간으로자동대체하지않는다()
    {
        var context = CreateContext();
        var created = context.Service.Create(CreateRequest());
        var request = Inbound("preferred");
        request.Task.PreferredSpatialStableId = PyeongchangSimulation공간StableIds.진부Hub창고공간;

        var preview = context.Service.PreviewDecision(created.SessionStableId, request);

        Assert.Contains(Simulation공간차단Codes.CapabilityMissing,
            preview.Decision.BlockReasonCodes);
        Assert.Equal(string.Empty, preview.TaskPlan.SelectedSpatialStableId);
    }

    [Fact]
    public void 같은검수작업영역은_활성작업이중복예약하지못하고_완료뒤해제된다()
    {
        var context = CreateContext();
        var current = context.Service.Create(CreateRequest());
        current = Confirm(context, current, Inbound("first"), "command:spatial:first");

        var reservation = Assert.Single(current.SpatialReservations);
        Assert.Equal(Simulation공간예약상태Codes.Reserved, reservation.StatusCode);
        Assert.Equal(1m, Capacity(current, PyeongchangSimulation공간StableIds.진부Hub검수공간,
            reserved: true, Simulation공간용량Codes.WorkArea));

        var second = context.Service.PreviewDecision(current.SessionStableId, Inbound("second"));
        Assert.Contains(Simulation공간차단Codes.ReservationConflict,
            second.Decision.BlockReasonCodes);

        current = Tick(context, current, "command:spatial:first:tick-1");
        current = Tick(context, current, "command:spatial:first:tick-2");
        current = Tick(context, current, "command:spatial:first:tick-3");
        Assert.Equal(Simulation공간예약상태Codes.Released,
            Assert.Single(current.SpatialReservations).StatusCode);
        Assert.Equal(0m, Capacity(current, PyeongchangSimulation공간StableIds.진부Hub검수공간,
            reserved: true, Simulation공간용량Codes.WorkArea));
    }

    [Fact]
    public void 창고적재는_보관용량과작업영역을예약하고_완료시사용량으로전환한다()
    {
        var context = CreateContext();
        var current = CompleteInbound(context, "put-away");
        var inventory = Assert.Single(current.NpcFacilityInventories);
        var request = PutAway(inventory);

        var preview = context.Service.PreviewWarehousePutAway(current.SessionStableId, request);
        Assert.Equal(PyeongchangSimulation공간StableIds.진부Hub창고공간,
            preview.TaskPlan.SelectedSpatialStableId);
        current = context.Service.ConfirmWarehousePutAway(current.SessionStableId,
            new SimulationWarehousePutAwayConfirmRequest
            {
                CommandId = "command:spatial:put-away:confirm",
                ExpectedRevision = current.Revision,
                PutAway = request,
            });

        Assert.Equal(2, current.SpatialReservations.Count(value =>
            value.TaskStableId.Contains("warehouse-put-away", StringComparison.Ordinal)));
        Assert.Equal(100m, Capacity(current, PyeongchangSimulation공간StableIds.진부Hub창고공간,
            reserved: true, Simulation공간용량Codes.StorageCapacity));

        current = Tick(context, current, "command:spatial:put-away:storage-tick-1");
        current = Tick(context, current, "command:spatial:put-away:storage-tick-2");
        current = Tick(context, current, "command:spatial:put-away:storage-tick-3");
        Assert.Equal(SimulationTaskStateCodes.Completed, current.Tasks.Single(value =>
            value.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove).StateCode);
        Assert.Equal(100m, Capacity(current, PyeongchangSimulation공간StableIds.진부Hub창고공간,
            reserved: false, Simulation공간용량Codes.StorageCapacity));
        Assert.Equal(0m, Capacity(current, PyeongchangSimulation공간StableIds.진부Hub창고공간,
            reserved: true, Simulation공간용량Codes.StorageCapacity));
        Assert.Equal(SimulationNpcInventoryStateCodes.PutAwayCompleted,
            Assert.Single(current.NpcFacilityInventories).StateCode);
    }

    [Fact]
    public void 공간용량이부족하면_적재실행전검토에서차단한다()
    {
        var request = CreateRequest();
        request.SpatialWorld!.Definitions.Single(value => value.CapabilityCodes.Contains(
            Simulation공간능력Codes.Storage)).BaseCapacities.Single(value =>
                value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity = 50m;
        var context = CreateContext(request);
        var current = CompleteInbound(context, "capacity");
        var inventory = Assert.Single(current.NpcFacilityInventories);

        var preview = context.Service.PreviewWarehousePutAway(
            current.SessionStableId,
            PutAway(inventory));

        Assert.Contains(Simulation공간차단Codes.CapacityInsufficient,
            preview.Decision.BlockReasonCodes);
        Assert.DoesNotContain(current.SpatialReservations, value =>
            value.StatusCode == Simulation공간예약상태Codes.Reserved);
    }

    [Fact]
    public void 예약된검수작업을취소하면_계보에속한공간예약과임시재고만반환하고_저장재생된다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var context = CreateContext(CreateRequest(), saveStore);
        var current = context.Service.Create(context.CreateRequest);
        current = Confirm(context, current, Inbound("cancel"), "command:spatial:cancel:confirm");
        var task = Assert.Single(current.Tasks);
        var cancel = new SimulationTaskCancelRequest
        {
            CommandId = "command:spatial:cancel",
            ExpectedRevision = current.Revision,
            ReasonCode = "UserCancelled",
        };

        var cancelled = context.Service.CancelTask(current.SessionStableId, task.TaskStableId, cancel);
        var retried = context.Service.CancelTask(current.SessionStableId, task.TaskStableId, cancel);
        var stale = Assert.Throws<SimulationConflictException>(() => context.Service.CancelTask(
            current.SessionStableId,
            task.TaskStableId,
            new SimulationTaskCancelRequest
            {
                CommandId = "command:spatial:cancel:stale",
                ExpectedRevision = current.Revision,
                ReasonCode = "UserCancelled",
            }));

        Assert.Equal(cancelled.Revision, retried.Revision);
        Assert.Equal("SimulationExpectedRevisionMismatch", stale.ErrorCode);
        Assert.Equal(SimulationTaskStateCodes.Cancelled, Assert.Single(cancelled.Tasks).StateCode);
        Assert.All(cancelled.SpatialReservations, value =>
            Assert.Equal(Simulation공간예약상태Codes.Cancelled, value.StatusCode));
        Assert.Empty(cancelled.NpcFacilityInventories);
        Assert.Equal(SimulationNpcActionPhaseCodes.Cancelled,
            Assert.Single(cancelled.NpcTaskAssignments).PhaseCode);

        var saved = context.Service.Save(cancelled.SessionStableId, new SimulationSessionSaveRequest
        {
            SaveStableId = "save:sim:spatial-cancel",
            ExpectedRevision = cancelled.Revision,
        });
        Assert.Equal(SimulationSaveSchemaVersions.V2, saved.SchemaVersion);
        var restored = new 경영SimulationSessionService(
                new InMemory경영SimulationSessionStore(), saveStore)
            .Restore(new SimulationSessionRestoreRequest { SaveStableId = saved.SaveStableId });
        Assert.Equal(saved.ReplayHash, restored.ReplayHash);
        Assert.Equal(SimulationTaskStateCodes.Cancelled,
            Assert.Single(restored.Session.Tasks).StateCode);
        Assert.All(restored.Session.SpatialReservations, value =>
            Assert.Equal(Simulation공간예약상태Codes.Cancelled, value.StatusCode));
    }

    [Fact]
    public void 예정된창고적재를취소하면_공간예약을반환하고_검수완료재고를유지한다()
    {
        var context = CreateContext();
        var current = CompleteInbound(context, "put-away-cancel");
        var inventory = Assert.Single(current.NpcFacilityInventories);
        current = context.Service.ConfirmWarehousePutAway(current.SessionStableId,
            new SimulationWarehousePutAwayConfirmRequest
            {
                CommandId = "command:spatial:put-away-cancel:confirm",
                ExpectedRevision = current.Revision,
                PutAway = PutAway(inventory),
            });
        var task = current.Tasks.Single(value =>
            value.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove);

        var cancelled = context.Service.CancelTask(current.SessionStableId, task.TaskStableId,
            new SimulationTaskCancelRequest
            {
                CommandId = "command:spatial:put-away-cancel:cancel-task",
                ExpectedRevision = current.Revision,
                ReasonCode = "UserCancelled",
            });

        Assert.Equal(SimulationTaskStateCodes.Cancelled,
            cancelled.Tasks.Single(value => value.TaskStableId == task.TaskStableId).StateCode);
        Assert.Equal(SimulationNpcInventoryStateCodes.StorageEligible,
            Assert.Single(cancelled.NpcFacilityInventories).StateCode);
        Assert.All(cancelled.SpatialReservations.Where(value =>
                value.TaskStableId == task.TaskStableId),
            value => Assert.Equal(Simulation공간예약상태Codes.Cancelled, value.StatusCode));
        Assert.Equal(0m, Capacity(cancelled,
            PyeongchangSimulation공간StableIds.진부Hub창고공간,
            reserved: true, Simulation공간용량Codes.StorageCapacity));
        Assert.Equal(0m, Capacity(cancelled,
            PyeongchangSimulation공간StableIds.진부Hub창고공간,
            reserved: false, Simulation공간용량Codes.StorageCapacity));
    }

    private static TestContext CreateContext(
        경영SimulationSession생성Request? request = null,
        InMemorySimulationSessionSaveStore? saveStore = null)
        => new(new 경영SimulationSessionService(
                new InMemory경영SimulationSessionStore(),
                saveStore ?? new InMemorySimulationSessionSaveStore()),
            request ?? CreateRequest());

    private static 경영SimulationSessionSnapshot CompleteInbound(TestContext context, string suffix)
    {
        var current = context.Service.Create(context.CreateRequest);
        current = Confirm(context, current, Inbound(suffix), "command:spatial:" + suffix);
        current = Tick(context, current, "command:spatial:" + suffix + ":tick-1");
        current = Tick(context, current, "command:spatial:" + suffix + ":tick-2");
        return Tick(context, current, "command:spatial:" + suffix + ":tick-3");
    }

    private static 경영SimulationSessionSnapshot Confirm(
        TestContext context,
        경영SimulationSessionSnapshot current,
        SimulationDecisionPreviewRequest preview,
        string commandId)
        => context.Service.ConfirmDecision(current.SessionStableId,
            new SimulationDecisionConfirmRequest
            {
                CommandId = commandId,
                ExpectedRevision = current.Revision,
                Preview = preview,
            });

    private static 경영SimulationSessionSnapshot Tick(
        TestContext context,
        경영SimulationSessionSnapshot current,
        string commandId)
        => context.Service.Advance(current.SessionStableId, new 경영SimulationTick진행Request
        {
            CommandId = commandId,
            ExpectedRevision = current.Revision,
            TickCount = 1,
        });

    private static decimal Capacity(
        경영SimulationSessionSnapshot snapshot,
        string spatialStableId,
        bool reserved,
        string capacityCode)
    {
        var runtime = snapshot.SpatialRuntimeStates.Single(value =>
            value.SpatialStableId == spatialStableId);
        var capacities = reserved ? runtime.ReservedCapacities : runtime.OccupiedCapacities;
        return capacities.Single(value => value.CapacityCode == capacityCode).Quantity;
    }

    private static SimulationDecisionPreviewRequest Inbound(string suffix)
        => new SimulationDecisionPreviewRequest
        {
            DecisionStableId = "decision:spatial:inbound:" + suffix,
            DecisionTypeCode = SimulationNpcActionCodes.WarehouseInboundInspection,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
            TargetStableIds = new[] { "cargo:spatial:" + suffix },
            ExpectedEffects = new[]
            {
                new SimulationValueProjection
                {
                    ValueTypeCode = "FreightReceiptQuantity",
                    TargetLedgerStableId = "cargo:spatial:" + suffix,
                    BeforeValue = 0m,
                    Delta = 100m,
                    AfterValue = 100m,
                    UnitCode = "KGM",
                    SourceStableIds = new[] { "source:fixture:spatial:" + suffix },
                },
            },
            SourceStableIds = new[] { "source:fixture:spatial:" + suffix },
            Task = new SimulationTaskPlanRequest
            {
                TaskStableId = "task:spatial:inbound:" + suffix,
                TaskTypeCode = "FreightReceiptConfirmation",
                FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                ActionCode = SimulationNpcActionCodes.WarehouseInboundInspection,
                AssignedCapacity = 100m,
                AssignedCapacityUnitCode = "KGM",
                DurationTicks = 1,
                InputLotStableIds = new[] { "cargo:spatial:" + suffix },
                OutputCandidateCodes = new[] { SimulationNpcInventoryStateCodes.StorageEligible },
                SourceStableIds = new[] { "source:fixture:spatial:" + suffix },
            },
        };

    private static SimulationWarehousePutAwayPreviewRequest PutAway(
        SimulationNpcFacilityInventorySnapshot inventory)
        => new SimulationWarehousePutAwayPreviewRequest
        {
            InventoryStableId = inventory.InventoryStableId,
            InventoryRevision = inventory.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
            PutAwayDurationTicks = 2,
            SourceStableIds = new[] { inventory.InventoryStableId,
                PyeongchangSimulationWorldStableIds.창고적재규칙 },
        };

    private static 경영SimulationSession생성Request CreateRequest()
        => new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:pyeongchang:spatial-interaction",
            ScenarioDataRevision = "scenario-data:spatial:r1",
            ScenarioSeed = 240817,
            RuleRevision = "simulation-spatial-interaction:r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim:pyeongchang",
                TerritoryStableId = "territory:sim:pyeongchang",
                SettlementStableId = "settlement:sim:pyeongchang",
                GameDateStartsOn = new DateTimeOffset(2026, 8, 17, 0, 0, 0, TimeSpan.Zero),
            },
            NpcWorkforce = PyeongchangSimulationNpcWorkforceFixture.Create(),
            SpatialWorld = PyeongchangSimulation공간상호작용Fixture.Create(),
        };

    private sealed record TestContext(
        경영SimulationSessionService Service,
        경영SimulationSession생성Request CreateRequest);
}
