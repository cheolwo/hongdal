using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationHexagramCampaignTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(8)]
    public void 이야기단계는_효수와WI개수에종속되지않고_저장재생된다(int count)
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        session.BeginHexagramCampaign(new SimulationHexagramCampaignEnterRequest
        {
            CommandId = "story:enter",
            ExpectedRevision = session.Revision,
            HexagramStableId = "story:hans-learning",
            StoryStageCount = count,
            LineWorldInteractionIds = new[] { "WI-FARM-05" },
        }, "save:story-entry");
        if (count > 1)
            Assert.Throws<SimulationConflictException>(() =>
                session.CompleteHexagramCampaign(new SimulationHexagramCampaignCompleteRequest
                {
                    CommandId = "story:too-early",
                    ExpectedRevision = session.Revision,
                }));
        for (var stage = 1; stage < count; stage++)
            session.CompleteHexagramLine(new SimulationHexagramCampaignLineCompleteRequest
            {
                CommandId = "story:stage:" + stage,
                ExpectedRevision = session.Revision,
                ExpectedLineOrdinal = stage,
            });
        var saved = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:story",
            ExpectedRevision = session.Revision,
        });
        var restored = SimulationSessionReplay.Restore(saved);
        if (count == 6)
        {
            var legacyJson = System.Text.Json.JsonSerializer.Serialize(saved)
                .Replace("\"StoryStageCount\":6,", string.Empty);
            var legacy = System.Text.Json.JsonSerializer.Deserialize<SimulationSessionSavePackage>(legacyJson)!;
            Assert.Equal(6, legacy.HexagramCampaign!.StoryStageCount);
            Assert.Equal(saved.ReplayHash, SimulationSessionReplay.Restore(legacy)
                .CreateSavePackage(new SimulationSessionSaveRequest
                {
                    SaveStableId = saved.SaveStableId,
                    ExpectedRevision = restored.Revision,
                }).ReplayHash);
        }
        Assert.Equal(count, restored.GetHexagramCampaignState().StoryStageCount);
        Assert.Equal(saved.ReplayHash, restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = saved.SaveStableId,
            ExpectedRevision = restored.Revision,
        }).ReplayHash);
        var completed = restored.CompleteHexagramCampaign(new SimulationHexagramCampaignCompleteRequest
        {
            CommandId = "story:complete",
            ExpectedRevision = restored.Revision,
        });
        Assert.Equal(SimulationHexagramCampaignCodes.FreeRoam, completed.CampaignStateCode);
        Assert.Equal(count, completed.Events.Last().LineOrdinal);
        Assert.Single(completed.PermanentlyUnlockedWorldInteractionIds);
        saved.HexagramCampaign!.StoryStageCount = count + 1;
        var mismatch = Assert.Throws<SimulationConflictException>(() => SimulationSessionReplay.Restore(saved));
        Assert.Equal("SimulationReplayHashMismatch", mismatch.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void 유효하지않은이야기단계수는_상태변경없이거부한다(int count)
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var revision = session.Revision;
        var error = Assert.Throws<SimulationContractException>(() =>
            session.BeginHexagramCampaign(new SimulationHexagramCampaignEnterRequest
            {
                CommandId = "story:invalid",
                ExpectedRevision = revision,
                HexagramStableId = "story:hans-learning",
                StoryStageCount = count,
            }, "save:invalid"));
        Assert.Equal("HexagramCampaignStoryStageCountInvalid", error.ErrorCode);
        Assert.Equal(revision, session.Revision);
    }

    [Fact]
    public async Task 수뢰둔_핵심목적상실은_진입상태와초효로복원한다()
    {
        using var fixture = new Fixture();
        var created = await fixture.Runtime.Sessions.CreateAsync(CreateRequest());
        var entered = await Enter(fixture.Runtime, created);
        var advanced = await fixture.Runtime.Sessions.AdvanceWorldTickAsync(
            created.SessionStableId, new 경영SimulationTick진행Request
            {
                CommandId = "tick:after-entry",
                ExpectedRevision = entered.EntryWorldRevision,
                TickCount = 3,
            });
        var line2 = await fixture.Runtime.HexagramCampaigns
            .CompleteHexagramLineAsync(created.SessionStableId,
                new SimulationHexagramCampaignLineCompleteRequest
                {
                    CommandId = "campaign:line1",
                    ExpectedRevision = advanced.Revision,
                    ExpectedLineOrdinal = 1,
                });

        var restarted = await fixture.Runtime.HexagramCampaigns
            .FailHexagramCampaignAsync(created.SessionStableId,
                new SimulationHexagramCampaignFailureRequest
                {
                    CommandId = "campaign:fail:1",
                    ExpectedRevision = advanced.Revision + 1,
                    FailureReasonCode = SimulationHexagramCampaignCodes
                        .HansFarmFullyLost,
                });
        var world = await fixture.Runtime.Sessions.GetAsync(
            created.SessionStableId);

        Assert.Equal(2, line2.CurrentLineOrdinal);
        Assert.Equal(1, restarted.CurrentLineOrdinal);
        Assert.Equal(2, restarted.AttemptOrdinal);
        Assert.NotEqual(entered.AttemptVariationSeed,
            restarted.AttemptVariationSeed);
        Assert.Equal(0, world.CurrentTick);
        Assert.Equal(restarted.AttemptOrdinal,
            world.HexagramCampaign!.AttemptOrdinal);
        Assert.Contains(restarted.Events, value =>
            value.EventCode == SimulationHexagramCampaignCodes.CampaignFailure);
    }

    [Fact]
    public async Task 세단계이야기의_실패복원은_단계수를보존한다()
    {
        using var fixture = new Fixture();
        var created = await fixture.Runtime.Sessions.CreateAsync(CreateRequest());
        var entered = await Enter(fixture.Runtime, created, 3);
        await fixture.Runtime.HexagramCampaigns.CompleteHexagramLineAsync(
            created.SessionStableId, new SimulationHexagramCampaignLineCompleteRequest
            {
                CommandId = "story:advance",
                ExpectedRevision = entered.EntryWorldRevision,
                ExpectedLineOrdinal = 1,
            });
        var restarted = await fixture.Runtime.HexagramCampaigns.FailHexagramCampaignAsync(
            created.SessionStableId, new SimulationHexagramCampaignFailureRequest
            {
                CommandId = "story:retry",
                ExpectedRevision = entered.EntryWorldRevision + 1,
                FailureReasonCode = SimulationHexagramCampaignCodes.HansLost,
            });
        Assert.Equal(3, restarted.StoryStageCount);
        Assert.Equal(1, restarted.CurrentLineOrdinal);
        Assert.Equal(2, restarted.AttemptOrdinal);
    }

    [Theory]
    [InlineData(SimulationHexagramCampaignCodes.Injury)]
    [InlineData(SimulationHexagramCampaignCodes.Delay)]
    [InlineData(SimulationHexagramCampaignCodes.PartialFacilityDamage)]
    [InlineData(SimulationHexagramCampaignCodes.ResourceLoss)]
    public async Task 회복가능손실은_현재효와시도를유지한다(string reasonCode)
    {
        using var fixture = new Fixture();
        var created = await fixture.Runtime.Sessions.CreateAsync(CreateRequest());
        var entered = await Enter(fixture.Runtime, created);

        var setback = await fixture.Runtime.HexagramCampaigns
            .RecordHexagramSetbackAsync(created.SessionStableId,
                new SimulationHexagramCampaignSetbackRequest
                {
                    CommandId = "campaign:setback:" + reasonCode,
                    ExpectedRevision = entered.EntryWorldRevision,
                    SetbackReasonCode = reasonCode,
                });

        Assert.Equal(1, setback.CurrentLineOrdinal);
        Assert.Equal(1, setback.AttemptOrdinal);
        Assert.Contains(setback.Events, value =>
            value.EventCode == SimulationHexagramCampaignCodes
                .RecoverableSetback
            && value.ReasonCode == reasonCode);
    }

    [Fact]
    public async Task 회복가능사유를_전체실패로제출하면_거부한다()
    {
        using var fixture = new Fixture();
        var created = await fixture.Runtime.Sessions.CreateAsync(CreateRequest());
        var entered = await Enter(fixture.Runtime, created);

        var error = await Assert.ThrowsAsync<SimulationContractException>(() =>
            fixture.Runtime.HexagramCampaigns.FailHexagramCampaignAsync(
                created.SessionStableId,
                new SimulationHexagramCampaignFailureRequest
                {
                    CommandId = "campaign:invalid-failure",
                    ExpectedRevision = entered.EntryWorldRevision,
                    FailureReasonCode = SimulationHexagramCampaignCodes
                        .PartialFacilityDamage,
                }).AsTask());

        Assert.Equal("HexagramCampaignFailureReasonRecoverable",
            error.ErrorCode);
    }

    [Fact]
    public async Task 상효완주는_현재괘WI를_영구해금하고_자유생활로전환한다()
    {
        using var fixture = new Fixture();
        var created = await fixture.Runtime.Sessions.CreateAsync(CreateRequest());
        var state = await Enter(fixture.Runtime, created);
        for (var line = 1; line <= 5; line++)
        {
            state = await fixture.Runtime.HexagramCampaigns
                .CompleteHexagramLineAsync(created.SessionStableId,
                    new SimulationHexagramCampaignLineCompleteRequest
                    {
                        CommandId = "campaign:line:" + line,
                        ExpectedRevision = state.EntryWorldRevision + line - 1,
                        ExpectedLineOrdinal = line,
                    });
        }

        var completed = await fixture.Runtime.HexagramCampaigns
            .CompleteHexagramCampaignAsync(created.SessionStableId,
                new SimulationHexagramCampaignCompleteRequest
                {
                    CommandId = "campaign:complete",
                    ExpectedRevision = state.EntryWorldRevision + 5,
                });

        Assert.Equal(SimulationHexagramCampaignCodes.FreeRoam,
            completed.CampaignStateCode);
        Assert.Empty(completed.TemporaryWorldInteractionIds);
        Assert.Equal(6,
            completed.PermanentlyUnlockedWorldInteractionIds.Length);
    }

    [Fact]
    public async Task 실패전시도의_수동저장은_재도전상태를덮어쓸수없다()
    {
        using var fixture = new Fixture();
        var created = await fixture.Runtime.Sessions.CreateAsync(CreateRequest());
        var entered = await Enter(fixture.Runtime, created);
        var saved = await fixture.Runtime.Sessions.SaveSlotAsync(
            created.SessionStableId, new SimulationLocalSaveSlotRequest
            {
                SlotStableId = "slot-attempt-one",
                ExpectedRevision = entered.EntryWorldRevision,
            });
        var package = fixture.Slots.Read(saved.SlotStableId).Package;
        Assert.Equal(SimulationSaveSchemaVersions.V31, package.SchemaVersion);

        await fixture.Runtime.HexagramCampaigns.FailHexagramCampaignAsync(
            created.SessionStableId,
            new SimulationHexagramCampaignFailureRequest
            {
                CommandId = "campaign:fail:stale-slot",
                ExpectedRevision = entered.EntryWorldRevision,
                FailureReasonCode = SimulationHexagramCampaignCodes.HansLost,
            });

        var error = await Assert.ThrowsAsync<SimulationConflictException>(() =>
            fixture.Runtime.Sessions.LoadSlotAsync(saved.SlotStableId).AsTask());
        Assert.Equal("HexagramCampaignSaveAttemptInvalidated", error.ErrorCode);
    }

    private static async Task<SimulationHexagramCampaignStateSnapshot> Enter(
        LocalSimulationRuntime runtime, 경영SimulationSessionSnapshot created,
        int storyStageCount = 6)
        => await runtime.HexagramCampaigns.EnterHexagramCampaignAsync(
            created.SessionStableId,
            new SimulationHexagramCampaignEnterRequest
            {
                CommandId = "campaign:enter",
                ExpectedRevision = created.Revision,
                HexagramStableId = SimulationHexagramCampaignCodes.ZhunStableId,
                StoryStageCount = storyStageCount,
                LineWorldInteractionIds = Enumerable.Range(1, 6)
                    .Select(value => "WI-STORY-ZHUN-L" + value).ToArray(),
            });

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:hexagram-campaign-test",
            ScenarioDataRevision = "fixture.r1",
            ScenarioSeed = 3304,
            RuleRevision = "simulation.rule.r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:adventurer",
                TerritoryStableId = "territory:hans-farm",
                SettlementStableId = "settlement:hans-farm",
                GameDateStartsOn = new DateTimeOffset(2026, 9, 5, 0, 0, 0,
                    TimeSpan.Zero),
            },
        };

    private sealed class Fixture : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(),
            "hexagram-campaign-" + Guid.NewGuid().ToString("N"));

        public Fixture()
        {
            Slots = new FileSimulationLocalSaveSlotStore(root);
            Runtime = new LocalSimulationRuntime(
                new InMemory경영SimulationSessionStore(),
                new InMemorySimulationSessionSaveStore(), Slots);
        }

        public FileSimulationLocalSaveSlotStore Slots { get; }
        public LocalSimulationRuntime Runtime { get; }

        public void Dispose()
        {
            Runtime.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
