using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationFarmSurvivalTests
{
    private const string Player = "actor:sim:player-survivor";
    private const string Npc = "actor:sim:farm-worker";
    private const string SoilA = "soil-tile:sim:daegwallyeong:0:0";
    private const string SoilB = "soil-tile:sim:daegwallyeong:0:1";

    [Fact]
    public void 플레이어직접노동과Npc위임은_같은농장원장에서다른비용으로진행된다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());

        var playerPreview = session.PreviewFarmWork(Preview(
            0, Player, SoilA, SimulationFarmSurvivalCodes.PlayerDirect));
        Assert.True(playerPreview.CanConfirm);
        Assert.Equal(0m, playerPreview.RequiredLabor);
        Assert.Equal(1, playerPreview.DurationTicks);

        var playerState = session.ConfirmFarmWork(Confirm(
            "command:farm-work:player-till", 0, Player, SoilA,
            SimulationFarmSurvivalCodes.PlayerDirect));
        var npcPreview = session.PreviewFarmWork(Preview(
            playerState.WorldRevision, Npc, SoilB,
            SimulationFarmSurvivalCodes.NpcDelegated));
        Assert.True(npcPreview.CanConfirm);
        Assert.Equal(3m, npcPreview.RequiredLabor);
        Assert.Equal(2, npcPreview.DurationTicks);

        var npcState = session.ConfirmFarmWork(Confirm(
            "command:farm-work:npc-till", playerState.WorldRevision,
            Npc, SoilB, SimulationFarmSurvivalCodes.NpcDelegated));
        Assert.Equal(3m, session.Snapshot().Settlement!.LaborReserved);
        Assert.All(npcState.WorkOrders,
            value => Assert.Equal(SimulationFarmSurvivalCodes.InProgress,
                value.StatusCode));

        var tickOne = session.Advance(Tick("command:tick:day-2",
            npcState.WorldRevision));
        Assert.Equal(SimulationFarmSurvivalCodes.Tilled,
            tickOne.FarmSurvival!.SoilTiles.Single(value =>
                value.SoilTileStableId == SoilA).StateCode);
        Assert.Equal(SimulationFarmSurvivalCodes.Untilled,
            tickOne.FarmSurvival.SoilTiles.Single(value =>
                value.SoilTileStableId == SoilB).StateCode);

        var tickTwo = session.Advance(Tick("command:tick:day-3", tickOne.Revision));
        Assert.Equal(SimulationFarmSurvivalCodes.Tilled,
            tickTwo.FarmSurvival!.SoilTiles.Single(value =>
                value.SoilTileStableId == SoilB).StateCode);
        Assert.Equal(0m, tickTwo.Settlement!.LaborReserved);
        Assert.Equal(85m, tickTwo.FarmSurvival.Actors.Single(value =>
            value.ActorStableId == Player).Stamina);
    }

    [Fact]
    public void 다섯째날위협은경고로먼저보이고_방어부족결과는복구가능하게남는다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());

        var dayFive = session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:tick:to-day-5",
            ExpectedRevision = 0,
            TickCount = 4,
        });
        var warning = Assert.Single(dayFive.FarmSurvival!.Encounters);
        Assert.Equal(SimulationFarmSurvivalCodes.Warning, warning.StateCode);
        Assert.Equal(SimulationFarmSurvivalCodes.ZombieWarningPresentation,
            warning.PresentationKey);

        var warningEvent = Assert.Single(session.GetWorldEvents(0).Events);
        Assert.Equal(SimulationWorldEventCodes.FarmThreatEncounter,
            warningEvent.EventTypeCode);
        Assert.Equal("kr5186:l2:438:419", Assert.Single(warningEvent.TileKeys));
        Assert.True(warningEvent.SimulationOnly);
        Assert.False(warningEvent.IsOperationalState);

        var daySix = session.Advance(Tick("command:tick:day-6", dayFive.Revision));
        var zombie = daySix.FarmSurvival!.Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.ZombiePressure);
        var raider = daySix.FarmSurvival.Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction);
        Assert.Equal(SimulationFarmSurvivalCodes.Resolved, zombie.StateCode);
        Assert.True(zombie.Recoverable);
        Assert.True(daySix.FarmSurvival.RecoverableDamageUnits > 0m);
        Assert.Equal(SimulationFarmSurvivalCodes.AwaitingResponse, raider.StateCode);

        var changes = session.GetWorldEvents(dayFive.Revision).Events;
        Assert.Equal(2, changes.Length);
        Assert.Contains(changes, value => value.EventStableId.EndsWith(
            raider.EncounterStableId, StringComparison.Ordinal)
            && value.Choices.Length == 3 && value.CanRespond);
    }

    [Fact]
    public void 약탈자대응은선택Id만받고_수치결과는서버Seed와방어상태가정한다()
    {
        var first = AdvanceToRaider(new 경영SimulationSessionAggregate(CreateRequest()));
        var second = AdvanceToRaider(new 경영SimulationSessionAggregate(CreateRequest()));
        var firstEncounter = first.Session.GetFarmSurvivalState().Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction);
        var secondEncounter = second.Session.GetFarmSurvivalState().Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction);

        var firstResult = first.Session.ConfirmThreatResponse(new SimulationThreatResponseConfirmRequest
        {
            CommandId = "command:threat:deception",
            ExpectedRevision = first.Revision,
            EncounterStableId = firstEncounter.EncounterStableId,
            ActorStableId = Player,
            ChoiceStableId = SimulationFarmSurvivalCodes.Deception,
        });
        var secondResult = second.Session.ConfirmThreatResponse(new SimulationThreatResponseConfirmRequest
        {
            CommandId = "command:threat:deception",
            ExpectedRevision = second.Revision,
            EncounterStableId = secondEncounter.EncounterStableId,
            ActorStableId = Player,
            ChoiceStableId = SimulationFarmSurvivalCodes.Deception,
        });

        var firstResolved = firstResult.Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction);
        var secondResolved = secondResult.Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction);
        Assert.Equal(firstResolved.OutcomeCode, secondResolved.OutcomeCode);
        Assert.Equal(firstResolved.SupplyLossUnits, secondResolved.SupplyLossUnits);
        Assert.Equal(firstResolved.DamageUnits, secondResolved.DamageUnits);
        Assert.True(firstResolved.Recoverable);
    }

    [Fact]
    public void 위협부상은_의료휴식작업으로회복할수있다()
    {
        var advanced = AdvanceToRaider(
            new 경영SimulationSessionAggregate(CreateRequest()));
        var injured = advanced.Session.GetFarmSurvivalState().Actors.Single(value =>
            value.Injured);
        Assert.True(injured.Injured);
        var assignment = injured.ActorKindCode == SimulationFarmSurvivalCodes.Player
            ? SimulationFarmSurvivalCodes.PlayerDirect
            : SimulationFarmSurvivalCodes.NpcDelegated;

        var resting = advanced.Session.ConfirmFarmWork(
            new SimulationFarmWorkConfirmRequest
            {
                CommandId = "command:farm-work:medical-rest",
                ExpectedRevision = advanced.Revision,
                ActorStableId = injured.ActorStableId,
                TargetStableId = injured.ActorStableId,
                ActionCode = SimulationFarmSurvivalCodes.MedicalRest,
                AssignmentKindCode = assignment,
            });
        var recovered = advanced.Session.Advance(
            Tick("command:tick:medical-rest", resting.WorldRevision));

        var actor = recovered.FarmSurvival!.Actors.Single(value =>
            value.ActorStableId == injured.ActorStableId);
        Assert.False(actor.Injured);
        Assert.Equal(100m, actor.Health);
        Assert.Equal(3m, recovered.FarmSurvival.RepairMaterialUnits);
    }

    [Fact]
    public void 농장노동과위협대응은SaveReplay후에도같은상태Hash를만든다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var work = session.ConfirmFarmWork(Confirm(
            "command:farm-work:save-replay", 0, Player, SoilA,
            SimulationFarmSurvivalCodes.PlayerDirect));
        session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:tick:save-replay-to-day-6",
            ExpectedRevision = work.WorldRevision,
            TickCount = 5,
        });
        var before = session.GetFarmSurvivalState();
        var raider = before.Encounters.Single(value =>
            value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction);
        session.ConfirmThreatResponse(new SimulationThreatResponseConfirmRequest
        {
            CommandId = "command:threat:save-replay",
            ExpectedRevision = before.WorldRevision,
            EncounterStableId = raider.EncounterStableId,
            ActorStableId = Player,
            ChoiceStableId = SimulationFarmSurvivalCodes.Trade,
        });

        var package = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:farm-survival:week-one",
            ExpectedRevision = session.Revision,
        });
        var restored = SimulationSessionReplay.Restore(package);

        var originalState = session.GetFarmSurvivalState();
        var restoredState = restored.GetFarmSurvivalState();
        Assert.Equal(originalState.WorldRevision, restoredState.WorldRevision);
        Assert.Equal(originalState.SupplyUnits, restoredState.SupplyUnits);
        Assert.Equal(originalState.RecoverableDamageUnits,
            restoredState.RecoverableDamageUnits);
        Assert.Equal(
            originalState.WorkOrders.Select(value =>
                (value.WorkOrderStableId, value.StatusCode)),
            restoredState.WorkOrders.Select(value =>
                (value.WorkOrderStableId, value.StatusCode)));
        Assert.Equal(
            originalState.Encounters.Select(value =>
                (value.EncounterStableId, value.StateCode, value.OutcomeCode)),
            restoredState.Encounters.Select(value =>
                (value.EncounterStableId, value.StateCode, value.OutcomeCode)));
        Assert.Equal(package.ReplayHash, restored.CreateSavePackage(
            new SimulationSessionSaveRequest
            {
                SaveStableId = package.SaveStableId,
                ExpectedRevision = restored.Revision,
            }).ReplayHash);
    }

    [Fact]
    public async Task HTTP에서도_농장노동Preview와Confirm을수직으로처리한다()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions", CreateRequest());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var session = await createResponse.Content
            .ReadFromJsonAsync<경영SimulationSessionSnapshot>();
        var route = "/api/simulation/v1/sessions/"
            + Uri.EscapeDataString(session!.SessionStableId)
            + "/farm-survival";

        var state = await client.GetFromJsonAsync<SimulationFarmSurvivalStateSnapshot>(
            route);
        Assert.NotNull(state);
        Assert.True(state.SimulationOnly);

        var previewResponse = await client.PostAsJsonAsync(route + "/work/preview",
            Preview(state.WorldRevision, Player, SoilA,
                SimulationFarmSurvivalCodes.PlayerDirect));
        Assert.Equal(HttpStatusCode.OK, previewResponse.StatusCode);
        var preview = await previewResponse.Content
            .ReadFromJsonAsync<SimulationFarmWorkPreviewSnapshot>();
        Assert.True(preview!.CanConfirm);

        var confirmResponse = await client.PostAsJsonAsync(route + "/work/confirm",
            Confirm("command:farm-work:http", state.WorldRevision,
                Player, SoilA, SimulationFarmSurvivalCodes.PlayerDirect));
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content
            .ReadFromJsonAsync<SimulationFarmSurvivalStateSnapshot>();
        Assert.Equal(SimulationFarmSurvivalCodes.InProgress,
            Assert.Single(confirmed!.WorkOrders).StatusCode);
    }

    private static (경영SimulationSessionAggregate Session, long Revision)
        AdvanceToRaider(경영SimulationSessionAggregate session)
    {
        var result = session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:tick:to-raider",
            ExpectedRevision = 0,
            TickCount = 5,
        });
        return (session, result.Revision);
    }

    private static SimulationFarmWorkPreviewRequest Preview(
        long revision,
        string actor,
        string target,
        string assignment)
        => new()
        {
            ExpectedRevision = revision,
            ActorStableId = actor,
            TargetStableId = target,
            ActionCode = SimulationFarmSurvivalCodes.Tilling,
            AssignmentKindCode = assignment,
        };

    private static SimulationFarmWorkConfirmRequest Confirm(
        string commandId,
        long revision,
        string actor,
        string target,
        string assignment)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = revision,
            ActorStableId = actor,
            TargetStableId = target,
            ActionCode = SimulationFarmSurvivalCodes.Tilling,
            AssignmentKindCode = assignment,
        };

    private static 경영SimulationTick진행Request Tick(
        string commandId,
        long revision)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = revision,
            TickCount = 1,
        };

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.Parse("5c5ed8c4-a504-4771-a5d8-c5e5a79054a0"),
            ScenarioStableId = "scenario:sim.daegwallyeong-spring-survival",
            ScenarioDataRevision = "scenario-data:2026-08-15",
            ScenarioSeed = 20260815,
            RuleRevision = "rule:survival-season-r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim.survivors",
                TerritoryStableId = "territory:sim.pyeongchang",
                SettlementStableId = "settlement:sim.daegwallyeong-farm",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
            Settlement = new SimulationSettlementInitialStateRequest
            {
                TreasuryBalance = 100m,
                CurrencyCode = "SIM",
                LaborCapacityTotal = 10m,
                LaborReserved = 0m,
                StorageCapacity = 20m,
                StorageOccupied = 5m,
                StorageUnitCode = "unit",
                PopulationCount = 2,
                PopulationFoodDemandPerTick = 2m,
                GarrisonCount = 0,
                GarrisonFoodDemandPerTick = 0m,
                FoodEquivalentUnitCode = "person-day",
                FoodEquivalentRuleRevision = "food-equivalent:sim-r1",
                Districts =
                [
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.daegwallyeong-farm",
                        DistrictTypeCode = "FarmDistrict",
                        SourceStableIds = ["source:scenario-farm-survival"],
                    },
                ],
                Facilities =
                [
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.farm-storage",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:sim.daegwallyeong-farm",
                        SourceStableIds = ["source:scenario-farm-survival"],
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.farm-market",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:sim.daegwallyeong-farm",
                        SourceStableIds = ["source:scenario-farm-survival"],
                    },
                ],
                SourceStableIds = ["source:scenario-farm-survival"],
            },
            FarmSurvival = new SimulationFarmSurvivalInitialStateRequest
            {
                RegionStableId = "region:legal-dong:5176031000",
                AreaStableId = "area:sim.daegwallyeong-farm",
                TileKey = "kr5186:l2:438:419",
                FarmBuildingStableId = "building:sim.daegwallyeong-farmhouse",
                SupplyUnits = 8m,
                RepairMaterialUnits = 4m,
                Actors =
                [
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = Player,
                        ActorKindCode = SimulationFarmSurvivalCodes.Player,
                        KoreanName = "플레이어 생존자",
                    },
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = Npc,
                        ActorKindCode = SimulationFarmSurvivalCodes.Npc,
                        KoreanName = "농장 일꾼",
                    },
                ],
                SoilTiles =
                [
                    new SimulationFarmSoilTileInitialStateRequest
                    {
                        SoilTileStableId = SoilA,
                        GridX = 0,
                        GridY = 0,
                    },
                    new SimulationFarmSoilTileInitialStateRequest
                    {
                        SoilTileStableId = SoilB,
                        GridX = 0,
                        GridY = 1,
                    },
                ],
                Defenses =
                [
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = "defense:sim:fence",
                        DefenseKindCode = SimulationFarmSurvivalCodes.Fence,
                        Durability = 60m,
                    },
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = "defense:sim:storage-lock",
                        DefenseKindCode = SimulationFarmSurvivalCodes.StorageLock,
                        Durability = 80m,
                    },
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = "defense:sim:lighting",
                        DefenseKindCode = SimulationFarmSurvivalCodes.Lighting,
                        Durability = 100m,
                    },
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = "defense:sim:guard-post",
                        DefenseKindCode = SimulationFarmSurvivalCodes.GuardPost,
                        Durability = 100m,
                    },
                ],
            },
        };

    private static WebApplicationFactory<Program> CreateFactory()
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["SsalddelExecution:Mode"] = "Simulation",
                            ["SimulationServer:Enabled"] = "true",
                            ["SimulationSharedPublicData:Enabled"] = "false",
                        });
                });
            });
}
