using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Driver.Transport;

namespace Ssalddel.Application.Driver.Transport;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Application,
    "현재 기사 운송 상태를 도심 물류센터 semantic waypoint NPC 이동으로 변환한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(NpcMovementResponse),
    FlowOrder = 50,
    Boundary = "현재 배정 운송의 상차 전·상차 중·센터 출발 상태만 표현하고 도착 animation으로 업무 상태를 변경하지 않는다.")]
public sealed class 도심물류센터운송자Npc이동조회QueryHandler
    : IRequestHandler<도심물류센터운송자Npc이동조회Query, NpcMovementResponse?>
{
    private const string RouteCode = "logistics-center-transporter-handoff";
    private const string VehicleGate = "logistics.vehicle-gate";
    private const string LoadingBay = "logistics.loading-bay";
    private const string VehicleExit = "logistics.vehicle-exit";

    private readonly IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader;

    public 도심물류센터운송자Npc이동조회QueryHandler(
        IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader)
    {
        this.currentTransportReader = currentTransportReader;
    }

    public async Task<NpcMovementResponse?> Handle(
        도심물류센터운송자Npc이동조회Query request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.기사Id);

        var transport = await currentTransportReader.Handle(
            new 운송현재조회Query(request.기사Id),
            cancellationToken);
        if (transport is null)
        {
            return null;
        }

        var movement = ResolveMovement(transport.상태);
        if (movement is null)
        {
            return null;
        }

        var revision = transport.UpdatedAt.Ticks;
        return new NpcMovementResponse
        {
            StableId = $"npc-movement:transport-{transport.Id}",
            Revision = revision,
            NpcStableId = $"npc:transport-driver.{transport.Id}",
            ActorRoleCode = RolePerspectiveRoleCodes.Transporter,
            WorldZoneCode = RolePerspectiveWorldZoneCodes.UrbanLogisticsCenter,
            RouteCode = RouteCode,
            CurrentWaypointKey = movement.Value.CurrentWaypoint,
            DestinationWaypointKey = movement.Value.DestinationWaypoint,
            MovementStateCode = movement.Value.StateCode,
            ArrivalActionCode = movement.Value.ArrivalActionCode,
            SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
            CanonicalTaskStableId = $"transport-task:{transport.Id}",
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    private static Movement? ResolveMovement(string currentState)
    {
        if (!string.Equals(currentState, 기사운송상태코드.상차지도착, StringComparison.Ordinal)
            && 기사운송상태전이Policy.가능한가(currentState, 기사운송상태코드.상차지도착))
        {
            return new Movement(
                VehicleGate,
                LoadingBay,
                NpcMovementStateCodes.Moving,
                "wait-for-loading");
        }

        if (string.Equals(currentState, 기사운송상태코드.상차지도착, StringComparison.Ordinal))
        {
            return new Movement(
                LoadingBay,
                LoadingBay,
                NpcMovementStateCodes.PerformingAction,
                "load-cargo");
        }

        if (string.Equals(currentState, 기사운송상태코드.상차완료, StringComparison.Ordinal)
            || string.Equals(currentState, 기사운송상태코드.운송중, StringComparison.Ordinal))
        {
            return new Movement(
                LoadingBay,
                VehicleExit,
                NpcMovementStateCodes.Moving,
                "depart-zone");
        }

        return null;
    }

    private readonly record struct Movement(
        string CurrentWaypoint,
        string DestinationWaypoint,
        string StateCode,
        string ArrivalActionCode);
}
