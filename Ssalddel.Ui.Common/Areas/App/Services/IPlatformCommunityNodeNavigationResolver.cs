namespace Ssalddel.Ui.Common.Areas.App.Services;

public enum PlatformCommunityNodeNavigationArea
{
    Community,
    Shipper,
    Driver,
    Warehouse,
    Food
}

public sealed record PlatformCommunityNodeNavigationRequest(
    string LedgerTemplateKey,
    string NodeTitle,
    string NodeKind,
    string? FormKind)
{
    public bool IsLedgerTemplate(string templateKey)
        => string.Equals(LedgerTemplateKey, templateKey, StringComparison.OrdinalIgnoreCase);

    public bool IsForm(string formKind)
        => string.Equals(FormKind, formKind, StringComparison.OrdinalIgnoreCase);

    public bool IsNodeKind(string nodeKind)
        => string.Equals(NodeKind, nodeKind, StringComparison.OrdinalIgnoreCase);

    public bool TitleContainsAny(params string[] values)
        => values.Any(value =>
            !string.IsNullOrWhiteSpace(value)
            && NodeTitle.Contains(value, StringComparison.OrdinalIgnoreCase));
}

public sealed record PlatformCommunityNodeNavigationTarget(
    string Path,
    string DestinationLabel,
    PlatformCommunityNodeNavigationArea Area);

public interface IPlatformCommunityNodeNavigationResolver
{
    PlatformCommunityNodeNavigationTarget? Resolve(
        PlatformCommunityNodeNavigationRequest request);
}

public sealed class UnsupportedPlatformCommunityNodeNavigationResolver
    : IPlatformCommunityNodeNavigationResolver
{
    public PlatformCommunityNodeNavigationTarget? Resolve(
        PlatformCommunityNodeNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return null;
    }
}
