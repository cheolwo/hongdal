using MudBlazor;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public sealed record CommunityLedgerNodeLayout(DiagramNodeDto Node, int X, int Y);

public static class CommunityLedgerDiagramPresentation
{
    public const int CanvasWidth = 560;
    public const int NodeWidth = 140;
    public const int NodeHeight = 108;

    private const int LeftMargin = 30;
    private const int TopMargin = 48;
    private const int ColumnGap = 40;
    private const int RowGap = 72;
    private const int ColumnCount = 3;

    public static IReadOnlyList<CommunityLedgerNodeLayout> BuildNodeLayouts(DiagramSnapshotDto diagram)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        return OrderedNodes(diagram)
            .Select((node, index) => new CommunityLedgerNodeLayout(
                node,
                LeftMargin + (index % ColumnCount * (NodeWidth + ColumnGap)),
                TopMargin + (index / ColumnCount * (NodeHeight + RowGap))))
            .ToArray();
    }

    public static IReadOnlyList<DiagramNodeDto> OrderedNodes(DiagramSnapshotDto diagram)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        return diagram.Nodes.OrderBy(node => node.Y).ThenBy(node => node.X).ToArray();
    }

    public static IReadOnlyList<DiagramEdgeDto> OutgoingEdges(DiagramSnapshotDto diagram, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        return diagram.Edges
            .Where(edge => string.Equals(edge.FromNodeId, nodeId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static int BuildCanvasHeight(int nodeCount)
    {
        var rows = Math.Max(1, (int)Math.Ceiling(nodeCount / (double)ColumnCount));
        return TopMargin + (rows * NodeHeight) + ((rows - 1) * RowGap) + 58;
    }

    public static string BuildNodeStyle(CommunityLedgerNodeLayout layout)
        => $"left:{layout.X}px;top:{layout.Y}px;width:{NodeWidth}px;height:{NodeHeight}px";

    public static string BuildEdgePath(CommunityLedgerNodeLayout from, CommunityLedgerNodeLayout to)
    {
        if (to.X > from.X)
        {
            var startX = from.X + NodeWidth;
            var startY = from.Y + (NodeHeight / 2d);
            var endX = to.X - 9;
            var endY = to.Y + (NodeHeight / 2d);
            return FormattableString.Invariant($"M {startX} {startY} H {endX}");
        }

        var downStartX = from.X + (NodeWidth / 2d);
        var downStartY = from.Y + NodeHeight;
        var downEndX = to.X + (NodeWidth / 2d);
        var downEndY = to.Y - 9;
        var middleY = (downStartY + downEndY) / 2d;
        var horizontalDirection = downEndX < downStartX ? -1 : 1;
        const double corner = 9d;
        return FormattableString.Invariant(
            $"M {downStartX} {downStartY} V {middleY - corner} Q {downStartX} {middleY} {downStartX + (horizontalDirection * corner)} {middleY} H {downEndX - (horizontalDirection * corner)} Q {downEndX} {middleY} {downEndX} {middleY + corner} V {downEndY}");
    }

    public static string BuildNodeClass(
        PlatformCommunityPostLedgerContextResponse context,
        DiagramNodeDto node,
        string? selectedNodeId)
        => $"ledger-diagram-detail__node {BuildNodeKindClass(node)} {BuildNodeStateClass(BuildNodeState(context, node))}" +
           (IsSelected(node.NodeId, selectedNodeId) ? " ledger-diagram-detail__node--selected" : string.Empty);

    public static string BuildMobileNodeClass(
        PlatformCommunityPostLedgerContextResponse context,
        DiagramNodeDto node,
        string? selectedNodeId)
        => $"ledger-diagram-detail__mobile-node {BuildNodeKindClass(node)} {BuildNodeStateClass(BuildNodeState(context, node))}" +
           (IsSelected(node.NodeId, selectedNodeId) ? " ledger-diagram-detail__mobile-node--selected" : string.Empty);

    public static string BuildNodeKindClass(DiagramNodeDto node)
        => node.Kind?.Trim().ToLowerInvariant() switch
        {
            "participant" => "ledger-diagram-detail__kind--participant",
            "place" => "ledger-diagram-detail__kind--place",
            "item" => "ledger-diagram-detail__kind--item",
            "state" => "ledger-diagram-detail__kind--state",
            "settlement" => "ledger-diagram-detail__kind--settlement",
            "order" => "ledger-diagram-detail__kind--order",
            "handoff" => "ledger-diagram-detail__kind--handoff",
            _ => "ledger-diagram-detail__kind--generic"
        };

    public static string BuildNodeStateClass(string state)
    {
        if (state.Contains("완료", StringComparison.OrdinalIgnoreCase)
            || state.Contains("확정", StringComparison.OrdinalIgnoreCase))
        {
            return "ledger-diagram-detail__state--complete";
        }

        if (state.Contains("대기", StringComparison.OrdinalIgnoreCase)
            || state.Contains("진행", StringComparison.OrdinalIgnoreCase)
            || state.Contains("준비", StringComparison.OrdinalIgnoreCase)
            || state.Contains("중", StringComparison.OrdinalIgnoreCase)
            || state.Contains("필요", StringComparison.OrdinalIgnoreCase)
            || state.Contains("모집", StringComparison.OrdinalIgnoreCase))
        {
            return "ledger-diagram-detail__state--active";
        }

        return "ledger-diagram-detail__state--upcoming";
    }

    public static string ResolveNodeIcon(DiagramNodeDto node)
        => node.Kind?.Trim().ToLowerInvariant() switch
        {
            "participant" => Icons.Material.Filled.Groups,
            "place" => Icons.Material.Filled.LocationOn,
            "item" => Icons.Material.Filled.Inventory2,
            "state" => Icons.Material.Filled.LocalShipping,
            "settlement" => Icons.Material.Filled.Payments,
            "order" => Icons.Material.Filled.ReceiptLong,
            "handoff" => Icons.Material.Filled.SwapHoriz,
            _ => Icons.Material.Filled.AccountTree
        };

    public static string ResolveNodeTypeLabel(DiagramNodeDto node)
        => node.Kind?.Trim().ToLowerInvariant() switch
        {
            "participant" => "참여",
            "place" => "장소",
            "item" => "물품",
            "state" => "진행",
            "settlement" => "정산",
            "order" => "주문",
            "handoff" => "전달",
            _ => "원장 블록"
        };

    public static bool IsSelected(string nodeId, string? selectedNodeId)
        => string.Equals(nodeId, selectedNodeId, StringComparison.OrdinalIgnoreCase);

    public static string BuildNodeState(
        PlatformCommunityPostLedgerContextResponse context,
        DiagramNodeDto node)
    {
        var block = context.블록목록.FirstOrDefault(item =>
            string.Equals(item.블록Id, node.NodeId, StringComparison.OrdinalIgnoreCase));
        return !string.IsNullOrWhiteSpace(block?.상태)
            ? block.상태
            : !string.IsNullOrWhiteSpace(node.Description)
                ? node.Description
                : "상태 정보 없음";
    }

    public static int BuildNodeSequence(DiagramSnapshotDto diagram, DiagramNodeDto node)
        => Array.FindIndex(OrderedNodes(diagram).ToArray(), item =>
            string.Equals(item.NodeId, node.NodeId, StringComparison.OrdinalIgnoreCase)) + 1;

    public static DiagramNodeDto? FindNode(
        PlatformCommunityPostLedgerContextResponse? context,
        string? nodeId)
        => context?.다이어그램?.Nodes.FirstOrDefault(node =>
            string.Equals(node.NodeId, nodeId, StringComparison.OrdinalIgnoreCase));

    public static PlatformCommunityLedgerBlockResponse? FindBlock(
        PlatformCommunityPostLedgerContextResponse? context,
        string? nodeId)
        => context?.블록목록.FirstOrDefault(block =>
            string.Equals(block.블록Id, nodeId, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<PlatformCommunityLedgerNodeActionResponse> FindNodeActions(
        PlatformCommunityPostLedgerContextResponse? context,
        string? nodeId)
        => context?.노드행동목록
               .Where(action => string.Equals(action.블록Id, nodeId, StringComparison.OrdinalIgnoreCase))
               .ToArray()
           ?? [];

    public static string BuildAssigneeSummary(
        PlatformCommunityPostLedgerContextResponse context,
        string nodeId)
    {
        var assignees = FindBlock(context, nodeId)?.담당자목록 ?? [];
        var primary = assignees.FirstOrDefault(assignee =>
            string.Equals(
                assignee.ResponsibilityType,
                CommunityLedgerBlockResponsibilityTypes.Primary,
                StringComparison.OrdinalIgnoreCase));
        if (primary is not null)
        {
            return primary.DisplayName;
        }

        return assignees.Count == 0
            ? string.Empty
            : assignees.Count == 1
                ? assignees[0].DisplayName
                : $"{assignees[0].DisplayName} 외 {assignees.Count - 1}명";
    }

    public static string ResolveAccessScopeLabel(PlatformCommunityPostLedgerContextResponse context)
        => context.역할범위조회여부
            ? $"{context.접근역할명} 역할 범위"
            : context.상세조회가능여부
                ? "참여자 상세"
                : "공개 항목";

    public static string ResolveOrganizationRelationshipLabel(DiagramOrganizationReferenceDto organization)
        => organization.IsPlatformPartner ? "플랫폼 제휴" : "플랫폼 제휴 없음";

    public static string ResolveOrganizationVerificationLabel(DiagramOrganizationReferenceDto organization)
        => organization.CanBeSelectedForOperations
            ? "업무 선택 가능"
            : string.Equals(
                organization.CompanySourceVerificationStatusCode,
                DiagramOrganizationVerificationStatusCodes.PublicSourceReviewed,
                StringComparison.OrdinalIgnoreCase)
                ? "공개 출처 확인"
                : "별도 검증 필요";

    public static string? ResolveExternalHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && uri.Scheme is "http" or "https"
            ? uri.AbsoluteUri
            : null;
}
