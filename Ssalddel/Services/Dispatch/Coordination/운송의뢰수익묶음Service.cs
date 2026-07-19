using 살뜰.Services.Dispatch.Recommendation;

namespace 살뜰.Services.Dispatch.Coordination;

public interface I운송의뢰수익묶음Service
{
    IReadOnlyList<운송의뢰수익묶음후보> 묶음생성(운송의뢰수익묶음요청 request);
}

public interface I운송의뢰수익묶음AIService
{
    IReadOnlyList<운송의뢰수익묶음후보> 후보선택(운송의뢰수익묶음AI요청 request);
}

public sealed class 운송의뢰수익묶음Service : I운송의뢰수익묶음Service
{
    private readonly I운송의뢰수익묶음AIService _aiService;
    private readonly I배차AI판단근거조회Service _판단근거조회Service;

    public 운송의뢰수익묶음Service()
        : this(new 규칙기반운송의뢰수익묶음AIService(), new 규칙기반배차AI판단근거조회Service())
    {
    }

    public 운송의뢰수익묶음Service(I운송의뢰수익묶음AIService aiService)
        : this(aiService, new 규칙기반배차AI판단근거조회Service())
    {
    }

    public 운송의뢰수익묶음Service(
        I운송의뢰수익묶음AIService aiService,
        I배차AI판단근거조회Service 판단근거조회Service)
    {
        _aiService = aiService;
        _판단근거조회Service = 판단근거조회Service;
    }

    public IReadOnlyList<운송의뢰수익묶음후보> 묶음생성(운송의뢰수익묶음요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var jobs = request.운송의뢰목록
            .Where(x => !string.IsNullOrWhiteSpace(x.의뢰Id))
            .GroupBy(x => x.의뢰Id, StringComparer.Ordinal)
            .Select(x => x.First())
            .ToArray();
        if (jobs.Length == 0)
        {
            return [];
        }

        var candidates = new List<운송의뢰수익묶음후보>();
        if (request.단건후보포함)
        {
            candidates.AddRange(jobs.Select(x => CreateSingleCandidate(x, request)));
        }

        var maxBundleSize = Math.Clamp(request.최대묶음크기, 1, Math.Min(jobs.Length, request.최대조합탐색크기));
        for (var size = 2; size <= maxBundleSize; size++)
        {
            foreach (var bundleJobs in GenerateCombinations(jobs, size))
            {
                candidates.Add(CreateBundleCandidate(bundleJobs, request));
            }
        }

        var 판단근거 = _판단근거조회Service.조회(new 배차AI판단근거요청(
            "국내화물운송OS:플랫폼수익묶음",
            jobs.Select(x => x.의뢰Id).ToArray(),
            [],
            jobs.Select(x => x.배달권키)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            maxBundleSize,
            request.목표건당플랫폼순이익,
            null,
            ["플랫폼", "수익", "묶음", "배달권"]));

        return _aiService.후보선택(new 운송의뢰수익묶음AI요청(candidates, request, 판단근거));
    }

    private static 운송의뢰수익묶음후보 CreateSingleCandidate(
        운송의뢰수익묶음대상 job,
        운송의뢰수익묶음요청 request)
    {
        var revenue = NormalizeMoney(job.예상운임);
        var cost = EstimateSingleCost(job, request);
        var profit = CalculateProfit(revenue, cost);
        var perJobProfit = CalculatePerJobProfit(profit, 1);
        var targetScore = CalculateTargetProfitScore(perJobProfit, request);
        var score = CalculateBaseScore(profit, revenue, cost) + targetScore;

        return new 운송의뢰수익묶음후보(
            job.의뢰Id,
            "단건",
            [job.의뢰Id],
            revenue,
            cost,
            profit,
            Math.Round(score, 2),
            1,
            [],
            ["단건"],
            [],
            true,
            [],
            perJobProfit,
            targetScore,
            "단건 후보");
    }

