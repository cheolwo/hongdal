using 살뜰.도메인.공통;
using 살뜰.Services.Dispatch.Coordination;

namespace 살뜰.Services.Dispatch.Recommendation;

public interface I멀티배차조합Service
{
    IReadOnlyList<멀티배차조합후보> 조합생성(멀티배차조합요청 request);
}

public interface I음식멀티배차조합Service
{
    IReadOnlyList<멀티배차조합후보> 조합생성(멀티배차조합요청 request);
}

public interface I음식멀티배차조합AIService
{
    IReadOnlyList<멀티배차조합후보> 후보정렬(음식멀티배차조합AI요청 request);
}

[Obsolete("음식 배달 묶음은 I음식멀티배차조합Service/음식멀티배차조합Service를 사용하세요. 용달 멀티배차는 별도 정책으로 분리합니다.")]
public sealed class 멀티배차조합Service(I배차추천경로Service routeService) : I멀티배차조합Service
{
    private readonly 음식멀티배차조합Service _inner = new(routeService);

    public IReadOnlyList<멀티배차조합후보> 조합생성(멀티배차조합요청 request)
        => _inner.조합생성(request);
}

public sealed class 음식멀티배차조합Service : I음식멀티배차조합Service
{
    private readonly I배차추천경로Service _routeService;
    private readonly I음식멀티배차조합AIService _aiService;
    private readonly I배차AI판단근거조회Service _판단근거조회Service;

    private const string 단건배차 = "단건배차";
    private const string 멀티배차 = "멀티배차";

    public 음식멀티배차조합Service(I배차추천경로Service routeService)
        : this(routeService, new 규칙기반음식멀티배차조합AIService(), new 규칙기반배차AI판단근거조회Service())
    {
    }

    public 음식멀티배차조합Service(
        I배차추천경로Service routeService,
        I음식멀티배차조합AIService aiService,
        I배차AI판단근거조회Service 판단근거조회Service)
    {
        _routeService = routeService;
        _aiService = aiService;
        _판단근거조회Service = 판단근거조회Service;
    }

    public IReadOnlyList<멀티배차조합후보> 조합생성(멀티배차조합요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var jobs = request.작업목록
            .Where(x => !string.IsNullOrWhiteSpace(x.의뢰Id))
            .GroupBy(x => x.의뢰Id, StringComparer.Ordinal)
            .Select(x => x.First())
            .ToArray();
        if (jobs.Length == 0)
        {
            return [];
        }

        var candidates = new List<멀티배차조합후보>();
        if (request.단건후보포함)
        {
            candidates.AddRange(jobs.Select(CreateSingleCandidate));
        }

        if (request.최대묶음크기 >= 2 && request.배차업무유형 == 상태값.배차업무유형.음식배달)
        {
            for (var i = 0; i < jobs.Length; i++)
            {
                for (var j = i + 1; j < jobs.Length; j++)
                {
                    candidates.Add(CreatePairCandidate(jobs[i], jobs[j], request));
                }
            }
        }

        var 판단근거 = _판단근거조회Service.조회(new 배차AI판단근거요청(
            "음식배달OS:멀티배차",
            jobs.Select(x => x.의뢰Id).ToArray(),
            [],
            jobs.SelectMany(x => new[] { x.픽업배달권키, x.하차배달권키 })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            묶음크기: Math.Min(request.최대묶음크기, jobs.Length),
            키워드: ["음식", "조리완료", "픽업", "고객전달", "배달완료시간", "멀티배차", "배달권"]));

        return _aiService
            .후보정렬(new 음식멀티배차조합AI요청(candidates, request, 판단근거))
            .Take(request.최대조합수 <= 0 ? candidates.Count : request.최대조합수)
            .ToArray();
    }

    private static 멀티배차조합후보 CreateSingleCandidate(픽업하차경로작업 job)
        => new(
            job.의뢰Id,
            단건배차,
            [job],
            [job.의뢰Id],
            null,
            null,
            null,
            50m,
            ["단건"],
            [],
            true,
            []);

