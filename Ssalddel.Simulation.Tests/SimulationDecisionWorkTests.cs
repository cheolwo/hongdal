using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationDecisionWorkTests
{
    [Fact]
    public void Preview는_예상값과작업계획을반환하지만_session을변경하지않는다()
    {
        var service = Service();
        var session = CreateSession(service);

        var preview = service.PreviewDecision(session.SessionStableId, PreviewRequest());
        var unchanged = service.Get(session.SessionStableId);

        Assert.Equal(SimulationDecisionStateCodes.Previewed, preview.Decision.StateCode);
        Assert.Equal(0, preview.Decision.Revision);
        Assert.Equal(session.SessionStableId, preview.Decision.SessionStableId);
        Assert.Equal(session.WorldContext.FactionStableId, preview.Decision.FactionStableId);
        Assert.Equal(session.WorldContext.TerritoryStableId, preview.Decision.TerritoryStableId);
        Assert.Equal(session.WorldContext.SettlementStableId, preview.Decision.SettlementStableId);
        Assert.Equal(300m, preview.TaskPlan.AssignedCapacity);
        Assert.Equal(0, unchanged.Revision);
        Assert.Empty(unchanged.Decisions);
        Assert.Empty(unchanged.Tasks);
        Assert.Empty(unchanged.Effects);
    }

    [Fact]
    public void Confirm은_Decision과ScheduledTask와PendingEffect를분리해기록한다()
    {
        var service = Service();
        var session = CreateSession(service);

        var confirmed = service.ConfirmDecision(
            session.SessionStableId,
            ConfirmRequest(0));

        Assert.Equal(0, confirmed.CurrentTick);
        Assert.Equal(1, confirmed.Revision);
        var decision = Assert.Single(confirmed.Decisions);
        var task = Assert.Single(confirmed.Tasks);
        var effect = Assert.Single(confirmed.Effects);
        Assert.Equal(SimulationDecisionStateCodes.Confirmed, decision.StateCode);
        Assert.Equal(0, decision.ConfirmedTick);
        Assert.Equal(SimulationTaskStateCodes.Scheduled, task.StateCode);
        Assert.Equal(decision.DecisionStableId, task.CausedByDecisionStableId);
        Assert.Equal(1, task.ScheduledStartTick);
        Assert.Equal(1, task.ExpectedEndTick);
        Assert.Equal(SimulationEffectStateCodes.Pending, effect.StateCode);
        Assert.Null(effect.AppliedTick);
        Assert.Equal(task.TaskStableId, effect.CausedByTaskStableId);
        Assert.Equal(1000m, effect.BeforeValue);
        Assert.Equal(-300m, effect.Delta);
        Assert.Equal(700m, effect.AfterValue);
    }

    [Fact]
    public void 완료Tick만_Task를완료하고_Effect를적용하며_Decision은변경하지않는다()
    {
        var service = Service();
        var session = CreateSession(service);
        var confirmed = service.ConfirmDecision(session.SessionStableId, ConfirmRequest(0));

        var advanced = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.decision-work-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });

        var decision = Assert.Single(advanced.Decisions);
        var task = Assert.Single(advanced.Tasks);
        var effect = Assert.Single(advanced.Effects);
        Assert.Equal(SimulationDecisionStateCodes.Confirmed, decision.StateCode);
        Assert.Equal(1, decision.Revision);
        Assert.Equal(SimulationTaskStateCodes.Completed, task.StateCode);
        Assert.Equal(2, task.Revision);
        Assert.Equal(1, task.ActualEndTick);
        Assert.Equal(SimulationEffectStateCodes.Applied, effect.StateCode);
        Assert.Equal(2, effect.Revision);
        Assert.Equal(1, effect.AppliedTick);
        Assert.Equal(1, advanced.WorldContext.WorldTick);
        Assert.Equal(2, advanced.WorldContext.WorldRevision);
    }

    [Fact]
    public void 여러Tick작업은_중간에InProgress이고_예정종료Tick에완료된다()
    {
        var service = Service();
        var session = CreateSession(service);
        var preview = PreviewRequest(durationTicks: 2);
        var confirmed = service.ConfirmDecision(
            session.SessionStableId,
            ConfirmRequest(0, preview));

        var progressing = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.decision-work-progress",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });
        Assert.Equal(SimulationTaskStateCodes.InProgress, Assert.Single(progressing.Tasks).StateCode);
        Assert.Equal(SimulationEffectStateCodes.Pending, Assert.Single(progressing.Effects).StateCode);

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.decision-work-complete",
                ExpectedRevision = progressing.Revision,
                TickCount = 1,
            });
        Assert.Equal(SimulationTaskStateCodes.Completed, Assert.Single(completed.Tasks).StateCode);
        Assert.Equal(2, Assert.Single(completed.Tasks).ActualEndTick);
        Assert.Equal(SimulationEffectStateCodes.Applied, Assert.Single(completed.Effects).StateCode);
        Assert.Equal(2, Assert.Single(completed.Effects).AppliedTick);
    }

    [Fact]
    public void 차단사유가있는Preview는_조회할수있지만_Confirm할수없다()
    {
        var service = Service();
        var session = CreateSession(service);
        var request = PreviewRequest();
        request.BlockReasonCodes = new[] { "InsufficientLaborCapacity" };

        var preview = service.PreviewDecision(session.SessionStableId, request);
        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmDecision(session.SessionStableId, ConfirmRequest(0, request)));

        Assert.Equal("InsufficientLaborCapacity", Assert.Single(preview.Decision.BlockReasonCodes));
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Empty(service.Get(session.SessionStableId).Decisions);
    }

    [Fact]
    public void 예상값의_Before와Delta와After_보존식이맞지않으면거부한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var request = PreviewRequest();
        request.ExpectedEffects[0].AfterValue = 701m;

        var error = Assert.Throws<SimulationContractException>(() =>
            service.PreviewDecision(session.SessionStableId, request));

        Assert.Equal("SimulationValueConservationInvalid", error.ErrorCode);
    }

    [Fact]
    public void Confirm_Command재시도는_deepClone된동일결과를반환한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var command = ConfirmRequest(0);
        var first = service.ConfirmDecision(session.SessionStableId, command);
        first.Decisions[0].DecisionTypeCode = "mutated-outside";
        first.Tasks[0].StateCode = "mutated-outside";
        first.Effects[0].AfterValue = -1m;

        var retry = service.ConfirmDecision(session.SessionStableId, command);

        Assert.Equal("HarvestDisposition", retry.Decisions[0].DecisionTypeCode);
        Assert.Equal(SimulationTaskStateCodes.Scheduled, retry.Tasks[0].StateCode);
        Assert.Equal(700m, retry.Effects[0].AfterValue);
        Assert.Equal(1, retry.Revision);
    }

    [Fact]
    public void 같은CommandId의다른payload는_충돌로거부한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var first = ConfirmRequest(0);
        service.ConfirmDecision(session.SessionStableId, first);
        var changed = ConfirmRequest(0);
        changed.Preview.DecisionTypeCode = "DifferentDecision";

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmDecision(session.SessionStableId, changed));

        Assert.Equal("SimulationCommandPayloadConflict", error.ErrorCode);
    }

    [Fact]
    public void Tick과DecisionConfirm은_같은CommandId를공유할수없다()
    {
        var service = Service();
        var session = CreateSession(service);
        var tick = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:shared-kind-1",
                ExpectedRevision = 0,
                TickCount = 1,
            });
        var confirm = ConfirmRequest(tick.Revision);
        confirm.CommandId = "command:shared-kind-1";

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmDecision(session.SessionStableId, confirm));

        Assert.Equal("SimulationCommandKindConflict", error.ErrorCode);
        Assert.Empty(service.Get(session.SessionStableId).Decisions);
    }

    [Fact]
    public void staleRevision과중복DecisionStableId를_각각거부한다()
    {
        var service = Service();
        var session = CreateSession(service);
        var confirmed = service.ConfirmDecision(session.SessionStableId, ConfirmRequest(0));

        var stale = ConfirmRequest(0, PreviewRequest("decision:sim.harvest.route-2", "task:sim.harvest.route-2"));
        stale.CommandId = "command:decision.confirm-stale";
        var staleError = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmDecision(session.SessionStableId, stale));

        var duplicate = ConfirmRequest(
            confirmed.Revision,
            PreviewRequest("decision:sim.harvest.route-1", "task:sim.harvest.route-2"));
        duplicate.CommandId = "command:decision.confirm-duplicate";
        var duplicateError = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmDecision(session.SessionStableId, duplicate));

        Assert.Equal("SimulationExpectedRevisionMismatch", staleError.ErrorCode);
        Assert.Equal("SimulationDecisionStableIdConflict", duplicateError.ErrorCode);
    }

    [Fact]
    public void source와target은_중복없이결정적순서로보존된다()
    {
        var service = Service();
        var session = CreateSession(service);
        var request = PreviewRequest();
        request.TargetStableIds = new[] { "harvest-lot:potato-1", "product:potato" };
        request.SourceStableIds = new[] { "source:rule-1", "harvest-lot:potato-1" };

        var preview = service.PreviewDecision(session.SessionStableId, request);

        Assert.Equal(
            new[] { "harvest-lot:potato-1", "product:potato" },
            preview.Decision.TargetStableIds);
        Assert.Equal(
            new[] { "harvest-lot:potato-1", "source:rule-1" },
            preview.Decision.SourceStableIds);
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
            ScenarioStableId = "scenario:sim.decision-work-0",
            ScenarioDataRevision = "scenario-data:r1",
            ScenarioSeed = 20260810,
            RuleRevision = "rule:r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim.farmers-1",
                TerritoryStableId = "territory:sim.farm-region-1",
                SettlementStableId = "settlement:sim.farm-town-1",
                GameDateStartsOn = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
        });

    private static SimulationDecisionConfirmRequest ConfirmRequest(
        long expectedRevision,
        SimulationDecisionPreviewRequest? preview = null)
        => new()
        {
            CommandId = "command:decision.confirm-harvest-route-1",
            ExpectedRevision = expectedRevision,
            Preview = preview ?? PreviewRequest(),
        };

    private static SimulationDecisionPreviewRequest PreviewRequest(
        string decisionStableId = "decision:sim.harvest.route-1",
        string taskStableId = "task:sim.harvest.route-1",
        int durationTicks = 1)
        => new()
        {
            DecisionStableId = decisionStableId,
            DecisionTypeCode = "HarvestDisposition",
            ActorStableId = "actor:sim.farmer-1",
            TargetStableIds = new[] { "harvest-lot:potato-1" },
            ExpectedCosts = new[]
            {
                new SimulationValueProjection
                {
                    ValueTypeCode = "LaborCost",
                    TargetLedgerStableId = "ledger:sim.labor-1",
                    BeforeValue = 40m,
                    Delta = -4m,
                    AfterValue = 36m,
                    UnitCode = "LaborHour",
                    SourceStableIds = new[] { "source:rule-1" },
                },
            },
            ExpectedEffects = new[]
            {
                new SimulationValueProjection
                {
                    ValueTypeCode = "ReserveStockAllocation",
                    TargetLedgerStableId = "ledger:sim.potato-stock-1",
                    BeforeValue = 1000m,
                    Delta = -300m,
                    AfterValue = 700m,
                    UnitCode = "KGM",
                    SourceStableIds = new[] { "harvest-lot:potato-1", "source:rule-1" },
                },
            },
            Uncertainties = new[] { "MarketPriceMayChange" },
            SourceStableIds = new[] { "harvest-lot:potato-1", "source:rule-1" },
            Task = new SimulationTaskPlanRequest
            {
                TaskStableId = taskStableId,
                TaskTypeCode = "HarvestDispositionWork",
                FacilityStableId = "facility:sim.farm-packing-1",
                AssignedCapacity = 300m,
                AssignedCapacityUnitCode = "KGM",
                DurationTicks = durationTicks,
                InputLotStableIds = new[] { "harvest-lot:potato-1" },
                OutputCandidateCodes = new[] { "CooperativeIntakeCandidate" },
                SourceStableIds = new[] { "harvest-lot:potato-1" },
            },
        };
}
