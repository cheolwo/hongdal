namespace 살뜰.Services.Dispatch.Coordination;

public interface I국내화물기사배정AIService
{
    IReadOnlyList<국내화물기사배정후보> 후보정렬(국내화물기사배정AI요청 request);

    long 비용보정(운송의뢰기사조합평가 candidate, 국내화물기사배정AI정책? policy);

    운송의뢰기사조합평가 표시보정(운송의뢰기사조합평가 candidate, 국내화물기사배정AI정책? policy);
}

public sealed class 규칙기반국내화물기사배정AIService : I국내화물기사배정AIService
{
    public IReadOnlyList<국내화물기사배정후보> 후보정렬(국내화물기사배정AI요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var 판단근거요약 = 배차AI판단근거Formatter.요약(request.판단근거);
        return request.후보목록
            .Select(x => x with
            {
                기사목표지급액보정비용 = 묶음비용보정(x, request.정책),
                판단근거요약 = 판단근거요약
            })
            .OrderBy(x => x.기본비용 + x.기사목표지급액보정비용)
            .ThenByDescending(x => x.기본점수)
            .ThenBy(x => x.기사Id, StringComparer.Ordinal)
            .ToArray();
    }

    public long 비용보정(운송의뢰기사조합평가 candidate, 국내화물기사배정AI정책? policy)
    {
        var target = ResolveTarget(policy);
        if (target <= 0m)
        {
            return 0;
        }

        var payout = ResolveDriverPayout(candidate);
        if (!payout.HasValue)
        {
            return 0;
        }

        return ToCostAdjustment(payout.Value, target, policy);
    }

    public 운송의뢰기사조합평가 표시보정(운송의뢰기사조합평가 candidate, 국내화물기사배정AI정책? policy)
    {
        var target = ResolveTarget(policy);
        if (target <= 0m)
        {
            return candidate;
        }

        var payout = ResolveDriverPayout(candidate);
        if (!payout.HasValue)
        {
            return candidate;
        }

        var adjustment = 비용보정(candidate, policy);
        var badges = candidate.배지
            .Concat(["기사목표단가회귀"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var reason = string.IsNullOrWhiteSpace(candidate.추천사유)
            ? $"기사 목표 건당 지급액 {target:0}원 기준으로 후보를 보정했습니다."
            : $"{candidate.추천사유} 기사 목표 건당 지급액 {target:0}원 기준으로 후보를 보정했습니다.";

        return candidate with
        {
            추천점수 = candidate.추천점수 + Math.Round(Math.Max(0m, target - Math.Abs(payout.Value - target)) / 100m, 2),
            추천사유 = reason,
            배지 = badges,
            경고 = adjustment > 0 && payout.Value < target
                ? candidate.경고.Concat([$"기사 예상 건당 지급액이 목표보다 낮습니다. 예상={payout.Value:0}, 목표={target:0}"]).Distinct(StringComparer.Ordinal).ToArray()
                : candidate.경고
        };
    }

    private static long 묶음비용보정(국내화물기사배정후보 candidate, 국내화물기사배정AI정책? policy)
    {
        var target = ResolveTarget(policy);
        if (target <= 0m || candidate.후보목록.Count == 0)
        {
            return 0;
        }

        var payouts = candidate.후보목록
            .Select(ResolveDriverPayout)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();
        if (payouts.Length == 0)
        {
            return 0;
        }

        var average = payouts.Sum() / payouts.Length;
        return ToCostAdjustment(average, target, policy) * candidate.후보목록.Count;
    }

    private static decimal ResolveTarget(국내화물기사배정AI정책? policy)
        => policy?.목표기사건당지급액 ?? 0m;

    private static decimal? ResolveDriverPayout(운송의뢰기사조합평가 candidate)
    {
        if (!candidate.예상총비용.HasValue)
        {
            return null;
        }

        return Math.Max(0m, candidate.예상총비용.Value - (candidate.예상톨비 ?? 0m));
    }

    private static long ToCostAdjustment(decimal averagePayout, decimal target, 국내화물기사배정AI정책? policy)
    {
        var gap = averagePayout - target;
        var multiplier = gap < 0m
            ? policy?.목표미달패널티배수 ?? 3m
            : policy?.목표초과패널티배수 ?? 0.2m;
        return (long)Math.Round(Math.Abs(gap) * multiplier, 0, MidpointRounding.AwayFromZero);
    }
}

public sealed record 국내화물기사배정AI정책(
    decimal 목표기사건당지급액 = 0m,
    decimal 목표미달패널티배수 = 3m,
    decimal 목표초과패널티배수 = 0.2m);

public sealed record 국내화물기사배정AI요청(
    IReadOnlyList<국내화물기사배정후보> 후보목록,
    국내화물기사배정AI정책? 정책,
    배차AI판단근거? 판단근거 = null);

public sealed record 국내화물기사배정후보(
    string 기사Id,
    IReadOnlyList<운송의뢰기사조합평가> 후보목록,
    long 기본비용,
    decimal 기본점수,
    long 기사목표지급액보정비용 = 0,
    string 판단근거요약 = "");
