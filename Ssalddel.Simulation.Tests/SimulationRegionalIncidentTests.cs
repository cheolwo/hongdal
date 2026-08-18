using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationRegionalIncidentTests
{
    [Fact]
    public void 경로압력은_원인심각도두배와전체삼분의일을합산하고_상한을적용한다()
    {
        var routes = SimulationNatureThreatPressurePolicy.Evaluate(
        [
            Incident("incident:farm", SimulationRegionalIncidentCodes.NatureToFarm, 2),
            Incident("incident:town", SimulationRegionalIncidentCodes.NatureToTown, 3),
            Incident("incident:hub", SimulationRegionalIncidentCodes.NatureToCityHub, 4),
        ]);

        var farm = routes.Single(value => value.NatureRouteCode ==
            SimulationRegionalIncidentCodes.NatureToFarm);
        var town = routes.Single(value => value.NatureRouteCode ==
            SimulationRegionalIncidentCodes.NatureToTown);
        var hub = routes.Single(value => value.NatureRouteCode ==
            SimulationRegionalIncidentCodes.NatureToCityHub);

        Assert.Equal(7, farm.EffectivePressure);
        Assert.Equal(SimulationRegionalIncidentCodes.Threatened,
            farm.PressureLevelCode);
        Assert.Equal(9, town.EffectivePressure);
        Assert.Equal(11, hub.EffectivePressure);
        Assert.Equal(3, SimulationNatureThreatPressurePolicy.ThreatUnitCount(
            farm.EffectivePressure));
    }

    [Fact]
    public void 안전하지않은Farm선택은_자연권조우를만들고_같은명령재전송과저장재생은동일하다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var before = HarvestAndCreateIncident(session);
        var incident = Assert.Single(before.RegionalIncidents);

        var preview = session.PreviewRegionalIncidentResponse(incident.EventStableId,
            new SimulationRegionalIncidentResponsePreviewRequest
            {
                ExpectedRevision = before.Revision,
                ActorStableId = "actor:test:manager",
                ChoiceStableId = SimulationRegionalIncidentCodes.FarmLeaveExposed,
            });
        Assert.True(preview.CanConfirm);
        Assert.Equal(2, preview.ProjectedThreatSeverityDelta);
        Assert.Equal(before.Revision, session.Revision);

        var request = new SimulationRegionalIncidentResponseConfirmRequest
        {
            CommandId = "command:test:farm-exposed",
            ExpectedRevision = before.Revision,
            ActorStableId = "actor:test:manager",
            ChoiceStableId = SimulationRegionalIncidentCodes.FarmLeaveExposed,
        };
        var confirmed = session.ConfirmRegionalIncidentResponse(
            incident.EventStableId, request);
        var retried = session.ConfirmRegionalIncidentResponse(
            incident.EventStableId, request);

        Assert.Equal(confirmed.Revision, retried.Revision);
        Assert.Equal(SimulationRegionalIncidentCodes.AdverseOutcome,
            Assert.Single(confirmed.RegionalIncidents).StateCode);
        var route = confirmed.NatureThreat.Routes.Single(value =>
            value.NatureRouteCode == SimulationRegionalIncidentCodes.NatureToFarm);
        Assert.Equal(4, route.EffectivePressure);
        var encounter = Assert.Single(confirmed.NatureThreat.Encounters);
        Assert.Equal(1, encounter.ThreatUnitCount);
        Assert.Equal(SimulationRegionalIncidentCodes.Active, encounter.StateCode);

        var afterVictory = session.ApplyNatureEncounterVictory(
            "battle:test:farm-pressure", encounter.EncounterStableId);
        Assert.Equal(1, Assert.Single(afterVictory.RegionalIncidents).RemainingSeverity);

        var package = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:test:regional-incident",
            ExpectedRevision = session.Revision,
        });
        Assert.Equal(SimulationSaveSchemaVersions.V4, package.SchemaVersion);
        var restored = SimulationSessionReplay.Restore(package);
        var restoredPackage = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = package.SaveStableId,
            ExpectedRevision = restored.Revision,
        });
        Assert.Equal(package.ReplayHash, restoredPackage.ReplayHash);
        Assert.Equal(encounter.EncounterStableId,
            Assert.Single(restored.Snapshot().NatureThreat.Encounters).EncounterStableId);
        Assert.Equal(SimulationRegionalIncidentCodes.Resolved,
            Assert.Single(restored.Snapshot().NatureThreat.Encounters).StateCode);
    }

    [Fact]
    public void 기한전원인Wi를완료하면_압력이생기지않고_기한초과때만압력이생긴다()
    {
        var contained = new 경영SimulationSessionAggregate(CreateRequest());
        contained.RegisterFarmHarvestExposureIncident(
            "harvest:test:contained", "facility:test:farm", 0);
        var incident = Assert.Single(contained.Snapshot().RegionalIncidents);
        contained.ConfirmRegionalIncidentResponse(incident.EventStableId,
            new SimulationRegionalIncidentResponseConfirmRequest
            {
                CommandId = "command:test:farm-safe",
                ExpectedRevision = contained.Revision,
                ActorStableId = "actor:test:manager",
                ChoiceStableId = SimulationRegionalIncidentCodes.FarmCollectAndPack,
            });
        contained.ObserveRegionalIncidentAction(
            SimulationFarmSurvivalCodes.HarvestCollection,
            incident.SourceTargetStableId, 1);
        contained.ObserveRegionalIncidentAction(
            SimulationFarmSurvivalCodes.OutboundPacking,
            incident.SourceTargetStableId, 2);
        var containedState = contained.Snapshot();
        Assert.Equal(SimulationRegionalIncidentCodes.Contained,
            Assert.Single(containedState.RegionalIncidents).OutcomeCode);
        Assert.All(containedState.NatureThreat.Routes,
            value => Assert.Equal(0, value.EffectivePressure));

        var missed = new 경영SimulationSessionAggregate(CreateRequest());
        missed.RegisterFarmHarvestExposureIncident(
            "harvest:test:missed", "facility:test:farm", 0);
        var advanced = missed.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:test:deadline",
            ExpectedRevision = 0,
            TickCount = 3,
        });
        Assert.Equal(SimulationRegionalIncidentCodes.DeadlineMissed,
            Assert.Single(advanced.RegionalIncidents).OutcomeCode);
        Assert.Equal(4, advanced.NatureThreat.Routes.Single(value =>
            value.NatureRouteCode == SimulationRegionalIncidentCodes.NatureToFarm)
            .EffectivePressure);
    }

    [Fact]
    public async Task 세계사건Api는_서버규칙으로선택을미리보고확정한다()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions", CreateRequest());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content
            .ReadFromJsonAsync<경영SimulationSessionSnapshot>();
        var sessionRoute = "/api/simulation/v1/sessions/"
            + Uri.EscapeDataString(created!.SessionStableId);

        var harvestResponse = await client.PostAsJsonAsync(
            sessionRoute + "/farm-survival/work/confirm",
            new SimulationFarmWorkConfirmRequest
            {
                CommandId = "command:http:harvest",
                ExpectedRevision = created.Revision,
                ActorStableId = "actor:test:farmer",
                TargetStableId = "cultivation:test:potato",
                ActionCode = SimulationFarmSurvivalCodes.Harvesting,
                AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
            });
        Assert.Equal(HttpStatusCode.OK, harvestResponse.StatusCode);
        var harvest = await harvestResponse.Content
            .ReadFromJsonAsync<SimulationFarmSurvivalStateSnapshot>();
        var tickResponse = await client.PostAsJsonAsync(sessionRoute + "/ticks",
            new 경영SimulationTick진행Request
            {
                CommandId = "command:http:harvest-tick",
                ExpectedRevision = harvest!.WorldRevision,
                TickCount = 1,
            });
        Assert.Equal(HttpStatusCode.OK, tickResponse.StatusCode);
        var ticked = await tickResponse.Content
            .ReadFromJsonAsync<경영SimulationSessionSnapshot>();
        var incident = Assert.Single(ticked!.RegionalIncidents);
        var eventRoute = sessionRoute + "/world-events/"
            + Uri.EscapeDataString(incident.EventStableId);

        var previewResponse = await client.PostAsJsonAsync(
            eventRoute + "/response-previews",
            new SimulationRegionalIncidentResponsePreviewRequest
            {
                ExpectedRevision = ticked.Revision,
                ActorStableId = "actor:test:manager",
                ChoiceStableId = SimulationRegionalIncidentCodes.FarmLeaveExposed,
            });
        Assert.Equal(HttpStatusCode.OK, previewResponse.StatusCode);
        var preview = await previewResponse.Content
            .ReadFromJsonAsync<SimulationRegionalIncidentResponsePreviewSnapshot>();
        Assert.True(preview!.CanConfirm);

        var confirmResponse = await client.PostAsJsonAsync(
            eventRoute + "/responses/confirm",
            new SimulationRegionalIncidentResponseConfirmRequest
            {
                CommandId = "command:http:farm-exposed",
                ExpectedRevision = ticked.Revision,
                ActorStableId = "actor:test:manager",
                ChoiceStableId = SimulationRegionalIncidentCodes.FarmLeaveExposed,
            });
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content
            .ReadFromJsonAsync<경영SimulationSessionSnapshot>();
        Assert.Single(confirmed!.NatureThreat.Encounters);
    }

    [Fact]
    public void 자연권전투승리는_해당경로의가장오래된원인을한단계만줄이고_재적용되지않는다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        session.RegisterHubCargoBacklogIncident(
            "cargo:test:battle", "facility:test:hub", 0);
        var incident = Assert.Single(session.Snapshot().RegionalIncidents);
        var adverse = session.ConfirmRegionalIncidentResponse(incident.EventStableId,
            new SimulationRegionalIncidentResponseConfirmRequest
            {
                CommandId = "command:test:hub-overflow-for-battle",
                ExpectedRevision = session.Revision,
                ActorStableId = "actor:test:manager",
                ChoiceStableId = SimulationRegionalIncidentCodes.HubOverflowOpenYard,
            });
        var encounter = Assert.Single(adverse.NatureThreat.Encounters);

        var first = session.ApplyNatureEncounterVictory(
            "battle:test:first", encounter.EncounterStableId);
        Assert.Equal(2, Assert.Single(first.RegionalIncidents).RemainingSeverity);
        var retried = session.ApplyNatureEncounterVictory(
            "battle:test:first", encounter.EncounterStableId);
        Assert.Equal(first.Revision, retried.Revision);

        var second = session.ApplyNatureEncounterVictory(
            "battle:test:second", encounter.EncounterStableId);
        Assert.Equal(1, Assert.Single(second.RegionalIncidents).RemainingSeverity);
        Assert.Equal(SimulationRegionalIncidentCodes.Resolved,
            Assert.Single(second.NatureThreat.Encounters).StateCode);
        Assert.Equal(2, second.NatureThreat.Routes.Single(value =>
            value.NatureRouteCode == SimulationRegionalIncidentCodes.NatureToCityHub)
            .EffectivePressure);
    }

    private static SimulationRegionalIncidentSnapshot Incident(
        string id, string route, int remaining)
        => new()
        {
            IncidentStableId = id,
            NatureRouteCode = route,
            RemainingSeverity = remaining,
            OccurredWorldTick = 1,
        };

    private static 경영SimulationSessionSnapshot HarvestAndCreateIncident(
        경영SimulationSessionAggregate session)
    {
        session.ConfirmFarmWork(new SimulationFarmWorkConfirmRequest
        {
            CommandId = "command:test:harvest",
            ExpectedRevision = session.Revision,
            ActorStableId = "actor:test:farmer",
            TargetStableId = "cultivation:test:potato",
            ActionCode = SimulationFarmSurvivalCodes.Harvesting,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
        });
        return session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:test:harvest-tick",
            ExpectedRevision = session.Revision,
            TickCount = 1,
        });
    }

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.Parse("90c74be8-45dc-4cb2-ad25-8607ba4a347b"),
            ScenarioStableId = "scenario:test.regional-incident",
            ScenarioDataRevision = "scenario-data:test:r1",
            ScenarioSeed = 20260818,
            RuleRevision = "rule:test:regional-incident:r1",
            DurationTicks = 30,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:test:player",
                TerritoryStableId = "territory:test:pyeongchang",
                SettlementStableId = "settlement:test:world",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 8, 18, 0, 0, 0, TimeSpan.Zero),
            },
            SpatialWorld = PyeongchangSimulation공간상호작용Fixture.CreateFarmHubSupply(
                "facility:test:farm", "facility:test:market"),
            FarmSurvival = new SimulationFarmSurvivalInitialStateRequest
            {
                RuleRevision = SimulationFarmSurvivalCodes.RuleRevision,
                RegionStableId = "region:test:farm",
                AreaStableId = "area:test:farm",
                TileKey = "kr5186:l2:700:1145",
                FarmBuildingStableId = "facility:test:farm",
                Actors =
                [
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = "actor:test:farmer",
                        ActorKindCode = SimulationFarmSurvivalCodes.Player,
                        KoreanName = "시험 농장 작업자",
                        CapabilityCodes =
                        [
                            SimulationFarmActorCapabilityCodes.FarmHarvest,
                            SimulationFarmActorCapabilityCodes.FarmCollection,
                            SimulationFarmActorCapabilityCodes.FarmPacking,
                        ],
                    },
                ],
                CultivationUnits =
                [
                    new Simulation재배단위Snapshot
                    {
                        CultivationUnitStableId = "cultivation:test:potato",
                        Revision = 1,
                        TileStableId = "soil:test:potato",
                        CultivationStableId = "crop:test:potato",
                        ProductStableId = "product:potato",
                        CropVariantStableId = "crop-variant:potato.fixture",
                        StateCode = Simulation재배단위상태Codes.HarvestReady,
                        PhysicalAreaSquareMeters = 100m,
                        EffectiveCultivationAreaRatio = 1m,
                        SourceStableIds = ["source:test:cultivation"],
                    },
                ],
                PotatoProductionRule = new Simulation감자생산RuleSnapshot
                {
                    RuleStableId = "rule:test:potato-production",
                    RuleRevision = 1,
                    SourceTypeCode = Simulation생산규칙SourceTypeCodes.Fixture,
                    ProductStableId = "product:potato",
                    CropVariantStableId = "crop-variant:potato.fixture",
                    BaseYieldKilogramsPerSquareMeter = 3m,
                    MinimumEnvironmentFactor = 0.5m,
                    MaximumEnvironmentFactor = 1m,
                    MinimumInputFactor = 0.8m,
                    MaximumInputFactor = 1.2m,
                    MinimumFacilityFactor = 0.8m,
                    MaximumFacilityFactor = 1.2m,
                    MinimumLossFactor = 0.1m,
                    MaximumLossFactor = 1m,
                    SourceStableIds = ["source:test:potato-rule"],
                    Limitations = ["시험 전용"],
                },
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