    private 멀티배차조합후보 CreatePairCandidate(
        픽업하차경로작업 first,
        픽업하차경로작업 second,
        멀티배차조합요청 request)
    {
        var pickupDistanceKm = CalculateDistance(first.픽업좌표, second.픽업좌표);
        var dropoffDistanceKm = CalculateDistance(first.하차좌표, second.하차좌표);
        var bundleDistanceKm = CalculateBestPairRouteDistance(first, second);
        var warnings = new List<string>();
        var exclusionReasons = new List<string>();
        var badges = new List<string> { "2건묶음" };
        var score = 100m;

        if (!pickupDistanceKm.HasValue)
        {
            exclusionReasons.Add("상차지 좌표가 부족합니다.");
            score -= 30m;
        }
        else
        {
            score -= Math.Clamp(pickupDistanceKm.Value * 6m, 0m, 45m);
            if (pickupDistanceKm.Value <= request.같은상권픽업권장거리Km)
            {
                badges.Add("상차지근접");
                score += 12m;
            }

            if (pickupDistanceKm.Value > request.최대상차지간거리Km)
            {
                exclusionReasons.Add($"상차지 간 거리가 멀어 음식 멀티배차에 부적합합니다. 거리={Math.Round(pickupDistanceKm.Value, 2):0.##}km");
            }
        }

        if (!dropoffDistanceKm.HasValue)
        {
            exclusionReasons.Add("하차지 좌표가 부족합니다.");
            score -= 15m;
        }
        else
        {
            score -= Math.Clamp(dropoffDistanceKm.Value * 2m, 0m, 25m);
            if (dropoffDistanceKm.Value <= request.하차권역권장거리Km)
            {
                badges.Add("하차권역근접");
                score += 6m;
            }

            if (dropoffDistanceKm.Value > request.최대하차지간거리Km)
            {
                exclusionReasons.Add($"하차지 간 거리가 멀어 음식 품질 저하 위험이 큽니다. 거리={Math.Round(dropoffDistanceKm.Value, 2):0.##}km");
            }
        }

        if (bundleDistanceKm.HasValue)
        {
            score -= Math.Clamp(bundleDistanceKm.Value * 4m, 0m, 35m);
            if (request.좌표근사총거리상한Km > 0m && bundleDistanceKm.Value > request.좌표근사총거리상한Km)
            {
                exclusionReasons.Add($"멀티배차 묶음 내 예상 운행거리가 상한을 넘습니다. 예상={Math.Round(bundleDistanceKm.Value, 2):0.##}km, 상한={request.좌표근사총거리상한Km:0.##}km");
            }
        }

        ApplyDeliveryScopePolicy(first, second, request, warnings, exclusionReasons, badges, ref score);

        if (SameAddress(first.픽업주소, second.픽업주소))
        {
            badges.Add("동일상차지");
            score += 20m;
        }

        var pickupWindowGapMinutes = CalculateWindowGapMinutes(first.픽업시간창종료일시, second.픽업시간창종료일시);
        if (pickupWindowGapMinutes.HasValue)
        {
            score -= Math.Clamp(pickupWindowGapMinutes.Value / 5m, 0m, 20m);
            if (pickupWindowGapMinutes.Value <= request.픽업시간창권장차이분)
            {
                badges.Add("상차시간창근접");
                score += 8m;
            }
            else
            {
                exclusionReasons.Add($"상차 시간창 차이가 큽니다. 차이={Math.Round(pickupWindowGapMinutes.Value, 0):0}분");
            }
        }

        if (request.최대묶음크기 > 2)
        {
            warnings.Add("음식배달 멀티배차는 현재 2건 묶음까지만 후보로 생성합니다.");
        }

        if (exclusionReasons.Count > 0)
        {
            score = Math.Min(score, 0m);
        }

        var keyIds = new[] { first.의뢰Id, second.의뢰Id }
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        return new 멀티배차조합후보(
            string.Join("+", keyIds),
            멀티배차,
            [first, second],
            keyIds,
            pickupDistanceKm.HasValue ? Math.Round(pickupDistanceKm.Value, 2) : null,
            dropoffDistanceKm.HasValue ? Math.Round(dropoffDistanceKm.Value, 2) : null,
            bundleDistanceKm.HasValue ? Math.Round(bundleDistanceKm.Value, 2) : null,
            Math.Round(score, 2),
            badges.Distinct(StringComparer.Ordinal).ToArray(),
            warnings.Distinct(StringComparer.Ordinal).ToArray(),
            exclusionReasons.Count == 0,
            exclusionReasons.Distinct(StringComparer.Ordinal).ToArray());
    }

