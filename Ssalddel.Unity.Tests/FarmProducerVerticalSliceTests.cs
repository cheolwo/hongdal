using Ssalddel.Unity.Farm;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Tests.UnityData;

public sealed class FarmProducerVerticalSliceTests
{
    [Fact]
    public async Task 공개작물기준과운영재배상태를_분리해보존한다()
    {
        var snapshot = await UseCase(Response()).실행Async();

        var cultivation = Assert.Single(Assert.Single(Assert.Single(snapshot.Farms).Plots).Cultivations);
        Assert.Equal("crop-reference-category:fc01", cultivation.CropReferenceStableId);
        Assert.Equal("nongsaro:crop-ebook", cultivation.CropReferenceSourceKey);
        Assert.Equal("Growing", cultivation.GrowthStatusCode);
    }

    [Fact]
    public async Task 센서원시값과서버판정근거를_같이보존한다()
    {
        var snapshot = await UseCase(Response()).실행Async();
        var observation = Assert.Single(Assert.Single(Assert.Single(snapshot.Farms).Plots).Sensors)
            .LatestObservation;

        Assert.NotNull(observation);
        Assert.Equal(18.5m, observation!.Value);
        Assert.Equal(FarmSensorConditionCodes.Dry, observation.ConditionCode);
        Assert.Equal("SOIL-WATER-001", observation.EvidenceCardId);
    }

    [Fact]
    public void Mapper는_작물기준Id와출처가_한쪽만있으면거부한다()
    {
        var response = Response();
        response.Farms[0].Plots[0].Cultivations[0].CropReferenceSourceKey = null;

        Assert.Equal(
            "FarmCultivationInvalid",
            Assert.Throws<InvalidOperationException>(() =>
                new FarmProducerPerspectiveMapper().Map(response)).Message);
    }

    [Fact]
    public void Applicator는_StableId로세View계층을적용한다()
    {
        var snapshot = new FarmProducerPerspectiveMapper().Map(Response());
        var plot = new PlotTarget("farm-plot:a.1");
        var crop = new CropTarget("cultivation:a.potato.2026");
        var sensor = new SensorTarget("sensor:a.soil-moisture.1");
        var worker = new WorkerTarget("farm-worker:a.1");

        var unresolved = new FarmProducerPerspectiveApplicator().Apply(
            snapshot,
            new[] { plot },
            new[] { crop },
            new[] { sensor },
            new[] { worker });

        Assert.Empty(unresolved);
        Assert.NotNull(plot.Last);
        Assert.NotNull(crop.Last);
        Assert.NotNull(sensor.Last);
        Assert.NotNull(worker.Last);
    }

    private static FarmProducerPerspectiveQueryUseCase UseCase(
        FarmProducerPerspectiveApiModel response)
        => new(new FarmProducerPerspectiveApiRepository(
            new Client(response),
            new FarmProducerPerspectiveMapper()));

    private static FarmProducerPerspectiveApiModel Response()
        => new()
        {
            StableId = "role-perspective:farm.producer",
            Revision = 7,
            AuthorizedRoleCode = FarmProducerRoleCodes.Producer,
            WorldZoneCode = "farm",
            ViewerScopeCode = "AuthorizedParty",
            SourceTypeCode = "OperationalProjection",
            AuthorizationDecisionId = "authorized:test",
            GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            Farms =
            [
                new FarmApiModel
                {
                    StableId = "farm:a",
                    Revision = 4,
                    FarmName = "A 농장",
                    StatusCode = "Operating",
                    Plots =
                    [
                        new FarmPlotApiModel
                        {
                            StableId = "farm-plot:a.1",
                            Revision = 5,
                            PlotName = "1번 밭",
                            Cultivations =
                            [
                                new FarmCultivationApiModel
                                {
                                    StableId = "cultivation:a.potato.2026",
                                    Revision = 6,
                                    CropName = "감자",
                                    CropReferenceStableId = "crop-reference-category:fc01",
                                    CropReferenceSourceKey = "nongsaro:crop-ebook",
                                    GrowthStatusCode = "Growing",
                                },
                            ],
                            Sensors =
                            [
                                new FarmSensorApiModel
                                {
                                    StableId = "sensor:a.soil-moisture.1",
                                    Revision = 7,
                                    SensorTypeCode = "SoilMoisture",
                                    StatusCode = "Online",
                                    LatestObservation = new FarmSensorObservationApiModel
                                    {
                                        Value = 18.5m,
                                        UnitCode = "Percent",
                                        ObservedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                                        FreshnessStatusCode = "Current",
                                        ConditionCode = FarmSensorConditionCodes.Dry,
                                        AssessmentRuleRevision = "soil-water-rule:3",
                                        EvidenceCardId = "SOIL-WATER-001",
                                    },
                                },
                            ],
                        },
                    ],
                },
            ],
            Workers =
            [
                new NpcMovementApiModel
                {
                    StableId = "npc-movement:farm-task:a.inspect.1",
                    Revision = 8,
                    NpcStableId = "farm-worker:a.1",
                    ActorRoleCode = "Producer",
                    WorldZoneCode = "farm",
                    RouteCode = "farm-producer-round",
                    CurrentWaypointKey = "farm.field-a",
                    DestinationWaypointKey = "farm.sensor-a",
                    MovementStateCode = NpcMovementStateCodes.Moving,
                    ArrivalActionCode = "InspectSensor",
                    SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
                    CanonicalTaskStableId = "farm-task:a.inspect.1",
                    GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            ],
        };

    private sealed class Client(FarmProducerPerspectiveApiModel response)
        : IFarmProducerPerspectiveApiClient
    {
        public Task<FarmProducerPerspectiveApiModel> GetAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(response);
    }

    private sealed class PlotTarget(string stableId) : IFarmPlotTarget
    {
        public string StableId { get; } = stableId;
        public FarmPlotSnapshot? Last { get; private set; }
        public void Apply(FarmPlotSnapshot plot) => Last = plot;
        public void Hide() => Last = null;
    }

    private sealed class CropTarget(string stableId) : IFarmCultivationTarget
    {
        public string StableId { get; } = stableId;
        public FarmCultivationSnapshot? Last { get; private set; }
        public void Apply(FarmCultivationSnapshot cultivation) => Last = cultivation;
        public void Hide() => Last = null;
    }

    private sealed class SensorTarget(string stableId) : IFarmSensorTarget
    {
        public string StableId { get; } = stableId;
        public FarmSensorSnapshot? Last { get; private set; }
        public void Apply(FarmSensorSnapshot sensor) => Last = sensor;
        public void Hide() => Last = null;
    }

    private sealed class WorkerTarget(string stableId) : INpcMovementTarget
    {
        public string NpcStableId { get; } = stableId;
        public NpcMovementSnapshot? Last { get; private set; }
        public void ApplyMovement(NpcMovementSnapshot snapshot) => Last = snapshot;
    }
}
