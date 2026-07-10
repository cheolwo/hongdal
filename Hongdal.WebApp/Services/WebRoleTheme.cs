namespace Hongdal.WebApp.Services;

public sealed record WebRoleTheme(
    string RoleLabel,
    string ThemeCode,
    string ShellClass,
    string Description);

public static class WebRoleThemeResolver
{
    private static readonly WebRoleTheme GuestTheme = new(
        "방문자",
        "guest",
        "web-shell web-shell--guest",
        "로그인 전 공통 웹앱 모드");

    public static WebRoleTheme Resolve(IEnumerable<string>? roles)
    {
        var primaryRole = ResolvePrimaryRole(roles);
        if (string.IsNullOrWhiteSpace(primaryRole))
        {
            return GuestTheme;
        }

        var normalized = Normalize(primaryRole);

        if (ContainsAny(normalized, "흥여회", "홍여회", "후원회", "흥여"))
        {
            return new WebRoleTheme(primaryRole, "purple", "web-shell web-shell--purple", "자주색 계열 역할 레이아웃");
        }

        if (ContainsAny(normalized, "기사", "용달기사", "배달기사", "driver"))
        {
            return new WebRoleTheme(primaryRole, "driver", "web-shell web-shell--driver", "기사 업무 레이아웃");
        }

        if (ContainsAny(normalized, "화주", "판매자", "shipper", "seller"))
        {
            return new WebRoleTheme(primaryRole, "shipper", "web-shell web-shell--shipper", "화주/판매자 업무 레이아웃");
        }

        if (ContainsAny(normalized, "창고", "warehouse"))
        {
            return new WebRoleTheme(primaryRole, "warehouse", "web-shell web-shell--warehouse", "창고 업무 레이아웃");
        }

        if (ContainsAny(normalized, "주문", "orderer"))
        {
            return new WebRoleTheme(primaryRole, "orderer", "web-shell web-shell--orderer", "주문자 업무 레이아웃");
        }

        if (ContainsAny(normalized, "관세", "customs"))
        {
            return new WebRoleTheme(primaryRole, "customs", "web-shell web-shell--customs", "통관 업무 레이아웃");
        }

        if (ContainsAny(normalized, "관리", "운영", "서버관리자", "admin", "operator"))
        {
            return new WebRoleTheme(primaryRole, "admin", "web-shell web-shell--admin", "운영자 레이아웃");
        }

        return new WebRoleTheme(primaryRole, "member", "web-shell web-shell--member", "구성원 레이아웃");
    }

    public static string ResolvePrimaryRole(IEnumerable<string>? roles)
    {
        var cleanedRoles = roles?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        if (cleanedRoles.Length == 0)
        {
            return string.Empty;
        }

        var priorityRole = cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "흥여회", "홍여회", "후원회", "흥여"))
            ?? cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "서버관리자", "관리", "운영", "admin"))
            ?? cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "기사", "driver"))
            ?? cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "화주", "판매자", "shipper", "seller"))
            ?? cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "창고", "warehouse"))
            ?? cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "주문", "orderer"))
            ?? cleanedRoles.FirstOrDefault(x => ContainsAny(Normalize(x), "관세", "customs"));

        return priorityRole ?? cleanedRoles[0];
    }

    private static bool ContainsAny(string text, params string[] needles)
        => needles.Any(needle => text.Contains(Normalize(needle), StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string text)
        => text.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Trim();
}