    private decimal? CalculateDistance(배차경로좌표? source, 배차경로좌표? target)
    {
        if (source is null || target is null)
        {
            return null;
        }

        return _routeService.CalculateDistanceKm(source, target);
    }

    private decimal? CalculateBestPairRouteDistance(픽업하차경로작업 first, 픽업하차경로작업 second)
    {
        var routeCandidates = new[]
        {
            new[] { first.픽업좌표, first.하차좌표, second.픽업좌표, second.하차좌표 },
            new[] { first.픽업좌표, second.픽업좌표, first.하차좌표, second.하차좌표 },
            new[] { first.픽업좌표, second.픽업좌표, second.하차좌표, first.하차좌표 },
            new[] { second.픽업좌표, second.하차좌표, first.픽업좌표, first.하차좌표 },
            new[] { second.픽업좌표, first.픽업좌표, first.하차좌표, second.하차좌표 },
            new[] { second.픽업좌표, first.픽업좌표, second.하차좌표, first.하차좌표 }
        };

        var distances = routeCandidates
            .Select(CalculateRouteDistance)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();
        return distances.Length == 0 ? null : distances.Min();
    }

    private decimal? CalculateRouteDistance(IReadOnlyList<배차경로좌표?> stops)
    {
        decimal total = 0m;
        for (var i = 1; i < stops.Count; i++)
        {
            if (stops[i - 1] is null || stops[i] is null)
            {
                return null;
            }

            var segment = _routeService.CalculateDistanceKm(stops[i - 1]!, stops[i]!);
            if (!segment.HasValue)
            {
                return null;
            }

            total += segment.Value;
        }

        return total;
    }

    private static void ApplyDeliveryScopePolicy(
        픽업하차경로작업 first,
        픽업하차경로작업 second,
        멀티배차조합요청 request,
        List<string> warnings,
        List<string> exclusionReasons,
        List<string> badges,
        ref decimal score)
    {
        var firstScope = NormalizeScope(first.하차배달권키);
        var secondScope = NormalizeScope(second.하차배달권키);
        if (firstScope is null || secondScope is null)
        {
            warnings.Add("하차 배달권 정보가 부족해 배달권 묶음 적합성을 약하게 평가했습니다.");
            score -= 6m;
            return;
        }

        if (string.Equals(firstScope, secondScope, StringComparison.Ordinal))
        {
            if (!request.같은배달권멀티허용)
            {
                exclusionReasons.Add("같은 배달권 멀티배차가 현재 정책에서 비활성화되어 있습니다.");
                return;
            }

            badges.Add("같은배달권");
            score += 14m;
            return;
        }

        if (국내행정구역배달권Catalog.인접배달권여부(firstScope, secondScope)
            || 국내행정구역배달권Catalog.인접배달권여부(secondScope, firstScope))
        {
            if (!request.인접배달권멀티허용)
            {
                exclusionReasons.Add($"인접 배달권 멀티배차가 현재 정책에서 비활성화되어 있습니다. 배달권={firstScope},{secondScope}");
                return;
            }

            badges.Add("인접배달권");
            score -= 8m;
            return;
        }

        if (!request.비인접배달권멀티허용)
        {
            exclusionReasons.Add($"비인접 배달권 주문은 음식 멀티배차에서 제외합니다. 배달권={firstScope},{secondScope}");
            return;
        }

        badges.Add("비인접배달권");
        warnings.Add("비인접 배달권 묶음은 음식 품질 저하 위험이 커서 낮은 점수로만 허용합니다.");
        score -= 28m;
    }

