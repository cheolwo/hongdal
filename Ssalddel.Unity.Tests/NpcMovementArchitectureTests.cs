using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Tests.UnityData;

public sealed class NpcMovementArchitectureTests
{
    private static readonly DateTimeOffset GeneratedAt =
        DateTimeOffset.Parse("2026-08-08T15:00:00+09:00");

    [Fact]
    public void ZoneNpcRouteCatalog는_공유Zone별_의미경로를_제공한다()
    {
        Assert.Empty(ZoneNpcRouteCatalog.Validate());

        var populatedZones = new[]
        {
            WorldZoneCodes.CommunityMarketSquare,
            WorldZoneCodes.PublicDataHall,
            WorldZoneCodes.Farm,
            WorldZoneCodes.CooperativeHall,
            WorldZoneCodes.MarketOrder,
            WorldZoneCodes.ResidentialCommunity,
            WorldZoneCodes.TraditionalMarket,
            WorldZoneCodes.UrbanLogisticsCenter,
            WorldZoneCodes.TransportNetwork,
            WorldZoneCodes.Warehouse,
        };

        Assert.All(populatedZones, zone => Assert.NotEmpty(ZoneNpcRouteCatalog.ForZone(zone)));
        Assert.Empty(ZoneNpcRouteCatalog.ForZone(WorldZoneCodes.PersonalMeditation));
        Assert.All(
            ZoneNpcRouteCatalog.All,
            route => Assert.All(route.WaypointKeys, key => Assert.DoesNotContain(',', key)));
    }

