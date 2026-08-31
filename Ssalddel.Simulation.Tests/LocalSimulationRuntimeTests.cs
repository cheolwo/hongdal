using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class LocalSimulationRuntimeTests
{
    [Fact]
    public async Task Nature표현관측은_읽기와반환사본수정으로_권위와저장을바꾸지않는다()
    {
        var root = Path.Combine(Path.GetTempPath(), "nature-observation-" + Guid.NewGuid().ToString("N"));
        using var runtime = CreateRuntime(new FileSimulationLocalSaveSlotStore(root));
        var created = await runtime.Sessions.CreateAsync(CreateRequest());
        var first = await runtime.GetNature표현관측Async(created.SessionStableId);
        var before = System.Text.Json.JsonSerializer.Serialize(first);
        Assert.Equal(created.Revision, first.Session.Revision);
        Assert.Equal(created.CurrentTick, first.Session.CurrentTick);
        Assert.Equal(first.Session.NatureSurvival.PlayerStableId, first.Nature.PlayerStableId);
        first.Session.Revision = 999;
        first.Nature.ResourceNodes[0].StateCode = "changed-by-reader";
        first.BuildingProgression.AreaCode = "changed-by-reader";
        var again = await runtime.GetNature표현관측Async(created.SessionStableId);
        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(again));
        Assert.False(Directory.Exists(root) && Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Any());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Nature표현관측은_완료와취소를_같은상태의실제행위기록으로구분한다(bool cancel)
    {
        var root = Path.Combine(Path.GetTempPath(), "nature-observation-terminal-" + Guid.NewGuid().ToString("N"));
        using var runtime = CreateRuntime(new FileSimulationLocalSaveSlotStore(root));
        var created = await runtime.Sessions.CreateAsync(CreateRequest());
        var axe = await runtime.Nature.ConfirmAsync(created.SessionStableId, new SimulationNatureSurvivalCommandRequest
        {
            CommandId = "observation:axe", ExpectedRevision = created.Revision, PlayerStableId = "player:solo",
            ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
            TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
        });
        var started = await runtime.Nature.ConfirmAsync(created.SessionStableId, new SimulationNatureSurvivalCommandRequest
        {
            CommandId = "observation:harvest", ExpectedRevision = axe.Revision, PlayerStableId = "player:solo",
            ActionCode = SimulationNatureSurvivalCodes.BeginHarvest, TargetStableId = "resource:nature-tree:01",
        });
        var working = await runtime.GetNature표현관측Async(created.SessionStableId);
        Assert.Equal("observation:harvest", working.Nature.ActiveWork!.OriginCommandId);
        Assert.DoesNotContain(working.ActionLedger!.TailRecords, r => r.PrimaryOutcomeCode == "HarvestCompleted");
        var terminal = cancel
            ? await runtime.Nature.ConfirmAsync(created.SessionStableId, new SimulationNatureSurvivalCommandRequest
            {
                CommandId = "observation:cancel", ExpectedRevision = started.Revision, PlayerStableId = "player:solo",
                ActionCode = SimulationNatureSurvivalCodes.CancelActiveWork, TargetStableId = "resource:nature-tree:01",
            })
            : await runtime.Nature.AdvanceRealtimeAsync(created.SessionStableId, new SimulationNatureSurvivalClockAdvanceRequest
            {
                CommandId = "observation:clock", ExpectedRevision = started.Revision,
                ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds, WorkInputHeld = true,
            });
        var observed = await runtime.GetNature표현관측Async(created.SessionStableId);
        Assert.Equal(terminal.Revision, observed.Session.Revision);
        Assert.Null(observed.Nature.ActiveWork);
        Assert.Null(observed.Session.NatureSurvival.ActiveWork);
        var completed = observed.ActionLedger!.TailRecords.Where(r => r.PrimaryOutcomeCode == "HarvestCompleted").ToArray();
        if (cancel) Assert.Empty(completed);
        else
        {
            var record = Assert.Single(completed);
            Assert.Equal("observation:harvest:completed", record.CommandId);
            Assert.Equal(created.SessionStableId, record.SessionStableId);
            Assert.Equal(observed.Nature.PlayerStableId, record.ActorStableId);
            Assert.Equal("resource:nature-tree:01", Assert.Single(record.TargetStableIds));
            Assert.Equal(terminal.Revision, record.AfterWorldRevision);
            Assert.Equal(SimulationNatureSurvivalCodes.Stump, observed.Nature.ResourceNodes[0].StateCode);
            var recordHash = record.기록HashSha256;
            record.TargetStableIds[0] = "changed-by-reader";
            record.ActorStableId = "changed-by-reader";
            var repeated = await runtime.GetNature표현관측Async(created.SessionStableId);
            var original = Assert.Single(repeated.ActionLedger!.TailRecords, r => r.PrimaryOutcomeCode == "HarvestCompleted");
            Assert.Equal("player:solo", original.ActorStableId);
            Assert.Equal("resource:nature-tree:01", original.TargetStableIds[0]);
            Assert.Equal(recordHash, original.기록HashSha256);
            Assert.Equal(terminal.Revision, repeated.Session.Revision);
        }
        Assert.False(Directory.Exists(root) && Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Any());
    }

    [Fact]
    public async Task Nature표현관측은_취소된조회와없는Session을_대체상태로숨기지않는다()
    {
        var root = Path.Combine(Path.GetTempPath(), "nature-observation-reject-" + Guid.NewGuid().ToString("N"));
        using var runtime = CreateRuntime(new FileSimulationLocalSaveSlotStore(root));
        var created = await runtime.Sessions.CreateAsync(CreateRequest());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runtime.GetNature표현관측Async(
            created.SessionStableId, new System.Threading.CancellationToken(true)).AsTask());
        await Assert.ThrowsAsync<SimulationNotFoundException>(() => runtime.GetNature표현관측Async("missing-session").AsTask());
        Assert.Equal(created.Revision, (await runtime.Sessions.GetAsync(created.SessionStableId)).Revision);
    }

    [Fact]
    public void LocalRuntime_WI포트를_같은권위인스턴스로노출한다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-runtime-" + Guid.NewGuid().ToString("N"));
        try
        {
            using var runtime = CreateRuntime(
                new FileSimulationLocalSaveSlotStore(savesRoot));

            Assert.Same(runtime, runtime.WorldInteractions);
            Assert.Same(runtime, runtime.Turns);
            Assert.Same(runtime, runtime.FarmChoices);
            Assert.Same(runtime, runtime.Logistics);
            Assert.Same(runtime, runtime.FarmWorldInteractions);
            Assert.Same(runtime, runtime.NatureWorldInteractions);
            Assert.Same(runtime, runtime.Battles);
            Assert.IsAssignableFrom<ISimulationRuntimeModules>(runtime);
            Assert.IsAssignableFrom<ISimulationWorldInteractionRuntime>(
                runtime.WorldInteractions);
            Assert.IsAssignableFrom<ISimulationTurnRuntime>(runtime.Gameplay);
            Assert.IsAssignableFrom<ISimulationFarmChoiceRuntime>(runtime.Gameplay);
            Assert.IsAssignableFrom<ISimulationLogisticsRuntime>(runtime.Gameplay);
            Assert.IsAssignableFrom<ISimulationFarmWorldInteractionRuntime>(
                runtime.WorldInteractions);
            Assert.IsAssignableFrom<ISimulationNatureWorldInteractionRuntime>(
                runtime.WorldInteractions);
            Assert.False(runtime.Descriptor.RequiresNetwork);
            Assert.True(runtime.Descriptor.IsPlayableAuthority);
            Assert.Equal(SimulationRuntimePurpose.Playable,
                runtime.Descriptor.Purpose);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    [Fact]
    public async Task LocalRuntime_Nature명령과V15_WI증거슬롯복원을_서버없이수행한다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-runtime-" + Guid.NewGuid().ToString("N"));
        try
        {
            var slotStore = new FileSimulationLocalSaveSlotStore(savesRoot);
            using var runtime = CreateRuntime(slotStore);
            var created = await runtime.Sessions.CreateAsync(CreateRequest());

            var initial = await runtime.Nature.GetAsync(created.SessionStableId);
            Assert.False(initial.HasAxe);
            Assert.Equal(SimulationAuthorityLocation.LocalProcess,
                runtime.Descriptor.AuthorityLocation);
            Assert.Equal(SimulationRuntimePurpose.Playable,
                runtime.Descriptor.Purpose);
            Assert.True(runtime.Descriptor.IsPlayableAuthority);
            Assert.False(runtime.Descriptor.RequiresNetwork);

            var axeRequest = new SimulationNatureSurvivalCommandRequest
            {
                CommandId = "nature-local:axe",
                ExpectedRevision = created.Revision,
                PlayerStableId = "player:solo",
                ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
                TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
            };
            var afterAxe = await runtime.Nature.ConfirmAsync(
                created.SessionStableId, axeRequest);
            var retriedAxe = await runtime.Nature.ConfirmAsync(
                created.SessionStableId, axeRequest);
            Assert.Equal(afterAxe.Revision, retriedAxe.Revision);
            var firstSave = await runtime.Sessions.SaveSlotAsync(
                created.SessionStableId,
                new SimulationLocalSaveSlotRequest
                {
                    SlotStableId = "slot-01",
                    ExpectedRevision = afterAxe.Revision,
                });
            Assert.Single(slotStore.Read("slot-01").Package
                .WorldInteractionManifestations);

            var afterHarvestStart = await runtime.Nature.ConfirmAsync(
                created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "nature-local:harvest",
                    ExpectedRevision = afterAxe.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.BeginHarvest,
                    TargetStableId = "resource:nature-tree:01",
                });
            var secondSave = await runtime.Sessions.SaveSlotAsync(
                created.SessionStableId,
                new SimulationLocalSaveSlotRequest
                {
                    SlotStableId = "slot-01",
                    ExpectedRevision = afterHarvestStart.Revision,
                });

            Assert.NotEqual(firstSave.SaveStableId, secondSave.SaveStableId);
            var secondPackage = slotStore.Read("slot-01").Package;
            Assert.Equal(SimulationSaveSchemaVersions.V28,
                secondPackage.SchemaVersion);
            Assert.Equal(2, secondPackage.WorldInteractionManifestations.Length);
            Assert.Equal(new[]
            {
                SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
            }, secondPackage.WorldInteractionManifestations.Select(value =>
                value.WorldInteractionId).ToArray());
            Assert.All(secondPackage.CommandLog.Where(value =>
                    value.WorldInteractionInvocation != null), value =>
                Assert.Equal(
                    SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                    value.WorldInteractionInvocation!.TriggerSourceCode));
            Assert.Equal(
                SimulationWorldInteractionMaturityStateCodes.ManifestationPartial,
                secondPackage.WorldInteractionManifestations[1].StateCode);

            var afterHarvest = await runtime.Nature.AdvanceRealtimeAsync(
                created.SessionStableId,
                new SimulationNatureSurvivalClockAdvanceRequest
                {
                    CommandId = "nature-local:harvest:realtime",
                    ExpectedRevision = afterHarvestStart.Revision,
                    ElapsedRealtimeSeconds = 4,
                    WorkInputHeld = true,
                });
            await runtime.Sessions.SaveSlotAsync(created.SessionStableId,
                new SimulationLocalSaveSlotRequest
                {
                    SlotStableId = "slot-finished",
                    ExpectedRevision = afterHarvest.Revision,
                });
            var completedHarvestEvidence = slotStore.Read("slot-finished").Package
                .WorldInteractionManifestations.Single(value =>
                    value.WorldInteractionId ==
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId);
            Assert.NotEmpty(completedHarvestEvidence.ResultStateCodes);
            Assert.True(completedHarvestEvidence.AfterWorldRevision >
                completedHarvestEvidence.BeforeWorldRevision);
            Assert.Equal(
                SimulationWorldInteractionMaturityStateCodes.ManifestationPartial,
                completedHarvestEvidence.StateCode);
            Assert.Contains("SpatialEvidence",
                completedHarvestEvidence.MissingEvidenceCodes);
            var verified = await runtime.Sessions.VerifySlotAsync("slot-01");
            Assert.Equal(SimulationSaveSchemaVersions.V28,
                verified.Restore.SchemaVersion);

            var tampered = SimulationSaveReplayCloner.ClonePackage(secondPackage);
            tampered.CommandLog.Single(value =>
                    value.WorldInteractionInvocation?.CommandId ==
                    "nature-local:axe")
                .WorldInteractionInvocation!.TriggerSourceCode =
                    SimulationWorldInteractionTriggerSourceCodes.WorldDerived;
            Assert.Throws<SimulationConflictException>(() =>
                SimulationSessionReplay.Restore(tampered));

            var primaryPath = Path.Combine(savesRoot, "slot-01.ssalddel");
            File.WriteAllText(primaryPath, "{corrupted", System.Text.Encoding.UTF8);

            using var restoredRuntime = CreateRuntime(slotStore);
            var restored = await restoredRuntime.Sessions.LoadSlotAsync("slot-01");

            Assert.True(restored.RecoveredFromBackup);
            Assert.Equal(firstSave.SaveStableId, restored.Restore.SaveStableId);
            Assert.Equal(firstSave.ReplayHash, restored.Restore.ReplayHash);
            Assert.True(restored.Restore.Session.NatureSurvival.HasAxe);
            Assert.Null(restored.Restore.Session.NatureSurvival.ActiveWork);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    [Fact]
    public async Task LocalRuntime_NaturePreview는_WI발현증거를생성하지않는다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-runtime-" + Guid.NewGuid().ToString("N"));
        try
        {
            var slotStore = new FileSimulationLocalSaveSlotStore(savesRoot);
            using var runtime = CreateRuntime(slotStore);
            var created = await runtime.Sessions.CreateAsync(CreateRequest());

            var opportunities = await runtime.Nature
                .GetPlayerOpportunitiesAsync(created.SessionStableId);
            var needs = await runtime.Nature.GetAreaNeedsAsync(
                created.SessionStableId);
            Assert.Contains(opportunities, value => value.PlayerActivityTrackCode ==
                Simulation플레이어활동경로Codes.FieldExpedition);
            Assert.Equal(2, needs.Length);
            Assert.False(runtime.Descriptor.RequiresNetwork);

            var preview = await runtime.Nature.PreviewAsync(
                created.SessionStableId,
                new SimulationNatureSurvivalActionPreviewRequest
                {
                    ObservedWorldRevision = created.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
                    TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
                });
            Assert.True(preview.CanConfirm);

            await runtime.Sessions.SaveSlotAsync(created.SessionStableId,
                new SimulationLocalSaveSlotRequest
                {
                    SlotStableId = "slot-preview",
                    ExpectedRevision = created.Revision,
                });
            var package = slotStore.Read("slot-preview").Package;
            Assert.NotEqual(SimulationSaveSchemaVersions.V15,
                package.SchemaVersion);
            Assert.Empty(package.WorldInteractionManifestations);
            Assert.DoesNotContain(package.CommandLog, value =>
                value.WorldInteractionInvocation != null);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    [Fact]
    public async Task LocalRuntime_Nature작업취소는_원작업과취소WI를_V15에결속한다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-runtime-" + Guid.NewGuid().ToString("N"));
        try
        {
            var slotStore = new FileSimulationLocalSaveSlotStore(savesRoot);
            using var runtime = CreateRuntime(slotStore);
            var created = await runtime.Sessions.CreateAsync(CreateRequest());
            var axe = await runtime.Nature.ConfirmAsync(created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "nature-cancel:axe",
                    ExpectedRevision = created.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
                    TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
                });
            var started = await runtime.Nature.ConfirmAsync(created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "nature-cancel:harvest",
                    ExpectedRevision = axe.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.BeginHarvest,
                    TargetStableId = "resource:nature-tree:01",
                });
            var cancelled = await runtime.Nature.ConfirmAsync(created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "nature-cancel:confirm",
                    ExpectedRevision = started.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.CancelActiveWork,
                    TargetStableId = "resource:nature-tree:01",
                });
            var saved = await runtime.Sessions.SaveSlotAsync(created.SessionStableId,
                new SimulationLocalSaveSlotRequest
                {
                    SlotStableId = "slot-cancelled",
                    ExpectedRevision = cancelled.Revision,
                });

            var package = slotStore.Read("slot-cancelled").Package;
            Assert.Equal(SimulationSaveSchemaVersions.V28, package.SchemaVersion);
            Assert.Contains(package.WorldInteractionManifestations, value =>
                value.WorldInteractionId ==
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId
                && value.ResultStateCodes.Contains("WorkCancelled"));
            Assert.Contains(package.WorldInteractionManifestations, value =>
                value.WorldInteractionId ==
                    SimulationNatureSurvivalCodes.CancelActiveWorkWorldInteractionId
                && value.ResultStateCodes.Contains("CancelActiveWork:Confirmed"));

            using var restoredRuntime = CreateRuntime(slotStore);
            var restored = await restoredRuntime.Sessions.LoadSlotAsync("slot-cancelled");
            Assert.Equal(saved.ReplayHash, restored.Restore.ReplayHash);
            Assert.Null(restored.Restore.Session.NatureSurvival.ActiveWork);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    [Fact]
    public void LocalSaveSlot은_경로문자를거부한다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-runtime-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileSimulationLocalSaveSlotStore(savesRoot);
            var error = Assert.Throws<SimulationContractException>(() =>
                store.Read("../outside"));
            Assert.Equal("SimulationLocalSaveSlotInvalid", error.Message);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    [Fact]
    public async Task LocalRuntime_턴마감은_같은SessionAggregate를사용한다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-runtime-" + Guid.NewGuid().ToString("N"));
        try
        {
            using var runtime = CreateRuntime(
                new FileSimulationLocalSaveSlotStore(savesRoot));
            var created = await runtime.Sessions.CreateAsync(CreateRequest());
            var context = await runtime.Gameplay.GetTurnClosingContextAsync(
                created.SessionStableId);
            var selected = Assert.Single(context.AvailableCards,
                card => card.CardStableId ==
                    "learning:hongik.fool.beginner-mind");

            var preview = await runtime.Gameplay.PreviewTurnClosingAsync(
                created.SessionStableId,
                new SimulationTurnClosingPreviewRequest
                {
                    ExpectedRevision = created.Revision,
                    SelectedCardStableIds = new[] { selected.CardStableId },
                });
            var confirmed = await runtime.Gameplay.ConfirmTurnClosingAsync(
                created.SessionStableId,
                new SimulationTurnClosingConfirmRequest
                {
                    CommandId = "command:test:local-turn-close",
                    ExpectedRevision = created.Revision,
                    Preview = new SimulationTurnClosingPreviewRequest
                    {
                        ExpectedRevision = created.Revision,
                        SelectedCardStableIds = new[] { selected.CardStableId },
                    },
                });

            Assert.Equal(created.CurrentTick + 1, confirmed.CurrentTick);
            Assert.Equal(created.Revision + 1, confirmed.Revision);
            Assert.Equal(preview.NextTurnNumber,
                (await runtime.Gameplay.GetTurnClosingContextAsync(
                    created.SessionStableId)).TurnNumber);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    [Fact]
    public async Task LocalRuntime_관찰운영전투를_HTTP없이_완료하고_슬롯에복원한다()
    {
        var savesRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-local-battle-" + Guid.NewGuid().ToString("N"));
        try
        {
            var slotStore = new FileSimulationLocalSaveSlotStore(savesRoot);
            using var runtime = CreateRuntime(slotStore);
            var request = CreateRequest();
            request.ClientRequestId = Guid.Parse(
                "70e70e70-e700-4700-8700-70e70e70e700");
            request.ScenarioSeed = 1;
            request.NatureSurvival!.ProfileRevision =
                SimulationNatureSurvivalCodes.ProfileRevisionR2;
            var created = await runtime.Sessions.CreateAsync(request);
            var axe = await runtime.Nature.ConfirmAsync(created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "command:local-battle:axe",
                    ExpectedRevision = created.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
                    TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
                });
            var harvest = await runtime.Nature.ConfirmAsync(created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "command:local-battle:harvest",
                    ExpectedRevision = axe.Revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.BeginHarvest,
                    TargetStableId = "resource:nature-tree:01",
                });
            var finishedHarvest = await runtime.Nature.AdvanceRealtimeAsync(
                created.SessionStableId,
                new SimulationNatureSurvivalClockAdvanceRequest
                {
                    CommandId = "command:local-battle:harvest-hold",
                    ExpectedRevision = harvest.Revision,
                    ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
                    WorkInputHeld = true,
                });
            var revision = finishedHarvest.Revision;
            var elapsed = (await runtime.Nature.GetAsync(created.SessionStableId))
                .ElapsedSecondsInCycle;
            var clockSequence = 0;
            while (elapsed <= NatureSurvivalRules.DaylightEndsAtSecond)
            {
                var advanced = await runtime.Nature.AdvanceRealtimeAsync(
                    created.SessionStableId,
                    new SimulationNatureSurvivalClockAdvanceRequest
                    {
                        CommandId = "command:local-battle:clock:" + clockSequence++,
                        ExpectedRevision = revision,
                        ElapsedRealtimeSeconds = Math.Min(60,
                            NatureSurvivalRules.DaylightEndsAtSecond + 1 - elapsed),
                    });
                revision = advanced.Revision;
                elapsed = (await runtime.Nature.GetAsync(created.SessionStableId))
                    .ElapsedSecondsInCycle;
            }
            var nature = await runtime.Nature.GetAsync(created.SessionStableId);
            Assert.NotNull(nature.Encounter);
            var linked = await runtime.Nature.ConfirmAsync(created.SessionStableId,
                new SimulationNatureSurvivalCommandRequest
                {
                    CommandId = "command:local-battle:link",
                    ExpectedRevision = revision,
                    PlayerStableId = "player:solo",
                    ActionCode = SimulationNatureSurvivalCodes.ResolveEncounter,
                    TargetStableId = nature.Encounter!.EncounterStableId,
                    ChoiceCode = SimulationNatureSurvivalCodes.Fight,
                });
            var preview = await runtime.Battles.PreviewBattleAsync(
                created.SessionStableId, new SimulationBattleCreatePreviewRequest
                {
                    ExpectedWorldRevision = linked.Revision,
                    EncounterStableId = nature.Encounter.EncounterStableId,
                    RequestingActorStableId = "player:solo",
                });
            Assert.True(preview.CanConfirm);
            var battle = await runtime.Battles.ConfirmBattleAsync(
                created.SessionStableId, new SimulationBattleCreateConfirmRequest
                {
                    CommandId = "command:local-battle:create",
                    ExpectedWorldRevision = linked.Revision,
                    EncounterStableId = nature.Encounter.EncounterStableId,
                    RequestingActorStableId = "player:solo",
                    ExpectedBattleWorldContextHashSha256 =
                        preview.LocalWorldContext.ContextHashSha256,
                });
            battle = await runtime.Battles.ConfirmBattleControlModeAsync(
                created.SessionStableId, battle.BattleStableId,
                new SimulationLocalCombatControlModeConfirmRequest
                {
                    CommandId = "command:local-battle:observer",
                    ExpectedBattleRevision = battle.BattleRevision,
                    RequestingActorStableId = "player:solo",
                    ControlModeCode = SimulationLocalCombatCodes.ObserverOperation,
                    ExpectedCardLoadoutHashSha256 = battle.LocalCombat
                        .FrozenCardLoadoutHashSha256,
                });
            var sequence = 0;
            while (battle.PhaseCode == SimulationBattleInstanceCodes.Active)
                battle = await runtime.Battles.AdvanceBattleAsync(
                    created.SessionStableId, battle.BattleStableId,
                    new SimulationBattleAdvanceRequest
                    {
                        CommandId = "command:local-battle:tick:" + sequence++,
                        ExpectedBattleRevision = battle.BattleRevision,
                        CombatTickCount = 5,
                    });

            Assert.True(battle.Outcome!.UsedDeterministicAutoCommand);
            Assert.Equal(SimulationNatureSurvivalCodes.Victory,
                (await runtime.Nature.GetAsync(created.SessionStableId))
                .LastCombatResultCode);
            var session = await runtime.Sessions.GetAsync(created.SessionStableId);
            var saved = await runtime.Sessions.SaveSlotAsync(created.SessionStableId,
                new SimulationLocalSaveSlotRequest
                {
                    SlotStableId = "slot-local-observer-battle",
                    ExpectedRevision = session.Revision,
                });
            Assert.Equal(SimulationSaveSchemaVersions.V28,
                slotStore.Read("slot-local-observer-battle").Package.SchemaVersion);

            using var restoredRuntime = CreateRuntime(slotStore);
            var restored = await restoredRuntime.Sessions.LoadSlotAsync(
                "slot-local-observer-battle");
            Assert.Equal(saved.ReplayHash, restored.Restore.ReplayHash);
            Assert.Equal(SimulationNatureSurvivalCodes.Victory,
                restored.Restore.Session.NatureSurvival.LastCombatResultCode);
        }
        finally
        {
            if (Directory.Exists(savesRoot)) Directory.Delete(savesRoot, true);
        }
    }

    private static LocalSimulationRuntime CreateRuntime(
        FileSimulationLocalSaveSlotStore slotStore)
        => new(new InMemory경영SimulationSessionStore(),
            new InMemorySimulationSessionSaveStore(), slotStore);

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:nature-local-runtime-test",
            ScenarioDataRevision = "fixture.r1",
            ScenarioSeed = 1701,
            RuleRevision = "simulation.rule.r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:solo",
                TerritoryStableId = "territory:nature",
                SettlementStableId = "settlement:nature-home",
                GameDateStartsOn = new DateTimeOffset(2026, 8, 23, 0, 0, 0,
                    TimeSpan.Zero),
            },
            NatureSurvival = new SimulationNatureSurvivalInitialStateRequest
            {
                PlayerStableId = "player:solo",
                StartsWithAxe = false,
                ResourceNodes = new[]
                {
                    new SimulationNatureResourceNodeInitialStateRequest
                    {
                        ResourceNodeStableId = "resource:nature-tree:01",
                        H1StableId = "h1-stock:nature-exploration-buffer",
                        LocalX = 2,
                        LocalZ = 8,
                    },
                },
            },
        };
}
