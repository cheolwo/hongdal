using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.WebApp.Services;

public static class CommunityWorldMapDeepLink
{
    public const string CountryQueryKey = CommunityWorldMapNavigationQueryNames.Country;
    public const string LayersQueryKey = CommunityWorldMapNavigationQueryNames.Layers;
    public const string MarkerQueryKey = CommunityWorldMapNavigationQueryNames.Marker;
    public const string ObservationQueryKey = CommunityWorldMapNavigationQueryNames.Observation;
    public const string SnapshotRevisionQueryKey = CommunityWorldMapNavigationQueryNames.SnapshotRevision;
    public const string SourceVersionQueryKey = CommunityWorldMapNavigationQueryNames.SourceVersion;
    public const string LedgerQueryKey = CommunityWorldMapNavigationQueryNames.Ledger;
    public const string NoLayersValue = "none";
    public const string SourceVersionMatched = "source-version-matched";
    public const string SourceVersionMatchedSnapshotUpdated =
        "source-version-matched-snapshot-updated";
    public const string SourceVersionUpdated = "source-version-updated";
    public const string SnapshotRevisionMatched = "snapshot-revision-matched";
    public const string SnapshotRevisionUpdated = "snapshot-revision-updated";

    public static string? NormalizeCountryCode(
        string? value,
        IEnumerable<string> allowedCountryCodes)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        return normalized is not null
               && allowedCountryCodes.Contains(normalized, StringComparer.Ordinal)
            ? normalized
            : null;
    }

    public static IReadOnlyList<string>? ParseLayerCodes(
        string? value,
        IEnumerable<string> allowedLayerCodes)
    {
        if (value is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(value)
            || string.Equals(value.Trim(), NoLayersValue, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var allowed = allowedLayerCodes.ToHashSet(StringComparer.Ordinal);
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(allowed.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public static string SerializeLayerCodes(
        IEnumerable<string> selectedLayerCodes,
        IEnumerable<string> orderedAvailableLayerCodes)
    {
        var selected = selectedLayerCodes.ToHashSet(StringComparer.Ordinal);
        var ordered = orderedAvailableLayerCodes
            .Where(selected.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return ordered.Length == 0
            ? NoLayersValue
            : string.Join(',', ordered);
    }

    public static string? NormalizeStableId(string? value)
    {
        var normalized = value?.Trim();
        return normalized is { Length: > 0 and <= 200 }
               && !normalized.Any(char.IsControl)
            ? normalized
            : null;
    }

    public static string? ResolveEvidenceVersionStatus(
        string? requestedSnapshotRevision,
        string? requestedSourceVersion,
        string? currentSnapshotRevision,
        string? currentSourceVersion)
    {
        if (!string.IsNullOrWhiteSpace(requestedSourceVersion))
        {
            if (!string.Equals(
                    requestedSourceVersion,
                    currentSourceVersion,
                    StringComparison.Ordinal))
            {
                return SourceVersionUpdated;
            }

            return string.Equals(
                requestedSnapshotRevision,
                currentSnapshotRevision,
                StringComparison.Ordinal)
                ? SourceVersionMatched
                : SourceVersionMatchedSnapshotUpdated;
        }

        if (string.IsNullOrWhiteSpace(requestedSnapshotRevision))
        {
            return null;
        }

        return string.Equals(
            requestedSnapshotRevision,
            currentSnapshotRevision,
            StringComparison.Ordinal)
            ? SnapshotRevisionMatched
            : SnapshotRevisionUpdated;
    }
}
