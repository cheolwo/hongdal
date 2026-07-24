namespace Ssalddel.Contracts.Common.Community;

public static class CommunityPostTopicClassificationCodes
{
    public const string General = "general";
    public const string Periodic = "periodic";

    public static string DisplayName(string? code)
        => string.Equals(code, Periodic, StringComparison.OrdinalIgnoreCase)
            ? "주기성"
            : "일반";
}

public static class CommunityPeriodicPostVisibilityModes
{
    public const string All = "all";
    public const string Exclude = "exclude";
    public const string Only = "only";

    public static string Normalize(string? value)
        => value?.Trim().ToLowerInvariant() switch
        {
            Exclude => Exclude,
            Only => Only,
            _ => All
        };
}

public static class CommunityPeriodicPostTopicCatalog
{
    public const string GeneralListFilter = "일반글";
    public const string PeriodicListFilter = "주기성";

    public static bool SupportsBoard(string? boardKeyOrName)
        => CommunityActivityBoardCatalog.IsActivityBoard(boardKeyOrName)
           || CommunityPeriodicDataBoardCatalog.IsDataBoard(boardKeyOrName);

    public static bool IsTopicFilter(string? listFilter)
        => string.Equals(
               listFilter?.Trim(),
               GeneralListFilter,
               StringComparison.OrdinalIgnoreCase)
           || string.Equals(
               listFilter?.Trim(),
               PeriodicListFilter,
               StringComparison.OrdinalIgnoreCase);

    public static string VisibilityFor(string? listFilter)
        => string.Equals(
            listFilter?.Trim(),
            PeriodicListFilter,
            StringComparison.OrdinalIgnoreCase)
            ? CommunityPeriodicPostVisibilityModes.Only
            : string.Equals(
                listFilter?.Trim(),
                GeneralListFilter,
                StringComparison.OrdinalIgnoreCase)
                ? CommunityPeriodicPostVisibilityModes.Exclude
                : CommunityPeriodicPostVisibilityModes.All;
}
