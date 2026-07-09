namespace 홍달.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율Service
{
    private static 배차조율전략 SelectStrategy(
        int requestCount,
        IReadOnlyDictionary<string, int> baseDriverCapacityMap)
    {
        if (requestCount <= 0)
        {
            return new 배차조율전략("대기의뢰없음", 배차수급상태.균형, baseDriverCapacityMap, 0m);
        }

        var availableDriverCount = baseDriverCapacityMap.Count(x => x.Value > 0);
        var ratio = Math.Round(availableDriverCount / (decimal)requestCount, 2);
        if (ratio < 0.8m)
        {
            return new 배차조율전략(
                "의뢰많음_경로삽입효율우선",
                배차수급상태.의뢰많음,
                baseDriverCapacityMap,
                ratio);
        }

        if (ratio > 1.2m)
        {
            var spreadCapacity = baseDriverCapacityMap
                .ToDictionary(
                    x => x.Key,
                    x => Math.Min(1, Math.Max(0, x.Value)),
                    StringComparer.Ordinal);
            return new 배차조율전략(
                "기사여유_근거리단건우선",
                배차수급상태.기사여유,
                spreadCapacity,
                ratio);
        }

        return new 배차조율전략(
            "균형_연속경로우선",
            배차수급상태.균형,
            baseDriverCapacityMap,
            ratio);
    }

    private static long ToOptimizationCost(운송의뢰기사조합평가 candidate, 배차수급상태 supplyState)
        => supplyState switch
        {
            배차수급상태.의뢰많음 => ToManyRequestsCost(candidate),
            배차수급상태.기사여유 => ToSurplusCost(candidate),
            _ => ToBalancedCost(candidate)
        };

    private static long ToBalancedCost(운송의뢰기사조합평가 candidate)
    {
        var directCost = candidate.예상총비용;
        var distanceCost = candidate.총예상거리Km.HasValue ? candidate.총예상거리Km.Value * 900m : (decimal?)null;
        var timeCost = candidate.총예상시간분.HasValue ? candidate.총예상시간분.Value * 250m : (decimal?)null;
        var baseCost = directCost ?? distanceCost ?? timeCost ?? 1_000_000m;
        var schedulePenalty = candidate.전체일정완수가능여부 ? 0m : 500_000m;
        var insertionPenalty = candidate.일정삽입가능여부 ? 0m : 200_000m;
        var delayCost = candidate.총추가지연분.HasValue ? candidate.총추가지연분.Value * 1000m : 0m;
        var scopePenalty = 배달권비용보정(candidate, 동일권보정: -30_000m, 인접권보정: 10_000m, 외부권보정: 80_000m);
        var returnPenalty = 복귀부담비용보정(candidate, 5000m);

        var profitBenefit = candidate.예상순이익.HasValue
            ? Math.Clamp(candidate.예상순이익.Value * 0.1m, -10_000m, 30_000m)
            : 0m;
        var scoreBenefit = Math.Clamp(candidate.추천점수 * 50m, -5_000m, 10_000m);
        var optimizationCost = Math.Max(0m, baseCost + (timeCost ?? 0m) + schedulePenalty + insertionPenalty + delayCost + scopePenalty + returnPenalty - profitBenefit - scoreBenefit);

        return (long)Math.Round(optimizationCost, 0, MidpointRounding.AwayFromZero);
    }

    private static long ToManyRequestsCost(운송의뢰기사조합평가 candidate)
    {
        var baseCost = candidate.예상총비용
            ?? (candidate.총예상거리Km.HasValue ? candidate.총예상거리Km.Value * 700m : (decimal?)null)
            ?? 1_000_000m;
        var timeCost = candidate.총예상시간분.HasValue ? candidate.총예상시간분.Value * 250m : 0m;
        var delayCost = candidate.총추가지연분.HasValue ? candidate.총추가지연분.Value * 1200m : 50_000m;
        var routeChangeBenefit = candidate.경로변경이점여부
            ? Math.Clamp((candidate.경로변경절감분 ?? 0m) * 2500m, 0m, 80_000m)
            : 0m;
        var schedulePenalty = candidate.전체일정완수가능여부 && candidate.일정삽입가능여부 ? 0m : 600_000m;
        var scopePenalty = 배달권비용보정(candidate, 동일권보정: -50_000m, 인접권보정: 20_000m, 외부권보정: 120_000m);
        var returnPenalty = 복귀부담비용보정(candidate, 6500m);
        var profitBenefit = candidate.예상순이익.HasValue
            ? Math.Clamp(candidate.예상순이익.Value * 0.05m, -5_000m, 20_000m)
            : 0m;
        var scoreBenefit = Math.Clamp(candidate.추천점수 * 80m, -10_000m, 20_000m);
        var optimizationCost = Math.Max(0m, baseCost * 0.5m + timeCost + delayCost + schedulePenalty + scopePenalty + returnPenalty - routeChangeBenefit - profitBenefit - scoreBenefit);

        return (long)Math.Round(optimizationCost, 0, MidpointRounding.AwayFromZero);
    }

    private static long ToSurplusCost(운송의뢰기사조합평가 candidate)
    {
        var pickupDistanceCost = candidate.상차지거리Km.HasValue ? candidate.상차지거리Km.Value * 1200m : 500_000m;
        var pickupTimeCost = candidate.상차지이동시간분.HasValue ? candidate.상차지이동시간분.Value * 400m : 100_000m;
        var baseCost = candidate.예상총비용.HasValue ? candidate.예상총비용.Value * 0.25m : 0m;
        var scopePenalty = 배달권비용보정(candidate, 동일권보정: -20_000m, 인접권보정: 15_000m, 외부권보정: 100_000m);
        var returnPenalty = 복귀부담비용보정(candidate, 4500m);
        var scoreBenefit = Math.Clamp(candidate.추천점수 * 80m, -5_000m, 12_000m);
        var optimizationCost = Math.Max(0m, pickupDistanceCost + pickupTimeCost + baseCost + scopePenalty + returnPenalty - scoreBenefit);

        return (long)Math.Round(optimizationCost, 0, MidpointRounding.AwayFromZero);
    }

    private static decimal 배달권비용보정(
        운송의뢰기사조합평가 candidate,
        decimal 동일권보정,
        decimal 인접권보정,
        decimal 외부권보정)
    {
        if (candidate.동일배달권여부)
        {
            return 동일권보정;
        }

        return candidate.인접배달권여부 ? 인접권보정 : 외부권보정;
    }

    private static decimal 복귀부담비용보정(운송의뢰기사조합평가 candidate, decimal 점수당비용)
        => Math.Max(0m, candidate.복귀시간대부담점수) * 점수당비용;

    private enum 배차수급상태
    {
        의뢰많음,
        균형,
        기사여유
    }

    private sealed record 배차조율전략(
        string 이름,
        배차수급상태 수급상태,
        IReadOnlyDictionary<string, int> 기사용량,
        decimal 가용기사운송의뢰비율);
}
