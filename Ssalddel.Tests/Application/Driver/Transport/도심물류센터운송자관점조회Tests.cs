using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Controllers.Driver.Progress05;
using System.Text.Json;

namespace Ssalddel.Tests.Application.Driver.Transport;

public sealed class 도심물류센터운송자관점조회Tests
{
    [Fact]
    public async Task 현재기사의_운송만_주소와연락처없이_운송자관점으로_변환한다()
    {
        var current = CurrentTransport(기사운송상태코드.배차확정);
        current.출발지 = "노출하면 안 되는 상차 주소";
        current.도착지 = "노출하면 안 되는 하차 주소";
        current.수령자명 = "수령자";
        current.수령자연락처 = "010-0000-0000";
        current.운임 = 125000m;
        var reader = new FakeCurrentTransportReader(current);
        var handler = new 도심물류센터운송자관점조회QueryHandler(reader);

        var response = await handler.Handle(
            new 도심물류센터운송자관점조회Query("driver-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(RolePerspectiveRoleCodes.Transporter, response.AuthorizedRoleCode);
        Assert.Equal(RolePerspectiveWorldZoneCodes.UrbanLogisticsCenter, response.WorldZoneCode);
        Assert.Equal(RolePerspectiveViewerScopeCodes.AuthorizedParty, response.ViewerScopeCode);
        Assert.Equal("driver-1", reader.LastDriverId);
        Assert.Contains(response.ObjectEmphases, item =>
            item.TargetStableId == "transport:71"
            && item.EmphasisCode == RolePerspectiveEmphasisCodes.Primary);
        Assert.Contains(response.ObjectEmphases, item =>
            item.TargetStableId == "transport-stop:71.pickup"
            && item.EmphasisCode == RolePerspectiveEmphasisCodes.Destination);
        Assert.DoesNotContain(
            typeof(RolePerspectiveResponse).GetProperties(),
            property => property.Name.Contains("Address", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("Contact", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("운임", StringComparison.Ordinal));
    }

    [Fact]
    public async Task 상태전이정책상_가능한_Command만_확인과재조회조건으로_제공한다()
    {
        var handler = new 도심물류센터운송자관점조회QueryHandler(
            new FakeCurrentTransportReader(CurrentTransport(기사운송상태코드.상차지도착)));

        var response = await handler.Handle(
            new 도심물류센터운송자관점조회Query("driver-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Contains(response.AllowedInteractions, item =>
            item.InteractionCode == "inspect-current-transport"
            && item.EffectCode == RolePerspectiveInteractionEffectCodes.ReadOnly);
        var command = Assert.Single(
            response.AllowedInteractions,
            item => item.EffectCode == RolePerspectiveInteractionEffectCodes.ServerCommand);
        Assert.Equal("complete-pickup", command.InteractionCode);
        Assert.True(command.RequiresExplicitConfirmation);
        Assert.True(command.RequiresCanonicalStateRefresh);
        Assert.DoesNotContain(response.AllowedInteractions, item => item.InteractionCode == "arrive-pickup");
    }

    [Fact]
    public async Task 현재배정운송이_없으면_관점도_없다()
    {
        var handler = new 도심물류센터운송자관점조회QueryHandler(
            new FakeCurrentTransportReader(null));

        var response = await handler.Handle(
            new 도심물류센터운송자관점조회Query("driver-1"),
            CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public void API는_기사인증과_고정된Zone경로를_요구한다()
    {
        var controllerType = typeof(기사World관점Controller);
        var authorize = Assert.Single(controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true))
            as AuthorizeAttribute;
        var route = Assert.Single(controllerType.GetCustomAttributes(typeof(RouteAttribute), true))
            as RouteAttribute;

        Assert.NotNull(authorize);
        Assert.Equal("기사", authorize.Roles);
        Assert.NotNull(route);
        Assert.Equal(RolePerspectiveRoutes.DriverUrbanLogisticsCenter, route.Template);
        Assert.DoesNotContain("{driverId", route.Template, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("{role", route.Template, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("배차확정", "logistics.vehicle-gate", "logistics.loading-bay", "Moving", "wait-for-loading")]
    [InlineData("상차지도착", "logistics.loading-bay", "logistics.loading-bay", "PerformingAction", "load-cargo")]
    [InlineData("상차완료", "logistics.loading-bay", "logistics.vehicle-exit", "Moving", "depart-zone")]
    [InlineData("운송중", "logistics.loading-bay", "logistics.vehicle-exit", "Moving", "depart-zone")]
    public async Task 운송상태는_물류센터SemanticWaypoint_Npc이동으로_변환된다(
        string transportState,
        string currentWaypoint,
        string destinationWaypoint,
        string movementState,
        string arrivalAction)
    {
        var reader = new FakeCurrentTransportReader(CurrentTransport(transportState));
        var handler = new 도심물류센터운송자Npc이동조회QueryHandler(reader);

        var response = await handler.Handle(
            new 도심물류센터운송자Npc이동조회Query("driver-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("npc:transport-driver.71", response.NpcStableId);
        Assert.Equal("transport-task:71", response.CanonicalTaskStableId);
        Assert.Equal("logistics-center-transporter-handoff", response.RouteCode);
        Assert.Equal(currentWaypoint, response.CurrentWaypointKey);
        Assert.Equal(destinationWaypoint, response.DestinationWaypointKey);
        Assert.Equal(movementState, response.MovementStateCode);
        Assert.Equal(arrivalAction, response.ArrivalActionCode);
    }

    [Fact]
    public async Task 물류센터를_떠난_운송상태에는_센터Npc이동을_만들지않는다()
    {
        var handler = new 도심물류센터운송자Npc이동조회QueryHandler(
            new FakeCurrentTransportReader(CurrentTransport(기사운송상태코드.하차지도착)));

        var response = await handler.Handle(
            new 도심물류센터운송자Npc이동조회Query("driver-1"),
            CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public void Npc이동계약은_Unity좌표와_개인정보를_포함하지않는다()
    {
        var propertyNames = typeof(NpcMovementResponse)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.Contains(nameof(NpcMovementResponse.DestinationWaypointKey), propertyNames);
        Assert.DoesNotContain(propertyNames, name =>
            name is "X" or "Y" or "Z" or "Position"
            || name.Contains("Address", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Contact", StringComparison.OrdinalIgnoreCase));

        var method = typeof(기사World관점Controller)
            .GetMethod(nameof(기사World관점Controller.도심물류센터Npc이동조회));
        var httpGet = Assert.Single(method!.GetCustomAttributes(typeof(HttpGetAttribute), true))
            as HttpGetAttribute;
        Assert.Equal("npc-movement", httpGet!.Template);
        Assert.Equal(
            RolePerspectiveRoutes.DriverUrbanLogisticsCenter + "/npc-movement",
            NpcMovementRoutes.DriverUrbanLogisticsCenter);
    }

    [Theory]
    [InlineData("상차완료", "운송중", "InTransit", 1, "transport-network")]
    [InlineData("운송중", "운송중", "InTransit", 1, "transport-network")]
    [InlineData("하차지도착", "운송중", "ArrivedAtWarehouse", 2, "warehouse")]
    [InlineData("하차지도착", "입고완료", "ReceivingCompleted", 2, "warehouse")]
    public async Task 운송과입고상태는_창고화물인계Workflow로_변환된다(
        string transportState,
        string inboundState,
        string workflowState,
        int movementCount,
        string expectedZone)
    {
        var handler = new 기사창고화물인계조회QueryHandler(
            new FakeCurrentTransportReader(CurrentTransport(transportState)),
            new FakeInboundReader(Inbound(inboundState)));

        var response = await handler.Handle(
            new 기사창고화물인계조회Query("driver-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(workflowState, response.HandoffStateCode);
        Assert.Equal("cargo:transport-71", response.CargoStableId);
        Assert.Equal("transport-task:71", response.TransportTaskStableId);
        Assert.Equal("inbound-task:91", response.InboundTaskStableId);
        Assert.Equal(movementCount, response.Movements.Count);
        Assert.All(response.Movements, item => Assert.Equal(expectedZone, item.WorldZoneCode));
    }

    [Fact]
    public async Task 창고도착시_운송Npc와입고Npc가_같은Dock으로_이동한다()
    {
        var handler = new 기사창고화물인계조회QueryHandler(
            new FakeCurrentTransportReader(CurrentTransport(기사운송상태코드.하차지도착)),
            new FakeInboundReader(Inbound(입고상태코드.운송중)));

        var response = await handler.Handle(
            new 기사창고화물인계조회Query("driver-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Contains(response.Movements, item =>
            item.ActorRoleCode == RolePerspectiveRoleCodes.Transporter
            && item.DestinationWaypointKey == "warehouse.inbound-dock"
            && item.ArrivalActionCode == "open-cargo-door");
        Assert.Contains(response.Movements, item =>
            item.ActorRoleCode == "WarehouseInboundWorker"
            && item.DestinationWaypointKey == "warehouse.inbound-dock"
            && item.ArrivalActionCode == "unload-cargo");
    }

    [Fact]
    public async Task 연계입고가_없으면_화물인계Workflow를_만들지않는다()
    {
        var handler = new 기사창고화물인계조회QueryHandler(
            new FakeCurrentTransportReader(CurrentTransport(기사운송상태코드.운송중)),
            new FakeInboundReader(null));

        var response = await handler.Handle(
            new 기사창고화물인계조회Query("driver-1"),
            CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public void 창고인계API는_기사인증을요구하고_개인정보와좌표를포함하지않는다()
    {
        var controllerType = typeof(기사창고인계WorldController);
        var authorize = Assert.Single(controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true))
            as AuthorizeAttribute;
        var route = Assert.Single(controllerType.GetCustomAttributes(typeof(RouteAttribute), true))
            as RouteAttribute;

        Assert.Equal("기사", authorize!.Roles);
        Assert.Equal(NpcMovementRoutes.DriverWarehouseHandoff, route!.Template);
        Assert.DoesNotContain(typeof(CargoWarehouseHandoffResponse).GetProperties(), property =>
            property.Name.Contains("Address", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Contact", StringComparison.OrdinalIgnoreCase)
            || property.Name is "X" or "Y" or "Z" or "Position");
    }

    [Fact]
    public async Task 창고인계응답은_UnityWire의_camelCase와_ISO시각으로직렬화된다()
    {
        var handler = new 기사창고화물인계조회QueryHandler(
            new FakeCurrentTransportReader(CurrentTransport(기사운송상태코드.하차지도착)),
            new FakeInboundReader(Inbound(입고상태코드.운송중)));
        var response = await handler.Handle(
            new 기사창고화물인계조회Query("driver-1"), CancellationToken.None);

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("ArrivedAtWarehouse", root.GetProperty("handoffStateCode").GetString());
        Assert.Equal("cargo:transport-71", root.GetProperty("cargoStableId").GetString());
        Assert.True(DateTimeOffset.TryParse(root.GetProperty("generatedAt").GetString(), out _));
        var movement = root.GetProperty("movements")[0];
        Assert.Equal("warehouse", movement.GetProperty("worldZoneCode").GetString());
        Assert.True(DateTimeOffset.TryParse(movement.GetProperty("generatedAt").GetString(), out _));
    }

    private static 기사운송요약응답 CurrentTransport(string state)
    {
        return new 기사운송요약응답
        {
            Id = 71,
            운송번호 = "transport-request-71",
            기사_운송자 = "driver-1",
            상태 = state,
            UpdatedAt = new DateTime(2026, 8, 8, 5, 0, 0, DateTimeKind.Utc),
        };
    }

    private static 운송연계입고Projection Inbound(string state)
    {
        return new 운송연계입고Projection(
            91,
            31,
            state,
            new DateTime(2026, 8, 8, 5, 5, 0, DateTimeKind.Utc));
    }

    private sealed class FakeCurrentTransportReader
        : IRequestHandler<운송현재조회Query, 기사운송요약응답?>
    {
        private readonly 기사운송요약응답? response;

        public FakeCurrentTransportReader(기사운송요약응답? response)
        {
            this.response = response;
        }

        public string? LastDriverId { get; private set; }

        public Task<기사운송요약응답?> Handle(
            운송현재조회Query request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastDriverId = request.기사Id;
            return Task.FromResult(response);
        }
    }

    private sealed class FakeInboundReader
        : IRequestHandler<운송연계입고조회Query, 운송연계입고Projection?>
    {
        private readonly 운송연계입고Projection? response;

        public FakeInboundReader(운송연계입고Projection? response)
        {
            this.response = response;
        }

        public Task<운송연계입고Projection?> Handle(
            운송연계입고조회Query request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal("transport-request-71", request.운송의뢰Id);
            return Task.FromResult(response);
        }
    }
}
