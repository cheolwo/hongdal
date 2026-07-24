using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.WebApp.Models;

public sealed record CommunityPersonalSectionDefinition(
    string Key,
    string Title,
    string Href);

public sealed record CommunityPersonalRouteContext(
    CommunityPersonalSectionDefinition Section)
{
    public static readonly IReadOnlyList<CommunityPersonalSectionDefinition> Sections =
    [
        new("overview", "내 정보", CommunityPageRoutes.Personal),
        new("posts", "내 글", $"{CommunityPageRoutes.Personal}/posts"),
        new("actions", "참여 중", $"{CommunityPageRoutes.Personal}/actions"),
        new("ledgers", "내 원장", $"{CommunityPageRoutes.Personal}/ledgers"),
        new("notifications", "알림", $"{CommunityPageRoutes.Personal}/notifications"),
        new("decorations", "꾸미기", $"{CommunityPageRoutes.Personal}/decorations"),
        new("settings", "사용 설정", $"{CommunityPageRoutes.Personal}/settings")
    ];

    public static IReadOnlyList<CommunityPersonalSectionDefinition> VisibleNavigationSections { get; } =
    [
        Sections[0],
        Sections[1]
    ];

    public string SectionKey => Section.Key;
    public string PageTitle => $"{Section.Title} · 살뜰 커뮤니티";

    public static CommunityPersonalRouteContext Resolve(string? sectionKey)
    {
        var requestedKey = sectionKey?.Trim();
        var section = Sections.FirstOrDefault(item =>
                          string.Equals(item.Key, requestedKey, StringComparison.OrdinalIgnoreCase))
                      ?? Sections[0];

        return new CommunityPersonalRouteContext(section);
    }
}
