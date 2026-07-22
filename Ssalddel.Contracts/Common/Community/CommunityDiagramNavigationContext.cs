namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// Web과 모바일의 공용 다이어그램 Screen이 복원하는 탐색 문맥입니다.
/// 실제 업무 식별자와 Command는 이 문맥에 넣지 않고 3단계 업무 Screen으로 전달합니다.
/// </summary>
public sealed record CommunityDiagramNavigationContext
{
    public const int DefaultZoomPercent = 100;
    public const int MinimumZoomPercent = 75;
    public const int MaximumZoomPercent = 150;

    public string? LedgerTemplateKey { get; init; }

    public string? SelectedNode { get; init; }

    public int ZoomPercent { get; init; } = DefaultZoomPercent;

    public string? Filter { get; init; }

    public string ReturnPath { get; init; } = CommunityPageRoutes.Workspace;

    public string ToRoute()
        => CommunityPageRoutes.DiagramFor(
            LedgerTemplateKey,
            SelectedNode,
            ZoomPercent,
            Filter,
            ReturnPath);

    public static int NormalizeZoom(int? zoomPercent)
        => Math.Clamp(
            zoomPercent ?? DefaultZoomPercent,
            MinimumZoomPercent,
            MaximumZoomPercent);

    public static string NormalizeReturnPath(string? returnPath)
    {
        var value = returnPath?.Trim();
        if (string.IsNullOrWhiteSpace(value)
            || !value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("//", StringComparison.Ordinal)
            || value.Contains('\\'))
        {
            return CommunityPageRoutes.Workspace;
        }

        return value;
    }
}

public static class CommunityDiagramNavigationQueryNames
{
    public const string LedgerTemplate = "ledgerTemplate";
    public const string SelectedNode = "node";
    public const string Zoom = "zoom";
    public const string Filter = "filter";
    public const string ReturnPath = "from";
}