    private static 운송의뢰수익묶음후보 CreateBundleCandidate(
        IReadOnlyList<운송의뢰수익묶음대상> jobs,
        운송의뢰수익묶음요청 request)
    {
        var keyIds = jobs
            .Select(x => x.의뢰Id)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        var badges = new List<string> { $"{jobs.Count}건묶음" };
        var warnings = new List<string>();
        var blockers = new List<string>();

        if (jobs.Any(x => !x.멀티배차허용))
        {
            blockers.Add("멀티배차를 허용하지 않은 운송 의뢰가 포함되어 있습니다.");
        }

        ApplyScopePolicy(jobs, request, badges, blockers, out var scopeScore);

        var revenue = SumMoney(jobs.Select(x => x.예상운임).ToArray());
        var cost = EstimateBundleCost(jobs, request);
        var profit = CalculateProfit(revenue, cost);
        var perJobProfit = CalculatePerJobProfit(profit, jobs.Count);
        var targetScore = CalculateTargetProfitScore(perJobProfit, request);
        var score = CalculateBaseScore(profit, revenue, cost)
                    + targetScore
                    + scopeScore
                    + request.멀티묶음기본보너스
                    + Math.Max(0, jobs.Count - 2) * request.추가묶음건당보너스;

        ApplyDistanceScore(
            jobs,
            x => x.상차좌표,
            request.상차지근접권장Km,
            request.상차지근접보너스,
            request.상차지분산패널티Km당,
            "상차지근접",
            "상차지 좌표가 부족해 묶음 동선을 약하게 평가했습니다.",
            badges,
            warnings,
            ref score);
        ApplyDistanceScore(
            jobs,
            x => x.하차좌표,
            request.하차지근접권장Km,
            request.하차지근접보너스,
            request.하차지분산패널티Km당,
            "하차지근접",
            "하차지 좌표가 부족해 묶음 동선을 약하게 평가했습니다.",
            badges,
            warnings,
            ref score);
        ApplyPickupWindowScore(jobs, request, badges, ref score);

        if (profit.HasValue && profit.Value < request.묶음최소예상순이익)
        {
            blockers.Add($"묶음 예상 순이익이 최소 기준보다 낮습니다. 예상={profit.Value:0}, 최소={request.묶음최소예상순이익:0}");
        }

        if (request.목표건당플랫폼순이익미달차단
            && perJobProfit.HasValue
            && perJobProfit.Value < request.목표건당플랫폼순이익)
        {
            blockers.Add($"건당 예상 순이익이 목표 기준보다 낮습니다. 예상={perJobProfit.Value:0}, 목표={request.목표건당플랫폼순이익:0}");
        }

        if (cost is null)
        {
            warnings.Add("묶음 원가를 계산할 거리 또는 원가 정보가 부족합니다.");
        }

        return new 운송의뢰수익묶음후보(
            string.Join("+", keyIds),
            "수익우선묶음",
            keyIds,
            revenue,
            cost,
            profit,
            Math.Round(score, 2),
            jobs.Count,
            keyIds,
            badges.Distinct(StringComparer.Ordinal).ToArray(),
            warnings.Distinct(StringComparer.Ordinal).ToArray(),
            blockers.Count == 0,
            blockers.Distinct(StringComparer.Ordinal).ToArray(),
            perJobProfit,
            targetScore,
            BuildSelectionReason(jobs.Count, perJobProfit, targetScore, badges));
    }

