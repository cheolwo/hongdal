namespace Ssalddel.WebApp.Models;

public sealed record CommunityPersonalSectionDefinition(
    string Key,
    string Title,
    string Href);

public sealed record CommunityPersonalRouteContext(
    CommunityPersonalSectionDefinition Section,
    string? ProductKey)
{
    public static readonly IReadOnlyList<CommunityPersonalSectionDefinition> Sections =
    [
        new("overview", "내 정보", "/community/me"),
        new("posts", "내 글", "/community/me/posts"),
        new("actions", "참여 중", "/community/me/actions"),
        new("ledgers", "내 원장", "/community/me/ledgers"),
        new("notifications", "알림", "/community/me/notifications"),
        new("decorations", "꾸미기", "/community/decorations"),
        new("settings", "사용 설정", "/community/me/settings")
    ];

    public string SectionKey => Section.Key;
    public string PageTitle => $"{Section.Title} · 살뜰 커뮤니티";

    public static CommunityPersonalRouteContext Resolve(
        string? relativePath,
        string? sectionKey,
        string? productKey)
    {
        var path = (relativePath ?? string.Empty)
            .Split('?', '#')[0]
            .Trim('/');
        var isDecorationRoute = path.StartsWith(
            "community/decorations",
            StringComparison.OrdinalIgnoreCase);
        var requestedKey = !string.IsNullOrWhiteSpace(productKey)
                           || isDecorationRoute
                           || string.Equals(sectionKey, "decorations", StringComparison.OrdinalIgnoreCase)
            ? "decorations"
            : sectionKey?.Trim();
        var section = Sections.FirstOrDefault(item =>
                          string.Equals(item.Key, requestedKey, StringComparison.OrdinalIgnoreCase))
                      ?? Sections[0];

        return new CommunityPersonalRouteContext(section, productKey);
    }
}
