namespace 홍달.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율Service
{
    private static IReadOnlyList<운송의뢰기사조합평가> Optimize(
        IReadOnlyList<string> requestIds,
        IReadOnlyList<string> driverIds,
        IReadOnlyList<운송의뢰기사조합평가> candidates,
        배차조율전략 strategy,
        IReadOnlyDictionary<string, decimal> revenueBundleCostBenefitMap,
        I국내화물기사배정AIService driverAssignmentAiService,
        국내화물기사배정AI정책? driverAssignmentPolicy)
    {
        if (requestIds.Count == 0 || driverIds.Count == 0 || candidates.Count == 0)
        {
            return [];
        }

        var requestIndex = requestIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index, StringComparer.Ordinal);
        var driverIndex = driverIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index, StringComparer.Ordinal);

        var source = 0;
        var requestOffset = 1;
        var driverOffset = requestOffset + requestIds.Count;
        var sink = driverOffset + driverIds.Count;
        var graph = new MinCostFlowGraph(sink + 1);

        for (var i = 0; i < requestIds.Count; i++)
        {
            graph.AddEdge(source, requestOffset + i, 1, 0, null);
        }

        foreach (var candidate in candidates)
        {
            if (!requestIndex.TryGetValue(candidate.의뢰Id, out var req)
                || !driverIndex.TryGetValue(candidate.기사Id, out var drv))
            {
                continue;
            }

            graph.AddEdge(
                requestOffset + req,
                driverOffset + drv,
                1,
                ToOptimizationCost(candidate, strategy.수급상태, revenueBundleCostBenefitMap)
                + driverAssignmentAiService.비용보정(candidate, driverAssignmentPolicy),
                candidate);
        }

        var maxFlow = 0;
        for (var i = 0; i < driverIds.Count; i++)
        {
            var capacity = strategy.기사용량.TryGetValue(driverIds[i], out var value)
                ? Math.Max(0, value)
                : 0;
            if (capacity <= 0)
            {
                continue;
            }

            maxFlow += capacity;
            graph.AddEdge(driverOffset + i, sink, capacity, 0, null);
        }

        graph.Run(source, sink, Math.Min(requestIds.Count, maxFlow));

        return graph.AssignedCandidates()
            .OrderBy(x => x.예상총비용 ?? decimal.MaxValue)
            .ThenByDescending(x => x.예상순이익 ?? decimal.MinValue)
            .ThenByDescending(x => x.추천점수)
            .ThenBy(x => x.상차지거리Km ?? decimal.MaxValue)
            .ToArray();
    }

    private static IReadOnlyList<운송의뢰기사조합평가> OptimizeByDeliveryScope(
        IReadOnlyList<string> requestIds,
        IReadOnlyList<string> driverIds,
        IReadOnlyList<운송의뢰기사조합평가> candidates,
        배차조율전략 strategy,
        IReadOnlyDictionary<string, string> requestScopeMap,
        IReadOnlyDictionary<string, string> driverScopeMap,
        IReadOnlyDictionary<string, decimal> revenueBundleCostBenefitMap,
        I국내화물기사배정AIService driverAssignmentAiService,
        국내화물기사배정AI정책? driverAssignmentPolicy)
    {
        if (requestIds.Count == 0 || driverIds.Count == 0 || candidates.Count == 0)
        {
            return [];
        }

        var assignedRequests = new HashSet<string>(StringComparer.Ordinal);
        var remainingCapacity = strategy.기사용량.ToDictionary(x => x.Key, x => Math.Max(0, x.Value), StringComparer.Ordinal);
        var matched = new List<운송의뢰기사조합평가>();
        var scopeKeys = requestScopeMap.Values
            .Concat(driverScopeMap.Values)
            .Where(x => !string.IsNullOrWhiteSpace(x) && x != "unknown")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        foreach (var scopeKey in scopeKeys)
        {
            var scopedRequests = requestIds
                .Where(id => !assignedRequests.Contains(id))
                .Where(id => requestScopeMap.TryGetValue(id, out var value) && string.Equals(value, scopeKey, StringComparison.Ordinal))
                .ToArray();
            var scopedDrivers = driverIds
                .Where(id => remainingCapacity.TryGetValue(id, out var capacity) && capacity > 0)
                .Where(id => driverScopeMap.TryGetValue(id, out var value) && string.Equals(value, scopeKey, StringComparison.Ordinal))
                .ToArray();
            if (scopedRequests.Length == 0 || scopedDrivers.Length == 0)
            {
                continue;
            }

            var scopedCapacity = remainingCapacity
                .Where(x => scopedDrivers.Contains(x.Key, StringComparer.Ordinal))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            var scopedCandidates = candidates
                .Where(x => scopedRequests.Contains(x.의뢰Id, StringComparer.Ordinal))
                .Where(x => scopedDrivers.Contains(x.기사Id, StringComparer.Ordinal))
                .ToArray();
            var scopedMatched = Optimize(
                scopedRequests,
                scopedDrivers,
                scopedCandidates,
                strategy with { 기사용량 = scopedCapacity },
                revenueBundleCostBenefitMap,
                driverAssignmentAiService,
                driverAssignmentPolicy);
            foreach (var candidate in scopedMatched)
            {
                matched.Add(candidate);
                assignedRequests.Add(candidate.의뢰Id);
                remainingCapacity[candidate.기사Id] = Math.Max(0, remainingCapacity[candidate.기사Id] - 1);
            }
        }

        var fallbackRequests = requestIds
            .Where(id => !assignedRequests.Contains(id))
            .ToArray();
        var fallbackDrivers = driverIds
            .Where(id => remainingCapacity.TryGetValue(id, out var capacity) && capacity > 0)
            .ToArray();
        if (fallbackRequests.Length > 0 && fallbackDrivers.Length > 0)
        {
            var fallbackCandidates = candidates
                .Where(x => fallbackRequests.Contains(x.의뢰Id, StringComparer.Ordinal))
                .Where(x => fallbackDrivers.Contains(x.기사Id, StringComparer.Ordinal))
                .ToArray();
            matched.AddRange(Optimize(
                fallbackRequests,
                fallbackDrivers,
                fallbackCandidates,
                strategy with { 기사용량 = remainingCapacity },
                revenueBundleCostBenefitMap,
                driverAssignmentAiService,
                driverAssignmentPolicy));
        }

        return matched;
    }
}
