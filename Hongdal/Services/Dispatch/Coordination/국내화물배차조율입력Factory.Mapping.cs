using 홍달.도메인.배차;
using 홍달.도메인.기사;
using 홍달.도메인.화주;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율입력Factory
{
    private static 운송의뢰조율입력 ToRequestInput(운송원장 queue, 화주운송의뢰 request)
    {
        var pickupPoint = CreatePoint(queue.픽업_위도, queue.픽업_경도);
        var deliveryScope = 국내화물배달권정책.판정(pickupPoint, queue.픽업_도로명주소);
        return new 운송의뢰조율입력(
            queue.Id,
            queue.의뢰Id,
            queue.원본의뢰유형,
            request.화물종류,
            request.화물온도조건,
            request.화물중량Kg,
            ResolveEstimatedRevenue(request),
            deliveryScope.배달권키,
            deliveryScope.배달권명,
            pickupPoint,
            CreatePoint(queue.하차_위도, queue.하차_경도),
            AsUtc(request.픽업_시간창_시작일시),
            AsUtc(request.픽업_시간창_종료일시),
            AsUtc(request.하차_시간창_시작일시),
            AsUtc(request.하차_시간창_종료일시),
            queue.추천라운드,
            AsUtc(queue.CreatedAt));
    }

    private static 기사후보조율입력 ToDriverInput(
        국내화물운송기사상태Snapshot state,
        용달기사 driver,
        int acceptedTransportCount)
    {
        var currentPoint = CreatePoint(state.Latitude, state.Longitude);
        var deliveryScope = 국내화물배달권정책.판정(currentPoint, driver.주_활동지역);
        return new 기사후보조율입력(
            state.DriverId,
            driver.차량,
            state.운행상태,
            acceptedTransportCount,
            deliveryScope.배달권키,
            deliveryScope.배달권명,
            currentPoint,
            state.Aging점수,
            state.Aging기준시각Utc,
            state.상차접근허용반경Km,
            state.위치수신시각Utc);
    }

    private static int GetAcceptedTransportCount(IReadOnlyDictionary<string, int> counts, string driverId)
        => counts.TryGetValue(driverId, out var count) ? count : 0;

    private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        => latitude.HasValue && longitude.HasValue
            ? new 배차경로좌표(latitude.Value, longitude.Value)
            : null;

    private static 배차경로좌표? ResolveReturnPoint(용달기사 driver, 기사근무? currentShift)
        => CreatePoint(currentShift?.오늘의복귀지위도, currentShift?.오늘의복귀지경도)
           ?? CreatePoint(driver.기본복귀지위도, driver.기본복귀지경도);

    private static decimal? ResolveEstimatedRevenue(화주운송의뢰 request)
    {
        if (request.최종운임.HasValue)
        {
            return request.최종운임.Value;
        }

        return request.결제예정금액.HasValue ? request.결제예정금액.Value : null;
    }

    private static decimal? EstimateCost(decimal? distanceKm, decimal? tollFare)
    {
        if (!distanceKm.HasValue && !tollFare.HasValue)
        {
            return null;
        }

        // 1차 기준 비용: 연료/마모/운행부담을 km당 900원으로 간산화하고 톨비를 더한다.
        return Math.Round((distanceKm ?? 0m) * 900m + (tollFare ?? 0m), 0);
    }

    private static decimal? ToMinutes(TimeSpan? value)
        => value.HasValue ? Math.Round((decimal)value.Value.TotalMinutes, 2) : null;

    private static decimal? Sum(params decimal?[] values)
    {
        decimal sum = 0m;
        var hasValue = false;
        foreach (var value in values)
        {
            if (!value.HasValue)
            {
                continue;
            }

            hasValue = true;
            sum += value.Value;
        }

        return hasValue ? Math.Round(sum, 2) : null;
    }

    private static DateTime? AsUtc(DateTime? value)
        => value.HasValue ? AsUtc(value.Value) : null;

    private static DateTime AsUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
