using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using 살뜰.도메인.설정;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.Services.Storage.Local;

namespace 살뜰.Services.Dispatch.Notification;

public sealed class 상차접근알림Service : I상차접근알림Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> 상차접근검사대상상태 = new(StringComparer.Ordinal)
    {
        "매칭중",
        "이동중",
        "상차지도착"
    };

    private readonly SsalddelContext _db;
    private readonly I배차추천경로Service _routeService;
    private readonly ILogger<상차접근알림Service> _logger;

    public 상차접근알림Service(
        SsalddelContext db,
        I배차추천경로Service routeService,
        ILogger<상차접근알림Service> logger)
    {
        _db = db;
        _routeService = routeService;
        _logger = logger;
    }

    public async Task<int> 상차지접근알림검사Async(
        DriverLocationSnapshot location,
        decimal 접근반경Km = 10m,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(location.DriverId) || 접근반경Km <= 0m)
        {
            return 0;
        }

        var activeTransports = await _db.운송원장
            .AsNoTracking()
            .Where(x => x.기사_운송자 == location.DriverId
                        && 상차접근검사대상상태.Contains(x.상태))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);
        if (activeTransports.Count == 0)
        {
            return 0;
        }

        var requestIds = activeTransports
            .Select(x => x.운송번호)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (requestIds.Length == 0)
        {
            return 0;
        }

        var requests = await _db.화주운송의뢰
            .AsNoTracking()
            .Where(x => requestIds.Contains(x.의뢰Id))
            .ToDictionaryAsync(x => x.의뢰Id, StringComparer.Ordinal, cancellationToken);

        var currentPoint = new 배차경로좌표(location.Latitude, location.Longitude);
        var created = 0;
        foreach (var transport in activeTransports)
        {
            if (!requests.TryGetValue(transport.운송번호, out var request)
                || !request.픽업_위도.HasValue
                || !request.픽업_경도.HasValue)
            {
                continue;
            }

            var pickupPoint = new 배차경로좌표(request.픽업_위도.Value, request.픽업_경도.Value);
            var distanceKm = _routeService.CalculateDistanceKm(currentPoint, pickupPoint);
            if (!distanceKm.HasValue || distanceKm.Value > 접근반경Km)
            {
                continue;
            }

            if (await 이미알림생성됨Async(request.의뢰Id, cancellationToken))
            {
                continue;
            }

            _db.Command알림Outbox.Add(new Command알림Outbox
            {
                CommandName = "위치갱신Command",
                EventName = "DriverApproachingPickup",
                FeatureName = "DispatchPickupApproach",
                Target = "Shipper",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    알림유형 = "상차지접근",
                    TargetUserId = request.화주Id,
                    ShipperUserId = request.화주Id,
                    DriverId = location.DriverId,
                    RequestId = request.의뢰Id,
                    CargoType = request.화물종류,
                    PickupAddress = request.픽업_도로명주소,
                    PickupAddressDetail = request.픽업_상세주소,
                    PickupContactName = request.픽업_연락처_이름,
                    PickupContactPhone = request.픽업_연락처_전화번호,
                    PickupWindowStartUtc = request.픽업_시간창_시작일시,
                    PickupWindowEndUtc = request.픽업_시간창_종료일시,
                    DriverLatitude = location.Latitude,
                    DriverLongitude = location.Longitude,
                    DistanceKm = Math.Round(distanceKm.Value, 2),
                    ApproachRadiusKm = 접근반경Km,
                    DetectedAtUtc = DateTime.UtcNow,
                    Title = "기사님이 상차지 근처에 도착하고 있습니다.",
                    Body = $"기사님이 상차지 약 {Math.Round(distanceKm.Value, 1):0.0}km 이내로 접근했습니다. 상차 준비를 확인해 주세요.",
                    Channels = new[] { "Push", "AlimTalk" }
                }, JsonOptions),
                Status = "Pending",
                TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            created++;
        }

        if (created > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "상차지 접근 알림 의도 적재 완료. DriverId={DriverId} Count={Count}",
                location.DriverId,
                created);
        }

        return created;
    }

    private Task<bool> 이미알림생성됨Async(string requestId, CancellationToken cancellationToken)
    {
        var requestIdFragment = $"\"requestId\":\"{requestId}\"";
        return _db.Command알림Outbox
            .AsNoTracking()
            .AnyAsync(x => x.FeatureName == "DispatchPickupApproach"
                           && x.Target == "Shipper"
                           && x.PayloadJson.Contains(requestIdFragment),
                cancellationToken);
    }
}
