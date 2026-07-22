namespace SsalddelAdmin.Services;

public sealed record AdminV1NavigationItem(
    string Label,
    string Route,
    string IconKey,
    bool Exact = false,
    bool ServerAdminOnly = true);

public static class AdminV1NavigationPolicy
{
    public static IReadOnlyList<AdminV1NavigationItem> MenuItems { get; } =
    [
        new("1.0 운영 대시보드", "/dashboard", "dashboard", true),
        new("1.5 공급·무역 준비", "/trade-readiness", "fact_check"),
        new("HS 코드 운영", "/customs/hs-codes", "manage_search", ServerAdminOnly: false),
        new("의뢰 목록", "/requests", "view_list"),
        new("배차대기", "/dispatch/wait", "inbox"),
        new("운행 기사", "/drivers/operating", "local_shipping"),
        new("운송 목록", "/transports", "route"),
        new("결제 관리", "/payments", "payments"),
        new("정산 관리", "/settlements", "account_balance"),
        new("문서 목록", "/documents", "description"),
        new("파일/POD", "/files/pod", "fact_check")
    ];

    private static readonly HashSet<string> ExactRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/dashboard",
        "/trade-readiness",
        "/customs/hs-codes",
        "/dispatch/wait",
        "/drivers/operating",
        "/payments",
        "/settlements",
        "/documents",
        "/documents/upload",
        "/documents/policies",
        "/documents/logs",
        "/files/pod",
        "/login",
        "/error",
        "/not-found"
    };

    private static readonly string[] PrefixRoutes =
    [
        "/requests/",
        "/transports/",
        "/documents/",
    ];

    public static bool IsAllowedRoute(string? relativePath)
    {
        var path = NormalizePath(relativePath);
        return ExactRoutes.Contains(path)
               || PrefixRoutes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    public static string ToHref(string route)
        => NormalizePath(route).TrimStart('/');

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var normalized = path.Trim();
        var queryIndex = normalized.IndexOfAny(['?', '#']);
        if (queryIndex >= 0)
        {
            normalized = normalized[..queryIndex];
        }

        normalized = normalized.StartsWith('/') ? normalized : "/" + normalized;
        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }
}