    private static string? NormalizeScope(string? value)
        => string.IsNullOrWhiteSpace(value) || string.Equals(value, "unknown", StringComparison.OrdinalIgnoreCase)
            ? null
            : value.Trim();

    private static bool SameAddress(string? left, string? right)
        => !string.IsNullOrWhiteSpace(left)
           && !string.IsNullOrWhiteSpace(right)
           && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);

    private static decimal? CalculateWindowGapMinutes(DateTime? left, DateTime? right)
        => left.HasValue && right.HasValue
            ? Math.Abs((decimal)(left.Value - right.Value).TotalMinutes)
            : null;
}

public sealed class 규칙기반음식멀티배차조합AIService : I음식멀티배차조합AIService
{
    private const string 멀티배차 = "멀티배차";

    public IReadOnlyList<멀티배차조합후보> 후보정렬(음식멀티배차조합AI요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var 판단근거요약 = 배차AI판단근거Formatter.요약(request.판단근거);
        return request.후보목록
            .Select(x => string.IsNullOrWhiteSpace(판단근거요약) ? x : AddEvidence(x, 판단근거요약))
            .OrderByDescending(x => x.조합가능여부)
            .ThenByDescending(x => x.배차묶음유형 == 멀티배차)
            .ThenByDescending(FoodDeliveryScore)
            .ThenBy(x => x.조합키, StringComparer.Ordinal)
            .ToArray();
    }

    private static decimal FoodDeliveryScore(멀티배차조합후보 candidate)
    {
        var score = candidate.조합점수;
        if (!candidate.조합가능여부)
        {
            score -= 1_000m;
        }

        if (candidate.배지.Contains("같은배달권", StringComparer.Ordinal))
        {
            score += 10m;
        }

        if (candidate.배지.Contains("인접배달권", StringComparer.Ordinal))
        {
            score += 3m;
        }

        if (candidate.배지.Contains("동일상차지", StringComparer.Ordinal))
        {
            score += 8m;
        }

        if (candidate.묶음내예상거리Km is > 0m and <= 6m)
        {
            score += 6m;
        }

        score -= candidate.경고.Count * 2m;
        score -= (candidate.제외사유?.Count ?? 0) * 20m;
        return score;
    }

    private static 멀티배차조합후보 AddEvidence(멀티배차조합후보 candidate, string 판단근거요약)
        => candidate with
        {
            배지 = candidate.배지
                .Concat(["판단근거반영"])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            경고 = candidate.경고
                .Concat([판단근거요약])
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
}

public sealed record 음식멀티배차조합AI요청(
    IReadOnlyList<멀티배차조합후보> 후보목록,
    멀티배차조합요청 원요청,
    배차AI판단근거? 판단근거 = null);

public sealed record 멀티배차조합요청(
    IReadOnlyList<픽업하차경로작업> 작업목록,
    int 배차업무유형 = 상태값.배차업무유형.음식배달,
    int 최대묶음크기 = 2,
    bool 단건후보포함 = true,
    int 최대조합수 = 50,
    decimal 같은상권픽업권장거리Km = 3m,
    decimal 하차권역권장거리Km = 5m,
    decimal 픽업시간창권장차이분 = 20m,
    decimal 최대상차지간거리Km = 3m,
    decimal 최대하차지간거리Km = 6m,
    decimal 좌표근사총거리상한Km = 6m,
    bool 같은배달권멀티허용 = true,
    bool 인접배달권멀티허용 = true,
    bool 비인접배달권멀티허용 = false);

public sealed record 멀티배차조합후보(
    string 조합키,
    string 배차묶음유형,
    IReadOnlyList<픽업하차경로작업> 작업목록,
    IReadOnlyList<string> 의뢰Ids,
    decimal? 상차지간거리Km,
    decimal? 하차지간거리Km,
    decimal? 묶음내예상거리Km,
    decimal 조합점수,
    IReadOnlyList<string> 배지,
    IReadOnlyList<string> 경고,
    bool 조합가능여부 = true,
    IReadOnlyList<string>? 제외사유 = null)
{
    public bool 기사할당전후보 => 조합가능여부;
}
