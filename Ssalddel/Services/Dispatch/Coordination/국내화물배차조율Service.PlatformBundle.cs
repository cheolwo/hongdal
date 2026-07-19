namespace 살뜰.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율Service
{
    private static IReadOnlyDictionary<string, decimal> BuildRevenueBundleCostBenefitMap(
        IReadOnlyList<운송의뢰수익묶음후보>? bundleCandidates)
    {
        if (bundleCandidates is null || bundleCandidates.Count == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.Ordinal);
        }

        var map = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var bundle in bundleCandidates
                     .Where(x => x.묶음가능여부)
                     .Where(x => x.묶음크기 > 1)
                     .OrderByDescending(x => x.예상플랫폼순이익 ?? decimal.MinValue)
                     .ThenByDescending(x => x.우선순위점수))
        {
            var benefit = Math.Clamp(
                (bundle.예상플랫폼순이익 ?? 0m) * 0.15m + bundle.우선순위점수 * 0.05m,
                0m,
                80_000m);
            if (benefit <= 0m)
            {
                continue;
            }

            foreach (var requestId in bundle.의뢰Ids)
            {
                if (map.TryGetValue(requestId, out var existing) && existing >= benefit)
                {
                    continue;
                }

                map[requestId] = benefit;
            }
        }

        return map;
    }

    private static 운송의뢰기사조합평가 ApplyRevenueBundlePriority(
        운송의뢰기사조합평가 candidate,
        IReadOnlyDictionary<string, decimal> revenueBundleCostBenefitMap)
    {
        if (!revenueBundleCostBenefitMap.TryGetValue(candidate.의뢰Id, out var benefit) || benefit <= 0m)
        {
            return candidate;
        }

        var badges = candidate.배지
            .Concat(["수익묶음우선"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var reason = string.IsNullOrWhiteSpace(candidate.추천사유)
            ? "수익 묶음 후보 우선순위가 반영되었습니다."
            : $"{candidate.추천사유} 수익 묶음 후보 우선순위가 반영되었습니다.";

        return candidate with
        {
            추천점수 = candidate.추천점수 + Math.Round(benefit / 1000m, 2),
            추천사유 = reason,
            배지 = badges
        };
    }

    private static 플랫폼수익의뢰선별결과 SelectPlatformFirstRequests(
        IReadOnlyList<string> rawRequestIds,
        IReadOnlyList<운송의뢰수익묶음후보>? bundleCandidates,
        int availableAssignmentCapacity,
        IReadOnlySet<string> requestIdsWithCandidate)
    {
        if (availableAssignmentCapacity <= 0
            || bundleCandidates is null
            || bundleCandidates.Count == 0)
        {
            return new 플랫폼수익의뢰선별결과(rawRequestIds, []);
        }

        var rawSet = rawRequestIds.ToHashSet(StringComparer.Ordinal);
        var selected = new List<string>();
        var selectedSet = new HashSet<string>(StringComparer.Ordinal);
        var selectedBundles = new List<운송의뢰수익묶음후보>();

        foreach (var bundle in bundleCandidates
                     .Where(x => x.묶음가능여부)
                     .Where(x => x.묶음크기 > 1)
                     .OrderByDescending(x => x.예상플랫폼순이익 ?? decimal.MinValue)
                     .ThenByDescending(x => x.우선순위점수))
        {
            var bundleRequestIds = bundle.의뢰Ids
                .Where(rawSet.Contains)
                .Where(requestIdsWithCandidate.Contains)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (bundleRequestIds.Length != bundle.묶음크기
                || selected.Count + bundleRequestIds.Length > availableAssignmentCapacity
                || bundleRequestIds.Any(x => selectedSet.Contains(x)))
            {
                continue;
            }

            foreach (var requestId in bundleRequestIds)
            {
                selectedSet.Add(requestId);
                selected.Add(requestId);
            }

            selectedBundles.Add(bundle);
        }

        if (selected.Count == 0)
        {
            return new 플랫폼수익의뢰선별결과(rawRequestIds, []);
        }

        foreach (var requestId in rawRequestIds)
        {
            if (selected.Count >= availableAssignmentCapacity)
            {
                break;
            }

            if (!requestIdsWithCandidate.Contains(requestId) || !selectedSet.Add(requestId))
            {
                continue;
            }

            selected.Add(requestId);
        }

        return new 플랫폼수익의뢰선별결과(selected, selectedBundles);
    }

    private IReadOnlyList<운송의뢰기사조합평가> AssignPlatformBundlesByDriverPerspective(
        IReadOnlyList<운송의뢰수익묶음후보> selectedBundles,
        IReadOnlyList<string> driverIds,
        IReadOnlyList<운송의뢰기사조합평가> candidates,
        배차조율전략 strategy,
        IDictionary<string, int> remainingDriverCapacityMap,
        IReadOnlyDictionary<string, decimal> revenueBundleCostBenefitMap,
        국내화물기사배정AI정책? driverAssignmentPolicy,
        배차AI판단근거 driverAssignmentEvidence)
    {
        if (selectedBundles.Count == 0 || driverIds.Count == 0 || candidates.Count == 0)
        {
            return [];
        }

        var candidateMap = candidates
            .GroupBy(x => (x.기사Id, x.의뢰Id))
            .ToDictionary(
                x => x.Key,
                x => x
                    .OrderBy(candidate => ToOptimizationCost(candidate, strategy.수급상태, revenueBundleCostBenefitMap))
                    .ThenByDescending(candidate => candidate.추천점수)
                    .First());
        var assignedRequestIds = new HashSet<string>(StringComparer.Ordinal);
        var matchedCandidates = new List<운송의뢰기사조합평가>();

        foreach (var bundle in selectedBundles
                     .OrderByDescending(x => x.예상플랫폼순이익 ?? decimal.MinValue)
                     .ThenByDescending(x => x.우선순위점수))
        {
            var requestIds = bundle.의뢰Ids
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (requestIds.Length <= 1 || requestIds.Any(x => assignedRequestIds.Contains(x)))
            {
                continue;
            }

            var driverAssignmentCandidates = driverIds
                .Where(driverId => remainingDriverCapacityMap.TryGetValue(driverId, out var capacity) && capacity >= requestIds.Length)
                .Select(driverId =>
                {
                    var driverCandidates = new List<운송의뢰기사조합평가>();
                    foreach (var requestId in requestIds)
                    {
                        if (!candidateMap.TryGetValue((driverId, requestId), out var candidate))
                        {
                            return null;
                        }

                        driverCandidates.Add(candidate);
                    }

                    var cost = driverCandidates.Sum(x =>
                        ToOptimizationCost(x, strategy.수급상태, revenueBundleCostBenefitMap)
                        + _기사배정AIService.비용보정(x, driverAssignmentPolicy));
                    var score = driverCandidates.Sum(x => x.추천점수);
                    return new 국내화물기사배정후보(driverId, driverCandidates, cost, score);
                })
                .Where(x => x is not null)
                .Cast<국내화물기사배정후보>()
                .ToArray();
            var bestDriver = _기사배정AIService
                .후보정렬(new 국내화물기사배정AI요청(driverAssignmentCandidates, driverAssignmentPolicy, driverAssignmentEvidence))
                .FirstOrDefault();
            if (bestDriver is null)
            {
                continue;
            }

            remainingDriverCapacityMap[bestDriver.기사Id] = Math.Max(0, remainingDriverCapacityMap[bestDriver.기사Id] - requestIds.Length);
            foreach (var candidate in bestDriver.후보목록)
            {
                assignedRequestIds.Add(candidate.의뢰Id);
                matchedCandidates.Add(MarkBundleDriverAssignment(candidate, bundle, bestDriver.판단근거요약));
            }
        }

        return matchedCandidates;
    }

    private static 운송의뢰기사조합평가 MarkBundleDriverAssignment(
        운송의뢰기사조합평가 candidate,
        운송의뢰수익묶음후보 bundle,
        string 판단근거요약)
    {
        var badgeSource = candidate.배지.Concat(["한 명의 기사에게 묶음 동시 배정"]);
        if (!string.IsNullOrWhiteSpace(판단근거요약))
        {
            badgeSource = badgeSource.Concat(["판단근거반영"]);
        }

        var badges = badgeSource
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var reason = string.IsNullOrWhiteSpace(candidate.추천사유)
            ? $"플랫폼 수익 묶음 {bundle.묶음키}을 한 명의 기사에게 묶음 동시 배정할 수 있습니다."
            : $"{candidate.추천사유} 플랫폼 수익 묶음 {bundle.묶음키}을 한 명의 기사에게 묶음 동시 배정할 수 있습니다.";
        if (!string.IsNullOrWhiteSpace(판단근거요약))
        {
            reason = $"{reason} {판단근거요약}";
        }

        return candidate with
        {
            추천사유 = reason,
            배지 = badges
        };
    }

    private sealed record 플랫폼수익의뢰선별결과(
        IReadOnlyList<string> 선별의뢰Ids,
        IReadOnlyList<운송의뢰수익묶음후보> 선별묶음후보목록);
}
