using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E3결정성검증)]
public sealed class SimulationNatureSurvivalTests
{
    [Theory]
    [InlineData(SimulationNatureSurvivalCodes.AcquireAxe,
        SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId)]
    [InlineData(SimulationNatureSurvivalCodes.BeginHarvest,
        SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId)]
    [InlineData(SimulationNatureSurvivalCodes.PlaceCabinBlueprint,
        SimulationNatureSurvivalCodes.PlaceCabinBlueprintWorldInteractionId)]
    [InlineData(SimulationNatureSurvivalCodes.BeginCabinBuild,
        SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId)]
    [InlineData(SimulationNatureSurvivalCodes.EnterCabin,
        SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId)]
    [InlineData(SimulationNatureSurvivalCodes.LeaveCabin,
        SimulationNatureSurvivalCodes.LeaveCabinWorldInteractionId)]
    [InlineData(SimulationNatureSurvivalCodes.ResolveEncounter,
        SimulationNatureSurvivalCodes.ResolveEncounterWorldInteractionId)]
    public void 플레이어가확정하는_Nature생존행위는_정식WI식별자를갖는다(
        string actionCode, string expectedWorldInteractionId)
    {
        Assert.Equal(expectedWorldInteractionId,
            SimulationNatureSurvivalCodes.WorldInteractionIdForAction(actionCode));
    }

    [Fact]
    public void 시간경과는_독립WI로잘못분류하지않는다()
    {
        Assert.Empty(SimulationNatureSurvivalCodes.WorldInteractionIdForAction(
            nameof(ISimulationNatureSurvivalRuntime.AdvanceRealtimeAsync)));
    }

    [Fact]
    public void 실시간규칙은_20분주기와_고정단계를_공유한다()
    {
        Assert.Equal(1_200, NatureSurvivalRules.CycleSeconds);
        Assert.Equal(NatureSurvivalClockPhaseCodes.Daylight,
            NatureSurvivalRules.PhaseAt(599));
        Assert.Equal(NatureSurvivalClockPhaseCodes.Dusk,
            NatureSurvivalRules.PhaseAt(600));
        Assert.Equal(NatureSurvivalClockPhaseCodes.Night,
            NatureSurvivalRules.PhaseAt(750));
        Assert.Equal(NatureSurvivalClockPhaseCodes.Dawn,
            NatureSurvivalRules.PhaseAt(1_110));

        var projected = NatureSurvivalRules.AdvanceClock(2, 1_190, 20);
        Assert.Equal(3, projected.CycleIndex);
        Assert.Equal(10, projected.ElapsedSecondsInCycle);
        Assert.Equal(1, projected.CompletedCycleCount);
    }

