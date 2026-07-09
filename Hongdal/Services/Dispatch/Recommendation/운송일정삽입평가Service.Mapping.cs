using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 운송일정삽입평가Service
    {
        private static 기사운송일정항목[] CreateCandidateItems(int baseIndex, 화주운송의뢰 request)
        {
            return
            [
                new 기사운송일정항목(
                    request.의뢰Id,
                    "pickup",
                    request.픽업_도로명주소,
                    CreatePoint(request.픽업_위도, request.픽업_경도),
                    request.픽업_시간창_시작일시,
                    request.픽업_시간창_종료일시,
                    baseIndex,
                    null,
                    false,
                    true),
                new 기사운송일정항목(
                    request.의뢰Id,
                    "dropoff",
                    request.하차_도로명주소,
                    CreatePoint(request.하차_위도, request.하차_경도),
                    request.하차_시간창_시작일시,
                    request.하차_시간창_종료일시,
                    baseIndex + 1,
                    null,
                    false,
                    true)
            ];
        }

        private static IReadOnlyList<string> BuildRouteOrder(IReadOnlyList<기사운송일정항목> items)
        {
            return items
                .OrderBy(x => x.순서)
                .Select(x =>
                {
                    var stage = string.Equals(x.단계유형, "pickup", StringComparison.OrdinalIgnoreCase)
                        ? "상차"
                        : "하차";
                    var source = x.후보의뢰여부 ? "추천" : "기존";
                    return $"{source} {stage} {x.의뢰Id}";
                })
                .ToArray();
        }

        private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        {
            return latitude.HasValue && longitude.HasValue
                ? new 배차경로좌표(latitude.Value, longitude.Value)
                : null;
        }
    }
}
