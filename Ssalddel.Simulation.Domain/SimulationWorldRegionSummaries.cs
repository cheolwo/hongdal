using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Simulation.Domain
{

public static class SimulationWorld지역표현요약선정이유Codes
{
    public const string 분포대표 = "DistributionQuota";
    public const string 지역특색 = "RegionalSignature";
    public const string 게임맥락 = "GameplayContext";
}

public static class SimulationWorld지역표현요약상태Codes
{
    public const string 완료 = "Completed";
    public const string 일부자료부족 = "PartialDataGap";
    public const string 자료없음 = "NoSourceCandidate";
}

public static class SimulationWorld지역표현요약LodCodes
{
    public const string L0 = "L0";
    public const string L1 = "L1";
    public const string L2 = "L2";

    public static bool IsSupported(string value) =>
        string.Equals(value, L0, StringComparison.Ordinal)
        || string.Equals(value, L1, StringComparison.Ordinal)
        || string.Equals(value, L2, StringComparison.Ordinal);
}

public sealed class SimulationWorld지역표현요약LodBudget
{
    public SimulationWorld지역표현요약LodBudget(
        string lodCode,
        int totalSlots,
        int distributionSlots,
        int regionalSignatureSlots,
        int gameplayContextSlots,
        int minimumVisibleCount)
    {
        LodCode = lodCode;
        TotalSlots = totalSlots;
        DistributionSlots = distributionSlots;
        RegionalSignatureSlots = regionalSignatureSlots;
        GameplayContextSlots = gameplayContextSlots;
        MinimumVisibleCount = minimumVisibleCount;
    }

    public string LodCode { get; }
    public int TotalSlots { get; }
    public int DistributionSlots { get; }
    public int RegionalSignatureSlots { get; }
    public int GameplayContextSlots { get; }
    public int MinimumVisibleCount { get; }
}

public sealed class SimulationWorld지역표현요약Profile
{
    public string ProfileRevision { get; set; } = string.Empty;
    public int Seed { get; set; }
    public decimal MaximumCategoryShare { get; set; } = 0.40m;
    public IReadOnlyList<SimulationWorld지역표현요약LodBudget> Budgets { get; set; } =
        Array.Empty<SimulationWorld지역표현요약LodBudget>();

    public static SimulationWorld지역표현요약Profile CreateDefault() => new()
    {
        ProfileRevision = "region-presentation-summary.v1",
        Seed = 20260815,
        MaximumCategoryShare = 0.40m,
        Budgets = new[]
        {
            new SimulationWorld지역표현요약LodBudget(SimulationWorld지역표현요약LodCodes.L0, 8, 5, 2, 1, 1),
            new SimulationWorld지역표현요약LodBudget(SimulationWorld지역표현요약LodCodes.L1, 32, 19, 8, 5, 1),
            new SimulationWorld지역표현요약LodBudget(SimulationWorld지역표현요약LodCodes.L2, 120, 72, 30, 18, 3),
        },
    };

    public SimulationWorld지역표현요약LodBudget GetBudget(string lodCode) =>
        Budgets.SingleOrDefault(item => string.Equals(item.LodCode, lodCode, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(lodCode), lodCode, "지원하지 않는 지역 표현 LOD입니다.");

    public string ComputeHash()
    {
        var canonical = new StringBuilder(ProfileRevision)
            .Append('|').Append(Seed)
            .Append('|').Append(MaximumCategoryShare.ToString(CultureInfo.InvariantCulture));
        foreach (var budget in Budgets.OrderBy(item => item.LodCode, StringComparer.Ordinal))
        {
            canonical.Append('|').Append(budget.LodCode)
                .Append(':').Append(budget.TotalSlots)
                .Append(':').Append(budget.DistributionSlots)
                .Append(':').Append(budget.RegionalSignatureSlots)
                .Append(':').Append(budget.GameplayContextSlots)
                .Append(':').Append(budget.MinimumVisibleCount);
        }

        return Hash(canonical.ToString());
    }

    internal static string Hash(string value)
    {
        using var sha256 = SHA256.Create();
        return string.Concat(sha256.ComputeHash(Encoding.UTF8.GetBytes(value))
            .Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
    }
}

public sealed class SimulationWorld지역표현요약Candidate
{
    public string StableId { get; set; } = string.Empty;
    public string RegionStableId { get; set; } = string.Empty;
    public string? TileKey { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string ObjectTypeCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string VisualKey { get; set; } = string.Empty;
    public int RepresentedRecordCount { get; set; } = 1;
    public decimal? RepresentedAreaSquareMeters { get; set; }
    public int QualityScore { get; set; }
    public string? SpatialBucketCode { get; set; }
    public decimal? RegionalShare { get; set; }
    public decimal? BaselineShare { get; set; }
    public int GameplayPriority { get; set; }
    public bool HasPublicDetail { get; set; }
}

public sealed class SimulationWorld지역표현요약Item
{
    public string StableId { get; set; } = string.Empty;
    public string SourceObjectStableId { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string ObjectTypeCode { get; set; } = string.Empty;
    public string SelectionReasonCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string VisualKey { get; set; } = string.Empty;
    public int RepresentedRecordCount { get; set; }
    public decimal? RepresentedAreaSquareMeters { get; set; }
    public int VisualSlotCount { get; set; }
    public int MinimumVisibleCount { get; set; }
    public bool HasPublicDetail { get; set; }
    public bool PresentationOnly { get; set; } = true;
}

public sealed class SimulationWorld지역표현요약CategoryReport
{
    public string CategoryCode { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
    public int TotalRepresentedRecordCount { get; set; }
    public int SelectedRepresentedRecordCount { get; set; }
    public int OmittedRepresentedRecordCount { get; set; }
    public decimal TotalRepresentedAreaSquareMeters { get; set; }
    public decimal SelectedRepresentedAreaSquareMeters { get; set; }
    public int AllocatedVisualSlotCount { get; set; }
}

public sealed class SimulationWorld지역표현요약Result
{
    public string RegionStableId { get; set; } = string.Empty;
    public string? TileKey { get; set; }
    public string LodCode { get; set; } = string.Empty;
    public string ProfileRevision { get; set; } = string.Empty;
    public string ProfileHashSha256 { get; set; } = string.Empty;
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string SummaryHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public int TotalCandidateCount { get; set; }
    public int SelectedItemCount { get; set; }
    public int TotalRepresentedRecordCount { get; set; }
    public int SelectedRepresentedRecordCount { get; set; }
    public int OmittedRepresentedRecordCount { get; set; }
    public int RequestedVisualSlotCount { get; set; }
    public int AllocatedVisualSlotCount { get; set; }
    public IReadOnlyList<SimulationWorld지역표현요약Item> Items { get; set; } =
        Array.Empty<SimulationWorld지역표현요약Item>();
    public IReadOnlyList<SimulationWorld지역표현요약CategoryReport> CategoryReports { get; set; } =
        Array.Empty<SimulationWorld지역표현요약CategoryReport>();
}

public static class SimulationWorld지역표현요약Engine
{
    public static SimulationWorld지역표현요약Result Generate(
        SimulationWorld지역표현요약Profile profile,
        string regionStableId,
        string? tileKey,
        string lodCode,
        IEnumerable<SimulationWorld지역표현요약Candidate> source,
        string inputFingerprintSha256,
        DateTimeOffset generatedAtUtc)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));
        if (string.IsNullOrWhiteSpace(regionStableId))
            throw new ArgumentException("지역 고유 식별자가 필요합니다.", nameof(regionStableId));
        if (!SimulationWorld지역표현요약LodCodes.IsSupported(lodCode))
            throw new ArgumentOutOfRangeException(nameof(lodCode), lodCode, "지원하지 않는 지역 표현 LOD입니다.");

        var budget = profile.GetBudget(lodCode);
        var candidates = source
            .Where(item => string.Equals(item.RegionStableId, regionStableId, StringComparison.Ordinal)
                && (tileKey == null || string.Equals(item.TileKey, tileKey, StringComparison.Ordinal)))
            .Where(IsValidCandidate)
            .GroupBy(item => item.StableId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.StableId, StringComparer.Ordinal)
            .ToArray();

        var selected = new List<SelectedCandidate>();
        var allocatedByCategory = new Dictionary<string, int>(StringComparer.Ordinal);
        var categorySlotCap = Math.Max(1,
            (int)Math.Floor(budget.TotalSlots * profile.MaximumCategoryShare));

        SelectLane(
            candidates.Where(item => Lane(item) == SimulationWorld지역표현요약선정이유Codes.분포대표),
            budget.DistributionSlots,
            SimulationWorld지역표현요약선정이유Codes.분포대표,
            profile,
            regionStableId,
            tileKey,
            lodCode,
            categorySlotCap,
            allocatedByCategory,
            selected);
        SelectLane(
            candidates.Where(item => Lane(item) == SimulationWorld지역표현요약선정이유Codes.지역특색),
            budget.RegionalSignatureSlots,
            SimulationWorld지역표현요약선정이유Codes.지역특색,
            profile,
            regionStableId,
            tileKey,
            lodCode,
            categorySlotCap,
            allocatedByCategory,
            selected);
        SelectLane(
            candidates.Where(item => Lane(item) == SimulationWorld지역표현요약선정이유Codes.게임맥락),
            budget.GameplayContextSlots,
            SimulationWorld지역표현요약선정이유Codes.게임맥락,
            profile,
            regionStableId,
            tileKey,
            lodCode,
            categorySlotCap,
            allocatedByCategory,
            selected);

        var items = selected
            .OrderBy(item => LaneOrder(item.ReasonCode))
            .ThenBy(item => item.Candidate.CategoryCode, StringComparer.Ordinal)
            .ThenBy(item => item.Candidate.StableId, StringComparer.Ordinal)
            .Select(item => new SimulationWorld지역표현요약Item
            {
                StableId = "region-summary-item:" + SimulationWorld지역표현요약Profile.Hash(
                    regionStableId + "|" + tileKey + "|" + lodCode + "|" + item.Candidate.StableId)[..24],
                SourceObjectStableId = item.Candidate.StableId,
                CategoryCode = item.Candidate.CategoryCode,
                ObjectTypeCode = item.Candidate.ObjectTypeCode,
                SelectionReasonCode = item.ReasonCode,
                EvidenceKindCode = item.Candidate.EvidenceKindCode,
                VisualKey = item.Candidate.VisualKey,
                RepresentedRecordCount = item.Candidate.RepresentedRecordCount,
                RepresentedAreaSquareMeters = item.Candidate.RepresentedAreaSquareMeters,
                VisualSlotCount = item.SlotCount,
                MinimumVisibleCount = budget.MinimumVisibleCount,
                HasPublicDetail = item.Candidate.HasPublicDetail,
            })
            .ToArray();

        var selectedIds = items.Select(item => item.SourceObjectStableId).ToHashSet(StringComparer.Ordinal);
        var reports = candidates
            .GroupBy(item => item.CategoryCode, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var categoryItems = group.ToArray();
                var selectedCandidates = categoryItems
                    .Where(item => selectedIds.Contains(item.StableId)).ToArray();
                var totalRecords = categoryItems.Sum(item => item.RepresentedRecordCount);
                var selectedRecords = selectedCandidates.Sum(item => item.RepresentedRecordCount);
                return new SimulationWorld지역표현요약CategoryReport
                {
                    CategoryCode = group.Key,
                    CandidateCount = categoryItems.Length,
                    TotalRepresentedRecordCount = totalRecords,
                    SelectedRepresentedRecordCount = selectedRecords,
                    OmittedRepresentedRecordCount = Math.Max(0, totalRecords - selectedRecords),
                    TotalRepresentedAreaSquareMeters = categoryItems.Sum(item => item.RepresentedAreaSquareMeters ?? 0m),
                    SelectedRepresentedAreaSquareMeters = selectedCandidates.Sum(item => item.RepresentedAreaSquareMeters ?? 0m),
                    AllocatedVisualSlotCount = items
                        .Where(item => string.Equals(item.CategoryCode, group.Key, StringComparison.Ordinal))
                        .Sum(item => item.VisualSlotCount),
                };
            })
            .ToArray();

        var profileHash = profile.ComputeHash();
        var summaryHash = ComputeSummaryHash(
            regionStableId, tileKey, lodCode, profileHash, inputFingerprintSha256, items, reports);
        var totalRepresented = reports.Sum(item => item.TotalRepresentedRecordCount);
        var selectedRepresented = reports.Sum(item => item.SelectedRepresentedRecordCount);
        var allocatedSlots = items.Sum(item => item.VisualSlotCount);
        var status = candidates.Length == 0
            ? SimulationWorld지역표현요약상태Codes.자료없음
            : allocatedSlots < budget.TotalSlots || selectedRepresented < totalRepresented
                ? SimulationWorld지역표현요약상태Codes.일부자료부족
                : SimulationWorld지역표현요약상태Codes.완료;

        return new SimulationWorld지역표현요약Result
        {
            RegionStableId = regionStableId,
            TileKey = tileKey,
            LodCode = lodCode,
            ProfileRevision = profile.ProfileRevision,
            ProfileHashSha256 = profileHash,
            InputFingerprintSha256 = inputFingerprintSha256,
            SummaryHashSha256 = summaryHash,
            StatusCode = status,
            GeneratedAtUtc = generatedAtUtc,
            TotalCandidateCount = candidates.Length,
            SelectedItemCount = items.Length,
            TotalRepresentedRecordCount = totalRepresented,
            SelectedRepresentedRecordCount = selectedRepresented,
            OmittedRepresentedRecordCount = Math.Max(0, totalRepresented - selectedRepresented),
            RequestedVisualSlotCount = budget.TotalSlots,
            AllocatedVisualSlotCount = allocatedSlots,
            Items = items,
            CategoryReports = reports,
        };
    }

    private static bool IsValidCandidate(SimulationWorld지역표현요약Candidate item) =>
        !string.IsNullOrWhiteSpace(item.StableId)
        && !string.IsNullOrWhiteSpace(item.RegionStableId)
        && !string.IsNullOrWhiteSpace(item.CategoryCode)
        && !string.IsNullOrWhiteSpace(item.ObjectTypeCode)
        && !string.IsNullOrWhiteSpace(item.EvidenceKindCode)
        && !string.IsNullOrWhiteSpace(item.VisualKey)
        && item.RepresentedRecordCount > 0;

    private static string Lane(SimulationWorld지역표현요약Candidate item)
    {
        if (item.GameplayPriority > 0
            || string.Equals(item.EvidenceKindCode, SimulationWorld근거종류Codes.시나리오, StringComparison.Ordinal))
            return SimulationWorld지역표현요약선정이유Codes.게임맥락;
        if (item.RegionalShare > 0m && item.BaselineShare > 0m
            && item.RegionalShare.Value > item.BaselineShare.Value)
            return SimulationWorld지역표현요약선정이유Codes.지역특색;
        return SimulationWorld지역표현요약선정이유Codes.분포대표;
    }

    private static void SelectLane(
        IEnumerable<SimulationWorld지역표현요약Candidate> source,
        int laneBudget,
        string reasonCode,
        SimulationWorld지역표현요약Profile profile,
        string regionStableId,
        string? tileKey,
        string lodCode,
        int categorySlotCap,
        IDictionary<string, int> allocatedByCategory,
        ICollection<SelectedCandidate> selected)
    {
        var candidates = source.ToArray();
        if (laneBudget <= 0 || candidates.Length == 0) return;

        var groups = candidates
            .GroupBy(item => item.CategoryCode, StringComparer.Ordinal)
            .Select(group => new CategoryBucket(
                group.Key,
                group.ToArray(),
                group.Sum(item => (long)item.RepresentedRecordCount),
                reasonCode == SimulationWorld지역표현요약선정이유Codes.지역특색
                    ? group.Max(SignatureScore)
                    : reasonCode == SimulationWorld지역표현요약선정이유Codes.게임맥락
                        ? group.Max(item => item.GameplayPriority)
                        : 0m))
            .OrderByDescending(item => item.PriorityScore)
            .ThenByDescending(item => item.Weight)
            .ThenBy(item => item.CategoryCode, StringComparer.Ordinal)
            .ToArray();
        var totalWeight = Math.Max(1L, groups.Sum(item => item.Weight));
        var allocations = groups.ToDictionary(item => item.CategoryCode, _ => 0, StringComparer.Ordinal);

        foreach (var group in groups)
        {
            var already = Allocated(allocatedByCategory, group.CategoryCode);
            if (already >= categorySlotCap) continue;
            allocations[group.CategoryCode] = 1;
        }

        while (allocations.Values.Sum() > laneBudget)
        {
            var remove = groups
                .Where(item => allocations[item.CategoryCode] > 0)
                .OrderBy(item => item.PriorityScore)
                .ThenBy(item => item.Weight)
                .ThenByDescending(item => item.CategoryCode, StringComparer.Ordinal)
                .First();
            allocations[remove.CategoryCode]--;
        }

        var remaining = laneBudget - allocations.Values.Sum();
        while (remaining > 0)
        {
            var next = groups
                .Where(item => Allocated(allocatedByCategory, item.CategoryCode)
                    + allocations[item.CategoryCode] < categorySlotCap)
                .OrderByDescending(item =>
                    (decimal)item.Weight / totalWeight * laneBudget - allocations[item.CategoryCode])
                .ThenByDescending(item => item.PriorityScore)
                .ThenBy(item => item.CategoryCode, StringComparer.Ordinal)
                .FirstOrDefault();
            if (next is null) break;
            allocations[next.CategoryCode]++;
            remaining--;
        }

        foreach (var group in groups)
        {
            var slotCount = allocations[group.CategoryCode];
            if (slotCount <= 0) continue;
            var ordered = OrderCandidates(
                group.Candidates, reasonCode, profile.Seed, regionStableId, tileKey, lodCode);
            var selectedCandidateCount = Math.Min(slotCount, ordered.Count);
            if (selectedCandidateCount == 0) continue;
            var baseSlots = slotCount / selectedCandidateCount;
            var extraSlots = slotCount % selectedCandidateCount;
            for (var index = 0; index < selectedCandidateCount; index++)
            {
                selected.Add(new SelectedCandidate(
                    ordered[index], reasonCode, baseSlots + (index < extraSlots ? 1 : 0)));
            }

            allocatedByCategory[group.CategoryCode] =
                Allocated(allocatedByCategory, group.CategoryCode) + slotCount;
        }
    }

    private static int Allocated(IDictionary<string, int> source, string categoryCode) =>
        source.TryGetValue(categoryCode, out var value) ? value : 0;

    private static IReadOnlyList<SimulationWorld지역표현요약Candidate> OrderCandidates(
        IReadOnlyList<SimulationWorld지역표현요약Candidate> candidates,
        string reasonCode,
        int seed,
        string regionStableId,
        string? tileKey,
        string lodCode)
    {
        var result = new List<SimulationWorld지역표현요약Candidate>();
        var remaining = candidates
            .OrderByDescending(item => reasonCode == SimulationWorld지역표현요약선정이유Codes.지역특색
                ? SignatureScore(item)
                : reasonCode == SimulationWorld지역표현요약선정이유Codes.게임맥락
                    ? item.GameplayPriority
                    : item.RepresentedRecordCount)
            .ThenByDescending(item => item.QualityScore)
            .ThenBy(item => StableOrder(seed, regionStableId, tileKey, lodCode, item.StableId), StringComparer.Ordinal)
            .ToList();
        var usedBuckets = new HashSet<string>(StringComparer.Ordinal);
        while (remaining.Count > 0)
        {
            var next = remaining.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.SpatialBucketCode)
                && usedBuckets.Add(item.SpatialBucketCode!)) ?? remaining[0];
            result.Add(next);
            remaining.Remove(next);
        }

        return result;
    }

    private static decimal SignatureScore(SimulationWorld지역표현요약Candidate item)
    {
        if (item.RegionalShare is not > 0m || item.BaselineShare is not > 0m) return 0m;
        var lift = item.RegionalShare.Value / item.BaselineShare.Value;
        var support = Math.Min(1m,
            (decimal)Math.Log(1d + item.RepresentedRecordCount) / (decimal)Math.Log(21d));
        return (decimal)Math.Log(1d + (double)lift) * support;
    }

    private static string StableOrder(
        int seed, string regionStableId, string? tileKey, string lodCode, string candidateStableId) =>
        SimulationWorld지역표현요약Profile.Hash(
            seed.ToString(CultureInfo.InvariantCulture) + "|" + regionStableId + "|" + tileKey
            + "|" + lodCode + "|" + candidateStableId);

    private static string ComputeSummaryHash(
        string regionStableId,
        string? tileKey,
        string lodCode,
        string profileHash,
        string inputFingerprint,
        IEnumerable<SimulationWorld지역표현요약Item> items,
        IEnumerable<SimulationWorld지역표현요약CategoryReport> reports)
    {
        var canonical = new StringBuilder(regionStableId)
            .Append('|').Append(tileKey)
            .Append('|').Append(lodCode)
            .Append('|').Append(profileHash)
            .Append('|').Append(inputFingerprint);
        foreach (var item in items)
        {
            canonical.Append("|I:").Append(item.SourceObjectStableId)
                .Append(':').Append(item.CategoryCode)
                .Append(':').Append(item.SelectionReasonCode)
                .Append(':').Append(item.VisualSlotCount)
                .Append(':').Append(item.RepresentedRecordCount);
        }
        foreach (var report in reports)
        {
            canonical.Append("|R:").Append(report.CategoryCode)
                .Append(':').Append(report.TotalRepresentedRecordCount)
                .Append(':').Append(report.SelectedRepresentedRecordCount)
                .Append(':').Append(report.AllocatedVisualSlotCount);
        }
        return SimulationWorld지역표현요약Profile.Hash(canonical.ToString());
    }

    private static int LaneOrder(string reasonCode) => reasonCode switch
    {
        SimulationWorld지역표현요약선정이유Codes.분포대표 => 0,
        SimulationWorld지역표현요약선정이유Codes.지역특색 => 1,
        _ => 2,
    };

    private sealed class CategoryBucket
    {
        public CategoryBucket(
            string categoryCode,
            IReadOnlyList<SimulationWorld지역표현요약Candidate> candidates,
            long weight,
            decimal priorityScore)
        {
            CategoryCode = categoryCode;
            Candidates = candidates;
            Weight = weight;
            PriorityScore = priorityScore;
        }

        public string CategoryCode { get; }
        public IReadOnlyList<SimulationWorld지역표현요약Candidate> Candidates { get; }
        public long Weight { get; }
        public decimal PriorityScore { get; }
    }

    private sealed class SelectedCandidate
    {
        public SelectedCandidate(
            SimulationWorld지역표현요약Candidate candidate,
            string reasonCode,
            int slotCount)
        {
            Candidate = candidate;
            ReasonCode = reasonCode;
            SlotCount = slotCount;
        }

        public SimulationWorld지역표현요약Candidate Candidate { get; }
        public string ReasonCode { get; }
        public int SlotCount { get; }
    }
}
}
