namespace Ssalddel.WebApp.Models;

public sealed record WebExperienceRole(
    string Key,
    string ThemeClass,
    string Eyebrow,
    string Title,
    string Description,
    string ImageUrl,
    string ImageAlt,
    string Icon,
    string StartHref,
    string AppHref,
    string StartLabel,
    IReadOnlyList<WebAppPageLink> Screens)
{
    public string HrefFor(string route)
        => $"{AppHref.TrimEnd('/')}/{route.TrimStart('/')}";
}
