using 홍달.도메인.배차;
using 홍달.도메인.기사;
using 홍달.도메인.차량;
using 홍달.도메인.화물;
using 홍달.도메인.화주;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율입력Factory
{
    private async Task<운송의뢰기사조합평가> EvaluateAsync(
        운송원장 queue,
        화주운송의뢰 request,
        화물요구조건? cargoRequirement,
        국내화물운송기사상태Snapshot state,
        용달기사 driver,
        기사근무? currentShift,
        차량제원? vehicleSpec,
        IReadOnlySet<string> rejectedDriverIds,
        int acceptedTransportCount,
        int maxAcceptedTransportCount,
        DateTime now)
    {
        var excluded = new List<string>();
        excluded.AddRange(국내화물배차후보금지정책.조합후보금지사유(driver.기사Id, rejectedDriverIds));
        excluded.AddRange(국내화물배차후보금지정책.기사후보금지사유(
            state,
            driver,
            acceptedTransportCount,
            maxAcceptedTransportCount));

        var pickupPoint = CreatePoint(queue.픽업_위도, queue.픽업_경도);
        var dropoffPoint = CreatePoint(queue.하차_위도, queue.하차_경도);
        var driverPoint = CreatePoint(state.Latitude, state.Longitude);
        if (pickupPoint is null)
        {
            excluded.Add("상차지 좌표가 없습니다.");
        }

        if (dropoffPoint is null)
        {
            excluded.Add("하차지 좌표가 없습니다.");
        }

        if (driverPoint is null)
        {
            excluded.Add("기사 현재 위치가 없습니다.");
        }

        decimal? pickupDistanceKm = null;
        decimal? totalDistanceKm = null;
        decimal? pickupMinutes = null;
        decimal? cargoMinutes = null;
        decimal? totalMinutes = null;
        decimal? tollFare = null;
        decimal? returnDistanceKm = null;
        운송삽입평가결과? scheduleEvaluation = null;
        if (driverPoint is not null && pickupPoint is not null)
        {
            pickupDistanceKm = _경로Service.CalculateDistanceKm(driverPoint, pickupPoint);
            if (pickupDistanceKm.HasValue && !상차접근허용(state, pickupDistanceKm.Value, request, now))
            {
                excluded.Add("기사 허용 반경 또는 상차 시간창 기준을 넘습니다.");
            }
        }

        var fit = _적합성Service.판정(vehicleSpec, request, cargoRequirement);
        if (!fit.적합여부)
        {
            excluded.AddRange(fit.부적합사유);
        }

        if (excluded.Count == 0 && driverPoint is not null && pickupPoint is not null && dropoffPoint is not null)
        {
            var toPickup = await _경로Service.EstimateRouteAsync(driverPoint, pickupPoint);
            var pickupToDropoff = await _경로Service.EstimateRouteAsync(pickupPoint, dropoffPoint);
            pickupMinutes = ToMinutes(toPickup?.Duration);
            cargoMinutes = ToMinutes(pickupToDropoff?.Duration);
            totalMinutes = Sum(pickupMinutes, cargoMinutes);
            totalDistanceKm = Sum(toPickup?.DistanceKm, pickupToDropoff?.DistanceKm);
            tollFare = Sum(toPickup?.TollFare, pickupToDropoff?.TollFare);
            var returnPoint = ResolveReturnPoint(driver, currentShift);
            if (returnPoint is not null)
            {
                var returnRoute = await _경로Service.EstimateRouteAsync(dropoffPoint, returnPoint);
                returnDistanceKm = returnRoute?.DistanceKm ?? _경로Service.CalculateDistanceKm(dropoffPoint, returnPoint);
            }

            if (acceptedTransportCount > 0)
            {
                var schedulePlan = await _기사운송일정구성Service.구성Async(driver.기사Id, driverPoint);
                scheduleEvaluation = await _운송일정삽입평가Service.평가Async(schedulePlan, request);
            }
        }

        decimal? pickupWindowSlackMinutes = null;
        if (request.픽업_시간창_종료일시 > now && pickupMinutes.HasValue)
        {
            pickupWindowSlackMinutes = (decimal)(request.픽업_시간창_종료일시 - now).TotalMinutes - pickupMinutes.Value;
        }

        var additionalDelayMinutes = scheduleEvaluation?.총추가지연분;
        var decision = _판정Service.판정(request, additionalDelayMinutes, pickupWindowSlackMinutes, pickupDistanceKm, fit, scheduleEvaluation);
        var requestDeliveryScope = 국내화물배달권정책.판정(pickupPoint, queue.픽업_도로명주소);
        var driverDeliveryScope = 국내화물배달권정책.판정(driverPoint, driver.주_활동지역);
        var sameDeliveryScope = string.Equals(requestDeliveryScope.배달권키, driverDeliveryScope.배달권키, StringComparison.Ordinal);
        var adjacentDeliveryScope = !sameDeliveryScope && 국내화물배달권정책.인접배달권여부(requestDeliveryScope, driverDeliveryScope);
        var estimatedRevenue = ResolveEstimatedRevenue(request);
        var estimatedCost = EstimateCost(totalDistanceKm, tollFare);
        var estimatedProfit = estimatedRevenue.HasValue && estimatedCost.HasValue
            ? estimatedRevenue.Value - estimatedCost.Value
            : (decimal?)null;
        var returnBurden = 시간대별복귀부담정책.평가(now, returnDistanceKm, state.복귀콜선호);
        var evaluation = _평가Service.평가(
            request,
            decision,
            scheduleEvaluation,
            estimatedProfit,
            additionalDelayMinutes,
            pickupDistanceKm,
            totalMinutes,
            pickupWindowSlackMinutes,
            null,
            false,
            null);

        var score = (evaluation.추천점수 ?? 0m) + state.Aging점수;
        if (estimatedCost.HasValue)
        {
            score -= Math.Clamp(estimatedCost.Value / 5000m, 0m, 20m);
        }

        if (estimatedProfit.HasValue)
        {
            score += Math.Clamp(estimatedProfit.Value / 3000m, -20m, 30m);
        }

        if (returnBurden.부담점수 > 0m)
        {
            score -= returnBurden.부담점수;
        }

        if (returnBurden.보너스점수 > 0m)
        {
            score += returnBurden.보너스점수;
        }

        if (excluded.Count > 0)
        {
            score = Math.Min(score, 0m);
        }

        var reason = state.Aging점수 > 0m
            ? $"{evaluation.추천사유} · 기사대기보정 +{state.Aging점수:0}"
            : evaluation.추천사유;
        if (!string.IsNullOrWhiteSpace(returnBurden.사유))
        {
            reason = $"{reason} · {returnBurden.사유}";
        }

        var badges = evaluation.배지.ToList();
        if (returnBurden.퇴근시간대부담여부)
        {
            badges.Add("퇴근복귀부담");
        }

        if (returnBurden.복귀콜선호 == 기사복귀선호코드.복귀우선 && returnBurden.보너스점수 > 0m)
        {
            badges.Add("복귀콜선호");
        }
        else if (returnBurden.복귀콜선호 == 기사복귀선호코드.수익우선 && returnBurden.부담점수 > 0m)
        {
            badges.Add("수익우선");
        }

        return new 운송의뢰기사조합평가(
            queue.의뢰Id,
            driver.기사Id,
            excluded.Count == 0,
            pickupDistanceKm.HasValue ? Math.Round(pickupDistanceKm.Value, 2) : null,
            pickupMinutes,
            cargoMinutes,
            totalMinutes,
            totalDistanceKm,
            tollFare,
            estimatedRevenue,
            estimatedCost,
            estimatedProfit,
            scheduleEvaluation?.삽입가능여부 ?? acceptedTransportCount == 0,
            scheduleEvaluation?.전체완수가능여부 ?? true,
            scheduleEvaluation?.최적삽입인덱스,
            scheduleEvaluation?.경로변경이점여부 ?? false,
            scheduleEvaluation?.경로변경절감분,
            scheduleEvaluation?.총추가지연분,
            sameDeliveryScope,
            adjacentDeliveryScope,
            returnDistanceKm.HasValue ? Math.Round(returnDistanceKm.Value, 2) : null,
            Math.Round(returnBurden.부담점수, 2),
            returnBurden.퇴근시간대부담여부,
            Math.Round(score, 2),
            reason,
            badges.Distinct(StringComparer.Ordinal).ToArray(),
            fit.경고.Concat(evaluation.경고).Distinct(StringComparer.Ordinal).ToArray(),
            excluded.Distinct(StringComparer.Ordinal).ToArray());
    }

    private bool 상차접근허용(국내화물운송기사상태Snapshot state, decimal distanceKm, 화주운송의뢰 request, DateTime now)
    {
        var baseRadius = Math.Max(1m, _options.기사후보검색반경Km);
        if (distanceKm <= baseRadius)
        {
            return true;
        }

        var driverAllowedRadius = state.상차접근허용반경Km ?? baseRadius;
        var maxRadius = Math.Max(baseRadius, _options.원거리상차접근최대반경Km);
        var allowedRadius = Math.Min(driverAllowedRadius, maxRadius);
        if (distanceKm > allowedRadius)
        {
            return false;
        }

        if (request.픽업_시간창_종료일시 <= now)
        {
            return false;
        }

        var speed = Math.Max(1m, _options.원거리상차평균속도KmH);
        var estimatedMinutes = distanceKm / speed * 60m;
        var remainingMinutes = (decimal)(request.픽업_시간창_종료일시 - now).TotalMinutes;
        return remainingMinutes >= estimatedMinutes + Math.Max(0m, _options.원거리상차도착여유분);
    }
}
