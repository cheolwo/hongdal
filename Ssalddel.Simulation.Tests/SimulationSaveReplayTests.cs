using System.Text.Json;
using System.Text.Json.Nodes;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E3저장재생검증)]
public sealed class SimulationSaveReplayTests
{
    [Fact]
    public void 기존_v1_JSON에_Battles가_없어도_빈목록과_기존hash로복원한다()
    {
        var service = Service(
            new InMemory경영SimulationSessionStore(),
            new InMemorySimulationSessionSaveStore());
        var session = CreateSession(service);
        var saved = service.Save(
            session.SessionStableId,
            SaveRequest("save:sim.legacy-without-battles", 0));
        var json = JsonNode.Parse(JsonSerializer.Serialize(saved))!.AsObject();

        Assert.True(json.Remove(nameof(SimulationSessionSavePackage.Battles)));
        var legacy = JsonSerializer.Deserialize<SimulationSessionSavePackage>(
            json.ToJsonString());
        var restored = SimulationSessionReplay.Restore(Assert.IsType<
            SimulationSessionSavePackage>(legacy));

        Assert.Empty(legacy.Battles);
        Assert.Equal(saved.ReplayHash, legacy.ReplayHash);
        Assert.Equal(saved.SessionStableId, restored.Snapshot().SessionStableId);
    }