    [Fact]
    public void 구형세션은_Nature실시간Profile을_자동활성화하지않는다()
    {
        var request = CreateRequest(includeNature: false);
        var aggregate = new 경영SimulationSessionAggregate(request);

        Assert.False(aggregate.Snapshot().NatureSurvival.IsEnabled);
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:legacy-tick",
            ExpectedRevision = 0,
            TickCount = 1,
        });

        Assert.Equal(1, aggregate.Snapshot().CurrentTick);
        Assert.False(aggregate.Snapshot().NatureSurvival.IsEnabled);
    }

    [Fact]
    public void 도끼벌목은_누르고있는시간을_누적해_기존소지품원장에_통나무를넣는다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        var started = Confirm(aggregate, "command:harvest-start", 0,
            SimulationNatureSurvivalCodes.BeginHarvest, "resource:nature-tree:01");

        Assert.Equal(SimulationNatureSurvivalCodes.Harvest,
            started.NatureSurvival.ActiveWork?.WorkKindCode);
        var completed = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:harvest-hold",
            ExpectedRevision = started.Revision,
            ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
            WorkInputHeld = true,
        });

        Assert.Null(completed.NatureSurvival.ActiveWork);
        Assert.Equal(NatureSurvivalRules.HarvestTimberQuantity,
            completed.NatureSurvival.TimberQuantity);
        Assert.Equal(SimulationNatureSurvivalCodes.Stump,
            completed.NatureSurvival.ResourceNodes.Single(value =>
                value.ResourceNodeStableId == "resource:nature-tree:01").StateCode);
        Assert.Contains(aggregate.GetWorldInventory().Players.Single().Items,
            value => value.ItemCode == SimulationNatureSurvivalCodes.TimberItemCode
                && value.Quantity == NatureSurvivalRules.HarvestTimberQuantity);

        var duplicate = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:harvest-hold",
            ExpectedRevision = started.Revision,
            ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
            WorkInputHeld = true,
        });
        Assert.Equal(completed.Revision, duplicate.Revision);
        Assert.Equal(completed.NatureSurvival.TimberQuantity,
            duplicate.NatureSurvival.TimberQuantity);
    }

    [Fact]
    public void 세그루벌목후_오두막도면과_30초건설이_회복보관방어거점을완성한다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        for (var index = 1; index <= 3; index++)
        {
            var target = $"resource:nature-tree:{index:00}";
            Confirm(aggregate, $"command:harvest-start:{index}", aggregate.Revision,
                SimulationNatureSurvivalCodes.BeginHarvest, target);
            aggregate.AdvanceNatureSurvivalClock(new()
            {
                CommandId = $"command:harvest-hold:{index}",
                ExpectedRevision = aggregate.Revision,
                ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
                WorkInputHeld = true,
            });
        }
        Assert.Equal(NatureSurvivalRules.CabinTimberCost,
            aggregate.GetNatureSurvivalState().TimberQuantity);

        Confirm(aggregate, "command:cabin-place", aggregate.Revision,
            SimulationNatureSurvivalCodes.PlaceCabinBlueprint,
            "facility:nature-cabin", localX: 3, localZ: -2, yaw: 90);
        Confirm(aggregate, "command:cabin-build", aggregate.Revision,
            SimulationNatureSurvivalCodes.BeginCabinBuild,
            "facility:nature-cabin");
        var completed = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:cabin-hold",
            ExpectedRevision = aggregate.Revision,
            ElapsedRealtimeSeconds = NatureSurvivalRules.CabinWorkSeconds,
            WorkInputHeld = true,
        });

        Assert.Equal(0, completed.NatureSurvival.TimberQuantity);
        Assert.Equal(SimulationNatureSurvivalCodes.Completed,
            completed.NatureSurvival.Cabin.StateCode);
        Assert.True(completed.NatureSurvival.Cabin.RecoveryAvailable);
        Assert.True(completed.NatureSurvival.Cabin.DefenseAvailable);
        Assert.Equal(NatureSurvivalRules.CabinStorageCapacity,
            completed.NatureSurvival.Cabin.StorageCapacity);

        var entered = Confirm(aggregate, "command:cabin-enter", aggregate.Revision,
            SimulationNatureSurvivalCodes.EnterCabin, "facility:nature-cabin");
        Assert.True(entered.NatureSurvival.PlayerInsideCabin);
        var duplicateEnter = aggregate.PreviewNatureSurvivalAction(new()
        {
            ObservedWorldRevision = aggregate.Revision,
            PlayerStableId = "player:solo",
            ActionCode = SimulationNatureSurvivalCodes.EnterCabin,
            TargetStableId = "facility:nature-cabin",
        });
        Assert.Equal(SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
            duplicateEnter.WorldInteractionId);
        Assert.False(duplicateEnter.CanConfirm);

        var left = Confirm(aggregate, "command:cabin-leave", aggregate.Revision,
            SimulationNatureSurvivalCodes.LeaveCabin, "facility:nature-cabin");
        Assert.False(left.NatureSurvival.PlayerInsideCabin);
    }

    [Fact]
    public void 작업취소는_벌목점유를해제하고_오두막예약통나무를반환한다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        var harvesting = Confirm(aggregate, "command:cancel:harvest-start", 0,
            SimulationNatureSurvivalCodes.BeginHarvest, "resource:nature-tree:01");
        var progressed = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:cancel:harvest-progress",
            ExpectedRevision = harvesting.Revision,
            ElapsedRealtimeSeconds = 2,
            WorkInputHeld = true,
        });

        var harvestCancelled = Confirm(aggregate, "command:cancel:harvest",
            progressed.Revision, SimulationNatureSurvivalCodes.CancelActiveWork,
            "resource:nature-tree:01");

        Assert.Null(harvestCancelled.NatureSurvival.ActiveWork);
        Assert.Equal(0, harvestCancelled.NatureSurvival.TimberQuantity);
        Assert.Equal(SimulationNatureSurvivalCodes.Standing,
            harvestCancelled.NatureSurvival.ResourceNodes.Single(value =>
                value.ResourceNodeStableId == "resource:nature-tree:01").StateCode);

        for (var index = 1; index <= 3; index++)
        {
            var target = $"resource:nature-tree:{index:00}";
            Confirm(aggregate, $"command:cancel:tree:{index}", aggregate.Revision,
                SimulationNatureSurvivalCodes.BeginHarvest, target);
            aggregate.AdvanceNatureSurvivalClock(new()
            {
                CommandId = $"command:cancel:tree-hold:{index}",
                ExpectedRevision = aggregate.Revision,
                ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
                WorkInputHeld = true,
            });
        }
        Confirm(aggregate, "command:cancel:cabin-place", aggregate.Revision,
            SimulationNatureSurvivalCodes.PlaceCabinBlueprint,
            "facility:nature-cabin", localX: 2, localZ: -1);
        Confirm(aggregate, "command:cancel:cabin-start", aggregate.Revision,
            SimulationNatureSurvivalCodes.BeginCabinBuild,
            "facility:nature-cabin");
        aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:cancel:cabin-progress",
            ExpectedRevision = aggregate.Revision,
            ElapsedRealtimeSeconds = 10,
            WorkInputHeld = true,
        });

        var cabinCancelled = Confirm(aggregate, "command:cancel:cabin",
            aggregate.Revision, SimulationNatureSurvivalCodes.CancelActiveWork,
            "facility:nature-cabin");

        Assert.Null(cabinCancelled.NatureSurvival.ActiveWork);
        Assert.Equal(NatureSurvivalRules.CabinTimberCost,
            cabinCancelled.NatureSurvival.TimberQuantity);
        Assert.Equal(0, cabinCancelled.NatureSurvival.Cabin.ReservedTimberQuantity);
        Assert.Equal(0, cabinCancelled.NatureSurvival.Cabin.CompletedWorkSeconds);
        Assert.Equal(SimulationNatureSurvivalCodes.Building,
            cabinCancelled.NatureSurvival.Cabin.StateCode);

        var missing = aggregate.PreviewNatureSurvivalAction(new()
        {
            ObservedWorldRevision = aggregate.Revision,
            PlayerStableId = "player:solo",
            ActionCode = SimulationNatureSurvivalCodes.CancelActiveWork,
        });
        Assert.Equal(SimulationNatureSurvivalCodes.CancelActiveWorkWorldInteractionId,
            missing.WorldInteractionId);
        Assert.False(missing.CanConfirm);
        Assert.Contains(SimulationNatureSurvivalCodes.ActiveWorkRequired,
            missing.BlockReasonCodes);
    }

    [Fact]
    public void Solo메뉴정지는_시계를멈추고_주기경계만_WorldTick을진행한다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        var paused = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:pause",
            ExpectedRevision = 0,
            ElapsedRealtimeSeconds = 60,
            PauseReasonCode = SimulationNatureSurvivalCodes.Menu,
        });
        Assert.True(paused.NatureSurvival.ClockPaused);
        Assert.Equal(0, paused.NatureSurvival.ElapsedSecondsInCycle);
        Assert.Equal(0, paused.CurrentTick);

        경영SimulationSessionSnapshot current = paused;
        for (var index = 0; index < 20; index++)
        {
            current = aggregate.AdvanceNatureSurvivalClock(new()
            {
                CommandId = $"command:clock:{index}",
                ExpectedRevision = aggregate.Revision,
                ElapsedRealtimeSeconds = 60,
            });
        }
        Assert.Equal(1, current.NatureSurvival.CycleIndex);
        Assert.Equal(0, current.NatureSurvival.ElapsedSecondsInCycle);
        Assert.Equal(1, current.CurrentTick);
    }

    [Fact]
    public void 소음후_첫황혼조우는_seed로결정되고_Skeleton표현은placeholder로표시된다()
    {
        var request = CreateRequest();
        request.ScenarioSeed = FindTriggeringSeed(request.ClientRequestId);
        var aggregate = new 경영SimulationSessionAggregate(request);
        Confirm(aggregate, "command:noise-start", 0,
            SimulationNatureSurvivalCodes.BeginHarvest, "resource:nature-tree:01");
        aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:noise-hold",
            ExpectedRevision = aggregate.Revision,
            ElapsedRealtimeSeconds = 4,
            WorkInputHeld = true,
        });

        for (var index = 0; index < 10; index++)
        {
            aggregate.AdvanceNatureSurvivalClock(new()
            {
                CommandId = $"command:dusk:{index}",
                ExpectedRevision = aggregate.Revision,
                ElapsedRealtimeSeconds = index == 9 ? 56 : 60,
            });
        }
        var encounter = aggregate.GetNatureSurvivalState().Encounter;
        Assert.NotNull(encounter);
        Assert.Equal(SimulationNatureSurvivalCodes.Pending, encounter!.StateCode);
        Assert.Equal(SimulationNatureSurvivalCodes.SkeletonPlaceholderCode,
            encounter.ThreatPresentationCode);
    }

    [Fact]
    public void Nature상태와명령은_v13저장자료에서_동일hash로재생된다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        Confirm(aggregate, "command:save-harvest", 0,
            SimulationNatureSurvivalCodes.BeginHarvest, "resource:nature-tree:01");
        aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:save-hold",
            ExpectedRevision = aggregate.Revision,
            ElapsedRealtimeSeconds = 4,
            WorkInputHeld = true,
        });
        var package = aggregate.CreateSavePackage(new()
        {
            SaveStableId = "save:nature-survival",
            ExpectedRevision = aggregate.Revision,
        });

        var restored = SimulationSessionReplay.Restore(package);
        var restoredPackage = restored.CreateSavePackage(new()
        {
            SaveStableId = "save:nature-survival",
            ExpectedRevision = restored.Revision,
        });

        Assert.Equal(SimulationSaveSchemaVersions.V13, package.SchemaVersion);
        Assert.Equal(package.ReplayHash, restoredPackage.ReplayHash);
        Assert.Equal(package.Snapshot.NatureSurvival.TimberQuantity,
            restored.Snapshot().NatureSurvival.TimberQuantity);
    }

    [Fact]
    public async Task HTTP는_Nature상태검토확정실시간경계를_노출한다()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var request = CreateRequest();
        request.ClientRequestId = Guid.NewGuid();
        request.NatureSurvival!.StartsWithAxe = false;
        var createResponse = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<
            경영SimulationSessionSnapshot>();
        Assert.NotNull(created);

        var preview = await client.PostAsJsonAsync(
            $"/api/simulation/v1/sessions/{created!.SessionStableId}/nature-survival/previews",
            new SimulationNatureSurvivalActionPreviewRequest
            {
                ObservedWorldRevision = created.Revision,
                PlayerStableId = "player:solo",
                ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
                TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
            });
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        var previewSnapshot = await preview.Content.ReadFromJsonAsync<
            SimulationNatureSurvivalActionPreviewSnapshot>();
        Assert.Equal(SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId,
            previewSnapshot!.WorldInteractionId);

        var confirm = await client.PostAsJsonAsync(
            $"/api/simulation/v1/sessions/{created.SessionStableId}/nature-survival/commands",
            new SimulationNatureSurvivalCommandRequest
            {
                CommandId = "command:http:acquire-axe",
                ExpectedRevision = created.Revision,
                PlayerStableId = "player:solo",
                ActionCode = SimulationNatureSurvivalCodes.AcquireAxe,
                TargetStableId = SimulationNatureSurvivalCodes.AxePickupStableId,
            });
        Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);

        var state = await client.GetFromJsonAsync<SimulationNatureSurvivalStateSnapshot>(
            $"/api/simulation/v1/sessions/{created.SessionStableId}/nature-survival");
        Assert.NotNull(state);
        Assert.True(state!.HasAxe);
        Assert.Equal(SimulationNatureSurvivalCodes.ProfileRevision,
            state.ProfileRevision);
    }

    private static 경영SimulationSessionSnapshot Confirm(
        경영SimulationSessionAggregate aggregate,
        string commandId,
        long expectedRevision,
        string actionCode,
        string target,
        string choice = "",
        double localX = 0,
        double localZ = 0,
        double yaw = 0)
        => aggregate.ConfirmNatureSurvivalAction(new()
        {
            CommandId = commandId,
            ExpectedRevision = expectedRevision,
            PlayerStableId = "player:solo",
            ActionCode = actionCode,
            TargetStableId = target,
            ChoiceCode = choice,
            LocalX = localX,
            LocalZ = localZ,
            YawDegrees = yaw,
        });

    private static int FindTriggeringSeed(Guid clientRequestId)
    {
        var sessionId = "simulation-session:" + clientRequestId.ToString("N");
        for (var seed = 1; seed < 10_000; seed++)
        {
            if (NatureSurvivalRules.RollFirstDuskEncounter(seed, sessionId, 0, 1))
                return seed;
        }
        throw new InvalidOperationException("결정적 조우 seed를 찾지 못했습니다.");
    }

    private static 경영SimulationSession생성Request CreateRequest(bool includeNature = true)
        => new()
        {
            ClientRequestId = Guid.Parse("a18a9c48-88ab-41bf-a6b4-02886629d03c"),
            ScenarioStableId = "scenario:nature-survival-fixture",
            ScenarioDataRevision = "fixture.r1",
            ScenarioSeed = 1234,
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
            NatureSurvival = includeNature ? new SimulationNatureSurvivalInitialStateRequest
            {
                PlayerStableId = "player:solo",
                ResourceNodes = Enumerable.Range(1, 6).Select(index =>
                    new SimulationNatureResourceNodeInitialStateRequest
                    {
                        ResourceNodeStableId = $"resource:nature-tree:{index:00}",
                        H2StableId = SimulationNatureSurvivalCodes.HarvestH2StableId,
                        H1StableId = "h1-stock:nature-exploration-buffer",
                        LocalX = -8 + index * 2,
                        LocalZ = 8,
                    }).ToArray(),
            } : null,
        };
}
