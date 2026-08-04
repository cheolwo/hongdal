using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Services.Community;

public interface I커뮤니티세계지도원장ProjectionCache
{
    bool TryGetPublic(
        커뮤니티세계지도원장ProjectionQuery query,
        out IReadOnlyList<커뮤니티세계지도원장ProjectionDto> projections);

    void SetPublic(
        커뮤니티세계지도원장ProjectionQuery query,
        IReadOnlyList<커뮤니티세계지도원장ProjectionDto> projections);

    void Invalidate(string ledgerTemplateKey, string mapMarkerId);
}

/// <summary>
/// 공개 집계 projection만 짧게 보관합니다. 개인·참여자·운영자 projection은 cache하지 않습니다.
/// 근거 version이 없으면 cache를 우회하며 process 재시작 시 비어 있어도 원장에서 재생성됩니다.
/// </summary>
public sealed class 커뮤니티세계지도원장ProjectionCache : I커뮤니티세계지도원장ProjectionCache
{
    private const int MaximumEntries = 1024;
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<CacheKey, CacheEntry> entries = new();
    private readonly TimeProvider timeProvider;

    public 커뮤니티세계지도원장ProjectionCache()
        : this(TimeProvider.System)
    {
    }

    public 커뮤니티세계지도원장ProjectionCache(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public bool TryGetPublic(
        커뮤니티세계지도원장ProjectionQuery query,
        out IReadOnlyList<커뮤니티세계지도원장ProjectionDto> projections)
    {
        projections = [];
        if (!TryBuildKey(query, out var key)
            || !entries.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAtUtc <= timeProvider.GetUtcNow())
        {
            entries.TryRemove(key, out _);
            return false;
        }

        projections = entry.Projections;
        return true;
    }

    public void SetPublic(
        커뮤니티세계지도원장ProjectionQuery query,
        IReadOnlyList<커뮤니티세계지도원장ProjectionDto> projections)
    {
        ArgumentNullException.ThrowIfNull(projections);
        if (!TryBuildKey(query, out var key)
            || projections.Any(item => !string.Equals(
                item.ViewerScopeCode,
                커뮤니티세계지도원장ViewerScopeCodes.Public,
                StringComparison.Ordinal)))
        {
            return;
        }

        TrimIfNeeded();
        entries[key] = new CacheEntry(
            projections.ToArray(),
            timeProvider.GetUtcNow().Add(Lifetime));
    }

    public void Invalidate(string ledgerTemplateKey, string mapMarkerId)
    {
        var templateKey = Clean(ledgerTemplateKey, 120);
        var markerId = Clean(mapMarkerId, 160);
        if (templateKey is null || markerId is null)
        {
            return;
        }

        foreach (var key in entries.Keys.Where(key =>
                     string.Equals(key.LedgerTemplateKey, templateKey, StringComparison.OrdinalIgnoreCase)
                     && string.Equals(key.MapMarkerId, markerId, StringComparison.Ordinal)))
        {
            entries.TryRemove(key, out _);
        }
    }

    private void TrimIfNeeded()
    {
        if (entries.Count < MaximumEntries)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        foreach (var pair in entries.Where(pair => pair.Value.ExpiresAtUtc <= now))
        {
            entries.TryRemove(pair.Key, out _);
        }

        if (entries.Count < MaximumEntries)
        {
            return;
        }

        foreach (var key in entries
                     .OrderBy(pair => pair.Value.ExpiresAtUtc)
                     .Take(Math.Max(1, entries.Count - MaximumEntries + 1))
                     .Select(pair => pair.Key))
        {
            entries.TryRemove(key, out _);
        }
    }

    private static bool TryBuildKey(
        커뮤니티세계지도원장ProjectionQuery query,
        out CacheKey key)
    {
        key = default;
        var templateKey = Clean(query.LedgerTemplateKey, 120);
        var markerId = Clean(query.MapMarkerId, 160);
        var evidenceVersion = Clean(query.EvidenceSnapshotVersion, 200);
        if (templateKey is null || markerId is null || evidenceVersion is null)
        {
            return false;
        }

        key = new CacheKey(
            templateKey,
            markerId,
            Clean(query.AdministrativeRegionKey, 120),
            Clean(query.CountryCode, 8)?.ToUpperInvariant(),
            query.EvidenceFreshnessCode,
            evidenceVersion);
        return true;
    }

    private static string? Clean(string? value, int maximumLength)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
               || normalized.Length > maximumLength
               || normalized.Any(char.IsControl)
            ? null
            : normalized;
    }

    private readonly record struct CacheKey(
        string LedgerTemplateKey,
        string MapMarkerId,
        string? AdministrativeRegionKey,
        string? CountryCode,
        string EvidenceFreshnessCode,
        string EvidenceSnapshotVersion);

    private sealed record CacheEntry(
        IReadOnlyList<커뮤니티세계지도원장ProjectionDto> Projections,
        DateTimeOffset ExpiresAtUtc);
}
