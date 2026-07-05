namespace Hongdal.Contracts.Common.ViewSettings;

public static class ViewCompositionSurfaceCode
{
    public const string PrimaryNavigation = "primary-navigation";
    public const string ProfileMenu = "profile-menu";
    public const string Dashboard = "dashboard";
    public const string Home = "home";
}

public static class ViewCompositionItemKindCode
{
    public const string View = "view";
    public const string Widget = "widget";
    public const string Action = "action";
    public const string Link = "link";
}

public sealed record ViewCompositionCatalogItem(
    string AppKey,
    string RoleName,
    string Surface,
    string ItemKey,
    string Kind,
    string DisplayName,
    string Route,
    string IconKey,
    string ComponentKey,
    bool IsRequired,
    bool DefaultPolicyEnabled,
    int SortOrder,
    int ColumnSpan = 1,
    int RowSpan = 1,
    string Area = "");

public sealed record ViewCompositionPolicyOverride(
    string AppKey,
    string RoleName,
    string Surface,
    string ItemKey,
    bool PolicyEnabled,
    int? SortOrder = null,
    int? ColumnSpan = null,
    int? RowSpan = null);

public sealed record ViewCompositionItem(
    string AppKey,
    string RoleName,
    string Surface,
    string ItemKey,
    string Kind,
    string DisplayName,
    string Route,
    string IconKey,
    string ComponentKey,
    string Area,
    bool IsRequired,
    bool PolicyEnabled,
    bool EffectiveVisible,
    int SortOrder,
    int ColumnSpan,
    int RowSpan);

public sealed record ViewCompositionPlan(
    string AppKey,
    string RoleName,
    string Surface,
    IReadOnlyList<ViewCompositionItem> Items);

public static class ViewCompositionPlanner
{
    public static ViewCompositionPlan BuildPlan(
        IEnumerable<ViewCompositionCatalogItem> catalogItems,
        string appKey,
        string roleName,
        string surface,
        IEnumerable<ViewCompositionPolicyOverride>? overrides = null,
        bool includeHidden = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(surface);

        var overrideMap = (overrides ?? [])
            .Where(x => Matches(x.AppKey, appKey) && Matches(x.RoleName, roleName) && Matches(x.Surface, surface))
            .ToDictionary(x => x.ItemKey, StringComparer.OrdinalIgnoreCase);

        var items = catalogItems
            .Where(x => Matches(x.AppKey, appKey) && Matches(x.RoleName, roleName) && Matches(x.Surface, surface))
            .Select(x => BuildItem(x, overrideMap.TryGetValue(x.ItemKey, out var policy) ? policy : null))
            .Where(x => includeHidden || x.EffectiveVisible)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ViewCompositionPlan(appKey, roleName, surface, items);
    }

    private static ViewCompositionItem BuildItem(ViewCompositionCatalogItem definition, ViewCompositionPolicyOverride? policy)
    {
        var policyEnabled = policy?.PolicyEnabled ?? definition.DefaultPolicyEnabled;
        var effectiveVisible = definition.IsRequired || policyEnabled;

        return new ViewCompositionItem(
            definition.AppKey,
            definition.RoleName,
            definition.Surface,
            definition.ItemKey,
            definition.Kind,
            definition.DisplayName,
            definition.Route,
            definition.IconKey,
            definition.ComponentKey,
            definition.Area,
            definition.IsRequired,
            policyEnabled,
            effectiveVisible,
            policy?.SortOrder ?? definition.SortOrder,
            NormalizeSpan(policy?.ColumnSpan ?? definition.ColumnSpan),
            NormalizeSpan(policy?.RowSpan ?? definition.RowSpan));
    }

    private static int NormalizeSpan(int value)
    {
        return Math.Clamp(value, 1, 12);
    }

    private static bool Matches(string actual, string expected)
    {
        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }
}
