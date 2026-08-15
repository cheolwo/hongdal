using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationNpcWorkforceTests
{
    [Fact]
    public void 진부Hub입고검수는_이동과작업을거쳐_보관가능재고가된다()
    {
        var context = CreateContext();
        var created = context.Service.Create(CreateSessionRequest());
        var previewRequest = InboundInspection("1");

        var preview = context.Service.PreviewDecision(created.SessionStableId, previewRequest);
        Assert.Equal(3, preview.TaskPlan.DurationTicks);
        Assert.Equal(PyeongchangSimulationNpcStableIds.진부입고검수담당,
            preview.TaskPlan.AssignedActorStableId);

        var scheduled = context.Service.ConfirmDecision(
            created.SessionStableId,
            new SimulationDecisionConfirmRequest
            {
                CommandId = "command:npc-inbound-inspection:1",
                ExpectedRevision = created.Revision,
                Preview = previewRequest,
            });
        var assignment = Assert.Single(scheduled.NpcTaskAssignments);
        Assert.Equal(SimulationNpcActionPhaseCodes.Scheduled, assignment.PhaseCode);
        Assert.Equal(PyeongchangSimulationNpcStableIds.진부입고검수담당,
            assignment.ActorStableId);
        Assert.Equal(SimulationNpcInventoryStateCodes.PendingInspection,
            Assert.Single(scheduled.NpcFacilityInventories).StateCode);

        var navigating = Advance(context, scheduled, "command:npc-tick:1");
        Assert.Equal(SimulationNpcActionPhaseCodes.Navigating,
            Assert.Single(navigating.NpcActionProjections).PhaseCode);

        var working = Advance(context, navigating, "command:npc-tick:2");
        var workingProjection = Assert.Single(working.NpcActionProjections);
        Assert.Equal(SimulationNpcActionPhaseCodes.Working, workingProjection.PhaseCode);
        Assert.Equal(0.5m, workingProjection.ProgressRate);

        var completed = Advance(context, working, "command:npc-tick:3");
        Assert.Equal(SimulationTaskStateCodes.Completed, Assert.Single(completed.Tasks).StateCode);
        Assert.Equal(SimulationNpcActionPhaseCodes.Completed,
            Assert.Single(completed.NpcActionProjections).PhaseCode);
        Assert.Equal(SimulationNpcInventoryStateCodes.StorageEligible,
            Assert.Single(completed.NpcFacilityInventories).StateCode);
        var workRecord = Assert.Single(completed.NpcWorkRecords);
        Assert.Contains(SimulationNpcInventoryStateCodes.StorageEligible, workRecord.ResultCodes);
    }

    [Fact]
    public void 검수완료된_같은입고재고는_적재Npc작업을거쳐야만_적재완료가된다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var context = new TestContext(new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore));
        var current = context.Service.Create(CreateSessionRequest());
        current = Confirm(context, current, InboundInspection("put-away"), "command:npc-put-away:inspection");
        current = Advance(context, current, "command:npc-put-away:inspection-tick-1");
        current = Advance(context, current, "command:npc-put-away:inspection-tick-2");
        current = Advance(context, current, "command:npc-put-away:inspection-tick-3");
        var inventory = Assert.Single(current.NpcFacilityInventories);
        var request = new SimulationWarehousePutAwayPreviewRequest
        {
            InventoryStableId = inventory.InventoryStableId,
            InventoryRevision = inventory.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
            PutAwayDurationTicks = 2,
            SourceStableIds = new[]
            {
                inventory.InventoryStableId,
                PyeongchangSimulationWorldStableIds.창고적재규칙,
            },
        };

        var preview = context.Service.PreviewWarehousePutAway(current.SessionStableId, request);
        Assert.Empty(preview.Decision.BlockReasonCodes);
        Assert.Equal(3, preview.TaskPlan.DurationTicks);
        Assert.Equal(PyeongchangSimulationNpcStableIds.진부적재담당,
            preview.TaskPlan.AssignedActorStableId);

        var confirmRequest = new SimulationWarehousePutAwayConfirmRequest
        {
            CommandId = "command:npc-put-away:confirm",
            ExpectedRevision = current.Revision,
            PutAway = request,
        };
        current = context.Service.ConfirmWarehousePutAway(current.SessionStableId, confirmRequest);
        var retried = context.Service.ConfirmWarehousePutAway(current.SessionStableId, confirmRequest);
        Assert.Equal(current.Revision, retried.Revision);
        var assignment = Assert.Single(current.NpcTaskAssignments, value =>
            value.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove);
        Assert.Equal(PyeongchangSimulationNpcStableIds.진부적재담당, assignment.ActorStableId);
        Assert.Equal(SimulationNpcInventoryStateCodes.StorageEligible,
            Assert.Single(current.NpcFacilityInventories).StateCode);

        current = Advance(context, current, "command:npc-put-away:tick-1");
        current = Advance(context, current, "command:npc-put-away:tick-2");
        current = Advance(context, current, "command:npc-put-away:tick-3");

        Assert.Equal(SimulationNpcInventoryStateCodes.PutAwayCompleted,
            Assert.Single(current.NpcFacilityInventories).StateCode);
        Assert.Contains(current.NpcWorkRecords, value =>
            value.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove
            && value.ResultCodes.Contains(SimulationNpcInventoryStateCodes.PutAwayCompleted));
        Assert.False(current.IsOperationalState);

        var saved = context.Service.Save(current.SessionStableId, new SimulationSessionSaveRequest
        {
            SaveStableId = "save:sim:npc-put-away:1",
            ExpectedRevision = current.Revision,
        });
        var restored = new 경영SimulationSessionService(
                new InMemory경영SimulationSessionStore(),
                saveStore)
            .Restore(new SimulationSessionRestoreRequest { SaveStableId = saved.SaveStableId });
        Assert.Equal(SimulationNpcInventoryStateCodes.PutAwayCompleted,
            Assert.Single(restored.Session.NpcFacilityInventories).StateCode);
        Assert.Equal(2, restored.Session.NpcWorkRecords.Length);
        Assert.Equal(saved.ReplayHash, restored.ReplayHash);
    }

    [Fact]
    public void 두번째검수작업은_관리자초기권한으로_보조Npc에게동적위임된다()
    {
        var context = CreateContext();
        var current = context.Service.Create(CreateSessionRequest());
        current = Confirm(context, current, InboundInspection("1"), "command:npc-backlog:1");
        current = Confirm(context, current, InboundInspection("2"), "command:npc-backlog:2");

        Assert.Equal(2, current.NpcTaskAssignments.Length);
        Assert.Contains(current.NpcTaskAssignments, value =>
            value.ActorStableId == PyeongchangSimulationNpcStableIds.진부입고검수담당);
        Assert.Contains(current.NpcTaskAssignments, value =>
            value.ActorStableId == PyeongchangSimulationNpcStableIds.진부물류보조);
        var delegated = Assert.Single(current.NpcCapabilityGrants, value =>
            value.GrantKindCode == SimulationNpcGrantKindCodes.Delegated);
        Assert.Equal(PyeongchangSimulationNpcStableIds.진부Hub관리자,
            delegated.GrantedByActorStableId);
        Assert.False(delegated.CanDelegate);
        Assert.Equal(PyeongchangSimulationWorldStableIds.진부Hub시설,
            delegated.FacilityStableId);
    }

    [Fact]
    public void 자동화정책을끄면_작업은완료되지않고_명시적차단상태로남는다()
    {
        var context = CreateContext();
        var created = context.Service.Create(CreateSessionRequest());
        var policyRequest = new SimulationNpcPolicyChangeRequest
        {
            CommandId = "command:npc-policy:disable",
            ExpectedRevision = created.Revision,
            PolicyStableId = PyeongchangSimulationNpcStableIds.진부입고검수정책,
            AutomationEnabled = false,
            Priority = 100,
            PreferredActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
            AutoDelegationEnabled = false,
        };

        var changed = context.Service.UpdateNpcPolicy(created.SessionStableId, policyRequest);
        var retried = context.Service.UpdateNpcPolicy(created.SessionStableId, policyRequest);
        Assert.Equal(changed.Revision, retried.Revision);

        var blocked = Confirm(
            context,
            changed,
            InboundInspection("blocked"),
            "command:npc-blocked:1");
        Assert.Equal(SimulationTaskStateCodes.Blocked, Assert.Single(blocked.Tasks).StateCode);
        Assert.Contains("SimulationNpcAutomationDisabled",
            Assert.Single(blocked.NpcTaskAssignments).BlockReasonCodes);

        var advanced = Advance(context, blocked, "command:npc-blocked:tick");
        Assert.Equal(SimulationTaskStateCodes.Blocked, Assert.Single(advanced.Tasks).StateCode);
        Assert.Empty(advanced.NpcWorkRecords);
        Assert.Empty(advanced.NpcFacilityInventories);
    }

    [Fact]
    public void 저장복원은_Npc배정과정책과작업이력과Hash를동일하게보존한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var context = new TestContext(new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore));
        var current = context.Service.Create(CreateSessionRequest());
        current = Confirm(context, current, InboundInspection("save"), "command:npc-save:confirm");
        current = Advance(context, current, "command:npc-save:tick-1");
        current = Advance(context, current, "command:npc-save:tick-2");
        current = Advance(context, current, "command:npc-save:tick-3");
        var saved = context.Service.Save(current.SessionStableId, new SimulationSessionSaveRequest
        {
            SaveStableId = "save:sim:npc-workforce:1",
            ExpectedRevision = current.Revision,
        });

        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore);
        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });
        var savedAgain = restoreService.Save(restored.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim:npc-workforce:2",
                ExpectedRevision = restored.Session.Revision,
            });

        Assert.Equal(saved.ReplayHash, savedAgain.ReplayHash);
        Assert.Equal(current.NpcTaskAssignments[0].ActorStableId,
            restored.Session.NpcTaskAssignments[0].ActorStableId);
        Assert.Equal(current.NpcWorkRecords[0].CompletedTick,
            restored.Session.NpcWorkRecords[0].CompletedTick);
        Assert.Equal(SimulationNpcInventoryStateCodes.StorageEligible,
            restored.Session.NpcFacilityInventories[0].StateCode);
    }

    [Fact]
    public void 같은입력과Seed는_같은Npc배정을만든다()
    {
        var first = RunDeterministicFixture();
        var second = RunDeterministicFixture();

        Assert.Equal(
            first.NpcTaskAssignments.Select(value => value.ActorStableId),
            second.NpcTaskAssignments.Select(value => value.ActorStableId));
        Assert.Equal(
            first.NpcCapabilityGrants.Select(value => value.GrantStableId),
            second.NpcCapabilityGrants.Select(value => value.GrantStableId));
    }

    [Fact]
    public void Npc규칙계층은_운영사용자와HrAssembly를참조하지않는다()
    {
        var properties = typeof(SimulationNpcActorSnapshot).GetProperties()
            .Concat(typeof(SimulationNpcCapabilityGrantSnapshot).GetProperties())
            .Select(value => value.Name)
            .ToArray();
        Assert.DoesNotContain(properties, value => value.Contains("UserId", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(경영SimulationSessionAggregate).Assembly.GetReferencedAssemblies(),
            value => value.Name != null
                && (value.Name.Contains("Infrastructure", StringComparison.Ordinal)
                    || value.Name.Contains("Identity", StringComparison.Ordinal)
                    || value.Name.Contains("Hr", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Npc정책API는_로그인없이_Simulation세션정책만변경한다()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var createRequest = CreateSessionRequest();
        using var createResponse = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions",
            createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<경영SimulationSessionSnapshot>();
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);

        using var response = await client.PostAsJsonAsync(
            $"/api/simulation/v1/sessions/{created.SessionStableId}/npc-policies",
            new SimulationNpcPolicyChangeRequest
            {
                CommandId = "command:http:npc-policy:1",
                ExpectedRevision = created.Revision,
                PolicyStableId = PyeongchangSimulationNpcStableIds.진부입고검수정책,
                AutomationEnabled = true,
                Priority = 120,
                PreferredActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
                AutoDelegationEnabled = true,
            });
        var changed = await response.Content.ReadFromJsonAsync<경영SimulationSessionSnapshot>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(changed);
        Assert.False(changed.IsOperationalState);
        Assert.Equal(120, Assert.Single(changed.NpcWorkPolicies, value =>
            value.PolicyStableId == PyeongchangSimulationNpcStableIds.진부입고검수정책).Priority);
    }

    private static 경영SimulationSessionSnapshot RunDeterministicFixture()
    {
        var context = CreateContext();
        var current = context.Service.Create(CreateSessionRequest());
        current = Confirm(context, current, InboundInspection("1"), "command:npc-deterministic:1");
        return Confirm(context, current, InboundInspection("2"), "command:npc-deterministic:2");
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

    private static 경영SimulationSessionSnapshot Advance(
        TestContext context,
        경영SimulationSessionSnapshot current,
        string commandId)
        => context.Service.Advance(current.SessionStableId, new 경영SimulationTick진행Request
        {
            CommandId = commandId,
            ExpectedRevision = current.Revision,
            TickCount = 1,
        });

    private static SimulationDecisionPreviewRequest InboundInspection(string suffix)
        => new SimulationDecisionPreviewRequest
        {
            DecisionStableId = "decision:npc-inbound:" + suffix,
            DecisionTypeCode = "WarehouseInboundInspection",
            ActorStableId = PyeongchangSimulationNpcStableIds.진부Hub관리자,
            TargetStableIds = new[] { "cargo:sim:potato:" + suffix },
            ExpectedEffects = new[]
            {
                new SimulationValueProjection
                {
                    ValueTypeCode = "StorageEligibleQuantity",
                    TargetLedgerStableId = "inventory:sim:potato:" + suffix,
                    BeforeValue = 0m,
                    Delta = 100m,
                    AfterValue = 100m,
                    UnitCode = "KGM",
                    SourceStableIds = new[] { "source:fixture:npc-inbound:" + suffix },
                },
            },
            SourceStableIds = new[] { "source:fixture:npc-inbound:" + suffix },
            Task = new SimulationTaskPlanRequest
            {
                TaskStableId = "task:npc-inbound:" + suffix,
                TaskTypeCode = "FreightReceiptConfirmation",
                FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                ActionCode = SimulationNpcActionCodes.WarehouseInboundInspection,
                AssignedCapacity = 100m,
                AssignedCapacityUnitCode = "KGM",
                DurationTicks = 1,
                InputLotStableIds = new[] { "cargo:sim:potato:" + suffix },
                OutputCandidateCodes = new[] { SimulationNpcInventoryStateCodes.StorageEligible },
                SourceStableIds = new[] { "source:fixture:npc-inbound:" + suffix },
            },
        };

    private static 경영SimulationSession생성Request CreateSessionRequest()
        => new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.Parse("2731b15d-1d1f-4f4f-88d4-89ae8013790a"),
            ScenarioStableId = "scenario:pyeongchang-farm-hub-town:npc-workforce",
            ScenarioDataRevision = "scenario-data:pyeongchang:npc-workforce:r1",
            ScenarioSeed = 240813,
            RuleRevision = "simulation-npc-workforce:r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim:pyeongchang",
                TerritoryStableId = "territory:sim:pyeongchang",
                SettlementStableId = "settlement:sim:pyeongchang",
                GameDateStartsOn = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            },
            NpcWorkforce = PyeongchangSimulationNpcWorkforceFixture.Create(),
        };

    private static TestContext CreateContext()
        => new(new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            new InMemorySimulationSessionSaveStore()));

    private static WebApplicationFactory<Program> CreateFactory()
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["SsalddelExecution:Mode"] = "Simulation",
                        ["SimulationServer:Enabled"] = "true",
                        ["SimulationSharedPublicData:Enabled"] = "false",
                    });
                });
            });

    private sealed record TestContext(경영SimulationSessionService Service);
}
