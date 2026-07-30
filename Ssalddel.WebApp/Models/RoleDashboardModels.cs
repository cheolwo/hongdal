namespace Ssalddel.WebApp.Models;

public enum RoleDashboardTone
{
    Default,
    Primary,
    Warning,
    Success
}

public sealed record RoleDashboardMetric(
    string Label,
    string Value,
    string? Hint = null);

public sealed record RoleDashboardTask(
    string Title,
    string Description,
    string ActionLabel,
    string Href,
    RoleDashboardTone Tone = RoleDashboardTone.Primary);

public enum RoleFlowPalette
{
    Driver,
    Warehouse
}

public sealed record RoleFlowListItem(
    string Title,
    string Description,
    string StatusLabel,
    string? Href = null,
    RoleDashboardTone Tone = RoleDashboardTone.Primary);
