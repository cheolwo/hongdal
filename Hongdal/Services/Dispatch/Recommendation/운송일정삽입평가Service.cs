using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I운송일정삽입평가Service
    {
        Task<운송삽입평가결과> 평가Async(기사운송일정계획 계획, 화주운송의뢰 후보의뢰, CancellationToken cancellationToken = default);
    }

    public sealed class 운송일정삽입평가Service : I운송일정삽입평가Service
    {
        private readonly I배차추천경로Service _routeService;

        public 운송일정삽입평가Service(I배차추천경로Service routeService)
        {
            _routeService = routeService;
        }

        public async Task<운송삽입평가결과> 평가Async(기사운송일정계획 계획, 화주운송의뢰 후보의뢰, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var baseline = await 시뮬레이션Async(계획.시작좌표, 계획.항목목록, 계획.기준시각, cancellationToken);
            var candidateItems = CreateCandidateItems(계획.항목목록.Count, 후보의뢰);
            var attempts = new List<운송삽입시도결과>();
            운송삽입시도결과? simpleAppendAttempt = null;

            for (var insertIndex = 0; insertIndex <= 계획.항목목록.Count; insertIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var simulatedItems = 계획.항목목록.ToList();
                simulatedItems.InsertRange(insertIndex, candidateItems.Select((x, offset) => x with { 순서 = insertIndex + offset }));
                var reordered = simulatedItems
                    .Select((x, index) => x with { 순서 = index })
                    .ToList();

                var simulation = await 시뮬레이션Async(계획.시작좌표, reordered, 계획.기준시각, cancellationToken);
                var extraDelay = simulation.총소요시간분.HasValue && baseline.총소요시간분.HasValue
                    ? Math.Max(0m, simulation.총소요시간분.Value - baseline.총소요시간분.Value)
                    : simulation.총소요시간분;

                var attempt = new 운송삽입시도결과(
                    insertIndex,
                    insertIndex == 계획.항목목록.Count ? "하차후이어가기" : "연속삽입",
                    false,
                    simulation.전체완수가능여부,
                    simulation.총소요시간분,
                    simulation.총거리Km,
                    extraDelay,
                    null,
                    simulation.최대시간위반분,
                    simulation.위반사유,
                    BuildRouteOrder(reordered),
                    simulation.도착예상목록);
                attempts.Add(attempt);

                if (insertIndex == 계획.항목목록.Count)
                {
                    simpleAppendAttempt = attempt;
                }
            }

            if (simpleAppendAttempt?.총소요시간분 is not null)
            {
                attempts = attempts
                    .Select(x => x.시도유형 == "하차후이어가기"
                        ? x
                        : x with
                        {
                            경로변경시도여부 = true,
                            단순이어가기대비절감분 = x.총소요시간분.HasValue
                                ? Math.Round(simpleAppendAttempt.총소요시간분.Value - x.총소요시간분.Value, 2)
                                : null
                        })
                    .ToList();
            }

            var splitAttempts = await CreateSplitInsertionAttemptsAsync(
                계획,
                candidateItems,
                baseline.총소요시간분,
                simpleAppendAttempt?.총소요시간분,
                cancellationToken);
            attempts.AddRange(splitAttempts);

            var beneficialRouteChange = attempts
                .Where(x => x.전체완수가능여부
                            && x.경로변경시도여부
                            && x.단순이어가기대비절감분.HasValue
                            && x.단순이어가기대비절감분.Value > 0m)
                .OrderByDescending(x => x.단순이어가기대비절감분 ?? 0m)
                .ThenBy(x => x.총소요시간분 ?? decimal.MaxValue)
                .FirstOrDefault();

            var feasible = attempts
                .Where(x => x.전체완수가능여부)
                .Where(x => !x.경로변경시도여부)
                .OrderBy(x => x.총추가지연분 ?? decimal.MaxValue)
                .ThenByDescending(x => x.총거리Km ?? 0m)
                .FirstOrDefault();

            var fallback = attempts
                .OrderBy(x => x.최대시간위반분 ?? decimal.MaxValue)
                .ThenBy(x => x.총추가지연분 ?? decimal.MaxValue)
                .FirstOrDefault();

            var selected = beneficialRouteChange ?? feasible ?? fallback;
            if (selected is null)
            {
                return new 운송삽입평가결과(false, false, null, false, null, null, null, null, null, ["삽입 시뮬레이션 결과가 없습니다."], [], [], attempts);
            }

            return new 운송삽입평가결과(
                beneficialRouteChange is not null || feasible is not null,
                selected.전체완수가능여부,
                selected.삽입인덱스,
                beneficialRouteChange is not null,
                beneficialRouteChange?.단순이어가기대비절감분,
                selected.총소요시간분,
                selected.총거리Km,
                selected.총추가지연분,
                selected.최대시간위반분,
                selected.위반사유,
                selected.경로순서,
                selected.도착예상목록,
                attempts);
        }

        private async Task<IReadOnlyList<운송삽입시도결과>> CreateSplitInsertionAttemptsAsync(
            기사운송일정계획 계획,
            IReadOnlyList<기사운송일정항목> 후보항목목록,
            decimal? 기준총소요시간분,
            decimal? 단순이어가기총소요시간분,
            CancellationToken cancellationToken)
        {
            if (후보항목목록.Count != 2)
            {
                return [];
            }

            var attempts = new List<운송삽입시도결과>();
            var pickup = 후보항목목록[0];
            var dropoff = 후보항목목록[1];
            var baseCount = 계획.항목목록.Count;

            for (var pickupIndex = 0; pickupIndex <= baseCount; pickupIndex++)
            {
                for (var dropoffIndex = pickupIndex + 1; dropoffIndex <= baseCount + 1; dropoffIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (dropoffIndex == pickupIndex + 1)
                    {
                        continue;
                    }

                    var simulatedItems = 계획.항목목록.ToList();
                    simulatedItems.Insert(pickupIndex, pickup);
                    simulatedItems.Insert(dropoffIndex, dropoff);
                    var reordered = simulatedItems
                        .Select((x, index) => x with { 순서 = index })
                        .ToList();

                    var simulation = await 시뮬레이션Async(계획.시작좌표, reordered, 계획.기준시각, cancellationToken);
                    var extraDelay = simulation.총소요시간분.HasValue && 기준총소요시간분.HasValue
                        ? Math.Max(0m, simulation.총소요시간분.Value - 기준총소요시간분.Value)
                        : simulation.총소요시간분;
                    var savedMinutes = simulation.총소요시간분.HasValue && 단순이어가기총소요시간분.HasValue
                        ? Math.Round(단순이어가기총소요시간분.Value - simulation.총소요시간분.Value, 2)
                        : (decimal?)null;

                    attempts.Add(new 운송삽입시도결과(
                        pickupIndex,
                        "분리삽입",
                        true,
                        simulation.전체완수가능여부,
                        simulation.총소요시간분,
                        simulation.총거리Km,
                        extraDelay,
                        savedMinutes,
                        simulation.최대시간위반분,
                        simulation.위반사유,
                        BuildRouteOrder(reordered),
                        simulation.도착예상목록));
                }
            }

            return attempts;
        }

        private async Task<(bool 전체완수가능여부, decimal? 총소요시간분, decimal? 총거리Km, decimal? 최대시간위반분, string[] 위반사유, IReadOnlyList<운송일정도착예상항목> 도착예상목록)> 시뮬레이션Async(
            배차경로좌표? 시작좌표,
            IReadOnlyList<기사운송일정항목> 항목목록,
            DateTime 기준시각,
            CancellationToken cancellationToken)
        {
            var nowPoint = 시작좌표;
            var nowTime = 기준시각;
            decimal totalMinutes = 0m;
            decimal totalDistance = 0m;
            decimal? maxViolation = null;
            var violations = new List<string>();
            var arrivals = new List<운송일정도착예상항목>(항목목록.Count);

            foreach (var item in 항목목록.OrderBy(x => x.순서))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var route = await _routeService.EstimateRouteAsync(nowPoint, item.좌표);
                if (route?.Duration is null)
                {
                    violations.Add($"{item.의뢰Id} {item.단계유형} 경로를 계산할 수 없습니다.");
                    arrivals.Add(new 운송일정도착예상항목(item.의뢰Id, item.단계유형, item.순서, item.주소, null, item.시간창종료일시, true, null));
                    nowPoint = item.좌표;
                    continue;
                }

                totalMinutes += (decimal)route.Duration.Value.TotalMinutes;
                totalDistance += route.DistanceKm ?? 0m;

                var rawArrival = nowTime.Add(route.Duration.Value);
                var effectiveArrival = item.기준시각.HasValue && rawArrival < item.기준시각.Value
                    ? item.기준시각.Value
                    : rawArrival;

                decimal? violationMinutes = null;
                var violates = false;
                if (item.시간창종료일시.HasValue && effectiveArrival > item.시간창종료일시.Value)
                {
                    violates = true;
                    violationMinutes = (decimal)(effectiveArrival - item.시간창종료일시.Value).TotalMinutes;
                    maxViolation = !maxViolation.HasValue
                        ? violationMinutes
                        : Math.Max(maxViolation.Value, violationMinutes.Value);
                    violations.Add($"{item.의뢰Id} {item.단계유형} 시간창을 {Math.Round(violationMinutes.Value, 0):0}분 초과합니다.");
                }

                arrivals.Add(new 운송일정도착예상항목(
                    item.의뢰Id,
                    item.단계유형,
                    item.순서,
                    item.주소,
                    effectiveArrival,
                    item.시간창종료일시,
                    violates,
                    violationMinutes.HasValue ? Math.Round(violationMinutes.Value, 2) : null));

                nowTime = effectiveArrival;
                nowPoint = item.좌표;
            }

            return (violations.Count == 0, Math.Round(totalMinutes, 2), Math.Round(totalDistance, 2), maxViolation.HasValue ? Math.Round(maxViolation.Value, 2) : null, violations.Distinct(StringComparer.Ordinal).ToArray(), arrivals);
        }

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