    [Fact]
    public void Save는_version과hash를기록하지만_session을변경하지않는다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);

        var saved = service.Save(session.SessionStableId, SaveRequest("save:sim.empty-1", 0));
        var unchanged = service.Get(session.SessionStableId);

        Assert.Equal(SimulationSaveSchemaVersions.V7, saved.SchemaVersion);
        Assert.Equal(SimulationReplayHashAlgorithmCodes.Sha256, saved.ReplayHashAlgorithmCode);
        Assert.Equal(64, saved.ReplayHash.Length);
        Assert.Equal(0, saved.SavedWorldTick);
        Assert.Equal(0, saved.SavedWorldRevision);
        Assert.Empty(saved.CommandLog);
        Assert.Equal(0, unchanged.Revision);
    }

    [Fact]
    public void Confirm과Tick은_appendOnly순서와결과위치를보존한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        var confirmed = service.ConfirmDecision(
            session.SessionStableId,
            ConfirmRequest(expectedRevision: 0));
        service.ConfirmDecision(
            session.SessionStableId,
            ConfirmRequest(expectedRevision: 0));
        var tickRequest = TickRequest("command:save-replay.tick-1", confirmed.Revision);
        var advanced = service.Advance(
            session.SessionStableId,
            tickRequest);
        service.Advance(session.SessionStableId, tickRequest);

        var saved = service.Save(
            session.SessionStableId,
            SaveRequest("save:sim.command-log-1", advanced.Revision));

        Assert.Collection(
            saved.CommandLog,
            first =>
            {
                Assert.Equal(1, first.Sequence);
                Assert.Equal(SimulationCommandTypeCodes.DecisionConfirm, first.CommandTypeCode);
                Assert.Equal(0, first.AppliedWorldTick);
                Assert.Equal(1, first.ResultingWorldRevision);
                Assert.NotNull(first.DecisionConfirmRequest);
                Assert.Null(first.TickRequest);
            },
            second =>
            {
                Assert.Equal(2, second.Sequence);
                Assert.Equal(SimulationCommandTypeCodes.TickAdvance, second.CommandTypeCode);
                Assert.Equal(1, second.AppliedWorldTick);
                Assert.Equal(2, second.ResultingWorldRevision);
                Assert.NotNull(second.TickRequest);
                Assert.Null(second.DecisionConfirmRequest);
            });
    }

    [Fact]
    public void 새sessionStore에서_Command를replay하면_동일hash와상태를복원한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var sourceService = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var source = CreateSession(sourceService);
        var confirmed = sourceService.ConfirmDecision(
            source.SessionStableId,
            ConfirmRequest(0));
        var completed = sourceService.Advance(
            source.SessionStableId,
            TickRequest("command:save-replay.tick-restore", confirmed.Revision));
        var saved = sourceService.Save(
            source.SessionStableId,
            SaveRequest("save:sim.restore-1", completed.Revision));

        var restoredService = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });
        var resaved = restoredService.Save(
            restored.Session.SessionStableId,
            SaveRequest("save:sim.restore-check-1", restored.Session.Revision));

        Assert.Equal(2, restored.ReplayedCommandCount);
        Assert.Equal(saved.ReplayHash, restored.ReplayHash);
        Assert.Equal(saved.ReplayHash, resaved.ReplayHash);
        Assert.Equal(completed.CurrentTick, restored.Session.CurrentTick);
        Assert.Equal(completed.Revision, restored.Session.Revision);
        Assert.Equal(completed.WorldContext.GameDate, restored.Session.WorldContext.GameDate);
        Assert.Equal(completed.Decisions[0].DecisionStableId, restored.Session.Decisions[0].DecisionStableId);
        Assert.Equal(SimulationTaskStateCodes.Completed, restored.Session.Tasks[0].StateCode);
        Assert.Equal(SimulationEffectStateCodes.Applied, restored.Session.Effects[0].StateCode);
    }

    [Fact]
    public void 활성session의_저장본검증은_현재상태를덮어쓰지않고_동일hash를재현한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        var savedTick = service.Advance(
            session.SessionStableId,
            TickRequest("command:save-replay.verify-saved", 0));
        var saved = service.Save(
            session.SessionStableId,
            SaveRequest("save:sim.verify-active", savedTick.Revision));
        var current = service.Advance(
            session.SessionStableId,
            TickRequest("command:save-replay.verify-current", savedTick.Revision));

        var verified = service.VerifyReplay(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });

        Assert.Equal(saved.ReplayHash, verified.ReplayHash);
        Assert.Equal(savedTick.Revision, verified.Session.Revision);
        Assert.Equal(savedTick.CurrentTick, verified.Session.CurrentTick);
        Assert.Equal(current.Revision, service.Get(session.SessionStableId).Revision);
        Assert.Equal(current.CurrentTick, service.Get(session.SessionStableId).CurrentTick);
    }

    [Fact]
    public void 같은저장점은_SaveStableId가달라도_동일ReplayHash를가진다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        var ticked = service.Advance(
            session.SessionStableId,
            TickRequest("command:save-replay.deterministic", 0));

        var first = service.Save(
            session.SessionStableId,
            SaveRequest("save:sim.deterministic-a", ticked.Revision));
        var second = service.Save(
            session.SessionStableId,
            SaveRequest("save:sim.deterministic-b", ticked.Revision));

        Assert.Equal(first.ReplayHash, second.ReplayHash);
    }

    [Fact]
    public void 저장반환값을변경해도_store의package는오염되지않는다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        var saved = service.Save(session.SessionStableId, SaveRequest("save:sim.clone-1", 0));
        var expectedHash = saved.ReplayHash;
        saved.Snapshot.WorldContext.SettlementStableId = "settlement:sim.mutated";
        saved.SessionCreateRequest.WorldContext.SettlementStableId = "settlement:sim.mutated";

        var stored = saveStore.Find("save:sim.clone-1");

        Assert.NotNull(stored);
        Assert.Equal(expectedHash, stored!.ReplayHash);
        Assert.Equal("settlement:sim.farm-town-1", stored.Snapshot.WorldContext.SettlementStableId);
        Assert.Equal("settlement:sim.farm-town-1", stored.SessionCreateRequest.WorldContext.SettlementStableId);
    }

    [Fact]
    public void staleRevision과_같은SaveStableId의다른상태를거부한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        service.Save(session.SessionStableId, SaveRequest("save:sim.conflict-1", 0));
        var ticked = service.Advance(
            session.SessionStableId,
            TickRequest("command:save-replay.conflict", 0));

        var staleError = Assert.Throws<SimulationConflictException>(() =>
            service.Save(session.SessionStableId, SaveRequest("save:sim.stale-1", 0)));
        var conflictError = Assert.Throws<SimulationConflictException>(() =>
            service.Save(
                session.SessionStableId,
                SaveRequest("save:sim.conflict-1", ticked.Revision)));

        Assert.Equal("SimulationExpectedRevisionMismatch", staleError.ErrorCode);
        Assert.Equal("SimulationSaveStableIdConflict", conflictError.ErrorCode);
    }

    [Fact]
    public void 지원하지않는schema는_Command를실행하기전에거부한다()
    {
        var package = CreateSavedPackage();
        package.SchemaVersion = "simulation-save.v999";
        var targetStore = new InMemory경영SimulationSessionStore();
        var service = Service(targetStore, new StubSaveStore(package));

        var error = Assert.Throws<SimulationContractException>(() =>
            service.Restore(new SimulationSessionRestoreRequest { SaveStableId = package.SaveStableId }));

        Assert.Equal("SimulationSaveSchemaUnsupported", error.ErrorCode);
        Assert.Null(targetStore.Find(package.SessionStableId));
    }

    [Fact]
    public void Command순서변조는복원하지않는다()
    {
        var package = CreateSavedPackage(withDecision: true);
        package.CommandLog[0].Sequence = 2;
        var targetStore = new InMemory경영SimulationSessionStore();
        var service = Service(targetStore, new StubSaveStore(package));

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.Restore(new SimulationSessionRestoreRequest { SaveStableId = package.SaveStableId }));

        Assert.Equal("SimulationCommandLogSequenceInvalid", error.ErrorCode);
        Assert.Null(targetStore.Find(package.SessionStableId));
    }

    [Fact]
    public void Snapshot이나hash변조는_실패원자성을유지한다()
    {
        var package = CreateSavedPackage(withDecision: true);
        package.Snapshot.WorldContext.SettlementStableId = "settlement:sim.tampered";
        var targetStore = new InMemory경영SimulationSessionStore();
        var service = Service(targetStore, new StubSaveStore(package));

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.Restore(new SimulationSessionRestoreRequest { SaveStableId = package.SaveStableId }));

        Assert.Equal("SimulationReplayHashMismatch", error.ErrorCode);
        Assert.Null(targetStore.Find(package.SessionStableId));
    }

    [Fact]
    public void 이미활성인Session위에는_restore로덮어쓰지않는다()
    {
        var package = CreateSavedPackage();
        var targetStore = new InMemory경영SimulationSessionStore();
        var service = Service(targetStore, new StubSaveStore(package));
        service.Create(package.SessionCreateRequest);

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.Restore(new SimulationSessionRestoreRequest { SaveStableId = package.SaveStableId }));

        Assert.Equal("SimulationSessionAlreadyActive", error.ErrorCode);
    }

    private static SimulationSessionSavePackage CreateSavedPackage(bool withDecision = false)
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(new InMemory경영SimulationSessionStore(), saveStore);
        var session = CreateSession(service);
        var revision = session.Revision;
        if (withDecision)
            revision = service.ConfirmDecision(session.SessionStableId, ConfirmRequest(revision)).Revision;
        return service.Save(
            session.SessionStableId,
            SaveRequest("save:sim.fixture-1", revision));
    }

    private static 경영SimulationSessionService Service(
        I경영SimulationSessionStore sessionStore,
        ISimulationSessionSaveStore saveStore)
        => new(sessionStore, saveStore);

    private static 경영SimulationSessionSnapshot CreateSession(
        경영SimulationSessionService service)
        => service.Create(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.Parse("ddcf98a2-8a7a-4dc4-82c7-0d2477efec31"),
            ScenarioStableId = "scenario:sim.save-replay-0",
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

    private static SimulationSessionSaveRequest SaveRequest(string saveStableId, long expectedRevision)
        => new()
        {
            SaveStableId = saveStableId,
            ExpectedRevision = expectedRevision,
        };

    private static 경영SimulationTick진행Request TickRequest(string commandId, long expectedRevision)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = expectedRevision,
            TickCount = 1,
        };

    private static SimulationDecisionConfirmRequest ConfirmRequest(long expectedRevision)
        => new()
        {
            CommandId = "command:save-replay.confirm-1",
            ExpectedRevision = expectedRevision,
            Preview = new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:sim.save-replay-1",
                DecisionTypeCode = "HarvestDisposition",
                ActorStableId = "actor:sim.farmer-1",
                TargetStableIds = new[] { "harvest-lot:potato-1" },
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
                        SourceStableIds = new[] { "harvest-lot:potato-1" },
                    },
                },
                SourceStableIds = new[] { "harvest-lot:potato-1" },
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:sim.save-replay-1",
                    TaskTypeCode = "HarvestDispositionWork",
                    FacilityStableId = "facility:sim.farm-packing-1",
                    AssignedCapacity = 300m,
                    AssignedCapacityUnitCode = "KGM",
                    DurationTicks = 1,
                    InputLotStableIds = new[] { "harvest-lot:potato-1" },
                    OutputCandidateCodes = new[] { "CooperativeIntakeCandidate" },
                    SourceStableIds = new[] { "harvest-lot:potato-1" },
                },
            },
        };

    private sealed class StubSaveStore : ISimulationSessionSaveStore
    {
        private readonly SimulationSessionSavePackage package;

        public StubSaveStore(SimulationSessionSavePackage package)
            => this.package = package;

        public SimulationSessionSavePackage SaveOrGet(SimulationSessionSavePackage value)
            => value;

        public SimulationSessionSavePackage? Find(string saveStableId)
            => string.Equals(saveStableId, package.SaveStableId, StringComparison.Ordinal)
                ? package
                : null;
    }
}
