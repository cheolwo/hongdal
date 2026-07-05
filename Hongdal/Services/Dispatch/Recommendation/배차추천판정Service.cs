using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I배차추천판정Service
    {
        배차추천판정결과 판정(화주운송의뢰? request, decimal? 추가지연분, decimal? 픽업시간창여유분, decimal? 경로기준거리Km, 운송삽입평가결과? 일정삽입평가결과 = null);
        배차추천판정결과 판정(화주운송의뢰? request, decimal? 추가지연분, decimal? 픽업시간창여유분, decimal? 경로기준거리Km, 차량화물적합성결과? 적합성결과, 운송삽입평가결과? 일정삽입평가결과 = null);
    }

    public sealed class 배차추천판정Service : I배차추천판정Service
    {
        private const decimal 묶음삽입최대지연분 = 10m;
        private const decimal 묶음삽입조건부최대지연분 = 20m;
        private const decimal 다음콜최대거리Km = 5m;

        public 배차추천판정결과 판정(화주운송의뢰? request, decimal? 추가지연분, decimal? 픽업시간창여유분, decimal? 경로기준거리Km, 운송삽입평가결과? 일정삽입평가결과 = null)
            => 판정(request, 추가지연분, 픽업시간창여유분, 경로기준거리Km, null, 일정삽입평가결과);

        public 배차추천판정결과 판정(화주운송의뢰? request, decimal? 추가지연분, decimal? 픽업시간창여유분, decimal? 경로기준거리Km, 차량화물적합성결과? 적합성결과, 운송삽입평가결과? 일정삽입평가결과 = null)
        {
            var 허용지연분 = ResolveAllowedDelayMinutes(request);
            var 화물민감여부 = IsCargoSensitive(request);
            var 단독배송여부 = request is not null && IsSingleOnlyRequest(request);
            var 차량적합여부 = 적합성결과?.적합여부 ?? true;
            var 차량부적합사유 = 적합성결과?.부적합사유 ?? Array.Empty<string>();
            var 차량경고 = 적합성결과?.경고 ?? Array.Empty<string>();

            if (!차량적합여부)
            {
                return new 배차추천판정결과(
                    "single",
                    허용지연분,
                    화물민감여부,
                    단독배송여부,
                    false,
                    false,
                    false,
                    차량부적합사유,
                    차량경고);
            }

            var 일정삽입가능 = 일정삽입평가결과?.삽입가능여부 ?? true;
            var 전체완수가능 = 일정삽입평가결과?.전체완수가능여부 ?? true;

            var 묶음삽입가능 = request is not null
                                 && 일정삽입가능
                                 && 전체완수가능
                                 && !단독배송여부
                                 && IsBundleInsertAllowed(request)
                                 && 경로기준거리Km.HasValue
                                 && 경로기준거리Km.Value <= 8m
                                 && (!추가지연분.HasValue || 추가지연분.Value <= Math.Min(허용지연분, 묶음삽입조건부최대지연분))
                                 && (!픽업시간창여유분.HasValue || 픽업시간창여유분.Value >= -3m);

            var 도착후추천가능 = request is not null
                                  && 전체완수가능
                                  && 경로기준거리Km.HasValue
                                  && 경로기준거리Km.Value <= 다음콜최대거리Km
                                  && (!픽업시간창여유분.HasValue || 픽업시간창여유분.Value >= -5m);

            var 추천유형 = 묶음삽입가능
                ? "bundle_insert"
                : 도착후추천가능
                    ? "next_after_dropoff"
                    : "single";

            return new 배차추천판정결과(
                추천유형,
                허용지연분,
                화물민감여부,
                단독배송여부,
                묶음삽입가능,
                도착후추천가능,
                차량적합여부,
                차량부적합사유,
                차량경고);
        }

        private static decimal ResolveAllowedDelayMinutes(화주운송의뢰? request)
        {
            if (request is null)
            {
                return 0m;
            }

            if (IsSingleOnlyRequest(request))
            {
                return 0m;
            }

            var text = string.Join(' ', new[] { request.운송방식, request.서비스레벨, request.요청사항, request.화물종류, request.화물설명 }
                .Where(x => !string.IsNullOrWhiteSpace(x))!);

            if (text.Contains("긴급", StringComparison.OrdinalIgnoreCase) || text.Contains("특급", StringComparison.OrdinalIgnoreCase))
            {
                return 5m;
            }

            if (text.Contains("경유", StringComparison.OrdinalIgnoreCase) || text.Contains("묶음", StringComparison.OrdinalIgnoreCase))
            {
                return 20m;
            }

            return 10m;
        }

        private static bool IsSingleOnlyRequest(화주운송의뢰 request)
        {
            var text = string.Join(' ', new[] { request.운송방식, request.서비스레벨, request.요청사항 }
                .Where(x => !string.IsNullOrWhiteSpace(x))!);

            return text.Contains("단독", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("혼적불가", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("경유불가", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("묶음불가", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBundleInsertAllowed(화주운송의뢰 request)
        {
            if (IsSingleOnlyRequest(request))
            {
                return false;
            }

            if (request.화물파손주의여부)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(request.화물온도조건) && !string.Equals(request.화물온도조건, "상온", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var text = string.Join(' ', new[] { request.운송방식, request.서비스레벨, request.요청사항, request.화물종류, request.화물설명 }
                .Where(x => !string.IsNullOrWhiteSpace(x))!);

            return !text.Contains("긴급", StringComparison.OrdinalIgnoreCase)
                   && !text.Contains("특급", StringComparison.OrdinalIgnoreCase)
                   && !text.Contains("서류", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCargoSensitive(화주운송의뢰? request)
        {
            if (request is null)
            {
                return false;
            }

            return request.화물파손주의여부
                   || (!string.IsNullOrWhiteSpace(request.화물온도조건) && !string.Equals(request.화물온도조건, "상온", StringComparison.OrdinalIgnoreCase))
                   || (request.서비스레벨?.Contains("긴급", StringComparison.OrdinalIgnoreCase) ?? false)
                   || (request.서비스레벨?.Contains("특급", StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}