    private static void ApplyScopePolicy(
        IReadOnlyList<운송의뢰수익묶음대상> jobs,
        운송의뢰수익묶음요청 request,
        List<string> badges,
        List<string> blockers,
        out decimal scopeScore)
    {
        scopeScore = 0m;
        var scopes = jobs
            .Select(x => x.배달권키)
            .Where(x => !string.IsNullOrWhiteSpace(x) && !string.Equals(x, "unknown", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (scopes.Length == 0)
        {
            scopeScore -= request.외부배달권패널티;
            return;
        }

        if (scopes.Length == 1)
        {
            scopeScore += request.같은배달권보너스;
            badges.Add("같은배달권");
            return;
        }

        var adjacentOrSame = AllScopesSameOrAdjacent(scopes);
        if (adjacentOrSame)
        {
            if (request.배달권묶음정책 == 운송의뢰묶음배달권정책.같은배달권만)
            {
                blockers.Add("같은 배달권만 허용하는 정책에서 인접 배달권 묶음이 생성되었습니다.");
                return;
            }

            scopeScore += request.인접배달권보너스;
            badges.Add("인접배달권");
            return;
        }

        if (request.배달권묶음정책 == 운송의뢰묶음배달권정책.제한없음)
        {
            scopeScore -= request.외부배달권패널티;
            badges.Add("외부배달권포함");
            return;
        }

        blockers.Add("같은 배달권 또는 인접 배달권이 아닌 운송 의뢰가 포함되어 있습니다.");
    }

    private static bool AllScopesSameOrAdjacent(IReadOnlyList<string> scopes)
    {
        for (var i = 0; i < scopes.Count; i++)
        {
            for (var j = i + 1; j < scopes.Count; j++)
            {
                if (string.Equals(scopes[i], scopes[j], StringComparison.Ordinal))
                {
                    continue;
                }

                if (!국내행정구역배달권Catalog.인접배달권여부(scopes[i], scopes[j])
                    && !국내행정구역배달권Catalog.인접배달권여부(scopes[j], scopes[i]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void ApplyDistanceScore(
        IReadOnlyList<운송의뢰수익묶음대상> jobs,
        Func<운송의뢰수익묶음대상, 배차경로좌표?> selector,
        decimal nearThresholdKm,
        decimal nearBonus,
        decimal spreadPenaltyPerKm,
        string nearBadge,
        string missingWarning,
        List<string> badges,
        List<string> warnings,
        ref decimal score)
    {
        var maxDistance = CalculateMaxPairDistance(jobs.Select(selector).ToArray());
        if (maxDistance.HasValue)
        {
            score -= maxDistance.Value * spreadPenaltyPerKm;
            if (maxDistance.Value <= nearThresholdKm)
            {
                score += nearBonus;
                badges.Add(nearBadge);
            }

            return;
        }

        warnings.Add(missingWarning);
        score -= spreadPenaltyPerKm;
    }

    private static void ApplyPickupWindowScore(
        IReadOnlyList<운송의뢰수익묶음대상> jobs,
        운송의뢰수익묶음요청 request,
        List<string> badges,
        ref decimal score)
    {
        var windows = jobs
            .Select(x => x.상차시간창종료Utc)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();
        if (windows.Length < 2)
        {
            return;
        }

        var gap = Math.Abs((decimal)(windows.Max() - windows.Min()).TotalMinutes);
        score -= gap * request.상차시간창차이패널티분당;
        if (gap <= request.상차시간창권장차이분)
        {
            score += request.상차시간창근접보너스;
            badges.Add("상차시간창근접");
        }
    }

    private static decimal CalculateBaseScore(decimal? profit, decimal? revenue, decimal? cost)
    {
        var score = profit ?? 0m;
        if (revenue.HasValue && revenue.Value > 0m && cost.HasValue)
        {
            var marginRate = (revenue.Value - cost.Value) / revenue.Value;
            score += Math.Clamp(marginRate * 50_000m, -20_000m, 30_000m);
        }

        return score;
    }

    private static decimal CalculateTargetProfitScore(decimal? perJobProfit, 운송의뢰수익묶음요청 request)
    {
        if (!perJobProfit.HasValue || request.목표건당플랫폼순이익 <= 0m)
        {
            return 0m;
        }

        var gap = perJobProfit.Value - request.목표건당플랫폼순이익;
        if (gap < 0m)
        {
            return gap * request.목표수익미달패널티배수;
        }

        var closeBand = Math.Max(1m, request.목표건당플랫폼순이익);
        var closeness = Math.Max(0m, closeBand - Math.Min(closeBand, Math.Abs(gap)));
        var convergenceBonus = closeness * request.목표수익회귀보너스배수;
        var excessBonus = Math.Min(gap * request.목표수익초과보너스배수, request.목표수익초과보너스상한);
        return convergenceBonus + excessBonus;
    }

    private static decimal? EstimateSingleCost(
        운송의뢰수익묶음대상 job,
        운송의뢰수익묶음요청 request)
    {
        if (job.예상직접원가.HasValue)
        {
            return NormalizeMoney(job.예상직접원가);
        }

        var distance = CalculateDistance(job.상차좌표, job.하차좌표);
        return distance.HasValue
            ? NormalizeMoney(distance.Value * request.거리원가기준Km당)
            : null;
    }

    private static decimal? EstimateBundleCost(
        IReadOnlyList<운송의뢰수익묶음대상> jobs,
        운송의뢰수익묶음요청 request)
    {
        if (jobs.All(x => x.예상직접원가.HasValue))
        {
            var rawCost = SumMoney(jobs.Select(x => x.예상직접원가).ToArray());
            return NormalizeMoney(rawCost * ResolveBundleCostAdjustmentRatio(jobs.Count, request));
        }

        var distance = EstimateBestBundleRouteDistance(jobs);
        return distance.HasValue
            ? NormalizeMoney(distance.Value * request.거리원가기준Km당)
            : SumMoney(jobs.Select(x => EstimateSingleCost(x, request)).ToArray());
    }

    private static decimal ResolveBundleCostAdjustmentRatio(int bundleSize, 운송의뢰수익묶음요청 request)
    {
        if (bundleSize <= 1)
        {
            return 1m;
        }

        return Math.Max(
            request.멀티묶음최소원가보정비율,
            request.멀티묶음원가보정비율 - Math.Max(0, bundleSize - 2) * request.묶음추가건당원가보정감소폭);
    }

    private static decimal? EstimateBestBundleRouteDistance(IReadOnlyList<운송의뢰수익묶음대상> jobs)
    {
        var stops = jobs
            .SelectMany((job, index) => new[]
            {
                new 묶음경유지(index, true, job.상차좌표),
                new 묶음경유지(index, false, job.하차좌표)
            })
            .ToArray();
        if (stops.Any(x => x.좌표 is null))
        {
            return null;
        }

        var bestDistance = (decimal?)null;
        GenerateValidRouteSequences(stops, [], [], sequence =>
        {
            var distance = CalculateRouteDistance(sequence.Select(x => x.좌표).ToArray());
            if (!distance.HasValue)
            {
                return;
            }

            bestDistance = bestDistance.HasValue ? Math.Min(bestDistance.Value, distance.Value) : distance.Value;
        });
        return bestDistance;
    }

    private static void GenerateValidRouteSequences(
        IReadOnlyList<묶음경유지> stops,
        IReadOnlyList<묶음경유지> selected,
        IReadOnlyList<int> pickedUpJobIndexes,
        Action<IReadOnlyList<묶음경유지>> onSequence)
    {
        if (selected.Count == stops.Count)
        {
            onSequence(selected);
            return;
        }

        foreach (var stop in stops)
        {
            if (selected.Contains(stop))
            {
                continue;
            }

            if (!stop.상차여부 && !pickedUpJobIndexes.Contains(stop.의뢰순번))
            {
                continue;
            }

            var nextSelected = selected.Concat([stop]).ToArray();
            var nextPickedUp = stop.상차여부
                ? pickedUpJobIndexes.Concat([stop.의뢰순번]).Distinct().ToArray()
                : pickedUpJobIndexes;
            GenerateValidRouteSequences(stops, nextSelected, nextPickedUp, onSequence);
        }
    }

    private static decimal? CalculateRouteDistance(IReadOnlyList<배차경로좌표?> stops)
    {
        decimal total = 0m;
        for (var i = 1; i < stops.Count; i++)
        {
            var segment = CalculateDistance(stops[i - 1], stops[i]);
            if (!segment.HasValue)
            {
                return null;
            }

            total += segment.Value;
        }

        return total;
    }

    private static decimal? CalculateMaxPairDistance(IReadOnlyList<배차경로좌표?> points)
    {
        if (points.Count < 2 || points.Any(x => x is null))
        {
            return null;
        }

        decimal max = 0m;
        for (var i = 0; i < points.Count; i++)
        {
            for (var j = i + 1; j < points.Count; j++)
            {
                var distance = CalculateDistance(points[i], points[j]);
                if (!distance.HasValue)
                {
                    return null;
                }

                max = Math.Max(max, distance.Value);
            }
        }

        return max;
    }

    private static decimal? CalculateDistance(배차경로좌표? source, 배차경로좌표? target)
    {
        if (source is null || target is null)
        {
            return null;
        }

        const double earthRadiusKm = 6371.0088;
        var dLat = ToRadians((double)(target.Latitude - source.Latitude));
        var dLng = ToRadians((double)(target.Longitude - source.Longitude));
        var lat1 = ToRadians((double)source.Latitude);
        var lat2 = ToRadians((double)target.Latitude);
        var a = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d)
            + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLng / 2d) * Math.Sin(dLng / 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return (decimal)(earthRadiusKm * c);
    }

    private static double ToRadians(double degrees)
        => degrees * Math.PI / 180d;

    private static decimal? CalculateProfit(decimal? revenue, decimal? cost)
        => revenue.HasValue && cost.HasValue ? revenue.Value - cost.Value : null;

    private static decimal? CalculatePerJobProfit(decimal? profit, int bundleSize)
        => profit.HasValue && bundleSize > 0
            ? decimal.Round(profit.Value / bundleSize, 2, MidpointRounding.AwayFromZero)
            : null;

    private static decimal? SumMoney(params decimal?[] values)
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

        return hasValue ? NormalizeMoney(sum) : null;
    }

    private static decimal? NormalizeMoney(decimal? value)
        => value.HasValue
            ? decimal.Round(Math.Max(0m, value.Value), 0, MidpointRounding.AwayFromZero)
            : null;

    private static IReadOnlyList<IReadOnlyList<T>> GenerateCombinations<T>(IReadOnlyList<T> source, int size)
    {
        var results = new List<IReadOnlyList<T>>();
        Build(0, []);
        return results;

        void Build(int startIndex, IReadOnlyList<T> selected)
        {
            if (selected.Count == size)
            {
                results.Add(selected);
                return;
            }

            for (var i = startIndex; i < source.Count; i++)
            {
                Build(i + 1, selected.Concat([source[i]]).ToArray());
            }
        }
    }

    private static string BuildSelectionReason(
        int bundleSize,
        decimal? perJobProfit,
        decimal targetScore,
        IReadOnlyList<string> badges)
        => $"플랫폼 수익 묶음 후보: {bundleSize}건, 건당예상순이익={perJobProfit?.ToString("0") ?? "미정"}, 목표회귀점수={targetScore:0}, 조건={string.Join(",", badges)}";

    private sealed record 묶음경유지(int 의뢰순번, bool 상차여부, 배차경로좌표? 좌표);
}

public sealed class 규칙기반운송의뢰수익묶음AIService : I운송의뢰수익묶음AIService
{
    public IReadOnlyList<운송의뢰수익묶음후보> 후보선택(운송의뢰수익묶음AI요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var max = request.원요청.최대묶음수 <= 0 ? request.후보목록.Count : request.원요청.최대묶음수;
        var 판단근거요약 = 배차AI판단근거Formatter.요약(request.판단근거);
        return request.후보목록
            .Select(x => string.IsNullOrWhiteSpace(판단근거요약)
                ? x
                : x with { 선택근거 = 배차AI판단근거Formatter.사유추가(x.선택근거, 판단근거요약) })
            .OrderByDescending(x => x.묶음가능여부)
            .ThenByDescending(x => x.목표수익회귀점수)
            .ThenByDescending(x => x.예상플랫폼순이익 ?? decimal.MinValue)
            .ThenByDescending(x => x.우선순위점수)
            .ThenBy(x => x.묶음크기)
            .ThenBy(x => x.묶음키, StringComparer.Ordinal)
            .Take(max)
            .ToArray();
    }
}

public enum 운송의뢰묶음배달권정책
{
    같은배달권만,
    같은배달권또는인접,
    제한없음
}

public sealed record 운송의뢰수익묶음AI요청(
    IReadOnlyList<운송의뢰수익묶음후보> 후보목록,
    운송의뢰수익묶음요청 원요청,
    배차AI판단근거? 판단근거 = null);

public sealed record 운송의뢰수익묶음요청(
    IReadOnlyList<운송의뢰수익묶음대상> 운송의뢰목록,
    int 최대묶음크기 = 3,
    bool 단건후보포함 = true,
    int 최대묶음수 = 50,
    int 최대조합탐색크기 = 4,
    decimal 거리원가기준Km당 = 900m,
    decimal 묶음최소예상순이익 = 0m,
    decimal 목표건당플랫폼순이익 = 500m,
    bool 목표건당플랫폼순이익미달차단 = true,
    decimal 목표수익미달패널티배수 = 3m,
    decimal 목표수익회귀보너스배수 = 10m,
    decimal 목표수익초과보너스배수 = 0.1m,
    decimal 목표수익초과보너스상한 = 20_000m,
    decimal 멀티묶음기본보너스 = 15_000m,
    decimal 추가묶음건당보너스 = 3_000m,
    decimal 멀티묶음원가보정비율 = 0.9m,
    decimal 묶음추가건당원가보정감소폭 = 0.05m,
    decimal 멀티묶음최소원가보정비율 = 0.75m,
    운송의뢰묶음배달권정책 배달권묶음정책 = 운송의뢰묶음배달권정책.같은배달권또는인접,
    decimal 같은배달권보너스 = 20_000m,
    decimal 인접배달권보너스 = 8_000m,
    decimal 외부배달권패널티 = 50_000m,
    decimal 상차지근접권장Km = 5m,
    decimal 상차지근접보너스 = 10_000m,
    decimal 상차지분산패널티Km당 = 2_500m,
    decimal 하차지근접권장Km = 8m,
    decimal 하차지근접보너스 = 8_000m,
    decimal 하차지분산패널티Km당 = 1_500m,
    decimal 상차시간창권장차이분 = 60m,
    decimal 상차시간창근접보너스 = 5_000m,
    decimal 상차시간창차이패널티분당 = 50m);

public sealed record 운송의뢰수익묶음대상(
    string 의뢰Id,
    string 배달권키,
    decimal? 예상운임,
    decimal? 예상직접원가,
    배차경로좌표? 상차좌표,
    배차경로좌표? 하차좌표,
    DateTime? 상차시간창종료Utc,
    bool 멀티배차허용 = true);

public sealed record 운송의뢰수익묶음후보(
    string 묶음키,
    string 묶음유형,
    IReadOnlyList<string> 의뢰Ids,
    decimal? 예상운임합계,
    decimal? 예상원가합계,
    decimal? 예상플랫폼순이익,
    decimal 우선순위점수,
    int 묶음크기,
    IReadOnlyList<string> 묶음의뢰Ids,
    IReadOnlyList<string> 배지,
    IReadOnlyList<string> 경고,
    bool 묶음가능여부,
    IReadOnlyList<string> 제외사유,
    decimal? 예상건당플랫폼순이익 = null,
    decimal 목표수익회귀점수 = 0m,
    string 선택근거 = "");