    [Fact]
    public void 운영Npc는_Canonical업무가_있을때만_이동Snapshot으로_변환된다()
    {
        var source = Movement(
            NpcMovementSourceTypeCodes.OperationalProjection,
            "transport-task:71");

        var snapshot = new NpcMovementMapper().Map(source);

        Assert.Equal("npc:driver-1", snapshot.NpcStableId);
        Assert.Equal("transport-task:71", snapshot.CanonicalTaskStableId);
        Assert.Equal("logistics.loading-bay", snapshot.DestinationWaypointKey);

        source.CanonicalTaskStableId = string.Empty;
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new NpcMovementMapper().Map(source));
        Assert.Equal("OperationalNpcCanonicalTaskMissing", exception.Message);
    }

    [Fact]
    public void SimulationNpc는_실제업무를_가졌다고_표시할수없다()
    {
        var source = Movement(
            NpcMovementSourceTypeCodes.SimulatedFixture,
            "transport-task:71");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new NpcMovementMapper().Map(source));

        Assert.Equal("SimulatedNpcMustNotClaimCanonicalTask", exception.Message);
    }

    [Fact]
    public void NpcMovement는_Vector좌표가_아니라_ZoneWaypoint를_사용한다()
    {
        var properties = typeof(NpcMovementApiModel).GetProperties();

        Assert.Contains(properties, property => property.Name == nameof(NpcMovementApiModel.DestinationWaypointKey));
        Assert.DoesNotContain(properties, property =>
            property.Name is "X" or "Y" or "Z" or "Position" or "DestinationPosition");
    }

    [Fact]
    public void Applicator는_NpcStableId가_일치하는_표현대상만_움직인다()
    {
        var snapshot = new NpcMovementMapper().Map(Movement(
            NpcMovementSourceTypeCodes.OperationalProjection,
            "transport-task:71"));
        var driver = new FakeNpcTarget("npc:driver-1");
        var other = new FakeNpcTarget("npc:worker-2");

        var unresolved = new NpcMovementApplicator().Apply(
            new[] { snapshot },
            new INpcMovementTarget[] { driver, other });

        Assert.Empty(unresolved);
        Assert.Same(snapshot, driver.LastSnapshot);
        Assert.Null(other.LastSnapshot);
    }

    [Fact]
    public void Moving상태는_현재와다른_목적Waypoint를_요구한다()
    {
        var source = Movement(
            NpcMovementSourceTypeCodes.OperationalProjection,
            "transport-task:71");
        source.DestinationWaypointKey = source.CurrentWaypointKey;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new NpcMovementMapper().Map(source));

        Assert.Equal("MovingNpcDestinationUnchanged", exception.Message);
    }

    [Fact]
    public void Npc는_자기역할에_정의된_Zone경로만_사용한다()
    {
        var source = Movement(
            NpcMovementSourceTypeCodes.OperationalProjection,
            "transport-task:71");
        source.ActorRoleCode = "Orderer";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new NpcMovementMapper().Map(source));

        Assert.Equal("NpcRouteActorRoleMismatch", exception.Message);
    }

    [Fact]
    public async Task Repository와UseCase는_Zone일치후_운영NpcSnapshot을_반환한다()
    {
        var apiClient = new FakeNpcMovementApiClient(Movement(
            NpcMovementSourceTypeCodes.OperationalProjection,
            "transport-task:71"));
        var useCase = new NpcMovementQueryUseCase(
            new NpcMovementApiRepository(apiClient, new NpcMovementMapper()));

        var snapshot = await useCase.실행Async(new NpcMovementQuery
        {
            WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
        });

        Assert.NotNull(snapshot);
        Assert.Equal("npc:driver-1", snapshot.NpcStableId);
        Assert.Equal(1, apiClient.CallCount);
        Assert.Equal(
            "api/v1/driver/world/zones/urban-logistics-center/perspective/npc-movement",
            NpcMovementApiRoutes.DriverUrbanLogisticsCenter);
    }

    [Theory]
    [InlineData("InTransit", 1)]
    [InlineData("ArrivedAtWarehouse", 2)]
    [InlineData("ReceivingCompleted", 2)]
    public async Task 화물인계Repository는_운송과창고NpcWorkflow를_검증한다(
        string stateCode,
        int expectedMovementCount)
    {
        var apiClient = new FakeCargoHandoffApiClient(Handoff(stateCode));
        var useCase = new CargoWarehouseHandoffQueryUseCase(
            new CargoWarehouseHandoffApiRepository(
                apiClient,
                new CargoWarehouseHandoffMapper(new NpcMovementMapper())));

        var snapshot = await useCase.실행Async();

        Assert.NotNull(snapshot);
        Assert.Equal(stateCode, snapshot.HandoffStateCode);
        Assert.Equal(expectedMovementCount, snapshot.Movements.Length);
        Assert.Equal("cargo:transport-71", snapshot.CargoStableId);
        Assert.Equal(
            "api/v1/driver/world/workflows/warehouse-handoff",
            CargoWarehouseHandoffApiRoutes.DriverWarehouseHandoff);
    }

    [Fact]
    public void 창고도착Workflow는_운송자와입고작업자를_모두요구한다()
    {
        var source = Handoff(CargoHandoffStateCodes.ArrivedAtWarehouse);
        source.Movements = source.Movements
            .Where(item => item.ActorRoleCode == "Transporter")
            .ToArray();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new CargoWarehouseHandoffMapper(new NpcMovementMapper()).Map(source));

        Assert.Equal("CargoHandoffWarehouseMovementsInvalid", exception.Message);
    }

    [Fact]
    public void 화물인계Applicator는_낮은Revision으로_되돌리지않는다()
    {
        var mapper = new CargoWarehouseHandoffMapper(new NpcMovementMapper());
        var latest = mapper.Map(Handoff(CargoHandoffStateCodes.ArrivedAtWarehouse));
        latest.Revision = 9;
        var stale = mapper.Map(Handoff(CargoHandoffStateCodes.InTransit));
        stale.Revision = 8;
        var target = new FakeCargoHandoffTarget();
        var applicator = new CargoWarehouseHandoffApplicator();

        Assert.True(applicator.Apply(latest, target));
        Assert.False(applicator.Apply(stale, target));
        Assert.Same(latest, target.LastSnapshot);
    }

    private static NpcMovementApiModel Movement(string sourceTypeCode, string canonicalTaskStableId)
    {
        return new NpcMovementApiModel
        {
            StableId = "npc-movement:driver-1",
            Revision = 3,
            NpcStableId = "npc:driver-1",
            ActorRoleCode = "Transporter",
            WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
            RouteCode = "logistics-center-transporter-handoff",
            CurrentWaypointKey = "logistics.vehicle-gate",
            DestinationWaypointKey = "logistics.loading-bay",
            MovementStateCode = NpcMovementStateCodes.Moving,
            ArrivalActionCode = "wait-for-loading",
            SourceTypeCode = sourceTypeCode,
            CanonicalTaskStableId = canonicalTaskStableId,
            GeneratedAt = GeneratedAt,
        };
    }

    private static CargoWarehouseHandoffApiModel Handoff(string stateCode)
    {
        var inTransit = string.Equals(stateCode, CargoHandoffStateCodes.InTransit, StringComparison.Ordinal);
        var completed = string.Equals(stateCode, CargoHandoffStateCodes.ReceivingCompleted, StringComparison.Ordinal);
        var movements = inTransit
            ? new[]
            {
                MovementForHandoff(
                    "npc:transport-driver.71", "Transporter", WorldZoneCodes.TransportNetwork,
                    "transport-network-hub-delivery", "network.logistics-center", "network.warehouse",
                    "transport-task:71", "arrive-at-warehouse")
            }
            : new[]
            {
                MovementForHandoff(
                    "npc:transport-driver.71", "Transporter", WorldZoneCodes.Warehouse,
                    "warehouse-transporter-dropoff",
                    completed ? "warehouse.inbound-dock" : "warehouse.approach",
                    completed ? "warehouse.vehicle-exit" : "warehouse.inbound-dock",
                    "transport-task:71", completed ? "depart-warehouse" : "open-cargo-door"),
                MovementForHandoff(
                    "npc:warehouse-inbound-worker.91", "WarehouseInboundWorker", WorldZoneCodes.Warehouse,
                    "warehouse-inbound-worker-handoff",
                    completed ? "warehouse.inspection-zone" : "warehouse.staff-entry",
                    completed ? "warehouse.storage-zone" : "warehouse.inbound-dock",
                    "inbound-task:91", completed ? "store-cargo" : "unload-cargo")
            };

        return new CargoWarehouseHandoffApiModel
        {
            StableId = "cargo-handoff:transport-71.inbound-91",
            Revision = 5,
            HandoffStateCode = stateCode,
            CargoStableId = "cargo:transport-71",
            TransportTaskStableId = "transport-task:71",
            InboundTaskStableId = "inbound-task:91",
            Movements = movements,
            GeneratedAt = GeneratedAt,
        };
    }

    private static NpcMovementApiModel MovementForHandoff(
        string npcStableId,
        string roleCode,
        string zoneCode,
        string routeCode,
        string currentWaypoint,
        string destinationWaypoint,
        string canonicalTaskStableId,
        string arrivalAction)
    {
        return new NpcMovementApiModel
        {
            StableId = "npc-movement:" + npcStableId.Replace(':', '-'),
            Revision = 5,
            NpcStableId = npcStableId,
            ActorRoleCode = roleCode,
            WorldZoneCode = zoneCode,
            RouteCode = routeCode,
            CurrentWaypointKey = currentWaypoint,
            DestinationWaypointKey = destinationWaypoint,
            MovementStateCode = NpcMovementStateCodes.Moving,
            ArrivalActionCode = arrivalAction,
            SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
            CanonicalTaskStableId = canonicalTaskStableId,
            GeneratedAt = GeneratedAt,
        };
    }

    private sealed class FakeNpcTarget : INpcMovementTarget
    {
        public FakeNpcTarget(string stableId)
        {
            NpcStableId = stableId;
        }

        public string NpcStableId { get; }

        public NpcMovementSnapshot? LastSnapshot { get; private set; }

        public void ApplyMovement(NpcMovementSnapshot snapshot)
        {
            LastSnapshot = snapshot;
        }
    }

    private sealed class FakeNpcMovementApiClient : INpcMovementApiClient
    {
        private readonly NpcMovementApiModel? response;

        public FakeNpcMovementApiClient(NpcMovementApiModel? response)
        {
            this.response = response;
        }

        public int CallCount { get; private set; }

        public Task<NpcMovementApiModel?> GetAsync(
            NpcMovementQuery query,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(response);
        }
    }

    private sealed class FakeCargoHandoffApiClient : ICargoWarehouseHandoffApiClient
    {
        private readonly CargoWarehouseHandoffApiModel? response;

        public FakeCargoHandoffApiClient(CargoWarehouseHandoffApiModel? response)
        {
            this.response = response;
        }

        public Task<CargoWarehouseHandoffApiModel?> GetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(response);
        }
    }

    private sealed class FakeCargoHandoffTarget : ICargoWarehouseHandoffTarget
    {
        public CargoWarehouseHandoffSnapshot? LastSnapshot { get; private set; }

        public void ApplyHandoff(CargoWarehouseHandoffSnapshot snapshot)
        {
            LastSnapshot = snapshot;
        }
    }
}
